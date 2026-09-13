using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AIEmployeeSupport.Application.DTOs.Common;
using AIEmployeeSupport.Application.DTOs.Knowledge;
using AIEmployeeSupport.Application.Interfaces.Services;

namespace AIEmployeeSupport.API.Controllers.Admin;

[ApiController]
[Route("api/admin/[controller]")]
[Route("api/knowledge/keywords")]
[Authorize(Policy = "AdminOnly")]
public class KeywordsController : ControllerBase
{
    private readonly IKeywordService _keywordService;

    public KeywordsController(IKeywordService keywordService)
    {
        _keywordService = keywordService;
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] PaginatedRequest? request, CancellationToken cancellationToken)
    {
        if (Request.Query.ContainsKey("page") || Request.Query.ContainsKey("search") || Request.Query.ContainsKey("pageSize"))
        {
            request ??= new PaginatedRequest();
            var pagedResult = await _keywordService.GetAllAsync(request, cancellationToken);
            return Ok(pagedResult);
        }

        var result = await _keywordService.GetAllAsync(cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _keywordService.GetByIdAsync(id, cancellationToken);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateKeywordRequest request, CancellationToken cancellationToken)
    {
        var result = await _keywordService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] CreateKeywordRequest request, CancellationToken cancellationToken)
    {
        await _keywordService.UpdateAsync(id, request, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _keywordService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
