namespace Rating.API.Models;

public class Experience
{
    public string ExperienceId { get; set; } = Guid.NewGuid().ToString("N");
    public string UserId { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string? LocationId { get; set; }
    public DateTime? ExperienceDate { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public double? LocationAccuracyMeters { get; set; }
    public string ExperienceType { get; set; } = "IN_STORE_VISIT"; // IN_STORE_VISIT, ONLINE_PURCHASE, AT_HOME_SERVICE, DIGITAL_PLAY, CIVIC_VISIT
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
