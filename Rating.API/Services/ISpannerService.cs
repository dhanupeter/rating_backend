using Rating.API.Models;
using Rating.API.DTOs;

namespace Rating.API.Services;

public interface ISpannerService
{
    // Entities
    Task<List<Entity>> GetAllEntitiesAsync(string? type = null, string? category = null);
    Task<Entity?> GetEntityByIdAsync(string entityId);
    Task<List<Entity>> SearchEntitiesAsync(string query);
    Task<Entity> CreateEntityAsync(CreateEntityRequest request);

    // Dynamic Criteria
    Task<List<RatingCriteria>> GetCriteriaByEntityTypeAsync(string entityType);

    // Reviews
    Task<List<Review>> GetReviewsByEntityIdAsync(string entityId);
    Task<Review> AddReviewAsync(string entityId, CreateReviewRequest request);
    Task<bool> VoteHelpfulAsync(string entityId, string reviewId, string userId);

    // Issues
    Task<List<Issue>> GetAllIssuesAsync(string? status = null, string? category = null);
    Task<Issue?> GetIssueByIdAsync(string issueId);
    Task<Issue> CreateIssueAsync(CreateIssueRequest request);
    Task<bool> ConfirmIssueAsync(string issueId, string userId);
    Task<Issue?> UpdateIssueStatusAsync(string issueId, UpdateIssueStatusRequest request);

    // User Profile
    Task<UserProfile> GetUserProfileAsync(string userId);
    Task<UserProfile> UpdateUserProfileAsync(UserProfile profile);
}
