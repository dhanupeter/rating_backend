namespace Rating.API.Models;

public class Entity
{
    public string EntityId { get; set; } = Guid.NewGuid().ToString("N");
    public string EntityType { get; set; } = "PRODUCT"; // PRODUCT, PLACE, SERVICE, DIGITAL, PUBLIC
    public string Category { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Brand { get; set; }
    public string? Model { get; set; }
    public string? Description { get; set; }
    public string? ExternalUrl { get; set; }
    public string? ExternalProvider { get; set; } // Geoapify, OSM, Amazon, Google
    public string? ExternalPlaceId { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public double OverallRating { get; set; }
    public long TotalReviews { get; set; }
    public long VerifiedReviews { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public List<EntityLocation> Locations { get; set; } = new();
    public List<Review> RecentReviews { get; set; } = new();
    public Dictionary<string, double> CriteriaAverages { get; set; } = new();
}

public class EntityLocation
{
    public string EntityId { get; set; } = string.Empty;
    public string LocationId { get; set; } = Guid.NewGuid().ToString("N");
    public string Name { get; set; } = string.Empty;
    public string? AddressLine1 { get; set; }
    public string? AddressLine2 { get; set; }
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string Country { get; set; } = "India";
    public string? PostalCode { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? ExternalProvider { get; set; }
    public string? ExternalPlaceId { get; set; }
    public bool IsPrimary { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
