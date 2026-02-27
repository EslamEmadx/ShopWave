using backend.Models;
using Microsoft.Extensions.Logging;

namespace backend.Services.Payments;

public class PaymobPaymentProviderPlaceholder : IPaymentProvider
{
    private readonly ILogger<PaymobPaymentProviderPlaceholder> _logger;

    public PaymobPaymentProviderPlaceholder(ILogger<PaymobPaymentProviderPlaceholder> logger)
    {
        _logger = logger;
    }

    public Task<string> CreateCheckoutSessionAsync(Order order, string successUrl, string cancelUrl)
    {
        _logger.LogInformation("Creating Paymob payment session for Order {OrderId}", order.Id);
        return Task.FromResult($"https://accept.paymob.com/api/acceptance/iframes/placeholder?payment_token=token_{Guid.NewGuid():N}");
    }

    public Task<bool> VerifyWebhookSignatureAsync(string payload, string signatureHeader)
    {
        _logger.LogInformation("Verifying Paymob webhook HMAC (Placeholder)");
        return Task.FromResult(true);
    }

    public Task<WebhookEvent?> ParseWebhookAsync(string payload)
    {
        return Task.FromResult<WebhookEvent?>(new WebhookEvent
        {
            Provider = "Paymob",
            EventId = $"pm_{Guid.NewGuid():N}",
            EventType = "TRANSACTION_SUCCESS",
            Payload = payload,
            CreatedAt = DateTime.UtcNow
        });
    }
}
