using Microsoft.AspNetCore.Mvc;
using Rating.API.Services;

namespace Rating.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LocationController : ControllerBase
{
    private readonly ILocationService _locationService;

    public LocationController(ILocationService locationService)
    {
        _locationService = locationService;
    }

    [HttpGet("reverse")]
    public async Task<ActionResult<GeocodeResult>> ReverseGeocode([FromQuery] double lat, [FromQuery] double lon)
    {
        var result = await _locationService.ReverseGeocodeAsync(lat, lon);
        return Ok(result);
    }

    [HttpGet("search")]
    public async Task<ActionResult<List<GeocodeResult>>> SearchPlaces([FromQuery] string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return BadRequest(new { message = "Query is required" });
        }

        var results = await _locationService.SearchPlacesAsync(query);
        return Ok(results);
    }

    [HttpGet("maps-url")]
    public ActionResult<object> GetGoogleMapsUrl([FromQuery] double lat, [FromQuery] double lon, [FromQuery] string name)
    {
        var navUrl = _locationService.GetGoogleMapsNavigationUrl(lat, lon, name);
        return Ok(new { googleMapsUrl = navUrl });
    }
}
