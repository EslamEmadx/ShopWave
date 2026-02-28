using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.DTOs;
using backend.Helpers;
using backend.Models;
using backend.Services;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReviewsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly AuditService _audit;
    private readonly InputSanitizer _sanitizer;
    private readonly ILogger<ReviewsController> _logger;

    public ReviewsController(AppDbContext db, AuditService audit, InputSanitizer sanitizer, ILogger<ReviewsController> logger)
    {
        _db = db;
        _audit = audit;
        _sanitizer = sanitizer;
        _logger = logger;
    }

    [HttpGet("product/{productId}")]
    public async Task<ActionResult<List<ReviewDto>>> GetProductReviews(int productId)
    {
        // Only return approved reviews to public
        var reviews = await _db.Reviews
            .AsNoTracking()
            .Where(r => r.ProductId == productId && r.Status == "Approved")
            .Include(r => r.User)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new ReviewDto(r.Id, r.Rating, r.Comment, r.CreatedAt, r.User.Username, r.UserId, r.Status, r.OrderItemId != null))
            .ToListAsync();
        return Ok(reviews);
    }

    [HttpGet("pending")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<List<ReviewDto>>> GetPendingReviews()
    {
        var reviews = await _db.Reviews
            .AsNoTracking()
            .Where(r => r.Status == "Pending")
            .Include(r => r.User)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new ReviewDto(r.Id, r.Rating, r.Comment, r.CreatedAt, r.User.Username, r.UserId, r.Status, r.OrderItemId != null))
            .ToListAsync();
        return Ok(reviews);
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult> CreateReview(CreateReviewDto dto)
    {
        var userId = ClaimsHelper.TryGetUserId(User);
        if (userId is null) return Unauthorized();
        
        // Anti-spam: Sanitize input
        var cleanComment = _sanitizer.SanitizeText(dto.Comment);
        if (string.IsNullOrWhiteSpace(cleanComment))
            return BadRequest(new { message = "Review comment is required and cannot be pure HTML/Links." });

        var existing = await _db.Reviews.FirstOrDefaultAsync(r => r.UserId == userId.Value && r.ProductId == dto.ProductId);
        if (existing != null) return BadRequest(new { message = "You already reviewed this product" });

        // Verified purchase check
        var paidOrderWithProduct = await _db.Orders
            .Include(o => o.OrderItems)
            .Where(o => o.UserId == userId.Value && (o.PaymentStatus == "Paid" || o.PaymentStatus == "COD_Delivered"))
            .SelectMany(o => o.OrderItems)
            .FirstOrDefaultAsync(oi => oi.ProductId == dto.ProductId);

        var review = new Review
        {
            UserId = userId.Value,
            ProductId = dto.ProductId,
            Rating = Math.Clamp(dto.Rating, 1, 5),
            Comment = cleanComment,
            Status = "Pending", // Moderation required
            OrderItemId = paidOrderWithProduct?.Id
        };

        _db.Reviews.Add(review);
        await _db.SaveChangesAsync();

        await _audit.LogAsync(userId, "CREATE_REVIEW", "Review", review.Id, $"ProductId={dto.ProductId}, Verified={paidOrderWithProduct != null}");

        return Ok(new { message = "Review submitted for moderation.", isVerified = paidOrderWithProduct != null });
    }

    [HttpPost("{id}/approve")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> ApproveReview(int id)
    {
        var review = await _db.Reviews.Include(r => r.Product).FirstOrDefaultAsync(r => r.Id == id);
        if (review == null) return NotFound();

        review.Status = "Approved";
        review.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        await UpdateProductRating(review.ProductId);
        
        await _audit.LogAsync(ClaimsHelper.TryGetUserId(User) ?? 0, "APPROVE_REVIEW", "Review", id);

        return Ok(new { message = "Review approved" });
    }

    [HttpPost("{id}/reject")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> RejectReview(int id)
    {
        var review = await _db.Reviews.FindAsync(id);
        if (review == null) return NotFound();

        review.Status = "Rejected";
        review.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        
        await _audit.LogAsync(ClaimsHelper.TryGetUserId(User) ?? 0, "REJECT_REVIEW", "Review", id);

        return Ok(new { message = "Review rejected" });
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> DeleteReview(int id)
    {
        var review = await _db.Reviews.FindAsync(id);
        if (review == null) return NotFound();

        var productId = review.ProductId;
        var wasApproved = review.Status == "Approved";

        _db.Reviews.Remove(review);
        await _db.SaveChangesAsync();

        if (wasApproved)
        {
            await UpdateProductRating(productId);
        }

        await _audit.LogAsync(ClaimsHelper.TryGetUserId(User) ?? 0, "DELETE_REVIEW", "Review", id);

        return Ok(new { message = "Review deleted" });
    }

    private async Task UpdateProductRating(int productId)
    {
        var product = await _db.Products.FindAsync(productId);
        if (product != null)
        {
            var approvedReviews = await _db.Reviews
                .Where(r => r.ProductId == productId && r.Status == "Approved")
                .ToListAsync();

            if (approvedReviews.Any())
            {
                product.Rating = Math.Round(approvedReviews.Average(r => r.Rating), 1);
                product.ReviewCount = approvedReviews.Count;
            }
            else
            {
                product.Rating = 0;
                product.ReviewCount = 0;
            }
            await _db.SaveChangesAsync();
        }
    }

}
