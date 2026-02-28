using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.DTOs;
using backend.Models;
using backend.Services;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly TokenService _tokenService;

    public AuthController(AppDbContext db, TokenService tokenService)
    {
        _db = db;
        _tokenService = tokenService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponseDto>> Register(RegisterDto dto)
    {
        if (await _db.Users.AnyAsync(u => u.Email == dto.Email))
            return BadRequest(new { message = "Email already exists" });

        if (await _db.Users.AnyAsync(u => u.Username == dto.Username))
            return BadRequest(new { message = "Username already exists" });

        var user = new User
        {
            Username = dto.Username,
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password)
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        var token = _tokenService.CreateToken(user);
        var refreshToken = _tokenService.CreateRefreshToken(user.Id);
        _db.RefreshTokens.Add(refreshToken);
        await _db.SaveChangesAsync();

        return Ok(new AuthResponseDto(token, refreshToken.Token, user.Username, user.Email, user.Role, user.Id));
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login(LoginDto dto)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
        if (user == null)
            return Unauthorized(new { message = "Invalid email or password" });

        if (user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTime.UtcNow)
            return StatusCode(403, new { message = "Account is locked out. Please try again later." });

        if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
        {
            user.FailedLoginCount++;
            if (user.FailedLoginCount >= 5)
            {
                user.LockoutEnd = DateTime.UtcNow.AddMinutes(15);
            }
            await _db.SaveChangesAsync();
            return Unauthorized(new { message = "Invalid email or password" });
        }

        user.FailedLoginCount = 0;
        user.LockoutEnd = null;

        var token = _tokenService.CreateToken(user);
        var refreshToken = _tokenService.CreateRefreshToken(user.Id);
        _db.RefreshTokens.Add(refreshToken);
        await _db.SaveChangesAsync();

        return Ok(new AuthResponseDto(token, refreshToken.Token, user.Username, user.Email, user.Role, user.Id));
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<AuthResponseDto>> Refresh(RefreshTokenDto dto)
    {
        var storedToken = await _db.RefreshTokens.Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == dto.RefreshToken);

        if (storedToken == null)
            return Unauthorized(new { message = "Invalid Refresh Token" });

        if (storedToken.IsRevoked)
        {
            // Reuse detection: revoke ALL tokens for this user
            var allTokens = await _db.RefreshTokens.Where(rt => rt.UserId == storedToken.UserId).ToListAsync();
            foreach (var t in allTokens) t.RevokedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return Unauthorized(new { message = "Attempted reuse of revoked token. All tokens invalidated. Please login again." });
        }

        if (storedToken.IsExpired)
            return Unauthorized(new { message = "Refresh Token has expired" });

        // Revoke the old token (rotation)
        storedToken.ReplacedByToken = "ROTATED";
        storedToken.RevokedAt = DateTime.UtcNow;

        var user = storedToken.User;
        var token = _tokenService.CreateToken(user);
        var newRefreshToken = _tokenService.CreateRefreshToken(user.Id);

        _db.RefreshTokens.Add(newRefreshToken);
        await _db.SaveChangesAsync();

        return Ok(new AuthResponseDto(token, newRefreshToken.Token, user.Username, user.Email, user.Role, user.Id));
    }

    [HttpPost("revoke")]
    public async Task<ActionResult> Revoke(RevokeTokenDto dto)
    {
        var storedToken = await _db.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == dto.RefreshToken);
        if (storedToken != null)
        {
            storedToken.RevokedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }
        return Ok(new { message = "Token revoked" });
    }

    [HttpGet("profile")]
    public async Task<ActionResult> GetProfile()
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var user = await _db.Users.FindAsync(userId);
        if (user == null) return NotFound();

        return Ok(new { user.Id, user.Username, user.Email, user.Phone, user.Address, user.City, user.Role, user.CreatedAt });
    }

    [HttpPut("profile")]
    public async Task<ActionResult> UpdateProfile(UpdateProfileDto dto)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var user = await _db.Users.FindAsync(userId);
        if (user == null) return NotFound();

        if (dto.Username != null) user.Username = dto.Username;
        if (dto.Phone != null) user.Phone = dto.Phone;
        if (dto.Address != null) user.Address = dto.Address;
        if (dto.City != null) user.City = dto.City;

        await _db.SaveChangesAsync();
        return Ok(new { message = "Profile updated" });
    }

    private int? GetUserId()
    {
        var claim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        return claim != null ? int.Parse(claim.Value) : null;
    }

    // --- Address Management ---
    
    [HttpGet("addresses")]
    [Microsoft.AspNetCore.Authorization.Authorize]
    public async Task<ActionResult<IEnumerable<AddressDto>>> GetAddresses()
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var addresses = await _db.Addresses
            .AsNoTracking()
            .Where(a => a.UserId == userId)
            .Select(a => new AddressDto(a.Id, a.Label, a.Street, a.City, a.State ?? "", a.ZipCode ?? "", a.Country, a.Phone ?? "", a.IsDefault))
            .ToListAsync();

        return Ok(addresses);
    }

    [HttpPost("addresses")]
    [Microsoft.AspNetCore.Authorization.Authorize]
    public async Task<ActionResult<AddressDto>> CreateAddress(CreateAddressDto dto)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        // If this is the first address, or IsDefault is true, manage defaults
        var isFirst = !await _db.Addresses.AnyAsync(a => a.UserId == userId);
        var shouldBeDefault = isFirst || dto.IsDefault;

        if (shouldBeDefault)
        {
            var existingDefaults = await _db.Addresses.Where(a => a.UserId == userId && a.IsDefault).ToListAsync();
            foreach (var addr in existingDefaults) addr.IsDefault = false;
        }

        var address = new Address
        {
            UserId = userId.Value,
            Label = dto.Label,
            Street = dto.Street,
            City = dto.City,
            State = dto.State,
            ZipCode = dto.ZipCode,
            Country = dto.Country,
            Phone = dto.Phone,
            IsDefault = shouldBeDefault
        };

        _db.Addresses.Add(address);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetAddresses), new { }, new AddressDto(
            address.Id, address.Label, address.Street, address.City, address.State ?? "", 
            address.ZipCode, address.Country, address.Phone, address.IsDefault));
    }

    [HttpPut("addresses/{id}")]
    [Microsoft.AspNetCore.Authorization.Authorize]
    public async Task<ActionResult> UpdateAddress(int id, CreateAddressDto dto)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var address = await _db.Addresses.FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);
        if (address == null) return NotFound();

        if (dto.IsDefault && !address.IsDefault)
        {
            var existingDefaults = await _db.Addresses.Where(a => a.UserId == userId && a.IsDefault).ToListAsync();
            foreach (var addr in existingDefaults) addr.IsDefault = false;
        }

        address.Label = dto.Label;
        address.Street = dto.Street;
        address.City = dto.City;
        address.State = dto.State;
        address.ZipCode = dto.ZipCode;
        address.Country = dto.Country;
        address.Phone = dto.Phone;
        
        // Prevent un-defaulting if it's the only default
        if (address.IsDefault && !dto.IsDefault)
        {
            var otherDefault = await _db.Addresses.AnyAsync(a => a.UserId == userId && a.Id != id && a.IsDefault);
            if (!otherDefault) return BadRequest(new { message = "You must have at least one default address." });
        }
        else
        {
            address.IsDefault = dto.IsDefault;
        }

        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("addresses/{id}")]
    [Microsoft.AspNetCore.Authorization.Authorize]
    public async Task<ActionResult> DeleteAddress(int id)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var address = await _db.Addresses.FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);
        if (address == null) return NotFound();

        _db.Addresses.Remove(address);
        
        if (address.IsDefault)
        {
            var nextAddress = await _db.Addresses.FirstOrDefaultAsync(a => a.UserId == userId && a.Id != id);
            if (nextAddress != null) nextAddress.IsDefault = true;
        }

        await _db.SaveChangesAsync();
        return NoContent();
    }
}
