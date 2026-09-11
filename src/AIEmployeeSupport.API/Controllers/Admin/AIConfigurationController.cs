using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AIEmployeeSupport.Application.DTOs.AI;
using AIEmployeeSupport.Application.Interfaces.Services;

namespace AIEmployeeSupport.API.Controllers.Admin;

[ApiController]
[Route("api/admin/[controller]")]
[Route("api/ai/configuration")]
[Authorize(Policy = "AdminOnly")]
public class AIConfigurationController : ControllerBase
{
    private readonly IAIConfigurationService _configurationService;

    public AIConfigurationController(IAIConfigurationService configurationService)
    {
        _configurationService = configurationService;
    }

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var result = await _configurationService.GetAsync(cancellationToken);
        return Ok(result);
    }

    [HttpPut]
    [HttpPost]
    public async Task<IActionResult> Update([FromBody] UpdateAIConfigurationRequest request, CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userId = string.IsNullOrEmpty(userIdClaim) ? Guid.Empty : Guid.Parse(userIdClaim);
        var result = await _configurationService.UpdateAsync(userId, request, cancellationToken);
        return Ok(result);
    }
}
