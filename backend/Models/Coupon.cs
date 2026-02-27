namespace backend.Models;

public class Coupon
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public int DiscountPercent { get; set; }
    public decimal? MaxDiscount { get; set; }
    public decimal? MinOrderAmount { get; set; }
    public bool IsActive { get; set; } = true;
    public int UsageLimit { get; set; } = 100;
    public int TimesUsed { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
