namespace Rating.API.Services;

public class GeocodeResult
{
    public string FormattedAddress { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string GoogleMapsUrl { get; set; } = string.Empty;
}

public interface ILocationService
{
    Task<GeocodeResult> ReverseGeocodeAsync(double lat, double lon);
    Task<List<GeocodeResult>> SearchPlacesAsync(string query);
    string GetGoogleMapsNavigationUrl(double lat, double lon, string placeName);
    string GetGoogleMapsSearchUrl(string query);
}
