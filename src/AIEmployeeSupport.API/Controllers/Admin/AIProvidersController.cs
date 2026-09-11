using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AIEmployeeSupport.Application.DTOs.AI;
using AIEmployeeSupport.Application.Interfaces.Services;

namespace AIEmployeeSupport.API.Controllers.Admin;

[ApiController]
[Route("api/admin/[controller]")]
[Route("api/ai/providers")]
[Authorize(Policy = "AdminOnly")]
public class AIProvidersController : ControllerBase
{
    private readonly IAIProviderService _providerService;

    public AIProvidersController(IAIProviderService providerService)
    {
        _providerService = providerService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _providerService.GetAllAsync(cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _providerService.GetByIdAsync(id, cancellationToken);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateAIProviderRequest request, CancellationToken cancellationToken)
    {
        var result = await _providerService.CreateAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateAIProviderRequest request, CancellationToken cancellationToken)
    {
        var result = await _providerService.UpdateAsync(id, request, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _providerService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id}/test")]
    public async Task<IActionResult> TestConnection(Guid id, CancellationToken cancellationToken)
    {
        var result = await _providerService.TestConnectionAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpPatch("{id}/activate")]
    public async Task<IActionResult> Activate(Guid id, CancellationToken cancellationToken)
    {
        await _providerService.ActivateAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpPatch("{id}/deactivate")]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken cancellationToken)
    {
        await _providerService.DeactivateAsync(id, cancellationToken);
        return NoContent();
    }
}
