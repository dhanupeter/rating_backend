-- =========================================================================
-- RATE ANYTHING — UNIVERSAL REPUTATION & EXPERIENCE PLATFORM SCHEMA
-- Dialect: Google Cloud Spanner (Google Standard SQL)
-- Project: event-506117 | Instance: event-spanner | Database: rating
-- =========================================================================

-- 1. User Profiles & Identity
CREATE TABLE UserProfiles (
    UserId STRING(64) NOT NULL,
    FirebaseUid STRING(128),
    FullName STRING(255) NOT NULL,
    Email STRING(255),
    PhoneNumber STRING(32),
    PhotoUrl STRING(1024),
    ReputationScore INT64 NOT NULL DEFAULT (100),
    VerifiedReviewsCount INT64 NOT NULL DEFAULT (0),
    HelpfulVotesCount INT64 NOT NULL DEFAULT (0),
    Badges JSON,
    IsVerified BOOL NOT NULL DEFAULT (FALSE),
    CreatedAt TIMESTAMP NOT NULL OPTIONS (allow_commit_timestamp=true),
    UpdatedAt TIMESTAMP NOT NULL OPTIONS (allow_commit_timestamp=true)
) PRIMARY KEY (UserId);

-- 2. User Devices & FCM Push Tokens
CREATE TABLE UserDevices (
    UserId STRING(64) NOT NULL,
    DeviceId STRING(128) NOT NULL,
    FcmToken STRING(4096) NOT NULL,
    Platform STRING(32), -- Android, iOS, Web
    AppVersion STRING(32),
    IsActive BOOL NOT NULL DEFAULT (TRUE),
    LastSeenAt TIMESTAMP,
    CreatedAt TIMESTAMP NOT NULL OPTIONS (allow_commit_timestamp=true)
) PRIMARY KEY (UserId, DeviceId),
  INTERLEAVE IN PARENT UserProfiles ON DELETE CASCADE;

-- 3. User Reputation Event Audit Log
CREATE TABLE UserReputationEvents (
    UserId STRING(64) NOT NULL,
    EventId STRING(64) NOT NULL,
    EventType STRING(64) NOT NULL, -- REVIEW_SUBMITTED (+5), VERIFIED_REVIEW (+15), HELPFUL_VOTE (+1), FALSE_REPORT (-20), SPAM_DETECTED (-30)
    PointsChanged INT64 NOT NULL,
    ReferenceType STRING(64),     -- REVIEW, ISSUE, VOTE, REPORT
    ReferenceId STRING(64),
    Description STRING(255),
    CreatedAt TIMESTAMP NOT NULL OPTIONS (allow_commit_timestamp=true)
) PRIMARY KEY (UserId, EventId),
  INTERLEAVE IN PARENT UserProfiles ON DELETE CASCADE;

-- 4. Universal Entities (PRODUCT, PLACE, SERVICE, DIGITAL, PUBLIC)
CREATE TABLE Entities (
    EntityId STRING(64) NOT NULL,
    EntityType STRING(32) NOT NULL,   -- PRODUCT, PLACE, SERVICE, DIGITAL, PUBLIC
    Category STRING(64) NOT NULL,     -- Headphones, Supermarket, Restaurant, Workshop, Mobile App, EB Office
    Name STRING(255) NOT NULL,
    Brand STRING(128),                -- e.g. "boAt", "Samsung", "Sony"
    Model STRING(128),                -- e.g. "Rockerz 450"
    Description STRING(MAX),
    ExternalUrl STRING(1024),         -- Amazon/Flipkart/Official Website
    ExternalProvider STRING(64),      -- Geoapify, OpenStreetMap, Amazon, Google
    ExternalPlaceId STRING(255),      -- Geoapify/OSM Place ID
    ImageUrl STRING(1024),
    OverallRating FLOAT64 NOT NULL DEFAULT (0.0),
    TotalReviews INT64 NOT NULL DEFAULT (0),
    VerifiedReviews INT64 NOT NULL DEFAULT (0),
    CreatedBy STRING(64),
    CreatedAt TIMESTAMP NOT NULL OPTIONS (allow_commit_timestamp=true),
    UpdatedAt TIMESTAMP NOT NULL OPTIONS (allow_commit_timestamp=true)
) PRIMARY KEY (EntityId);

