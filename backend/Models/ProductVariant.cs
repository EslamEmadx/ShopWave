namespace backend.Models;

public class ProductVariant
{
    public int Id { get; set; }
    public string Size { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public string? SKU { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// SQL Server rowversion for optimistic concurrency control.
    /// </summary>
    public byte[] RowVersion { get; set; } = null!;

    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;
}
