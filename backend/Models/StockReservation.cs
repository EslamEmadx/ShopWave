namespace backend.Models;

public class StockReservation
{
    public int Id { get; set; }
    public int Quantity { get; set; }
    public DateTime ExpiresAt { get; set; }
    public string Status { get; set; } = "Active"; // Active, Released, Consumed
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int OrderId { get; set; }
    public Order Order { get; set; } = null!;

    public int ProductVariantId { get; set; }
    public ProductVariant ProductVariant { get; set; } = null!;
}
