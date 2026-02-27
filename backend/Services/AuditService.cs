using backend.Data;
using backend.Models;

namespace backend.Services;

public class AuditService
{
    private readonly AppDbContext _db;
    private readonly ILogger<AuditService> _logger;

    public AuditService(AppDbContext db, ILogger<AuditService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task LogAsync(int? actorUserId, string action, string entityType, int entityId, string? metadata = null)
    {
        var audit = new AuditLog
        {
            ActorUserId = actorUserId,
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            MetadataJson = metadata
        };

        _db.AuditLogs.Add(audit);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Audit: {Action} on {EntityType}#{EntityId} by User#{ActorUserId}",
            action, entityType, entityId, actorUserId);
    }
}
