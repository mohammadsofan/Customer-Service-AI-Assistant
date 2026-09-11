using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using AIEmployeeSupport.Application.DTOs.Common;
using AIEmployeeSupport.Application.DTOs.Support;
using AIEmployeeSupport.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AIEmployeeSupport.API.Controllers;

[ApiController]
[Route("api/support/questions")]
[Authorize(Policy = "EmployeeOrAdmin")]
public class SupportController : ControllerBase
{
    private readonly ISupportService _supportService;

    public SupportController(ISupportService supportService)
    {
        _supportService = supportService;
    }

    [HttpPost]
    public async Task<ActionResult<QuestionResponse>> SubmitQuestion([FromBody] SubmitQuestionRequest request, CancellationToken cancellationToken)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var employeeId))
        {
            return Unauthorized();
        }

        var response = await _supportService.SubmitQuestionAsync(employeeId, request, cancellationToken);
        return Ok(response);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<QuestionResponse>> GetQuestionById(Guid id, CancellationToken cancellationToken)
    {
        var response = await _supportService.GetQuestionByIdAsync(id, cancellationToken);
        if (response == null)
        {
            return NotFound();
        }
        return Ok(response);
    }

    [HttpGet("history")]
    public async Task<ActionResult<PaginatedResponse<QuestionHistoryDto>>> GetQuestionHistory([FromQuery] PaginatedRequest request, CancellationToken cancellationToken)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var employeeId))
        {
            return Unauthorized();
        }

        var response = await _supportService.GetQuestionHistoryAsync(employeeId, request, cancellationToken);
        return Ok(response);
    }
}
