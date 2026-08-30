using Microsoft.AspNetCore.Mvc;
using Rating.API.Services;

namespace Rating.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationsController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpPost("send")]
    public async Task<ActionResult> SendNotification([FromBody] SendNotificationDto dto)
    {
        var result = await _notificationService.SendPushNotificationAsync(
            title: dto.Title,
            body: dto.Body,
            topic: dto.Topic,
            deviceToken: dto.DeviceToken,
            data: dto.Data
        );

        return Ok(new { success = result, message = "Notification queued for delivery" });
    }
}

public class SendNotificationDto
{
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string? Topic { get; set; }
    public string? DeviceToken { get; set; }
    public Dictionary<string, string>? Data { get; set; }
}
