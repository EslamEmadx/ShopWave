using backend.Models;
using Microsoft.Extensions.Logging;

namespace backend.Services.Payments;

public class StripePaymentProviderPlaceholder : IPaymentProvider
{
    private readonly ILogger<StripePaymentProviderPlaceholder> _logger;

    public StripePaymentProviderPlaceholder(ILogger<StripePaymentProviderPlaceholder> logger)
    {
        _logger = logger;
    }

    public Task<string> CreateCheckoutSessionAsync(Order order, string successUrl, string cancelUrl)
    {
        _logger.LogInformation("Creating Stripe checkout session for Order {OrderId}", order.Id);
        return Task.FromResult($"https://checkout.stripe.com/pay/sess_{Guid.NewGuid():N}?orderId={order.Id}");
    }

    public Task<bool> VerifyWebhookSignatureAsync(string payload, string signatureHeader)
    {
        _logger.LogInformation("Verifying Stripe webhook signature (Placeholder)");
        return Task.FromResult(true);
    }

    public Task<WebhookEvent?> ParseWebhookAsync(string payload)
    {
        return Task.FromResult<WebhookEvent?>(new WebhookEvent
        {
            Provider = "Stripe",
            EventId = $"evt_{Guid.NewGuid():N}",
            EventType = "checkout.session.completed",
            Payload = payload,
            CreatedAt = DateTime.UtcNow
        });
    }
}
