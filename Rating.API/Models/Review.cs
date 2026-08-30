namespace Rating.API.Models;

public class Review
{
    public string EntityId { get; set; } = string.Empty;
    public string ReviewId { get; set; } = Guid.NewGuid().ToString("N");
    public string UserId { get; set; } = string.Empty;
    public string? ExperienceId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string UserPhotoUrl { get; set; } = string.Empty;
    public double OverallRating { get; set; }
    public string? Title { get; set; }
    public string? ReviewText { get; set; }
    public int VerificationLevel { get; set; } = 0; // 0: Basic, 1: Photo, 2: Location/GPS, 3: Receipt, 4: Strong
    public long HelpfulVotes { get; set; }
    public bool IsModerated { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<ReviewRating> CriteriaRatings { get; set; } = new();
    public List<ReviewMedia> Media { get; set; } = new();
}

public class ReviewRating
{
    public string EntityId { get; set; } = string.Empty;
    public string ReviewId { get; set; } = string.Empty;
    public string CriteriaId { get; set; } = string.Empty;
    public string CriteriaName { get; set; } = string.Empty;
    public double Score { get; set; }
}

public class ReviewMedia
{
    public string EntityId { get; set; } = string.Empty;
    public string ReviewId { get; set; } = string.Empty;
    public string MediaId { get; set; } = Guid.NewGuid().ToString("N");
    public string MediaType { get; set; } = "PHOTO";
    public string StorageUrl { get; set; } = string.Empty;
    public string? ThumbnailUrl { get; set; }
    public string? Caption { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
