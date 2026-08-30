using Microsoft.AspNetCore.Mvc;
using Rating.API.DTOs;
using Rating.API.Models;
using Rating.API.Services;

namespace Rating.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class IssuesController : ControllerBase
{
    private readonly ISpannerService _spannerService;

    public IssuesController(ISpannerService spannerService)
    {
        _spannerService = spannerService;
    }

    [HttpGet]
    public async Task<ActionResult<List<Issue>>> GetAll([FromQuery] string? status, [FromQuery] string? category)
    {
        var issues = await _spannerService.GetAllIssuesAsync(status, category);
        return Ok(issues);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Issue>> GetById(string id)
    {
        var issue = await _spannerService.GetIssueByIdAsync(id);
        if (issue == null) return NotFound(new { message = $"Issue {id} not found" });
        return Ok(issue);
    }

    [HttpPost]
    public async Task<ActionResult<Issue>> Create([FromBody] CreateIssueRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Title) || string.IsNullOrWhiteSpace(request.Description))
        {
            return BadRequest(new { message = "Title and Description are required" });
        }

        var issue = await _spannerService.CreateIssueAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = issue.IssueId }, issue);
    }

    [HttpPost("{id}/confirm")]
    public async Task<ActionResult> Confirm(string id, [FromQuery] string userId)
    {
        var success = await _spannerService.ConfirmIssueAsync(id, userId);
        if (!success) return NotFound(new { message = "Issue not found" });
        return Ok(new { success = true });
    }

    [HttpPatch("{id}/status")]
    public async Task<ActionResult<Issue>> UpdateStatus(string id, [FromBody] UpdateIssueStatusRequest request)
    {
        var updated = await _spannerService.UpdateIssueStatusAsync(id, request);
        if (updated == null) return NotFound(new { message = "Issue not found" });
        return Ok(updated);
    }
}
