namespace backend.Models;

public class OrderItem
{
    public int Id { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public int? VariantId { get; set; }
    public string? VariantLabel { get; set; } // e.g. "Size: L, Color: Black"

    public int OrderId { get; set; }
    public Order Order { get; set; } = null!;

    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;
}
