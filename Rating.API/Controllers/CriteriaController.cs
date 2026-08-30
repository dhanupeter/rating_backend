using Microsoft.AspNetCore.Mvc;
using Rating.API.Models;
using Rating.API.Services;

namespace Rating.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CriteriaController : ControllerBase
{
    private readonly ISpannerService _spannerService;

    public CriteriaController(ISpannerService spannerService)
    {
        _spannerService = spannerService;
    }

    [HttpGet("{entityType}")]
    public async Task<ActionResult<List<RatingCriteria>>> GetCriteria(string entityType)
    {
        var criteria = await _spannerService.GetCriteriaByEntityTypeAsync(entityType);
        return Ok(criteria);
    }
}
