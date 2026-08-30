using Microsoft.AspNetCore.Mvc;
using Rating.API.DTOs;
using Rating.API.Models;
using Rating.API.Services;

namespace Rating.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EntitiesController : ControllerBase
{
    private readonly ISpannerService _spannerService;
    private readonly ILogger<EntitiesController> _logger;

    public EntitiesController(ISpannerService spannerService, ILogger<EntitiesController> logger)
    {
        _spannerService = spannerService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<List<Entity>>> GetAll([FromQuery] string? type, [FromQuery] string? category)
    {
        var entities = await _spannerService.GetAllEntitiesAsync(type, category);
        return Ok(entities);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Entity>> GetById(string id)
    {
        var entity = await _spannerService.GetEntityByIdAsync(id);
        if (entity == null) return NotFound(new { message = $"Entity {id} not found" });
        return Ok(entity);
    }

    [HttpGet("search")]
    public async Task<ActionResult<List<Entity>>> Search([FromQuery] string q)
    {
        var results = await _spannerService.SearchEntitiesAsync(q);
        return Ok(results);
    }

    [HttpPost]
    public async Task<ActionResult<Entity>> Create([FromBody] CreateEntityRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(new { message = "Entity Name is required" });
        }

        var entity = await _spannerService.CreateEntityAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = entity.EntityId }, entity);
    }
}
