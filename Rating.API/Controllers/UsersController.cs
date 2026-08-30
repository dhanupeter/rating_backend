using Microsoft.AspNetCore.Mvc;
using Rating.API.Models;
using Rating.API.Services;

namespace Rating.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly ISpannerService _spannerService;

    public UsersController(ISpannerService spannerService)
    {
        _spannerService = spannerService;
    }

    [HttpGet("{userId}")]
    public async Task<ActionResult<UserProfile>> GetProfile(string userId)
    {
        var profile = await _spannerService.GetUserProfileAsync(userId);
        return Ok(profile);
    }

    [HttpPost]
    public async Task<ActionResult<UserProfile>> CreateUser([FromBody] UserProfile profile)
    {
        if (string.IsNullOrEmpty(profile.UserId))
        {
            profile.UserId = Guid.NewGuid().ToString("N");
        }
        var created = await _spannerService.UpdateUserProfileAsync(profile);
        return CreatedAtAction(nameof(GetProfile), new { userId = created.UserId }, created);
    }

    [HttpPut("{userId}")]
    public async Task<ActionResult<UserProfile>> UpdateProfile(string userId, [FromBody] UserProfile profile)
    {
        profile.UserId = userId;
        var updated = await _spannerService.UpdateUserProfileAsync(profile);
        return Ok(updated);
    }
}
