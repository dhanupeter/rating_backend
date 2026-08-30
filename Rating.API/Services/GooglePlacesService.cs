using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Nodes;
using Rating.API.Models;

namespace Rating.API.Services;

public class GooglePlacesService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly ILogger<GooglePlacesService> _logger;

    public GooglePlacesService(IConfiguration configuration, ILogger<GooglePlacesService> logger, HttpClient httpClient)
    {
        _logger = logger;
        _httpClient = httpClient;
        _apiKey = configuration["GoogleMaps:ApiKey"] ?? "AIzaSyDtvXi81OUxvl_cuTRiyVwiHSNQc1wUDKo";
    }

    public async Task<List<Entity>> SearchNearbyAsync(double lat, double lng, int radiusMeters = 5000, string? category = null)
    {
        var entities = new List<Entity>();

        try
        {
            var requestUrl = "https://places.googleapis.com/v1/places:searchNearby";

            var includedTypes = new List<string>();
            if (string.IsNullOrEmpty(category) || category.Equals("ALL", StringComparison.OrdinalIgnoreCase))
            {
                includedTypes.AddRange(new[] { "electrician", "restaurant", "supermarket", "car_repair", "hospital", "cafe", "store" });
            }
            else if (category.Equals("SERVICE", StringComparison.OrdinalIgnoreCase) || category.Contains("electric", StringComparison.OrdinalIgnoreCase))
            {
                includedTypes.AddRange(new[] { "electrician", "car_repair", "hardware_store", "home_improvement_store" });
            }
            else if (category.Equals("PLACE", StringComparison.OrdinalIgnoreCase))
            {
                includedTypes.AddRange(new[] { "restaurant", "cafe", "supermarket", "shopping_mall", "bakery" });
            }
            else if (category.Equals("PUBLIC", StringComparison.OrdinalIgnoreCase))
            {
                includedTypes.AddRange(new[] { "hospital", "pharmacy", "post_office", "city_hall", "bank" });
            }
            else
            {
                includedTypes.Add(category.ToLowerInvariant());
            }

            var requestBody = new
            {
                includedTypes = includedTypes,
                maxResultCount = 20,
                locationRestriction = new
                {
                    circle = new
                    {
                        center = new { latitude = lat, longitude = lng },
                        radius = (double)Math.Min(radiusMeters, 50000)
                    }
                }
            };

            using var req = new HttpRequestMessage(HttpMethod.Post, requestUrl);
            req.Headers.Add("X-Goog-Api-Key", _apiKey);
            req.Headers.Add("X-Goog-FieldMask", "places.id,places.displayName,places.formattedAddress,places.location,places.primaryType,places.types,places.rating,places.userRatingCount,places.googleMapsUri");
            req.Content = new StringContent(JsonSerializer.Serialize(requestBody), System.Text.Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(req);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var json = JsonNode.Parse(content);
                var places = json?["places"]?.AsArray();

                if (places != null)
                {
                    foreach (var p in places)
                    {
                        var placeId = p?["id"]?.ToString() ?? Guid.NewGuid().ToString("N");
                        var name = p?["displayName"]?["text"]?.ToString() ?? "Local Place";
                        var address = p?["formattedAddress"]?.ToString() ?? "";
                        var placeLat = p?["location"]?["latitude"]?.GetValue<double>() ?? lat;
                        var placeLng = p?["location"]?["longitude"]?.GetValue<double>() ?? lng;
                        var primaryType = p?["primaryType"]?.ToString() ?? "store";
                        var rating = p?["rating"]?.GetValue<double>() ?? 4.5;
                        var totalReviews = p?["userRatingCount"]?.GetValue<int>() ?? 42;

                        var displayCategory = primaryType.Replace("_", " ").ToUpperInvariant();
                        if (displayCategory.Contains("ELECTRIC")) displayCategory = "⚡ ELECTRICAL SERVICES";
                        else if (displayCategory.Contains("RESTAURANT")) displayCategory = "🍴 DINING & RESTAURANT";
                        else if (displayCategory.Contains("CAR")) displayCategory = "🔧 AUTO & WORKSHOP";
                        else if (displayCategory.Contains("HOSPITAL")) displayCategory = "🏥 HEALTHCARE";

                        // Assign high quality imagery
                        string photo = "https://images.unsplash.com/photo-1581092160607-ee22621dd758?w=600"; // Electrical / workshop
                        if (primaryType.Contains("restaurant") || primaryType.Contains("cafe"))
                            photo = "https://images.unsplash.com/photo-1517248135467-4c7edcad34c4?w=600";
                        else if (primaryType.Contains("supermarket") || primaryType.Contains("store"))
                            photo = "https://images.unsplash.com/photo-1578916171728-46686eac8d58?w=600";
                        else if (primaryType.Contains("car") || primaryType.Contains("repair"))
                            photo = "https://images.unsplash.com/photo-1486006920555-c77dce18193b?w=600";

                        entities.Add(new Entity
                        {
                            EntityId = $"google-{placeId}",
                            EntityType = "PLACE",
                            Category = displayCategory,
                            Name = name,
                            Description = address,
                            ImageUrl = photo,
                            OverallRating = Math.Round(rating, 1),
                            TotalReviews = totalReviews,
                            VerifiedReviews = (int)(totalReviews * 0.8),
                            ExternalProvider = "GOOGLE",
                            Locations = new List<EntityLocation>
                            {
                                new EntityLocation
                                {
                                    EntityId = $"google-{placeId}",
                                    LocationId = $"loc-{placeId}",
                                    Name = name,
                                    AddressLine1 = address,
                                    Latitude = placeLat,
                                    Longitude = placeLng,
                                    IsPrimary = true
                                }
                            }
                        });
                    }
                }
            }
            else
            {
                _logger.LogWarning("Google Places Nearby search returned {StatusCode}", response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching nearby places from Google Places API");
        }

        return entities;
    }
}
