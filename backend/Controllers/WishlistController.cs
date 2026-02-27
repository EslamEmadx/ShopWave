using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.DTOs;
using backend.Models;
using System.Security.Claims;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class WishlistController : ControllerBase
{
    private readonly AppDbContext _db;
    public WishlistController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<List<WishlistItemDto>>> GetWishlist()
    {
        var userId = GetUserId();
        var items = await _db.WishlistItems
            .Where(w => w.UserId == userId)
            .Include(w => w.Product)
            .Select(w => new WishlistItemDto(w.Id, w.ProductId, w.Product.Name, w.Product.ImageUrl, w.Product.Price, w.Product.Stock))
            .ToListAsync();
        return Ok(items);
    }

    [HttpPost("{productId}")]
    public async Task<ActionResult> ToggleWishlist(int productId)
    {
        var userId = GetUserId();
        var existing = await _db.WishlistItems.FirstOrDefaultAsync(w => w.UserId == userId && w.ProductId == productId);

        if (existing != null)
        {
            _db.WishlistItems.Remove(existing);
            await _db.SaveChangesAsync();
            return Ok(new { message = "Removed from wishlist", isWishlisted = false });
        }

        var product = await _db.Products.FindAsync(productId);
        if (product == null) return NotFound();

        _db.WishlistItems.Add(new WishlistItem { UserId = userId, ProductId = productId });
        await _db.SaveChangesAsync();
        return Ok(new { message = "Added to wishlist", isWishlisted = true });
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> RemoveFromWishlist(int id)
    {
        var item = await _db.WishlistItems.FirstOrDefaultAsync(w => w.Id == id && w.UserId == GetUserId());
        if (item == null) return NotFound();
        _db.WishlistItems.Remove(item);
        await _db.SaveChangesAsync();
        return Ok(new { message = "Removed from wishlist" });
    }

    private int GetUserId() => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
}
