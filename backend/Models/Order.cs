namespace backend.Models;

public class Order
{
    public int Id { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = "Pending";
    // Pending, Processing, Confirmed, Shipped, Delivered, Cancelled, PaymentFailed, Refunded
    public string ShippingAddress { get; set; } = string.Empty;
    public string ShippingCity { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = "COD"; // COD, Card
    public string? PaymentProvider { get; set; } // stripe, paymob
    public string? PaymentIntentId { get; set; }
    public string PaymentStatus { get; set; } = "Pending"; // Pending, Paid, Failed, Refunded
    public string? CouponCode { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal ShippingCost { get; set; }
    public decimal TaxAmount { get; set; }
    public string? ShippingMethod { get; set; }
    public string? IdempotencyKey { get; set; }
    public string? TrackingNumber { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    public ICollection<OrderStatusHistory> StatusHistory { get; set; } = new List<OrderStatusHistory>();
    public ICollection<StockReservation> StockReservations { get; set; } = new List<StockReservation>();
}
