using backend.Models;

namespace backend.Services.Payments;

public interface IPaymentProvider
{
    Task<string> CreateCheckoutSessionAsync(Order order, string successUrl, string cancelUrl);
    Task<bool> VerifyWebhookSignatureAsync(string payload, string signatureHeader);
    Task<WebhookEvent?> ParseWebhookAsync(string payload);
}