-- 5. Entity Physical Locations / Branches (Interleaved in Entities)
CREATE TABLE EntityLocations (
    EntityId STRING(64) NOT NULL,
    LocationId STRING(64) NOT NULL,
    Name STRING(255),                 -- e.g. "Saravana Bhavan - Tiruppur Branch"
    AddressLine1 STRING(255),
    AddressLine2 STRING(255),
    City STRING(128),
    State STRING(128),
    Country STRING(128),
    PostalCode STRING(32),
    Latitude FLOAT64,
    Longitude FLOAT64,
    ExternalProvider STRING(64),
    ExternalPlaceId STRING(255),
    IsPrimary BOOL NOT NULL DEFAULT (TRUE),
    CreatedAt TIMESTAMP NOT NULL OPTIONS (allow_commit_timestamp=true)
) PRIMARY KEY (EntityId, LocationId),
  INTERLEAVE IN PARENT Entities ON DELETE CASCADE;

-- 6. Dynamic Rating Criteria Templates
CREATE TABLE RatingCriteria (
    CriteriaId STRING(64) NOT NULL,
    EntityType STRING(32) NOT NULL,   -- PRODUCT, PLACE, SERVICE, DIGITAL, PUBLIC
    Name STRING(64) NOT NULL,         -- Build Quality, Food Taste, Turnaround Time, Gameplay, Staff Courtesy
    IconName STRING(64) NOT NULL,
    Weight FLOAT64 NOT NULL DEFAULT (1.0),
    DisplayOrder INT64 NOT NULL DEFAULT (0)
) PRIMARY KEY (CriteriaId);

-- 7. User Experiences (Separates the physical occurrence from the review itself)
CREATE TABLE Experiences (
    ExperienceId STRING(64) NOT NULL,
    UserId STRING(64) NOT NULL,
    EntityId STRING(64) NOT NULL,
    LocationId STRING(64),
    ExperienceDate DATE,
    Latitude FLOAT64,
    Longitude FLOAT64,
    LocationAccuracyMeters FLOAT64,
    ExperienceType STRING(32) NOT NULL, -- IN_STORE_VISIT, ONLINE_PURCHASE, AT_HOME_SERVICE, DIGITAL_PLAY, CIVIC_VISIT
    CreatedAt TIMESTAMP NOT NULL OPTIONS (allow_commit_timestamp=true)
) PRIMARY KEY (ExperienceId);

-- 8. Reviews Table (Interleaved in Entities)
CREATE TABLE Reviews (
    EntityId STRING(64) NOT NULL,
    ReviewId STRING(64) NOT NULL,
    UserId STRING(64) NOT NULL,
    ExperienceId STRING(64),
    OverallRating FLOAT64 NOT NULL,
    Title STRING(255),
    ReviewText STRING(MAX),
    VerificationLevel INT64 NOT NULL DEFAULT (0), -- 0: Basic, 1: Photo, 2: Location/GPS, 3: Invoice/Receipt, 4: Strong Verified
    HelpfulVotes INT64 NOT NULL DEFAULT (0),
    IsModerated BOOL NOT NULL DEFAULT (FALSE),
    CreatedAt TIMESTAMP NOT NULL OPTIONS (allow_commit_timestamp=true)
) PRIMARY KEY (EntityId, ReviewId),
  INTERLEAVE IN PARENT Entities ON DELETE CASCADE;

-- 9. Dynamic Multi-Criteria Breakdown (Interleaved in Reviews)
CREATE TABLE ReviewRatings (
    EntityId STRING(64) NOT NULL,
    ReviewId STRING(64) NOT NULL,
    CriteriaId STRING(64) NOT NULL,
    CriteriaName STRING(64) NOT NULL,
    Score FLOAT64 NOT NULL
) PRIMARY KEY (EntityId, ReviewId, CriteriaId),
  INTERLEAVE IN PARENT Reviews ON DELETE CASCADE;

-- 10. Review Media & Proof Evidence (Interleaved in Reviews)
CREATE TABLE ReviewMedia (
    EntityId STRING(64) NOT NULL,
    ReviewId STRING(64) NOT NULL,
    MediaId STRING(64) NOT NULL,
    MediaType STRING(32) NOT NULL,     -- PHOTO, VIDEO, RECEIPT, GEO_SNAPSHOT
    StorageUrl STRING(1024) NOT NULL,
    ThumbnailUrl STRING(1024),
    Caption STRING(255),
    CreatedAt TIMESTAMP NOT NULL OPTIONS (allow_commit_timestamp=true)
) PRIMARY KEY (EntityId, ReviewId, MediaId),
  INTERLEAVE IN PARENT Reviews ON DELETE CASCADE;

