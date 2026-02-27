namespace backend.Models;

public class WebhookEvent
{
    public int Id { get; set; }
    public string Provider { get; set; } = string.Empty; // stripe, paymob
    public string EventId { get; set; } = string.Empty;   // Provider's unique event ID
    public string EventType { get; set; } = string.Empty;  // e.g. payment_intent.succeeded
    public string? Payload { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
