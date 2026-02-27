namespace backend.Models;

public class IdempotencyRecord
{
    public int Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string? ResponseJson { get; set; }
    public int StatusCode { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; }
}
