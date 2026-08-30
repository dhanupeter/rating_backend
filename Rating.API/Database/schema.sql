-- =========================================================================
-- Google Cloud Spanner Schema (Google Standard SQL)
-- Database: rating | Project: event-506117
-- =========================================================================

-- 1. User Profiles
CREATE TABLE UserProfiles (
    UserId STRING(64) NOT NULL,
    FullName STRING(255) NOT NULL,
    Email STRING(255),
    PhoneNumber STRING(32),
    PhotoUrl STRING(1024),
    ReputationScore INT64 NOT NULL DEFAULT (100),
    VerifiedReviewsCount INT64 NOT NULL DEFAULT (0),
    HelpfulVotesCount INT64 NOT NULL DEFAULT (0),
    Badges JSON,
    CreatedAt TIMESTAMP NOT NULL OPTIONS (allow_commit_timestamp=true)
) PRIMARY KEY (UserId);

-- 2. Entities Table (Universal: PRODUCT, PLACE, SERVICE, DIGITAL, PUBLIC)
CREATE TABLE Entities (
    EntityId STRING(64) NOT NULL,
    EntityType STRING(32) NOT NULL, -- PRODUCT, PLACE, SERVICE, DIGITAL, PUBLIC
    Category STRING(64) NOT NULL,   -- e.g. Headphones, Restaurant, Workshop, Mobile App, EB Office
    Name STRING(255) NOT NULL,
    Description STRING(MAX),
    Location STRING(255),          -- e.g. "Tiruppur, Tamil Nadu"
    Latitude FLOAT64,
    Longitude FLOAT64,
    ExternalUrl STRING(1024),
    ImageUrl STRING(1024),
    OverallRating FLOAT64 NOT NULL DEFAULT (0.0),
    TotalReviews INT64 NOT NULL DEFAULT (0),
    VerifiedReviews INT64 NOT NULL DEFAULT (0),
    CreatedBy STRING(64),
    CreatedAt TIMESTAMP NOT NULL OPTIONS (allow_commit_timestamp=true)
) PRIMARY KEY (EntityId);

-- 3. Dynamic Rating Templates & Criteria
CREATE TABLE RatingCriteria (
    CriteriaId STRING(64) NOT NULL,
    EntityType STRING(32) NOT NULL, -- PRODUCT, PLACE, SERVICE, DIGITAL, PUBLIC
    Name STRING(64) NOT NULL,       -- Quality, Food, Service, Cleanliness, Waiting Time, Gameplay, etc.
    IconName STRING(64) NOT NULL,
    Weight FLOAT64 NOT NULL DEFAULT (1.0),
    DisplayOrder INT64 NOT NULL DEFAULT (0)
) PRIMARY KEY (CriteriaId);

-- 4. Reviews Table (Interleaved in Entities for extreme co-location performance)
CREATE TABLE Reviews (
    EntityId STRING(64) NOT NULL,
    ReviewId STRING(64) NOT NULL,
    UserId STRING(64) NOT NULL,
    UserName STRING(255) NOT NULL,
    UserPhotoUrl STRING(1024),
    OverallRating FLOAT64 NOT NULL,
    Title STRING(255),
    ReviewText STRING(MAX),
    ExperienceDate DATE,
    LocationId STRING(255),
    VerificationLevel INT64 NOT NULL DEFAULT (0), -- 0: Basic, 1: Photo, 2: Location/Visit, 3: Invoice/Receipt, 4: Highly Verified
    HelpfulVotes INT64 NOT NULL DEFAULT (0),
    CreatedAt TIMESTAMP NOT NULL OPTIONS (allow_commit_timestamp=true)
) PRIMARY KEY (EntityId, ReviewId),
  INTERLEAVE IN PARENT Entities ON DELETE CASCADE;

-- 5. Dynamic Review Rating Breakdown
CREATE TABLE ReviewRatings (
    EntityId STRING(64) NOT NULL,
    ReviewId STRING(64) NOT NULL,
    CriteriaId STRING(64) NOT NULL,
    CriteriaName STRING(64) NOT NULL,
    Score FLOAT64 NOT NULL
) PRIMARY KEY (EntityId, ReviewId, CriteriaId),
  INTERLEAVE IN PARENT Reviews ON DELETE CASCADE;

-- 6. Review Media / Evidence (Interleaved in Reviews)
CREATE TABLE ReviewMedia (
    EntityId STRING(64) NOT NULL,
    ReviewId STRING(64) NOT NULL,
    MediaId STRING(64) NOT NULL,
    MediaType STRING(32) NOT NULL, -- PHOTO, VIDEO, RECEIPT
    StorageUrl STRING(1024) NOT NULL,
    ThumbnailUrl STRING(1024),
    Caption STRING(255),
    CreatedAt TIMESTAMP NOT NULL OPTIONS (allow_commit_timestamp=true)
) PRIMARY KEY (EntityId, ReviewId, MediaId),
  INTERLEAVE IN PARENT Reviews ON DELETE CASCADE;

-- 7. Community Helpful Votes
CREATE TABLE ReviewHelpfulVotes (
    EntityId STRING(64) NOT NULL,
    ReviewId STRING(64) NOT NULL,
    UserId STRING(64) NOT NULL,
    VotedAt TIMESTAMP NOT NULL OPTIONS (allow_commit_timestamp=true)
) PRIMARY KEY (EntityId, ReviewId, UserId),
  INTERLEAVE IN PARENT Reviews ON DELETE CASCADE;

-- 8. Civic & Local Issues
CREATE TABLE Issues (
    IssueId STRING(64) NOT NULL,
    Title STRING(255) NOT NULL,
    Category STRING(64) NOT NULL,   -- Electrical Safety, Road & Potholes, Water Supply, Streetlight, Public Facility
    Description STRING(MAX) NOT NULL,
    Location STRING(255) NOT NULL,
    Latitude FLOAT64,
    Longitude FLOAT64,
    ImageUrl STRING(1024),
    Status STRING(32) NOT NULL DEFAULT ('Open'), -- Open, Under Review, Response Received, Resolved
    ConfirmationsCount INT64 NOT NULL DEFAULT (1),
    ReportedByUserId STRING(64) NOT NULL,
    ReportedByUserName STRING(255) NOT NULL,
    OfficialResponse STRING(MAX),
    RespondedBy STRING(255),
    CreatedAt TIMESTAMP NOT NULL OPTIONS (allow_commit_timestamp=true),
    UpdatedAt TIMESTAMP NOT NULL OPTIONS (allow_commit_timestamp=true)
) PRIMARY KEY (IssueId);

-- 9. Issue Community Confirmations
CREATE TABLE IssueConfirmations (
    IssueId STRING(64) NOT NULL,
    UserId STRING(64) NOT NULL,
    ConfirmedAt TIMESTAMP NOT NULL OPTIONS (allow_commit_timestamp=true)
) PRIMARY KEY (IssueId, UserId),
  INTERLEAVE IN PARENT Issues ON DELETE CASCADE;
