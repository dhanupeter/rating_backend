namespace Rating.API.Services;

public interface INotificationService
{
    Task<bool> SendPushNotificationAsync(string title, string body, string? topic = null, string? deviceToken = null, Dictionary<string, string>? data = null);
    Task<bool> SendReviewAlertAsync(string entityName, string reviewerName, double rating);
    Task<bool> SendIssueStatusUpdateAsync(string issueTitle, string newStatus, string? authorityResponse);
}
