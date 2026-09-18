using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using AIEmployeeSupport.Application.DTOs.Common;
using AIEmployeeSupport.Application.DTOs.Support;
using AIEmployeeSupport.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

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
    [EnableRateLimiting("SupportRateLimit")]
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
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var currentUserId))
        {
            return Unauthorized();
        }

        var isAdmin = User.IsInRole("Administrator");
        var response = await _supportService.GetQuestionByIdAsync(id, currentUserId, isAdmin, cancellationToken);
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

    [HttpGet("top-scenarios")]
    public async Task<ActionResult<IEnumerable<TopScenarioDto>>> GetTopScenarios([FromQuery] int count = 5, CancellationToken cancellationToken = default)
    {
        var scenarios = await _supportService.GetTopScenariosAsync(count, cancellationToken);
        return Ok(scenarios);
    }

    [HttpGet("audit")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> GetAuditLogs([FromServices] AIEmployeeSupport.Infrastructure.Persistence.ApplicationDbContext context, [FromQuery] int count = 10, CancellationToken cancellationToken = default)
    {
        var logs = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.ToListAsync(
            System.Linq.Queryable.Take(
                System.Linq.Queryable.OrderByDescending(
                    context.SupportQuestions, q => q.CreatedAt
                ), count
            ), cancellationToken
        );
        return Ok(logs.Select(q => new {
            q.QuestionText,
            q.Status,
            q.ScenarioId,
            q.MathTopScenarioId,
            q.RerankedScenarioId,
            q.RerankingUsed,
            q.RerankingFailed,
            q.RerankingNoMatch,
            q.CreatedAt
        }));
    }
}
