namespace Rating.API.DTOs;

public class CreateEntityRequest
{
    public string EntityType { get; set; } = "PRODUCT";
    public string Category { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string ExternalUrl { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public string CreatedBy { get; set; } = "user-1";
}

public class CreateReviewRequest
{
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string UserPhotoUrl { get; set; } = string.Empty;
    public double OverallRating { get; set; } = 5.0;
    public string Title { get; set; } = string.Empty;
    public string ReviewText { get; set; } = string.Empty;
    public DateTime? ExperienceDate { get; set; }
    public string LocationId { get; set; } = string.Empty;
    public int VerificationLevel { get; set; } = 0;
    public List<CriteriaScoreRequest> CriteriaRatings { get; set; } = new();
    public List<MediaUploadRequest> MediaItems { get; set; } = new();
}

public class CriteriaScoreRequest
{
    public string CriteriaId { get; set; } = string.Empty;
    public string CriteriaName { get; set; } = string.Empty;
    public double Score { get; set; } = 5.0;
}

public class MediaUploadRequest
{
    public string MediaType { get; set; } = "PHOTO";
    public string StorageUrl { get; set; } = string.Empty;
    public string ThumbnailUrl { get; set; } = string.Empty;
    public string Caption { get; set; } = string.Empty;
}

public class CreateIssueRequest
{
    public string Title { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string ReportedByUserId { get; set; } = string.Empty;
    public string ReportedByUserName { get; set; } = string.Empty;
}

public class UpdateIssueStatusRequest
{
    public string Status { get; set; } = "Under Review";
    public string? OfficialResponse { get; set; }
    public string? RespondedBy { get; set; }
}
