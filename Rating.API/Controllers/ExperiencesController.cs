using Microsoft.AspNetCore.Mvc;
using Rating.API.Models;
using Rating.API.Services;

namespace Rating.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ExperiencesController : ControllerBase
{
    private readonly ISpannerService _spannerService;
    private readonly IAuditLogService _auditLogService;

    public ExperiencesController(ISpannerService spannerService, IAuditLogService auditLogService)
    {
        _spannerService = spannerService;
        _auditLogService = auditLogService;
    }

    [HttpPost("capture")]
    public async Task<ActionResult<Experience>> CaptureExperience([FromBody] CaptureExperienceDto dto)
    {
        var exp = new Experience
        {
            ExperienceId = Guid.NewGuid().ToString("N"),
            UserId = dto.UserId,
            EntityId = dto.EntityId,
            LocationId = dto.LocationId,
            ExperienceDate = dto.ExperienceDate ?? DateTime.UtcNow,
            Latitude = dto.Latitude,
            Longitude = dto.Longitude,
            LocationAccuracyMeters = dto.LocationAccuracyMeters,
            ExperienceType = dto.ExperienceType ?? "IN_STORE_VISIT"
        };

        await _auditLogService.LogActivityAsync("EXPERIENCE_CAPTURED", dto.EntityId, dto.UserId, $"Lat: {dto.Latitude}, Lon: {dto.Longitude}");
        return Ok(exp);
    }
}

public class CaptureExperienceDto
{
    public string UserId { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string? LocationId { get; set; }
    public DateTime? ExperienceDate { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public double? LocationAccuracyMeters { get; set; }
    public string? ExperienceType { get; set; }
}
