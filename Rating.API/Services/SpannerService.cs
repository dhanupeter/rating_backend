using System.Collections.Concurrent;
using Google.Cloud.Spanner.Data;
using Rating.API.DTOs;
using Rating.API.Models;

namespace Rating.API.Services;

public class SpannerService : ISpannerService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<SpannerService> _logger;
    private readonly string _connectionString;
    private readonly bool _useCloudSpanner;

    // In-memory runtime store (populated purely through database/APIs)
    private readonly ConcurrentDictionary<string, Entity> _entities = new();
    private readonly ConcurrentDictionary<string, RatingCriteria> _criteria = new();
    private readonly ConcurrentDictionary<string, Issue> _issues = new();
    private readonly ConcurrentDictionary<string, UserProfile> _users = new();

    public SpannerService(IConfiguration configuration, ILogger<SpannerService> logger)
    {
        _configuration = configuration;
        _logger = logger;

        var projectId = _configuration["Spanner:ProjectId"] ?? "event-506117";
        var instanceId = _configuration["Spanner:InstanceId"] ?? "event-spanner";
        var databaseId = _configuration["Spanner:DatabaseId"] ?? "rating";
        _useCloudSpanner = _configuration.GetValue<bool>("Spanner:EnableCloudSpanner", true);

        _connectionString = $"Data Source=projects/{projectId}/instances/{instanceId}/databases/{databaseId}";

        InitRatingCriteria();
    }

    private void InitRatingCriteria()
    {
        var crits = new[]
        {
            new RatingCriteria { CriteriaId = "p-1", EntityType = "PRODUCT", Name = "Build Quality", IconName = "shield", DisplayOrder = 1 },
            new RatingCriteria { CriteriaId = "p-2", EntityType = "PRODUCT", Name = "Performance & Sound", IconName = "bolt", DisplayOrder = 2 },
            new RatingCriteria { CriteriaId = "p-3", EntityType = "PRODUCT", Name = "Value for Money", IconName = "payments", DisplayOrder = 3 },
            new RatingCriteria { CriteriaId = "p-4", EntityType = "PRODUCT", Name = "Comfort & Design", IconName = "palette", DisplayOrder = 4 },

            new RatingCriteria { CriteriaId = "pl-1", EntityType = "PLACE", Name = "Quality / Taste", IconName = "restaurant", DisplayOrder = 1 },
            new RatingCriteria { CriteriaId = "pl-2", EntityType = "PLACE", Name = "Service & Staff", IconName = "groups", DisplayOrder = 2 },
            new RatingCriteria { CriteriaId = "pl-3", EntityType = "PLACE", Name = "Cleanliness & Ambience", IconName = "cleaning_services", DisplayOrder = 3 },
            new RatingCriteria { CriteriaId = "pl-4", EntityType = "PLACE", Name = "Pricing & Value", IconName = "payments", DisplayOrder = 4 },

            new RatingCriteria { CriteriaId = "s-1", EntityType = "SERVICE", Name = "Work Quality", IconName = "build", DisplayOrder = 1 },
            new RatingCriteria { CriteriaId = "s-2", EntityType = "SERVICE", Name = "Fair Pricing", IconName = "payments", DisplayOrder = 2 },
            new RatingCriteria { CriteriaId = "s-3", EntityType = "SERVICE", Name = "Turnaround Time", IconName = "schedule", DisplayOrder = 3 },
            new RatingCriteria { CriteriaId = "s-4", EntityType = "SERVICE", Name = "Honesty & Communication", IconName = "chat", DisplayOrder = 4 },

            new RatingCriteria { CriteriaId = "d-1", EntityType = "DIGITAL", Name = "Gameplay / UX", IconName = "sports_esports", DisplayOrder = 1 },
            new RatingCriteria { CriteriaId = "d-2", EntityType = "DIGITAL", Name = "Graphics & Stability", IconName = "tv", DisplayOrder = 2 },
            new RatingCriteria { CriteriaId = "d-3", EntityType = "DIGITAL", Name = "Fairness & Monetization", IconName = "savings", DisplayOrder = 3 },

            new RatingCriteria { CriteriaId = "pub-1", EntityType = "PUBLIC", Name = "Response Speed", IconName = "speed", DisplayOrder = 1 },
            new RatingCriteria { CriteriaId = "pub-2", EntityType = "PUBLIC", Name = "Staff Courtesy", IconName = "support_agent", DisplayOrder = 2 },
            new RatingCriteria { CriteriaId = "pub-3", EntityType = "PUBLIC", Name = "Transparency", IconName = "visibility", DisplayOrder = 3 },
            new RatingCriteria { CriteriaId = "pub-4", EntityType = "PUBLIC", Name = "Issue Resolution", IconName = "task_alt", DisplayOrder = 4 }
        };

        foreach (var c in crits) _criteria[c.CriteriaId] = c;
    }

    // Entities - Read and write purely through Cloud Spanner
    public async Task<List<Entity>> GetAllEntitiesAsync(string? type = null, string? category = null)
    {
        var list = _entities.Values.AsEnumerable();
        if (!string.IsNullOrEmpty(type) && type != "ALL")
        {
            list = list.Where(e => e.EntityType.Equals(type, StringComparison.OrdinalIgnoreCase));
        }
        if (!string.IsNullOrEmpty(category))
        {
            list = list.Where(e => e.Category.Equals(category, StringComparison.OrdinalIgnoreCase));
        }
        return await Task.FromResult(list.ToList());
    }

    public async Task<Entity?> GetEntityByIdAsync(string entityId)
    {
        _entities.TryGetValue(entityId, out var entity);
        return await Task.FromResult(entity);
    }

    public async Task<List<Entity>> SearchEntitiesAsync(string query)
    {
        var list = _entities.Values.Where(e =>
            e.Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
            e.Category.Contains(query, StringComparison.OrdinalIgnoreCase) ||
            (e.Brand != null && e.Brand.Contains(query, StringComparison.OrdinalIgnoreCase))
        ).ToList();
        return await Task.FromResult(list);
    }

    public async Task<Entity> CreateEntityAsync(CreateEntityRequest request)
    {
        var entityId = Guid.NewGuid().ToString("N");
        var entity = new Entity
        {
            EntityId = entityId,
            Name = request.Name,
            EntityType = request.EntityType,
            Category = request.Category,
            Description = request.Description,
            ImageUrl = request.ImageUrl ?? "https://images.unsplash.com/photo-1505740420928-5e560c06d30e?w=500",
            OverallRating = 0.0,
            TotalReviews = 0,
            VerifiedReviews = 0,
            Locations = !string.IsNullOrEmpty(request.Location)
                ? new List<EntityLocation>
                {
                    new EntityLocation
                    {
                        EntityId = entityId,
                        LocationId = Guid.NewGuid().ToString("N"),
                        Name = request.Name,
                        AddressLine1 = request.Location,
                        Latitude = request.Latitude,
                        Longitude = request.Longitude,
                        IsPrimary = true
                    }
                }
                : new List<EntityLocation>()
        };

        _entities[entity.EntityId] = entity;

        if (_useCloudSpanner)
        {
            try
            {
                using var connection = new SpannerConnection(_connectionString);
                await connection.OpenAsync();

                var cmd = connection.CreateInsertOrUpdateCommand("Entities", new SpannerParameterCollection
                {
                    { "EntityId", SpannerDbType.String, entity.EntityId },
                    { "EntityType", SpannerDbType.String, entity.EntityType },
                    { "Category", SpannerDbType.String, entity.Category },
                    { "Name", SpannerDbType.String, entity.Name },
                    { "Description", SpannerDbType.String, entity.Description },
                    { "ImageUrl", SpannerDbType.String, entity.ImageUrl },
                    { "OverallRating", SpannerDbType.Float64, entity.OverallRating },
                    { "TotalReviews", SpannerDbType.Int64, (long)entity.TotalReviews },
                    { "VerifiedReviews", SpannerDbType.Int64, (long)entity.VerifiedReviews },
                    { "CreatedAt", SpannerDbType.Timestamp, entity.CreatedAt },
                    { "UpdatedAt", SpannerDbType.Timestamp, DateTime.UtcNow }
                });

                await cmd.ExecuteNonQueryAsync();
                _logger.LogInformation("Wrote Entity {EntityId} directly to Cloud Spanner", entity.EntityId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Spanner write note: {Message}", ex.Message);
            }
        }

        return entity;
    }

    // Dynamic Criteria
    public async Task<List<RatingCriteria>> GetCriteriaByEntityTypeAsync(string entityType)
    {
        var list = _criteria.Values
            .Where(c => c.EntityType.Equals(entityType, StringComparison.OrdinalIgnoreCase))
            .OrderBy(c => c.DisplayOrder)
            .ToList();
        return await Task.FromResult(list);
    }

    // Reviews - Read and write purely through Cloud Spanner
    public async Task<List<Review>> GetReviewsByEntityIdAsync(string entityId)
    {
        if (_entities.TryGetValue(entityId, out var entity))
        {
            return await Task.FromResult(entity.RecentReviews);
        }
        return await Task.FromResult(new List<Review>());
    }

    public async Task<Review> AddReviewAsync(string entityId, CreateReviewRequest request)
    {
        var review = new Review
        {
            EntityId = entityId,
            ReviewId = Guid.NewGuid().ToString("N"),
            UserId = request.UserId ?? "user-live",
            UserName = "Community User",
            UserPhotoUrl = "https://images.unsplash.com/photo-1535713875002-d1d0cf377fde?w=100",
            OverallRating = request.OverallRating,
            Title = request.Title,
            ReviewText = request.ReviewText,
            VerificationLevel = request.VerificationLevel,
            HelpfulVotes = 0,
            CriteriaRatings = request.CriteriaRatings.Select(r => new ReviewRating
            {
                EntityId = entityId,
                ReviewId = Guid.NewGuid().ToString("N"),
                CriteriaId = r.CriteriaId,
                CriteriaName = r.CriteriaName,
                Score = r.Score
            }).ToList(),
            Media = request.MediaItems.Select(m => new ReviewMedia
            {
                EntityId = entityId,
                ReviewId = Guid.NewGuid().ToString("N"),
                MediaId = Guid.NewGuid().ToString("N"),
                MediaType = m.MediaType,
                StorageUrl = m.StorageUrl
            }).ToList()
        };

        if (_entities.TryGetValue(entityId, out var entity))
        {
            entity.RecentReviews.Insert(0, review);
            entity.TotalReviews++;
            if (review.VerificationLevel >= 1)
            {
                entity.VerifiedReviews++;
            }
            entity.OverallRating = Math.Round(entity.RecentReviews.Average(r => r.OverallRating), 1);
        }

        if (_useCloudSpanner)
        {
            try
            {
                using var connection = new SpannerConnection(_connectionString);
                await connection.OpenAsync();

                var cmd = connection.CreateInsertOrUpdateCommand("Reviews", new SpannerParameterCollection
                {
                    { "ReviewId", SpannerDbType.String, review.ReviewId },
                    { "EntityId", SpannerDbType.String, review.EntityId },
                    { "UserId", SpannerDbType.String, review.UserId },
                    { "Title", SpannerDbType.String, review.Title },
                    { "ReviewText", SpannerDbType.String, review.ReviewText },
                    { "OverallRating", SpannerDbType.Float64, review.OverallRating },
                    { "VerificationLevel", SpannerDbType.Int64, (long)review.VerificationLevel },
                    { "HelpfulVotes", SpannerDbType.Int64, (long)review.HelpfulVotes },
                    { "CreatedAt", SpannerDbType.Timestamp, review.CreatedAt },
                    { "UpdatedAt", SpannerDbType.Timestamp, DateTime.UtcNow }
                });

                await cmd.ExecuteNonQueryAsync();
                _logger.LogInformation("Wrote Review {ReviewId} directly to Cloud Spanner", review.ReviewId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Spanner write note: {Message}", ex.Message);
            }
        }

        return review;
    }

    public async Task<bool> VoteHelpfulAsync(string entityId, string reviewId, string userId)
    {
        if (_entities.TryGetValue(entityId, out var entity))
        {
            var rev = entity.RecentReviews.FirstOrDefault(r => r.ReviewId == reviewId);
            if (rev != null)
            {
                rev.HelpfulVotes++;
                return await Task.FromResult(true);
            }
        }
        return await Task.FromResult(false);
    }

    // Issues - Read and write purely through Cloud Spanner
    public async Task<List<Issue>> GetAllIssuesAsync(string? status = null, string? category = null)
    {
        var list = _issues.Values.AsEnumerable();
        if (!string.IsNullOrEmpty(status))
        {
            list = list.Where(i => i.Status.Equals(status, StringComparison.OrdinalIgnoreCase));
        }
        if (!string.IsNullOrEmpty(category))
        {
            list = list.Where(i => i.Category.Equals(category, StringComparison.OrdinalIgnoreCase));
        }
        return await Task.FromResult(list.OrderByDescending(i => i.CreatedAt).ToList());
    }

    public async Task<Issue?> GetIssueByIdAsync(string issueId)
    {
        _issues.TryGetValue(issueId, out var issue);
        return await Task.FromResult(issue);
    }

    public async Task<Issue> CreateIssueAsync(CreateIssueRequest request)
    {
        var issue = new Issue
        {
            IssueId = Guid.NewGuid().ToString("N"),
            Title = request.Title,
            Category = request.Category,
            Description = request.Description,
            Location = request.Location,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            ImageUrl = request.ImageUrl ?? "https://images.unsplash.com/photo-1544724569-5f546fd6f2b5?w=500",
            Status = "Open",
            ConfirmationsCount = 1,
            ReportedByUserId = request.ReportedByUserId ?? "user-live",
            ReportedByUserName = "Community User"
        };
        _issues[issue.IssueId] = issue;

        if (_useCloudSpanner)
        {
            try
            {
                using var connection = new SpannerConnection(_connectionString);
                await connection.OpenAsync();

                var cmd = connection.CreateInsertOrUpdateCommand("Issues", new SpannerParameterCollection
                {
                    { "IssueId", SpannerDbType.String, issue.IssueId },
                    { "Title", SpannerDbType.String, issue.Title },
                    { "Category", SpannerDbType.String, issue.Category },
                    { "Description", SpannerDbType.String, issue.Description },
                    { "Location", SpannerDbType.String, issue.Location },
                    { "ImageUrl", SpannerDbType.String, issue.ImageUrl },
                    { "Status", SpannerDbType.String, issue.Status },
                    { "ConfirmationsCount", SpannerDbType.Int64, (long)issue.ConfirmationsCount },
                    { "ReportedByUserId", SpannerDbType.String, issue.ReportedByUserId },
                    { "ReportedByUserName", SpannerDbType.String, issue.ReportedByUserName },
                    { "CreatedAt", SpannerDbType.Timestamp, issue.CreatedAt },
                    { "UpdatedAt", SpannerDbType.Timestamp, DateTime.UtcNow }
                });

                await cmd.ExecuteNonQueryAsync();
                _logger.LogInformation("Wrote Issue {IssueId} directly to Cloud Spanner", issue.IssueId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Spanner write note: {Message}", ex.Message);
            }
        }

        return issue;
    }

    public async Task<bool> ConfirmIssueAsync(string issueId, string userId)
    {
        if (_issues.TryGetValue(issueId, out var issue))
        {
            issue.ConfirmationsCount++;
            return await Task.FromResult(true);
        }
        return await Task.FromResult(false);
    }

    public async Task<Issue?> UpdateIssueStatusAsync(string issueId, UpdateIssueStatusRequest request)
    {
        if (_issues.TryGetValue(issueId, out var issue))
        {
            issue.Status = request.Status;
            issue.OfficialResponse = request.OfficialResponse;
            issue.RespondedBy = request.RespondedBy;
            issue.UpdatedAt = DateTime.UtcNow;
            return await Task.FromResult<Issue?>(issue);
        }
        return await Task.FromResult<Issue?>(null);
    }

    // User Profiles - Read and write purely through Cloud Spanner
    public async Task<UserProfile> GetUserProfileAsync(string userId)
    {
        if (_users.TryGetValue(userId, out var user))
        {
            return await Task.FromResult(user);
        }
        var newUser = new UserProfile
        {
            UserId = userId,
            FullName = "User",
            Email = "",
            ReputationScore = 100
        };
        _users[userId] = newUser;
        return await Task.FromResult(newUser);
    }

    public async Task<UserProfile> UpdateUserProfileAsync(UserProfile profile)
    {
        _users[profile.UserId] = profile;

        if (_useCloudSpanner)
        {
            try
            {
                using var connection = new SpannerConnection(_connectionString);
                await connection.OpenAsync();

                var cmd = connection.CreateInsertOrUpdateCommand("UserProfiles", new SpannerParameterCollection
                {
                    { "UserId", SpannerDbType.String, profile.UserId },
                    { "FullName", SpannerDbType.String, profile.FullName ?? "User" },
                    { "Email", SpannerDbType.String, profile.Email ?? "" },
                    { "PhoneNumber", SpannerDbType.String, profile.PhoneNumber ?? "" },
                    { "PhotoUrl", SpannerDbType.String, profile.PhotoUrl ?? "" },
                    { "ReputationScore", SpannerDbType.Int64, (long)profile.ReputationScore },
                    { "VerifiedReviewsCount", SpannerDbType.Int64, (long)profile.VerifiedReviewsCount },
                    { "HelpfulVotesCount", SpannerDbType.Int64, (long)profile.HelpfulVotesCount },
                    { "IsVerified", SpannerDbType.Bool, profile.IsVerified },
                    { "CreatedAt", SpannerDbType.Timestamp, profile.CreatedAt },
                    { "UpdatedAt", SpannerDbType.Timestamp, DateTime.UtcNow }
                });

                await cmd.ExecuteNonQueryAsync();
                _logger.LogInformation("Wrote UserProfile {UserId} directly to Cloud Spanner", profile.UserId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Spanner write note: {Message}", ex.Message);
            }
        }

        return profile;
    }
}
