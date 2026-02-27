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
public class ReviewsController : ControllerBase
{
    private readonly AppDbContext _db;
    public ReviewsController(AppDbContext db) => _db = db;

    [HttpGet("product/{productId}")]
    public async Task<ActionResult<List<ReviewDto>>> GetProductReviews(int productId)
    {
        var reviews = await _db.Reviews
            .Where(r => r.ProductId == productId)
            .Include(r => r.User)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new ReviewDto(r.Id, r.Rating, r.Comment, r.CreatedAt, r.User.Username, r.UserId))
            .ToListAsync();
        return Ok(reviews);
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult> CreateReview(CreateReviewDto dto)
    {
        var userId = GetUserId();
        var existing = await _db.Reviews.FirstOrDefaultAsync(r => r.UserId == userId && r.ProductId == dto.ProductId);
        if (existing != null) return BadRequest(new { message = "You already reviewed this product" });

        var review = new Review
        {
            UserId = userId,
            ProductId = dto.ProductId,
            Rating = Math.Clamp(dto.Rating, 1, 5),
            Comment = dto.Comment
        };

        _db.Reviews.Add(review);
        await _db.SaveChangesAsync();

        // Update product rating
        var product = await _db.Products.FindAsync(dto.ProductId);
        if (product != null)
        {
            var reviews = await _db.Reviews.Where(r => r.ProductId == dto.ProductId).ToListAsync();
            product.Rating = Math.Round(reviews.Average(r => r.Rating), 1);
            product.ReviewCount = reviews.Count;
            await _db.SaveChangesAsync();
        }

        return Ok(new { message = "Review added" });
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> DeleteReview(int id)
    {
        var review = await _db.Reviews.FindAsync(id);
        if (review == null) return NotFound();

        var productId = review.ProductId;
        _db.Reviews.Remove(review);
        await _db.SaveChangesAsync();

        // Update product rating
        var product = await _db.Products.FindAsync(productId);
        if (product != null)
        {
            var reviews = await _db.Reviews.Where(r => r.ProductId == productId).ToListAsync();
            product.Rating = reviews.Any() ? Math.Round(reviews.Average(r => r.Rating), 1) : 0;
            product.ReviewCount = reviews.Count;
            await _db.SaveChangesAsync();
        }

        return Ok(new { message = "Review deleted" });
    }

    private int GetUserId() => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
}
