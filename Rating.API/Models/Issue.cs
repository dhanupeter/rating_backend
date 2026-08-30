namespace Rating.API.Models;

public class Issue
{
    public string IssueId { get; set; } = Guid.NewGuid().ToString();
    public string Title { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty; // Electrical Safety, Road & Potholes, Water Supply, Streetlight, Public Facility
    public string Description { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string Status { get; set; } = "Open"; // Open, Under Review, Response Received, Resolved
    public long ConfirmationsCount { get; set; } = 1;
    public string ReportedByUserId { get; set; } = string.Empty;
    public string ReportedByUserName { get; set; } = string.Empty;
    public string? OfficialResponse { get; set; }
    public string? RespondedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
