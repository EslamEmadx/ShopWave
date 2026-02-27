using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.DTOs;
using backend.Models;
using backend.Services.Payments;
using System.Security.Claims;
using System.Text.Json;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PaymentController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly PaymentProviderFactory _paymentFactory;
    private readonly ILogger<PaymentController> _logger;

    public PaymentController(AppDbContext db, PaymentProviderFactory paymentFactory, ILogger<PaymentController> logger)
    {
        _db = db;
        _paymentFactory = paymentFactory;
        _logger = logger;
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

        // Idempotency check
        var idempotencyKey = Request.Headers["X-Idempotency-Key"].ToString();
        if (!string.IsNullOrEmpty(idempotencyKey))
        {
            var existingRecord = await _db.IdempotencyRecords.FirstOrDefaultAsync(r => r.Key == idempotencyKey);
            if (existingRecord != null && existingRecord.ExpiresAt > DateTime.UtcNow)
            {
                return StatusCode(existingRecord.StatusCode, JsonSerializer.Deserialize<CheckoutSessionDto>(existingRecord.ResponseJson ?? "{}"));
            }
        }

        CheckoutSessionDto result;
        if (order.PaymentMethod == "COD")
        {
            // Simple COD flow — just mark as "COD Pending"
            order.PaymentStatus = "COD_Pending";
            await _db.SaveChangesAsync();
            result = new CheckoutSessionDto("cod_none", "/order-success/" + order.Id);
        }
        else
        {
            var providerName = order.PaymentMethod?.ToLower() ?? "stripe";
            var provider = _paymentFactory.GetProvider(providerName);
            var sessionUrl = await provider.CreateCheckoutSessionAsync(order, "http://localhost:5173/payment-success", "http://localhost:5173/payment-cancel");
            result = new CheckoutSessionDto($"sess_{Guid.NewGuid():N}", sessionUrl);
        }

        // Save idempotency record
        if (!string.IsNullOrEmpty(idempotencyKey))
        {
            _db.IdempotencyRecords.Add(new IdempotencyRecord
            {
                Key = idempotencyKey,
                ResponseJson = JsonSerializer.Serialize(result),
                StatusCode = 200,
                ExpiresAt = DateTime.UtcNow.AddHours(24)
            });
            await _db.SaveChangesAsync();
        }

        return Ok(result);
    }

    [HttpPost("webhook/{provider}")]
    [AllowAnonymous]
    public async Task<ActionResult> Webhook(string provider, [FromBody] object payload)
    {
        var providerImpl = _paymentFactory.GetProvider(provider);
        var json = payload.ToString() ?? "";
        
        if (!await providerImpl.VerifyWebhookSignatureAsync(json, Request.Headers["X-Signature"].ToString() ?? ""))
        {
            return BadRequest("Invalid signature");
        }

        var webEvent = await providerImpl.ParseWebhookAsync(json);
        if (webEvent == null) return BadRequest("Invalid payload");

        // Check if already processed
        if (await _db.WebhookEvents.AnyAsync(e => e.Provider == provider && e.EventId == webEvent.EventId))
        {
            return Ok(new { message = "Already processed" });
        }

        _db.WebhookEvents.Add(webEvent);

        if (webEvent.EventType == "checkout.session.completed" || webEvent.EventType == "TRANSACTION_SUCCESS")
        {
            // Extract order ID from payload (placeholder logic)
            // In reality, you'd parse the specific provider's DTO
            _logger.LogInformation("Payment successful for event {EventId} via {Provider}", webEvent.EventId, provider);
            // order.PaymentStatus = "Paid";
        }

        webEvent.ProcessedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return Ok();
    }

    private int GetUserId() => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
}
