namespace Rating.API.Models;

public class Entity
{
    public string EntityId { get; set; } = Guid.NewGuid().ToString();
    public string EntityType { get; set; } = "PRODUCT"; // PRODUCT, PLACE, SERVICE, DIGITAL, PUBLIC
    public string Category { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string ExternalUrl { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public double OverallRating { get; set; } = 0.0;
    public long TotalReviews { get; set; } = 0;
    public long VerifiedReviews { get; set; } = 0;
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Computed / Dynamic Criteria Breakdowns
    public Dictionary<string, double> CriteriaAverages { get; set; } = new();
    public List<Review> RecentReviews { get; set; } = new();
}
