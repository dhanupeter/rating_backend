using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;

namespace Rating.API.Services;

public class FirebaseNotificationService : INotificationService
{
    private readonly ILogger<FirebaseNotificationService> _logger;
    private readonly IConfiguration _configuration;
    private readonly bool _isFirebaseInitialized;

    public FirebaseNotificationService(ILogger<FirebaseNotificationService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;

        try
        {
            if (FirebaseApp.DefaultInstance == null)
            {
                var projectId = _configuration["Firebase:ProjectId"] ?? "event-506117";
                FirebaseApp.Create(new AppOptions
                {
                    ProjectId = projectId,
                    Credential = GoogleCredential.GetApplicationDefault()
                });
                _isFirebaseInitialized = true;
                _logger.LogInformation("Firebase Admin SDK initialized successfully with project {ProjectId}", projectId);
            }
            else
            {
                _isFirebaseInitialized = true;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Firebase Admin initialization deferred (using logger mode): {Message}", ex.Message);
            _isFirebaseInitialized = false;
        }
    }

    public async Task<bool> SendPushNotificationAsync(string title, string body, string? topic = null, string? deviceToken = null, Dictionary<string, string>? data = null)
    {
        _logger.LogInformation("🚀 [Notification Dispatch] Title: {Title} | Body: {Body} | Topic: {Topic} | Token: {Token}", title, body, topic, deviceToken);

        if (!_isFirebaseInitialized)
        {
            return true; // Graceful simulation in dev
        }

        try
        {
            var message = new Message
            {
                Notification = new Notification
                {
                    Title = title,
                    Body = body
                },
                Data = data ?? new Dictionary<string, string>()
            };

            if (!string.IsNullOrEmpty(topic))
            {
                message.Topic = topic;
            }
            else if (!string.IsNullOrEmpty(deviceToken))
            {
                message.Token = deviceToken;
            }
            else
            {
                message.Topic = "all_users";
            }

            var response = await FirebaseMessaging.DefaultInstance.SendAsync(message);
            _logger.LogInformation("FCM Push Notification sent successfully: {Response}", response);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send FCM push notification");
            return false;
        }
    }

    public Task<bool> SendReviewAlertAsync(string entityName, string reviewerName, double rating)
    {
        return SendPushNotificationAsync(
            title: $"⭐ New {rating}★ Review on {entityName}",
            body: $"{reviewerName} just rated and verified their experience on {entityName}.",
            topic: "entity_reviews",
            data: new Dictionary<string, string>
            {
                ["type"] = "review",
                ["entityName"] = entityName,
                ["rating"] = rating.ToString()
            }
        );
    }

    public Task<bool> SendIssueStatusUpdateAsync(string issueTitle, string newStatus, string? authorityResponse)
    {
        return SendPushNotificationAsync(
            title: $"🚧 Civic Issue Status: {newStatus}",
            body: string.IsNullOrEmpty(authorityResponse)
                ? $"'{issueTitle}' status changed to {newStatus}."
                : $"'{issueTitle}' response: {authorityResponse}",
            topic: "civic_issues",
            data: new Dictionary<string, string>
            {
                ["type"] = "issue_update",
                ["status"] = newStatus
            }
        );
    }
}
