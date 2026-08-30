using Microsoft.AspNetCore.Mvc;
using Rating.API.DTOs;
using Rating.API.Models;
using Rating.API.Services;

namespace Rating.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReviewsController : ControllerBase
{
    private readonly ISpannerService _spannerService;
    private readonly ILogger<ReviewsController> _logger;

    public ReviewsController(ISpannerService spannerService, ILogger<ReviewsController> logger)
    {
        _spannerService = spannerService;
        _logger = logger;
    }

    [HttpGet("entity/{entityId}")]
    public async Task<ActionResult<List<Review>>> GetByEntity(string entityId)
    {
        var reviews = await _spannerService.GetReviewsByEntityIdAsync(entityId);
        return Ok(reviews);
    }

    [HttpPost("entity/{entityId}")]
    public async Task<ActionResult<Review>> CreateReview(string entityId, [FromBody] CreateReviewRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.UserId))
        {
            return BadRequest(new { message = "UserId is required" });
        }

        var review = await _spannerService.AddReviewAsync(entityId, request);
        return Ok(review);
    }

    [HttpPost("entity/{entityId}/{reviewId}/helpful")]
    public async Task<ActionResult> VoteHelpful(string entityId, string reviewId, [FromQuery] string userId)
    {
        var success = await _spannerService.VoteHelpfulAsync(entityId, reviewId, userId);
        if (!success) return NotFound(new { message = "Review not found" });
        return Ok(new { success = true });
    }
}
