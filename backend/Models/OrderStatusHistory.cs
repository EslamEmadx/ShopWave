namespace backend.Models;

public class OrderStatusHistory
{
    public int Id { get; set; }
    public string FromStatus { get; set; } = string.Empty;
    public string ToStatus { get; set; } = string.Empty;
    public int? ChangedByUserId { get; set; }
    public string? Note { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int OrderId { get; set; }
    public Order Order { get; set; } = null!;
}
