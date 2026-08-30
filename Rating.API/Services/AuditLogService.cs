using System.Collections.Concurrent;

namespace Rating.API.Services;

public class AuditLogService : IAuditLogService
{
    private static readonly ConcurrentBag<AuditLogEntry> _logs = new();
    private readonly ILogger<AuditLogService> _logger;

    public AuditLogService(ILogger<AuditLogService> logger)
    {
        _logger = logger;
    }

    public Task LogActivityAsync(string action, string entityId, string userId, string details, string clientIp = "")
    {
        var entry = new AuditLogEntry
        {
            Action = action,
            EntityId = entityId,
            UserId = userId,
            Details = details,
            ClientIp = clientIp,
            Timestamp = DateTime.UtcNow
        };

        _logs.Add(entry);
        _logger.LogInformation("📋 [AUDIT LOG] {Action} | Entity: {EntityId} | User: {UserId} | {Details}", action, entityId, userId, details);
        return Task.CompletedTask;
    }

    public Task<List<AuditLogEntry>> GetRecentLogsAsync(int limit = 50)
    {
        var list = _logs.OrderByDescending(l => l.Timestamp).Take(limit).ToList();
        return Task.FromResult(list);
    }
}
