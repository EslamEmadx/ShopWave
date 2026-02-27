namespace backend.Models;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? NameNormalized { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? Brand { get; set; }
    public string? BrandNormalized { get; set; }
    public decimal Price { get; set; }
    public decimal? OldPrice { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string? VideoUrl { get; set; }
    public int Stock { get; set; }
    public double Rating { get; set; }
    public int ReviewCount { get; set; }
    public bool IsFeatured { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;

    // Navigation properties
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    public ICollection<ProductVariant> Variants { get; set; } = new List<ProductVariant>();
    public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
}