-- 11. Review Community Helpful Votes (Interleaved in Reviews)
CREATE TABLE ReviewHelpfulVotes (
    EntityId STRING(64) NOT NULL,
    ReviewId STRING(64) NOT NULL,
    UserId STRING(64) NOT NULL,
    VotedAt TIMESTAMP NOT NULL OPTIONS (allow_commit_timestamp=true)
) PRIMARY KEY (EntityId, ReviewId, UserId),
  INTERLEAVE IN PARENT Reviews ON DELETE CASCADE;

-- 12. Review Moderation & Community Reports (Interleaved in Reviews)
CREATE TABLE ReviewReports (
    EntityId STRING(64) NOT NULL,
    ReviewId STRING(64) NOT NULL,
    ReportId STRING(64) NOT NULL,
    ReporterUserId STRING(64) NOT NULL,
    Reason STRING(64) NOT NULL,        -- SPAM, HARASSMENT, FALSE_INFO, PRIVATE_DATA, INAPPROPRIATE
    Details STRING(MAX),
    Status STRING(32) NOT NULL DEFAULT ('PENDING'), -- PENDING, INVESTIGATED, DISMISSED, ACTION_TAKEN
    CreatedAt TIMESTAMP NOT NULL OPTIONS (allow_commit_timestamp=true)
) PRIMARY KEY (EntityId, ReviewId, ReportId),
  INTERLEAVE IN PARENT Reviews ON DELETE CASCADE;

-- 13. Local Civic Issues Tracker
CREATE TABLE Issues (
    IssueId STRING(64) NOT NULL,
    Title STRING(255) NOT NULL,
    Category STRING(64) NOT NULL,     -- Electrical Safety, Road & Potholes, Water Supply, Streetlight, Public Facility
    Description STRING(MAX) NOT NULL,
    Location STRING(255) NOT NULL,
    Latitude FLOAT64,
    Longitude FLOAT64,
    Status STRING(32) NOT NULL DEFAULT ('Open'), -- Open, Under Review, Response Received, Resolved
    ConfirmationsCount INT64 NOT NULL DEFAULT (1),
    ReportedByUserId STRING(64) NOT NULL,
    CreatedAt TIMESTAMP NOT NULL OPTIONS (allow_commit_timestamp=true),
    UpdatedAt TIMESTAMP NOT NULL OPTIONS (allow_commit_timestamp=true)
) PRIMARY KEY (IssueId);

-- 14. Issue Media Attachments (Interleaved in Issues)
CREATE TABLE IssueMedia (
    IssueId STRING(64) NOT NULL,
    MediaId STRING(64) NOT NULL,
    MediaType STRING(32) NOT NULL,
    StorageUrl STRING(1024) NOT NULL,
    CreatedAt TIMESTAMP NOT NULL OPTIONS (allow_commit_timestamp=true)
) PRIMARY KEY (IssueId, MediaId),
  INTERLEAVE IN PARENT Issues ON DELETE CASCADE;

-- 15. Issue Confirmations (Interleaved in Issues)
CREATE TABLE IssueConfirmations (
    IssueId STRING(64) NOT NULL,
    UserId STRING(64) NOT NULL,
    ConfirmedAt TIMESTAMP NOT NULL OPTIONS (allow_commit_timestamp=true)
) PRIMARY KEY (IssueId, UserId),
  INTERLEAVE IN PARENT Issues ON DELETE CASCADE;

-- 16. Issue Status History Audit Trail (Interleaved in Issues)
CREATE TABLE IssueStatusHistory (
    IssueId STRING(64) NOT NULL,
    HistoryId STRING(64) NOT NULL,
    OldStatus STRING(32),
    NewStatus STRING(32) NOT NULL,
    AuthorityResponse STRING(MAX),
    UpdatedBy STRING(255) NOT NULL,
    CreatedAt TIMESTAMP NOT NULL OPTIONS (allow_commit_timestamp=true)
) PRIMARY KEY (IssueId, HistoryId),
  INTERLEAVE IN PARENT Issues ON DELETE CASCADE;

-- 17. In-App Notifications
CREATE TABLE Notifications (
    UserId STRING(64) NOT NULL,
    NotificationId STRING(64) NOT NULL,
    Type STRING(64) NOT NULL,         -- REVIEW_ALERT, HELPFUL_VOTE, ISSUE_STATUS, REPUTATION_TIER
    Title STRING(255) NOT NULL,
    Body STRING(MAX),
    ReferenceType STRING(64),
    ReferenceId STRING(64),
    IsRead BOOL NOT NULL DEFAULT (FALSE),
    CreatedAt TIMESTAMP NOT NULL OPTIONS (allow_commit_timestamp=true)
) PRIMARY KEY (UserId, NotificationId),
  INTERLEAVE IN PARENT UserProfiles ON DELETE CASCADE;
