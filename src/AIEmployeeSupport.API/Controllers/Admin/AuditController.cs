using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AIEmployeeSupport.Application.DTOs.Common;
using AIEmployeeSupport.Application.Interfaces.Services;
using AIEmployeeSupport.Domain.Enums;

namespace AIEmployeeSupport.API.Controllers.Admin;

[ApiController]
[Route("api/admin/[controller]")]
[Route("api/audit/logs")]
[Authorize(Policy = "AdminOnly")]
public class AuditController : ControllerBase
{
    private readonly IAuditService _auditService;

    public AuditController(IAuditService auditService)
    {
        _auditService = auditService;
    }

    [HttpGet]
    public async Task<IActionResult> GetLogs([FromQuery] PaginatedRequest request, [FromQuery] Guid? userId, [FromQuery] AuditAction? action, [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate, CancellationToken cancellationToken)
    {
        request ??= new PaginatedRequest { Page = 1, PageSize = 100 };
        var result = await _auditService.GetLogsAsync(request, userId, action, fromDate, toDate, cancellationToken);
        return Ok(result);
    }
}
