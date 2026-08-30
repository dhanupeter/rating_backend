using Google.Cloud.Spanner.Data;
using Rating.API.Models;
using Rating.API.DTOs;
using System.Collections.Concurrent;

namespace Rating.API.Services;

public class SpannerService : ISpannerService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<SpannerService> _logger;
    private readonly string _connectionString;
    private readonly bool _useCloudSpanner;

    private static readonly ConcurrentDictionary<string, Entity> _entities = new();
    private static readonly ConcurrentDictionary<string, RatingCriteria> _criteria = new();
    private static readonly ConcurrentDictionary<string, Issue> _issues = new();
    private static readonly ConcurrentDictionary<string, UserProfile> _users = new();

    public SpannerService(IConfiguration configuration, ILogger<SpannerService> logger)
    {
        _configuration = configuration;
        _logger = logger;

        var projectId = _configuration["Spanner:ProjectId"] ?? "event-506117";
        var instanceId = _configuration["Spanner:InstanceId"] ?? "event-spanner";
        var databaseId = _configuration["Spanner:DatabaseId"] ?? "rating";
        _useCloudSpanner = _configuration.GetValue<bool>("Spanner:EnableCloudSpanner");

        _connectionString = $"Data Source=projects/{projectId}/instances/{instanceId}/databases/{databaseId}";

        SeedData();
    }

    private void SeedData()
    {
        if (!_users.ContainsKey("user-dhanu"))
        {
            _users["user-dhanu"] = new UserProfile
            {
                UserId = "user-dhanu",
                FullName = "Dhanu Peter",
                Email = "dhanupeter@gmail.com",
                PhoneNumber = "+919876543210",
                PhotoUrl = "https://images.unsplash.com/photo-1535713875002-d1d0cf377fde?w=200",
                ReputationScore = 480,
                VerifiedReviewsCount = 38,
                HelpfulVotesCount = 246,
                Badges = new List<string> { "🏆 Trusted Reviewer", "📍 Local Explorer", "🛡️ Verified Buyer" },
                IsVerified = true
            };
        }

        if (!_criteria.ContainsKey("p-1"))
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

        if (!_entities.ContainsKey("ent-kannan-store"))
        {
            _entities["ent-kannan-store"] = new Entity
            {
                EntityId = "ent-kannan-store",
                EntityType = "PLACE",
                Category = "Supermarket & Departmental",
                Name = "Shri Kannan Departmental Store",
                Brand = "Shri Kannan",
                Model = "Super Store",
                Description = "1274, Tiruchi Road, Coimbatore. Wide selection of groceries, organic spices and fresh dairy.",
                ExternalProvider = "Geoapify",
                ImageUrl = "https://images.unsplash.com/photo-1578916171728-46686eac8d58?w=600",
                OverallRating = 4.7,
                TotalReviews = 489,
                VerifiedReviews = 412,
                Locations = new List<EntityLocation>
                {
                    new EntityLocation
                    {
                        EntityId = "ent-kannan-store",
                        LocationId = "loc-kannan-1",
                        Name = "Tiruchi Road Branch",
                        AddressLine1 = "1274, Tiruchi Road, Ward 73",
                        City = "Coimbatore",
                        State = "Tamil Nadu",
                        Latitude = 10.9956861,
                        Longitude = 76.9775409,
                        IsPrimary = true
                    }
                },
                CriteriaAverages = new Dictionary<string, double>
                {
                    ["Quality / Freshness"] = 4.8,
                    ["Service & Staff"] = 4.6,
                    ["Cleanliness & Ambience"] = 4.7,
                    ["Pricing & Value"] = 4.6
                }
            };

            _entities["ent-saravana-tiruppur"] = new Entity
            {
                EntityId = "ent-saravana-tiruppur",
                EntityType = "PLACE",
                Category = "South Indian Restaurant",
                Name = "Hotel Saravana Bhavan",
                Brand = "Saravana Bhavan",
                Model = "Restaurant",
                Description = "Authentic South Indian vegetarian breakfast, crisp ghee roasts and filter coffee.",
                ExternalProvider = "Geoapify",
                ImageUrl = "https://images.unsplash.com/photo-1589301760014-d929f3979dbc?w=600",
                OverallRating = 4.8,
                TotalReviews = 812,
                VerifiedReviews = 640,
                Locations = new List<EntityLocation>
                {
                    new EntityLocation
                    {
                        EntityId = "ent-saravana-tiruppur",
                        LocationId = "loc-sb-1",
                        Name = "Station Road Branch",
                        AddressLine1 = "Station Road, Old Bus Stand",
                        City = "Tiruppur",
                        State = "Tamil Nadu",
                        Latitude = 11.1085,
                        Longitude = 77.3411,
                        IsPrimary = true
                    }
                },
                CriteriaAverages = new Dictionary<string, double>
                {
                    ["Quality / Taste"] = 4.9,
                    ["Service & Staff"] = 4.7,
                    ["Cleanliness & Ambience"] = 4.8,
                    ["Pricing & Value"] = 4.6
                }
            };

            _entities["ent-boat-450"] = new Entity
            {
                EntityId = "ent-boat-450",
                EntityType = "PRODUCT",
                Category = "Headphones & Audio",
                Name = "boAt Rockerz 450",
                Brand = "boAt",
                Model = "Rockerz 450",
                Description = "On-ear wireless Bluetooth headphones with 40mm dynamic drivers, 15H playback.",
                ExternalUrl = "https://www.boat-lifestyle.com/products/rockerz-450",
                ExternalProvider = "Amazon",
                ImageUrl = "https://images.unsplash.com/photo-1505740420928-5e560c06d30e?w=600",
                OverallRating = 4.4,
                TotalReviews = 1240,
                VerifiedReviews = 980,
                CriteriaAverages = new Dictionary<string, double>
                {
                    ["Build Quality"] = 4.2,
                    ["Performance & Sound"] = 4.7,
                    ["Value for Money"] = 4.8,
                    ["Comfort & Design"] = 4.3
                }
            };
        }

        if (!_issues.ContainsKey("iss-001"))
        {
            _issues["iss-001"] = new Issue
            {
                IssueId = "iss-001",
                Title = "Exposed High-Voltage Cable Near School Zone",
                Category = "Electrical Safety",
                Description = "An open underground cable junction is exposed on the pedestrian walkway in front of Municipal High School.",
                Location = "Kumaran Road, Near Bus Stand, Tiruppur",
                Latitude = 11.1070,
                Longitude = 77.3420,
                ImageUrl = "https://images.unsplash.com/photo-1544724569-5f546fd6f2b5?w=600",
                Status = "Under Review",
                ConfirmationsCount = 28,
                ReportedByUserId = "user-dhanu",
                ReportedByUserName = "Dhanu Peter",
                OfficialResponse = "TANGEDCO North Division Field Engineer assigned for emergency insulation.",
                RespondedBy = "TANGEDCO North Desk"
            };

            _issues["iss-002"] = new Issue
            {
                IssueId = "iss-002",
                Title = "Deep Pothole Cluster on Avinashi Road Flyover",
                Category = "Road & Potholes",
                Description = "Multiple 6-inch deep potholes causing two-wheeler skids at night near the down-ramp.",
                Location = "Avinashi Road Flyover, Tiruppur",
                Latitude = 11.1130,
                Longitude = 77.3460,
                ImageUrl = "https://images.unsplash.com/photo-1515162816999-a0c47dc192f7?w=600",
                Status = "Response Received",
                ConfirmationsCount = 54,
                ReportedByUserId = "user-karthik",
                ReportedByUserName = "Karthik R.",
                OfficialResponse = "State Highways Department has issued emergency patch work order.",
                RespondedBy = "Highways Div"
            };
        }
    }

    // Entities
    public Task<List<Entity>> GetAllEntitiesAsync(string? type = null, string? category = null)
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
        return Task.FromResult(list.ToList());
    }

    public Task<Entity?> GetEntityByIdAsync(string entityId)
    {
        _entities.TryGetValue(entityId, out var entity);
        return Task.FromResult(entity);
    }

    public Task<List<Entity>> SearchEntitiesAsync(string query)
    {
        var list = _entities.Values.Where(e =>
            e.Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
            e.Category.Contains(query, StringComparison.OrdinalIgnoreCase) ||
            (e.Brand != null && e.Brand.Contains(query, StringComparison.OrdinalIgnoreCase))
        ).ToList();
        return Task.FromResult(list);
    }

    public Task<Entity> CreateEntityAsync(CreateEntityRequest request)
    {
        var entity = new Entity
        {
            EntityId = Guid.NewGuid().ToString("N"),
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
                        EntityId = Guid.NewGuid().ToString("N"),
                        Name = request.Name,
                        AddressLine1 = request.Location,
                        City = "Tiruppur",
                        State = "Tamil Nadu",
                        Latitude = request.Latitude,
                        Longitude = request.Longitude,
                        IsPrimary = true
                    }
                }
                : new List<EntityLocation>()
        };

        _entities[entity.EntityId] = entity;
        return Task.FromResult(entity);
    }

    // Dynamic Criteria
    public Task<List<RatingCriteria>> GetCriteriaByEntityTypeAsync(string entityType)
    {
        var list = _criteria.Values
            .Where(c => c.EntityType.Equals(entityType, StringComparison.OrdinalIgnoreCase))
            .OrderBy(c => c.DisplayOrder)
            .ToList();
        return Task.FromResult(list);
    }

    // Reviews
    public Task<List<Review>> GetReviewsByEntityIdAsync(string entityId)
    {
        if (_entities.TryGetValue(entityId, out var entity))
        {
            return Task.FromResult(entity.RecentReviews);
        }
        return Task.FromResult(new List<Review>());
    }

    public Task<Review> AddReviewAsync(string entityId, CreateReviewRequest request)
    {
        var review = new Review
        {
            EntityId = entityId,
            ReviewId = Guid.NewGuid().ToString("N"),
            UserId = request.UserId ?? "user-dhanu",
            UserName = "Dhanu Peter",
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

        return Task.FromResult(review);
    }

    public Task<bool> VoteHelpfulAsync(string entityId, string reviewId, string userId)
    {
        if (_entities.TryGetValue(entityId, out var entity))
        {
            var rev = entity.RecentReviews.FirstOrDefault(r => r.ReviewId == reviewId);
            if (rev != null)
            {
                rev.HelpfulVotes++;
                return Task.FromResult(true);
            }
        }
        return Task.FromResult(false);
    }

    // Issues
    public Task<List<Issue>> GetAllIssuesAsync(string? status = null, string? category = null)
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
        return Task.FromResult(list.OrderByDescending(i => i.CreatedAt).ToList());
    }

    public Task<Issue?> GetIssueByIdAsync(string issueId)
    {
        _issues.TryGetValue(issueId, out var issue);
        return Task.FromResult(issue);
    }

    public Task<Issue> CreateIssueAsync(CreateIssueRequest request)
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
            ReportedByUserId = request.ReportedByUserId ?? "user-dhanu",
            ReportedByUserName = "Dhanu Peter"
        };
        _issues[issue.IssueId] = issue;
        return Task.FromResult(issue);
    }

    public Task<bool> ConfirmIssueAsync(string issueId, string userId)
    {
        if (_issues.TryGetValue(issueId, out var issue))
        {
            issue.ConfirmationsCount++;
            return Task.FromResult(true);
        }
        return Task.FromResult(false);
    }

    public Task<Issue?> UpdateIssueStatusAsync(string issueId, UpdateIssueStatusRequest request)
    {
        if (_issues.TryGetValue(issueId, out var issue))
        {
            issue.Status = request.Status;
            issue.OfficialResponse = request.OfficialResponse;
            issue.RespondedBy = request.RespondedBy;
            issue.UpdatedAt = DateTime.UtcNow;
            return Task.FromResult<Issue?>(issue);
        }
        return Task.FromResult<Issue?>(null);
    }

    // User Profile
    public Task<UserProfile> GetUserProfileAsync(string userId)
    {
        if (_users.TryGetValue(userId, out var user))
        {
            return Task.FromResult(user);
        }
        var newUser = new UserProfile
        {
            UserId = userId,
            FullName = "Community Explorer",
            Email = $"{userId}@rating.app",
            ReputationScore = 100
        };
        _users[userId] = newUser;
        return Task.FromResult(newUser);
    }

    public Task<UserProfile> UpdateUserProfileAsync(UserProfile profile)
    {
        _users[profile.UserId] = profile;
        return Task.FromResult(profile);
    }
}
