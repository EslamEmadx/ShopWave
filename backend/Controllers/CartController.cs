using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.DTOs;
using backend.Helpers;
using backend.Models;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CartController : ControllerBase
{
    private readonly AppDbContext _db;
    public CartController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<List<CartItemDto>>> GetCart()
    {
        var userId = ClaimsHelper.TryGetUserId(User);
        if (userId is null) return Unauthorized();

        var items = await _db.CartItems
            .Where(ci => ci.UserId == userId.Value)
            .Include(ci => ci.Product)
            .Select(ci => new CartItemDto(ci.Id, ci.ProductId, ci.Product.Name, ci.Product.ImageUrl, ci.Product.Price, ci.Quantity, ci.Product.Stock))
            .ToListAsync();
        return Ok(items);
    }

    [HttpPost]
    public async Task<ActionResult> AddToCart(AddToCartDto dto)
    {
        var userId = ClaimsHelper.TryGetUserId(User);
        if (userId is null) return Unauthorized();

        var product = await _db.Products.FindAsync(dto.ProductId);
        if (product == null) return NotFound(new { message = "Product not found" });
        if (product.Stock < dto.Quantity) return BadRequest(new { message = "Not enough stock" });

        var existing = await _db.CartItems.FirstOrDefaultAsync(ci => ci.UserId == userId.Value && ci.ProductId == dto.ProductId);
        if (existing != null)
        {
            existing.Quantity += dto.Quantity;
        }
        else
        {
            _db.CartItems.Add(new CartItem { UserId = userId.Value, ProductId = dto.ProductId, Quantity = dto.Quantity });
        }

        await _db.SaveChangesAsync();
        return Ok(new { message = "Added to cart" });
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateQuantity(int id, UpdateCartDto dto)
    {
        var userId = ClaimsHelper.TryGetUserId(User);
        if (userId is null) return Unauthorized();

        var item = await _db.CartItems.FirstOrDefaultAsync(ci => ci.Id == id && ci.UserId == userId.Value);
        if (item == null) return NotFound();

        if (dto.Quantity <= 0)
        {
            _db.CartItems.Remove(item);
        }
        else
        {
            item.Quantity = dto.Quantity;
        }

        await _db.SaveChangesAsync();
        return Ok(new { message = "Cart updated" });
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> RemoveFromCart(int id)
    {
        var userId = ClaimsHelper.TryGetUserId(User);
        if (userId is null) return Unauthorized();

        var item = await _db.CartItems.FirstOrDefaultAsync(ci => ci.Id == id && ci.UserId == userId.Value);
        if (item == null) return NotFound();
        _db.CartItems.Remove(item);
        await _db.SaveChangesAsync();
        return Ok(new { message = "Removed from cart" });
    }

    [HttpDelete]
    public async Task<ActionResult> ClearCart()
    {
        var userId = ClaimsHelper.TryGetUserId(User);
        if (userId is null) return Unauthorized();

        var items = await _db.CartItems.Where(ci => ci.UserId == userId.Value).ToListAsync();
        _db.CartItems.RemoveRange(items);
        await _db.SaveChangesAsync();
        return Ok(new { message = "Cart cleared" });
    }
}
