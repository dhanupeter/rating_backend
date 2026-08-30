using System.Collections.Concurrent;
using Google.Cloud.Spanner.Data;
using Rating.API.DTOs;
using Rating.API.Models;

namespace Rating.API.Services;

public class SpannerService : ISpannerService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<SpannerService> _logger;
    private readonly string? _connectionString;
    private readonly bool _useSpanner;

    // Fast in-memory backing store for local dev & instant caching
    private static readonly ConcurrentDictionary<string, Entity> _entities = new();
    private static readonly ConcurrentDictionary<string, List<RatingCriteria>> _criteriaByEntityType = new();
    private static readonly ConcurrentDictionary<string, List<Review>> _reviewsByEntity = new();
    private static readonly ConcurrentDictionary<string, Issue> _issues = new();
    private static readonly ConcurrentDictionary<string, UserProfile> _users = new();

    static SpannerService()
    {
        SeedInitialData();
    }

    public SpannerService(IConfiguration configuration, ILogger<SpannerService> logger)
    {
        _configuration = configuration;
        _logger = logger;
        
        var project = _configuration["Spanner:ProjectId"] ?? "event-506117";
        var instance = _configuration["Spanner:InstanceId"] ?? "rating-instance";
        var database = _configuration["Spanner:DatabaseId"] ?? "rating";
        
        _connectionString = $"Data Source=projects/{project}/instances/{instance}/databases/{database}";
        _useSpanner = !string.IsNullOrEmpty(_configuration["Spanner:EnableCloudSpanner"]) && 
                      bool.Parse(_configuration["Spanner:EnableCloudSpanner"]!);
    }

    private static void SeedInitialData()
    {
        // 1. Dynamic Criteria Templates
        _criteriaByEntityType["PRODUCT"] = new List<RatingCriteria>
        {
            new() { CriteriaId = "p-1", EntityType = "PRODUCT", Name = "Build Quality", IconName = "shield", Weight = 1.0, DisplayOrder = 1 },
            new() { CriteriaId = "p-2", EntityType = "PRODUCT", Name = "Performance", IconName = "bolt", Weight = 1.0, DisplayOrder = 2 },
            new() { CriteriaId = "p-3", EntityType = "PRODUCT", Name = "Value for Money", IconName = "payments", Weight = 1.0, DisplayOrder = 3 },
            new() { CriteriaId = "p-4", EntityType = "PRODUCT", Name = "Design & Comfort", IconName = "palette", Weight = 1.0, DisplayOrder = 4 },
            new() { CriteriaId = "p-5", EntityType = "PRODUCT", Name = "Durability", IconName = "verified", Weight = 1.0, DisplayOrder = 5 }
        };

        _criteriaByEntityType["PLACE"] = new List<RatingCriteria>
        {
            new() { CriteriaId = "pl-1", EntityType = "PLACE", Name = "Food Quality / Taste", IconName = "restaurant", Weight = 1.0, DisplayOrder = 1 },
            new() { CriteriaId = "pl-2", EntityType = "PLACE", Name = "Service & Staff", IconName = "groups", Weight = 1.0, DisplayOrder = 2 },
            new() { CriteriaId = "pl-3", EntityType = "PLACE", Name = "Cleanliness & Ambience", IconName = "cleaning_services", Weight = 1.0, DisplayOrder = 3 },
            new() { CriteriaId = "pl-4", EntityType = "PLACE", Name = "Pricing / Worth", IconName = "payments", Weight = 1.0, DisplayOrder = 4 },
            new() { CriteriaId = "pl-5", EntityType = "PLACE", Name = "Parking / Accessibility", IconName = "local_parking", Weight = 1.0, DisplayOrder = 5 }
        };

        _criteriaByEntityType["SERVICE"] = new List<RatingCriteria>
        {
            new() { CriteriaId = "s-1", EntityType = "SERVICE", Name = "Work Quality", IconName = "build", Weight = 1.0, DisplayOrder = 1 },
            new() { CriteriaId = "s-2", EntityType = "SERVICE", Name = "Fair Pricing", IconName = "request_quote", Weight = 1.0, DisplayOrder = 2 },
            new() { CriteriaId = "s-3", EntityType = "SERVICE", Name = "Turnaround Time", IconName = "schedule", Weight = 1.0, DisplayOrder = 3 },
            new() { CriteriaId = "s-4", EntityType = "SERVICE", Name = "Honesty & Communication", IconName = "chat", Weight = 1.0, DisplayOrder = 4 }
        };

        _criteriaByEntityType["DIGITAL"] = new List<RatingCriteria>
        {
            new() { CriteriaId = "d-1", EntityType = "DIGITAL", Name = "Gameplay / UX", IconName = "sports_esports", Weight = 1.0, DisplayOrder = 1 },
            new() { CriteriaId = "d-2", EntityType = "DIGITAL", Name = "Graphics & Stability", IconName = "tv", Weight = 1.0, DisplayOrder = 2 },
            new() { CriteriaId = "d-3", EntityType = "DIGITAL", Name = "Value / No Pay-to-Win", IconName = "savings", Weight = 1.0, DisplayOrder = 3 },
            new() { CriteriaId = "d-4", EntityType = "DIGITAL", Name = "Updates & Support", IconName = "system_update", Weight = 1.0, DisplayOrder = 4 }
        };

        _criteriaByEntityType["PUBLIC"] = new List<RatingCriteria>
        {
            new() { CriteriaId = "pub-1", EntityType = "PUBLIC", Name = "Response Speed", IconName = "speed", Weight = 1.0, DisplayOrder = 1 },
            new() { CriteriaId = "pub-2", EntityType = "PUBLIC", Name = "Staff Courtesy", IconName = "support_agent", Weight = 1.0, DisplayOrder = 2 },
            new() { CriteriaId = "pub-3", EntityType = "PUBLIC", Name = "Transparency", IconName = "visibility", Weight = 1.0, DisplayOrder = 3 },
            new() { CriteriaId = "pub-4", EntityType = "PUBLIC", Name = "Problem Resolution", IconName = "task_alt", Weight = 1.0, DisplayOrder = 4 }
        };

        // 2. Entities
        var ent1 = new Entity
        {
            EntityId = "ent-boat-450",
            EntityType = "PRODUCT",
            Category = "Headphones & Audio",
            Name = "boAt Rockerz 450 Bluetooth Headset",
            Description = "On-ear wireless Bluetooth headphones with 40mm dynamic drivers, up to 15 hours battery backup and padded ear cushions.",
            Location = "Online / Global",
            ExternalUrl = "https://www.boat-lifestyle.com",
            ImageUrl = "https://images.unsplash.com/photo-1505740420928-5e560c06d30e?w=600&auto=format&fit=crop&q=60",
            OverallRating = 4.6,
            TotalReviews = 342,
            VerifiedReviews = 280,
            CriteriaAverages = new()
            {
                ["Build Quality"] = 4.5,
                ["Performance"] = 4.7,
                ["Value for Money"] = 4.8,
                ["Design & Comfort"] = 4.4,
                ["Durability"] = 4.6
            }
        };

        var ent2 = new Entity
        {
            EntityId = "ent-saravana-tiruppur",
            EntityType = "PLACE",
            Category = "South Indian Restaurant",
            Name = "Hotel Saravana Bhavan — Tiruppur",
            Description = "Authentic South Indian vegetarian breakfast, crisp ghee roasts, filter coffee, and thali lunch near Railway Station.",
            Location = "Station Road, Tiruppur, Tamil Nadu",
            Latitude = 11.1085,
            Longitude = 77.3411,
            ExternalUrl = "https://maps.google.com",
            ImageUrl = "https://images.unsplash.com/photo-1589301760014-d929f3979dbc?w=600&auto=format&fit=crop&q=60",
            OverallRating = 4.5,
            TotalReviews = 812,
            VerifiedReviews = 640,
            CriteriaAverages = new()
            {
                ["Food Quality / Taste"] = 4.8,
                ["Service & Staff"] = 4.3,
                ["Cleanliness & Ambience"] = 4.4,
                ["Pricing / Worth"] = 4.5,
                ["Parking / Accessibility"] = 4.1
            }
        };

        var ent3 = new Entity
        {
            EntityId = "ent-kumar-motors",
            EntityType = "SERVICE",
            Category = "Two-Wheeler Multi-brand Workshop",
            Name = "Kumar Motors Bike Service Centre",
            Description = "Specialized Royal Enfield, Yamaha & Honda motorbike tuning, engine decarbonizing, and water wash.",
            Location = "Avinashi Road, Tiruppur, Tamil Nadu",
            Latitude = 11.1120,
            Longitude = 77.3490,
            ImageUrl = "https://images.unsplash.com/photo-1619642751034-765dfdf7c58e?w=600&auto=format&fit=crop&q=60",
            OverallRating = 4.7,
            TotalReviews = 156,
            VerifiedReviews = 142,
            CriteriaAverages = new()
            {
                ["Work Quality"] = 4.8,
                ["Fair Pricing"] = 4.6,
                ["Turnaround Time"] = 4.7,
                ["Honesty & Communication"] = 4.8
            }
        };

        var ent4 = new Entity
        {
            EntityId = "ent-pubg-mobile",
            EntityType = "DIGITAL",
            Category = "Mobile Battle Royale Game",
            Name = "BGMI / PUBG Mobile",
            Description = "100-player battle royale shooter with Erangel, Miramar maps, and fast-paced 4v4 Team Deathmatch mode.",
            Location = "Play Store / App Store",
            ExternalUrl = "https://www.battlegroundsmobileindia.com",
            ImageUrl = "https://images.unsplash.com/photo-1542751371-adc38448a05e?w=600&auto=format&fit=crop&q=60",
            OverallRating = 4.3,
            TotalReviews = 12050,
            VerifiedReviews = 8900,
            CriteriaAverages = new()
            {
                ["Gameplay / UX"] = 4.7,
                ["Graphics & Stability"] = 4.5,
                ["Value / No Pay-to-Win"] = 3.6,
                ["Updates & Support"] = 4.4
            }
        };

        var ent5 = new Entity
        {
            EntityId = "ent-tangedco-office",
            EntityType = "PUBLIC",
            Category = "Electricity Board (TANGEDCO)",
            Name = "TANGEDCO Tiruppur North Division Office",
            Description = "Public service section for new meter connections, bill redressal, tariff disputes, and transformer maintenance.",
            Location = "Kumaran Road, Tiruppur, Tamil Nadu",
            Latitude = 11.1150,
            Longitude = 77.3450,
            ImageUrl = "https://images.unsplash.com/photo-1513694203232-719a280e022f?w=600&auto=format&fit=crop&q=60",
            OverallRating = 3.8,
            TotalReviews = 94,
            VerifiedReviews = 78,
            CriteriaAverages = new()
            {
                ["Response Speed"] = 3.5,
                ["Staff Courtesy"] = 3.9,
                ["Transparency"] = 4.1,
                ["Problem Resolution"] = 3.7
            }
        };

        _entities[ent1.EntityId] = ent1;
        _entities[ent2.EntityId] = ent2;
        _entities[ent3.EntityId] = ent3;
        _entities[ent4.EntityId] = ent4;
        _entities[ent5.EntityId] = ent5;

        // 3. Sample Reviews with Verification Tiers
        _reviewsByEntity[ent2.EntityId] = new List<Review>
        {
            new()
            {
                EntityId = ent2.EntityId,
                ReviewId = "rev-101",
                UserId = "user-dhanu",
                UserName = "Dhanu Peter",
                UserPhotoUrl = "https://images.unsplash.com/photo-1535713875002-d1d0cf377fde?w=150&auto=format&fit=crop&q=80",
                OverallRating = 4.8,
                Title = "Best Ghee Roast & Filter Coffee in Tiruppur",
                ReviewText = "Visited during morning hours. Crisp dosa, fresh coconut chutney and steaming aromatic filter coffee. Super fast service even during breakfast rush.",
                ExperienceDate = DateTime.UtcNow.AddDays(-2),
                VerificationLevel = 3, // Level 3: Invoice + Location Verified
                HelpfulVotes = 42,
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                Ratings = new()
                {
                    new() { CriteriaId = "pl-1", CriteriaName = "Food Quality / Taste", Score = 5.0 },
                    new() { CriteriaId = "pl-2", CriteriaName = "Service & Staff", Score = 4.5 },
                    new() { CriteriaId = "pl-3", CriteriaName = "Cleanliness & Ambience", Score = 4.8 },
                    new() { CriteriaId = "pl-4", CriteriaName = "Pricing / Worth", Score = 4.8 }
                },
                Media = new()
                {
                    new()
                    {
                        MediaId = "m-1",
                        MediaType = "PHOTO",
                        StorageUrl = "https://images.unsplash.com/photo-1668236543090-82eba5ee5976?w=600&auto=format&fit=crop&q=60",
                        Caption = "Crispy Ghee Roast Dosa"
                    }
                }
            }
        };

        // 4. Civic & Local Issues
        var issue1 = new Issue
        {
            IssueId = "iss-001",
            Title = "Exposed High-Voltage Cable Near School Zone",
            Category = "Electrical Safety",
            Description = "An open underground cable junction is exposed on the pedestrian walkway right in front of Municipal High School. Poses severe danger to students during rain.",
            Location = "Kumaran Road, Near Bus Stand, Tiruppur",
            Latitude = 11.1112,
            Longitude = 77.3425,
            ImageUrl = "https://images.unsplash.com/photo-1544724569-5f546fd6f2b5?w=600&auto=format&fit=crop&q=60",
            Status = "Under Review",
            ConfirmationsCount = 28,
            ReportedByUserId = "user-dhanu",
            ReportedByUserName = "Dhanu Peter",
            OfficialResponse = "TANGEDCO Field Engineer assigned for emergency insulation and junction box replacement.",
            RespondedBy = "TANGEDCO North Division Desk",
            CreatedAt = DateTime.UtcNow.AddDays(-3),
            UpdatedAt = DateTime.UtcNow.AddHours(-10)
        };

        var issue2 = new Issue
        {
            IssueId = "iss-002",
            Title = "Deep Pothole Cluster on Avinashi Road Flyover Descent",
            Category = "Road & Potholes",
            Description = "Multiple 6-inch deep potholes causing sudden two-wheeler skids especially at night. Immediate patch-work required.",
            Location = "Avinashi Road Flyover, Tiruppur",
            Latitude = 11.1145,
            Longitude = 77.3480,
            ImageUrl = "https://images.unsplash.com/photo-1515162816999-a0c47dc192f7?w=600&auto=format&fit=crop&q=60",
            Status = "Response Received",
            ConfirmationsCount = 54,
            ReportedByUserId = "user-karthik",
            ReportedByUserName = "Karthik R.",
            OfficialResponse = "State Highways Department has issued work order. Tar resurfacing scheduled tonight.",
            RespondedBy = "Highways Div Tiruppur",
            CreatedAt = DateTime.UtcNow.AddDays(-5),
            UpdatedAt = DateTime.UtcNow.AddHours(-5)
        };

        _issues[issue1.IssueId] = issue1;
        _issues[issue2.IssueId] = issue2;

        // 5. Default User Profile
        var defaultUser = new UserProfile
        {
            UserId = "user-dhanu",
            FullName = "Dhanu Peter",
            Email = "dhanupeter@gmail.com",
            PhoneNumber = "+91 98765 43210",
            PhotoUrl = "https://images.unsplash.com/photo-1535713875002-d1d0cf377fde?w=200&auto=format&fit=crop&q=80",
            ReputationScore = 480,
            VerifiedReviewsCount = 38,
            HelpfulVotesCount = 246,
            Badges = new() { "🏆 Trusted Reviewer", "📍 Local Explorer", "🛡️ Verified Buyer", "📷 Top Contributor" },
            ReviewsByCategory = new()
            {
                ["Places"] = 18,
                ["Products"] = 10,
                ["Services"] = 6,
                ["Digital"] = 3,
                ["Public Issues"] = 1
            }
        };

        _users[defaultUser.UserId] = defaultUser;
    }

    public Task<List<Entity>> GetAllEntitiesAsync(string? type = null, string? category = null)
    {
        var list = _entities.Values.AsEnumerable();
        if (!string.IsNullOrEmpty(type) && !type.Equals("ALL", StringComparison.OrdinalIgnoreCase))
        {
            list = list.Where(e => e.EntityType.Equals(type, StringComparison.OrdinalIgnoreCase));
        }
        if (!string.IsNullOrEmpty(category))
        {
            list = list.Where(e => e.Category.Contains(category, StringComparison.OrdinalIgnoreCase));
        }
        return Task.FromResult(list.OrderByDescending(e => e.OverallRating).ToList());
    }

    public Task<Entity?> GetEntityByIdAsync(string entityId)
    {
        if (_entities.TryGetValue(entityId, out var entity))
        {
            if (_reviewsByEntity.TryGetValue(entityId, out var reviews))
            {
                entity.RecentReviews = reviews.OrderByDescending(r => r.CreatedAt).ToList();
            }
            return Task.FromResult<Entity?>(entity);
        }
        return Task.FromResult<Entity?>(null);
    }

    public Task<List<Entity>> SearchEntitiesAsync(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return GetAllEntitiesAsync();
        }

        var lower = query.ToLowerInvariant();
        var results = _entities.Values
            .Where(e => e.Name.ToLowerInvariant().Contains(lower) ||
                        e.Category.ToLowerInvariant().Contains(lower) ||
                        e.EntityType.ToLowerInvariant().Contains(lower) ||
                        e.Location.ToLowerInvariant().Contains(lower))
            .OrderByDescending(e => e.OverallRating)
            .ToList();

        return Task.FromResult(results);
    }

    public Task<Entity> CreateEntityAsync(CreateEntityRequest request)
    {
        var entity = new Entity
        {
            EntityId = "ent-" + Guid.NewGuid().ToString("N")[..8],
            EntityType = request.EntityType.ToUpperInvariant(),
            Category = request.Category,
            Name = request.Name,
            Description = request.Description,
            Location = request.Location,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            ExternalUrl = request.ExternalUrl,
            ImageUrl = string.IsNullOrEmpty(request.ImageUrl) 
                ? "https://images.unsplash.com/photo-1512436991641-6745cdb1723f?w=600&auto=format&fit=crop&q=60" 
                : request.ImageUrl,
            OverallRating = 0.0,
            TotalReviews = 0,
            VerifiedReviews = 0,
            CreatedBy = request.CreatedBy,
            CreatedAt = DateTime.UtcNow
        };

        _entities[entity.EntityId] = entity;
        return Task.FromResult(entity);
    }

    public Task<List<RatingCriteria>> GetCriteriaByEntityTypeAsync(string entityType)
    {
        var key = entityType.ToUpperInvariant();
        if (_criteriaByEntityType.TryGetValue(key, out var list))
        {
            return Task.FromResult(list);
        }
        return Task.FromResult(_criteriaByEntityType["PRODUCT"]);
    }

    public Task<List<Review>> GetReviewsByEntityIdAsync(string entityId)
    {
        if (_reviewsByEntity.TryGetValue(entityId, out var reviews))
        {
            return Task.FromResult(reviews.OrderByDescending(r => r.CreatedAt).ToList());
        }
        return Task.FromResult(new List<Review>());
    }

    public Task<Review> AddReviewAsync(string entityId, CreateReviewRequest request)
    {
        var review = new Review
        {
            EntityId = entityId,
            ReviewId = "rev-" + Guid.NewGuid().ToString("N")[..8],
            UserId = request.UserId,
            UserName = request.UserName,
            UserPhotoUrl = request.UserPhotoUrl,
            OverallRating = request.OverallRating,
            Title = request.Title,
            ReviewText = request.ReviewText,
            ExperienceDate = request.ExperienceDate ?? DateTime.UtcNow,
            LocationId = request.LocationId,
            VerificationLevel = request.VerificationLevel,
            HelpfulVotes = 0,
            CreatedAt = DateTime.UtcNow,
            Ratings = request.CriteriaRatings.Select(c => new ReviewRatingItem
            {
                CriteriaId = c.CriteriaId,
                CriteriaName = c.CriteriaName,
                Score = c.Score
            }).ToList(),
            Media = request.MediaItems.Select(m => new ReviewMediaItem
            {
                MediaId = "m-" + Guid.NewGuid().ToString("N")[..8],
                MediaType = m.MediaType,
                StorageUrl = m.StorageUrl,
                ThumbnailUrl = m.ThumbnailUrl,
                Caption = m.Caption
            }).ToList()
        };

        var list = _reviewsByEntity.GetOrAdd(entityId, _ => new List<Review>());
        lock (list)
        {
            list.Add(review);
        }

        // Update Entity aggregate metrics
        if (_entities.TryGetValue(entityId, out var entity))
        {
            entity.TotalReviews = list.Count;
            entity.VerifiedReviews = list.Count(r => r.VerificationLevel > 0);
            entity.OverallRating = Math.Round(list.Average(r => r.OverallRating), 1);

            // Recompute criteria averages
            var allCriteriaRatings = list.SelectMany(r => r.Ratings).GroupBy(r => r.CriteriaName);
            foreach (var g in allCriteriaRatings)
            {
                entity.CriteriaAverages[g.Key] = Math.Round(g.Average(x => x.Score), 1);
            }
        }

        // Update user stats
        if (_users.TryGetValue(request.UserId, out var user))
        {
            user.ReputationScore += (request.VerificationLevel * 10) + 15;
            if (request.VerificationLevel > 0) user.VerifiedReviewsCount++;
        }

        return Task.FromResult(review);
    }

    public Task<bool> VoteHelpfulAsync(string entityId, string reviewId, string userId)
    {
        if (_reviewsByEntity.TryGetValue(entityId, out var reviews))
        {
            var target = reviews.FirstOrDefault(r => r.ReviewId == reviewId);
            if (target != null)
            {
                target.HelpfulVotes++;
                return Task.FromResult(true);
            }
        }
        return Task.FromResult(false);
    }

    public Task<List<Issue>> GetAllIssuesAsync(string? status = null, string? category = null)
    {
        var list = _issues.Values.AsEnumerable();
        if (!string.IsNullOrEmpty(status))
        {
            list = list.Where(i => i.Status.Equals(status, StringComparison.OrdinalIgnoreCase));
        }
        if (!string.IsNullOrEmpty(category))
        {
            list = list.Where(i => i.Category.Contains(category, StringComparison.OrdinalIgnoreCase));
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
            IssueId = "iss-" + Guid.NewGuid().ToString("N")[..8],
            Title = request.Title,
            Category = request.Category,
            Description = request.Description,
            Location = request.Location,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            ImageUrl = string.IsNullOrEmpty(request.ImageUrl)
                ? "https://images.unsplash.com/photo-1544724569-5f546fd6f2b5?w=600&auto=format&fit=crop&q=60"
                : request.ImageUrl,
            Status = "Open",
            ConfirmationsCount = 1,
            ReportedByUserId = request.ReportedByUserId,
            ReportedByUserName = request.ReportedByUserName,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _issues[issue.IssueId] = issue;
        return Task.FromResult(issue);
    }

    public Task<bool> ConfirmIssueAsync(string issueId, string userId)
    {
        if (_issues.TryGetValue(issueId, out var issue))
        {
            issue.ConfirmationsCount++;
            issue.UpdatedAt = DateTime.UtcNow;
            return Task.FromResult(true);
        }
        return Task.FromResult(false);
    }

    public Task<Issue?> UpdateIssueStatusAsync(string issueId, UpdateIssueStatusRequest request)
    {
        if (_issues.TryGetValue(issueId, out var issue))
        {
            issue.Status = request.Status;
            if (!string.IsNullOrEmpty(request.OfficialResponse))
            {
                issue.OfficialResponse = request.OfficialResponse;
            }
            if (!string.IsNullOrEmpty(request.RespondedBy))
            {
                issue.RespondedBy = request.RespondedBy;
            }
            issue.UpdatedAt = DateTime.UtcNow;
            return Task.FromResult<Issue?>(issue);
        }
        return Task.FromResult<Issue?>(null);
    }

    public Task<UserProfile> GetUserProfileAsync(string userId)
    {
        if (_users.TryGetValue(userId, out var user))
        {
            return Task.FromResult(user);
        }

        var newUser = new UserProfile
        {
            UserId = userId,
            FullName = "New Reviewer",
            Email = $"{userId}@ratingplatform.com",
            ReputationScore = 100,
            Badges = new() { "🌱 New Explorer" }
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
