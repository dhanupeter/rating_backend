namespace Rating.API.Services;

public class AuditLogEntry
{
    public string LogId { get; set; } = Guid.NewGuid().ToString("N");
    public string Action { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
    public string ClientIp { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

public interface IAuditLogService
{
    Task LogActivityAsync(string action, string entityId, string userId, string details, string clientIp = "");
    Task<List<AuditLogEntry>> GetRecentLogsAsync(int limit = 50);
}
