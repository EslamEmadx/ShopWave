using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.DTOs;
using System.Security.Claims;

namespace backend.Controllers;

/// <summary>
/// Temporary stub. Will be fully rewritten with IPaymentProvider in Part 4.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PaymentController : ControllerBase
{
    private readonly AppDbContext _db;

    public PaymentController(AppDbContext db)
    {
        _db = db;
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

        // Placeholder redirect URL — will be replaced by provider abstraction
        var fakeUrl = $"https://checkout.placeholder.com/session/{Guid.NewGuid()}?orderId={order.Id}";
        return Ok(new CheckoutSessionDto($"sess_{Guid.NewGuid():N}", fakeUrl));
    }

    [HttpPost("webhook")]
    [AllowAnonymous]
    public async Task<ActionResult> Webhook()
    {
        // Placeholder — will be replaced in Part 4
        return Ok();
    }

    private int GetUserId() => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
}
