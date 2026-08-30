namespace Rating.API.Models;

public class UserProfile
{
    public string UserId { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string PhotoUrl { get; set; } = string.Empty;
    public long ReputationScore { get; set; } = 100;
    public long VerifiedReviewsCount { get; set; } = 0;
    public long HelpfulVotesCount { get; set; } = 0;
    public List<string> Badges { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Counts by category
    public Dictionary<string, int> ReviewsByCategory { get; set; } = new();
}
