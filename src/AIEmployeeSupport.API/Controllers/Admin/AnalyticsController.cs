using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AIEmployeeSupport.Application.Interfaces.Services;

namespace AIEmployeeSupport.API.Controllers.Admin;

[ApiController]
[Route("api/admin/[controller]")]
[Route("api/analytics")]
[Authorize(Policy = "AdminOnly")]
public class AnalyticsController : ControllerBase
{
    private readonly IAnalyticsService _analyticsService;

    public AnalyticsController(IAnalyticsService analyticsService)
    {
        _analyticsService = analyticsService;
    }

    [HttpGet("overview")]
    public async Task<IActionResult> GetOverview(CancellationToken cancellationToken)
    {
        var result = await _analyticsService.GetOverviewAsync(cancellationToken);
        return Ok(result);
    }

    [HttpGet("questions")]
    public async Task<IActionResult> GetQuestions([FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate, CancellationToken cancellationToken)
    {
        var result = await _analyticsService.GetQuestionAnalyticsAsync(fromDate, toDate, cancellationToken);
        return Ok(result);
    }

    [HttpGet("knowledge")]
    public async Task<IActionResult> GetKnowledge([FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default)
    {
        var result = await _analyticsService.GetKnowledgeAnalyticsAsync(page, pageSize, cancellationToken);
        return Ok(result);
    }

    [HttpGet("unanswered")]
    public async Task<IActionResult> GetUnanswered([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string sortOrder = "desc", CancellationToken cancellationToken = default)
    {
        var result = await _analyticsService.GetUnansweredAnalyticsAsync(page, pageSize, sortOrder, cancellationToken);
        return Ok(result);
    }

    [HttpGet("categories")]
    public async Task<IActionResult> GetCategories([FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default)
    {
        var result = await _analyticsService.GetCategoryAnalyticsAsync(page, pageSize, cancellationToken);
        return Ok(result);
    }
}
