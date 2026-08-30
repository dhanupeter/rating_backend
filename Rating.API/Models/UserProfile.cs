namespace Rating.API.Models;

public class UserProfile
{
    public string? UserId { get; set; }
    public string? FullName { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? PhotoUrl { get; set; }
    public long? ReputationScore { get; set; } = 100;
    public long? VerifiedReviewsCount { get; set; } = 0;
    public long? HelpfulVotesCount { get; set; } = 0;
    public List<string>? Badges { get; set; } = new();
    public bool? IsVerified { get; set; } = true;
    public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;
    public Dictionary<string, int>? ReviewsByCategory { get; set; } = new();
}
