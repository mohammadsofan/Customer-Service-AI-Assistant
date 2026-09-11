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
    public IActionResult GetOverview() => Ok();

    [HttpGet("questions")]
    public IActionResult GetQuestions() => Ok();

    [HttpGet("knowledge")]
    public IActionResult GetKnowledge() => Ok();

    [HttpGet("unanswered")]
    public IActionResult GetUnanswered() => Ok();
}
