using System.Text.Json;

namespace Rating.API.Services;

public class GeoapifyLocationService : ILocationService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<GeoapifyLocationService> _logger;
    private readonly string _apiKey;

    public GeoapifyLocationService(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<GeoapifyLocationService> logger)
    {
        _httpClient = httpClientFactory.CreateClient();
        _configuration = configuration;
        _logger = logger;
        _apiKey = _configuration["Geoapify:ApiKey"] ?? "d6a3b2b7352345d398e8dfb153b6d268"; // Default dev key
    }

    public async Task<GeocodeResult> ReverseGeocodeAsync(double lat, double lon)
    {
        try
        {
            var url = $"https://api.geoapify.com/v1/geocode/reverse?lat={lat}&lon={lon}&apiKey={_apiKey}";
            var response = await _httpClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);
                var features = doc.RootElement.GetProperty("features");
                if (features.GetArrayLength() > 0)
                {
                    var props = features[0].GetProperty("properties");
                    var formatted = props.TryGetProperty("formatted", out var f) ? f.GetString() : $"{lat}, {lon}";
                    var city = props.TryGetProperty("city", out var c) ? c.GetString() : "Tiruppur";
                    var state = props.TryGetProperty("state", out var s) ? s.GetString() : "Tamil Nadu";
                    var country = props.TryGetProperty("country", out var cnt) ? cnt.GetString() : "India";

                    return new GeocodeResult
                    {
                        FormattedAddress = formatted ?? $"{lat}, {lon}",
                        City = city ?? "Tiruppur",
                        State = state ?? "Tamil Nadu",
                        Country = country ?? "India",
                        Latitude = lat,
                        Longitude = lon,
                        GoogleMapsUrl = GetGoogleMapsNavigationUrl(lat, lon, formatted ?? "Location")
                    };
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Geoapify reverse geocode failed, using local resolver");
        }

        return new GeocodeResult
        {
            FormattedAddress = "Tiruppur, Tamil Nadu, India",
            City = "Tiruppur",
            State = "Tamil Nadu",
            Country = "India",
            Latitude = lat,
            Longitude = lon,
            GoogleMapsUrl = GetGoogleMapsNavigationUrl(lat, lon, "Tiruppur")
        };
    }

    public async Task<List<GeocodeResult>> SearchPlacesAsync(string query)
    {
        var list = new List<GeocodeResult>();
        try
        {
            var encoded = Uri.EscapeDataString(query);
            var url = $"https://api.geoapify.com/v1/geocode/autocomplete?text={encoded}&filter=countrycode:in&apiKey={_apiKey}";
            var response = await _httpClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);
                var features = doc.RootElement.GetProperty("features");
                foreach (var item in features.EnumerateArray())
                {
                    var props = item.GetProperty("properties");
                    var formatted = props.TryGetProperty("formatted", out var f) ? f.GetString() ?? "" : "";
                    var lat = props.TryGetProperty("lat", out var lt) ? lt.GetDouble() : 11.1085;
                    var lon = props.TryGetProperty("lon", out var ln) ? ln.GetDouble() : 77.3411;
                    var city = props.TryGetProperty("city", out var c) ? c.GetString() ?? "" : "";
                    var state = props.TryGetProperty("state", out var s) ? s.GetString() ?? "" : "";

                    list.Add(new GeocodeResult
                    {
                        FormattedAddress = formatted,
                        City = city,
                        State = state,
                        Country = "India",
                        Latitude = lat,
                        Longitude = lon,
                        GoogleMapsUrl = GetGoogleMapsNavigationUrl(lat, lon, formatted)
                    });
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Geoapify search failed");
        }

        if (list.Count == 0)
        {
            list.Add(new GeocodeResult
            {
                FormattedAddress = $"{query}, Tiruppur, Tamil Nadu",
                City = "Tiruppur",
                State = "Tamil Nadu",
                Country = "India",
                Latitude = 11.1085,
                Longitude = 77.3411,
                GoogleMapsUrl = GetGoogleMapsSearchUrl(query)
            });
        }

        return list;
    }

    public string GetGoogleMapsNavigationUrl(double lat, double lon, string placeName)
    {
        var encodedName = Uri.EscapeDataString(placeName);
        return $"https://www.google.com/maps/search/?api=1&query={lat},{lon}+({encodedName})";
    }

    public string GetGoogleMapsSearchUrl(string query)
    {
        var encoded = Uri.EscapeDataString(query);
        return $"https://www.google.com/maps/search/?api=1&query={encoded}";
    }
}
