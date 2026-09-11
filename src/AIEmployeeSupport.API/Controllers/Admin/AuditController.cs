using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using AIEmployeeSupport.Application.Interfaces.Services;
namespace AIEmployeeSupport.API.Controllers.Admin;

[ApiController]
[Route("api/admin/[controller]")]
[Authorize(Policy = "AdminOnly")]
public class AuditController : ControllerBase
{
    private readonly IAuditService _auditService;

    public AuditController(IAuditService auditService)
    {
        _auditService = auditService;
    }

    [HttpGet]
    public IActionResult GetLogs() => Ok();
}
