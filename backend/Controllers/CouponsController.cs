using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.DTOs;
using backend.Models;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CouponsController : ControllerBase
{
    private readonly AppDbContext _db;
    public CouponsController(AppDbContext db) => _db = db;

    [HttpPost("validate")]
    public async Task<ActionResult<CouponValidationResult>> ValidateCoupon(ValidateCouponDto dto)
    {
        var coupon = await _db.Coupons.FirstOrDefaultAsync(c => c.Code == dto.Code);
        if (coupon == null)
            return Ok(new CouponValidationResult(false, "Invalid coupon code", 0, 0));

        if (!coupon.IsActive)
            return Ok(new CouponValidationResult(false, "Coupon is inactive", 0, 0));

        if (coupon.ExpiresAt.HasValue && coupon.ExpiresAt < DateTime.UtcNow)
            return Ok(new CouponValidationResult(false, "Coupon has expired", 0, 0));

        if (coupon.TimesUsed >= coupon.UsageLimit)
            return Ok(new CouponValidationResult(false, "Coupon usage limit reached", 0, 0));

        if (coupon.MinOrderAmount.HasValue && dto.OrderTotal < coupon.MinOrderAmount)
            return Ok(new CouponValidationResult(false, $"Minimum order amount is ${coupon.MinOrderAmount}", 0, 0));

        var discount = dto.OrderTotal * coupon.DiscountPercent / 100;
        if (coupon.MaxDiscount.HasValue && discount > coupon.MaxDiscount.Value)
            discount = coupon.MaxDiscount.Value;

        return Ok(new CouponValidationResult(true, $"{coupon.DiscountPercent}% discount applied!", coupon.DiscountPercent, discount));
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<PaginatedResult<CouponDto>>> GetCoupons(
        [FromQuery] int page = 1, 
        [FromQuery] int pageSize = 10)
    {
        var query = _db.Coupons.AsNoTracking().OrderByDescending(c => c.CreatedAt);
        
        var totalCount = await query.CountAsync();
        var coupons = await query.Skip((page - 1) * pageSize).Take(pageSize)
            .Select(c => new CouponDto(c.Id, c.Code, c.DiscountPercent, c.MaxDiscount, c.MinOrderAmount,
                c.IsActive, c.UsageLimit, c.TimesUsed, c.ExpiresAt))
            .ToListAsync();

        return Ok(new PaginatedResult<CouponDto>(
            coupons, 
            totalCount, 
            page, 
            pageSize, 
            (int)Math.Ceiling((double)totalCount / pageSize)));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> CreateCoupon(CreateCouponDto dto)
    {
        if (await _db.Coupons.AnyAsync(c => c.Code == dto.Code))
            return BadRequest(new { message = "Coupon code already exists" });

        var coupon = new Coupon
        {
            Code = dto.Code.ToUpperInvariant(),
            DiscountPercent = dto.DiscountPercent,
            MaxDiscount = dto.MaxDiscount,
            MinOrderAmount = dto.MinOrderAmount,
            UsageLimit = dto.UsageLimit,
            ExpiresAt = dto.ExpiresAt,
            IsActive = true
        };

        _db.Coupons.Add(coupon);
        await _db.SaveChangesAsync();
        return Ok(new { message = "Coupon created", coupon.Id });
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> UpdateCoupon(int id, UpdateCouponDto dto)
    {
        var coupon = await _db.Coupons.FindAsync(id);
        if (coupon == null) return NotFound();

        if (dto.Code != null)
        {
            if (await _db.Coupons.AnyAsync(c => c.Code == dto.Code && c.Id != id))
                return BadRequest(new { message = "Coupon code already exists" });
            coupon.Code = dto.Code.ToUpperInvariant();
        }

        if (dto.DiscountPercent.HasValue) coupon.DiscountPercent = dto.DiscountPercent.Value;
        if (dto.MaxDiscount.HasValue) coupon.MaxDiscount = dto.MaxDiscount;
        if (dto.MinOrderAmount.HasValue) coupon.MinOrderAmount = dto.MinOrderAmount;
        if (dto.UsageLimit.HasValue) coupon.UsageLimit = dto.UsageLimit.Value;
        if (dto.ExpiresAt.HasValue) coupon.ExpiresAt = dto.ExpiresAt;
        if (dto.IsActive.HasValue) coupon.IsActive = dto.IsActive.Value;

        coupon.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return Ok(new { message = "Coupon updated" });
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> DeleteCoupon(int id)
    {
        var coupon = await _db.Coupons.FindAsync(id);
        if (coupon == null) return NotFound();
        _db.Coupons.Remove(coupon);
        await _db.SaveChangesAsync();
        return Ok(new { message = "Coupon deleted" });
    }
}
