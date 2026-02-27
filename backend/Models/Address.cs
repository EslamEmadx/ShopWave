namespace backend.Models;

public class Address
{
    public int Id { get; set; }
    public string Label { get; set; } = string.Empty; // e.g. "Home", "Work"
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string? State { get; set; }
    public string? ZipCode { get; set; }
    public string Country { get; set; } = "Egypt";
    public string? Phone { get; set; }
    public bool IsDefault { get; set; }

    public int UserId { get; set; }
    public User User { get; set; } = null!;
}
