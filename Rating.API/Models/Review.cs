namespace Rating.API.Models;

public class Review
{
    public string EntityId { get; set; } = string.Empty;
    public string ReviewId { get; set; } = Guid.NewGuid().ToString();
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string UserPhotoUrl { get; set; } = string.Empty;
    public double OverallRating { get; set; } = 5.0;
    public string Title { get; set; } = string.Empty;
    public string ReviewText { get; set; } = string.Empty;
    public DateTime? ExperienceDate { get; set; }
    public string LocationId { get; set; } = string.Empty;
    public int VerificationLevel { get; set; } = 0; // 0: Basic, 1: Photo, 2: Location, 3: Invoice/Receipt, 4: Highly Verified
    public long HelpfulVotes { get; set; } = 0;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<ReviewRatingItem> Ratings { get; set; } = new();
    public List<ReviewMediaItem> Media { get; set; } = new();
}

public class ReviewRatingItem
{
    public string CriteriaId { get; set; } = string.Empty;
    public string CriteriaName { get; set; } = string.Empty;
    public double Score { get; set; } = 5.0;
}

public class ReviewMediaItem
{
    public string MediaId { get; set; } = Guid.NewGuid().ToString();
    public string MediaType { get; set; } = "PHOTO"; // PHOTO, VIDEO, RECEIPT
    public string StorageUrl { get; set; } = string.Empty;
    public string ThumbnailUrl { get; set; } = string.Empty;
    public string Caption { get; set; } = string.Empty;
}
