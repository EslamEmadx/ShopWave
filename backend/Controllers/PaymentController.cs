using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.DTOs;
using backend.Services;
using System.Security.Claims;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PaymentController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly StripeService _stripe;

    public PaymentController(AppDbContext db, StripeService stripe)
    {
        _db = db;
        _stripe = stripe;
    }

    [HttpPost("create-checkout-session")]
    public async Task<ActionResult<CheckoutSessionDto>> CreateCheckoutSession(CreateCheckoutDto dto)
    {
        var userId = GetUserId();
        var order = await _db.Orders
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.Id == dto.OrderId && o.UserId == userId);

        if (order == null) return NotFound();
        if (order.PaymentStatus == "Paid") return BadRequest(new { message = "Order already paid" });

        var productIds = order.OrderItems.Select(oi => oi.ProductId).ToList();
        var products = await _db.Products.Where(p => productIds.Contains(p.Id)).ToListAsync();

        var session = await _stripe.CreateCheckoutSessionAsync(order, order.OrderItems.ToList(), products);

        order.PaymentIntentId = session.PaymentIntentId;
        await _db.SaveChangesAsync();

        return Ok(new CheckoutSessionDto(session.Id, session.Url));
    }

    [HttpPost("webhook")]
    [AllowAnonymous]
    public async Task<ActionResult> Webhook()
    {
        var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
        // In production, verify the webhook signature
        // For now, just acknowledge
        return Ok();
    }

    private int GetUserId() => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
}
