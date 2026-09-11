using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AIEmployeeSupport.Application.DTOs.Common;
using AIEmployeeSupport.Application.DTOs.Knowledge;
using AIEmployeeSupport.Application.Interfaces.Services;
using AIEmployeeSupport.Domain.Enums;

namespace AIEmployeeSupport.API.Controllers.Admin;

[ApiController]
[Route("api/admin/[controller]")]
[Route("api/admin/scenarios")]
[Route("api/knowledge/scenarios")]
[Authorize(Policy = "AdminOnly")]
public class KnowledgeController : ControllerBase
{
    private readonly IKnowledgeService _knowledgeService;

    public KnowledgeController(IKnowledgeService knowledgeService)
    {
        _knowledgeService = knowledgeService;
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return string.IsNullOrEmpty(userIdClaim) ? Guid.Empty : Guid.Parse(userIdClaim);
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] PaginatedRequest request, [FromQuery] ScenarioStatus? status, [FromQuery] Guid? categoryId, CancellationToken cancellationToken)
    {
        var result = await _knowledgeService.GetScenariosAsync(request, status, categoryId, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _knowledgeService.GetScenarioByIdAsync(id, cancellationToken);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateScenarioRequest request, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var result = await _knowledgeService.CreateScenarioAsync(userId, request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateScenarioRequest request, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        await _knowledgeService.UpdateScenarioAsync(id, userId, request, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _knowledgeService.DeleteScenarioAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpPatch("{id}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] ScenarioStatus status, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        await _knowledgeService.UpdateStatusAsync(id, userId, status, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id}/reindex")]
    public async Task<IActionResult> Reindex(Guid id, CancellationToken cancellationToken)
    {
        await _knowledgeService.ReindexAsync(id, cancellationToken);
        return Accepted();
    }

    [HttpGet("{id}/versions")]
    public async Task<IActionResult> GetVersions(Guid id, CancellationToken cancellationToken)
    {
        var result = await _knowledgeService.GetVersionsAsync(id, cancellationToken);
        return Ok(result);
    }
}
