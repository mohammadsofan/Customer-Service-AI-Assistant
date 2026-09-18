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
[Route("api/ai/models")]
[Authorize(Policy = "AdminOnly")]
public class AIModelsController : ControllerBase
{
    private readonly IAIModelService _modelService;

    public AIModelsController(IAIModelService modelService)
    {
        _modelService = modelService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] Guid? providerId, CancellationToken cancellationToken)
    {
        if (providerId.HasValue)
        {
            var providerModels = await _modelService.GetByProviderIdAsync(providerId.Value, cancellationToken);
            return Ok(providerModels);
        }
        var result = await _modelService.GetAllAsync(cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _modelService.GetByIdAsync(id, cancellationToken);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateAIModelRequest request, CancellationToken cancellationToken)
    {
        var result = await _modelService.CreateAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateAIModelRequest request, CancellationToken cancellationToken)
    {
        var result = await _modelService.UpdateAsync(id, request, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _modelService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
