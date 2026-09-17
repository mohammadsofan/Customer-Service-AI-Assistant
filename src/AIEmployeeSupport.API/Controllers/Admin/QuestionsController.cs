using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AIEmployeeSupport.Application.DTOs.Common;
using AIEmployeeSupport.Application.DTOs.Support;
using AIEmployeeSupport.Application.Interfaces;
using AIEmployeeSupport.Domain.Enums;

namespace AIEmployeeSupport.API.Controllers.Admin;

[ApiController]
[Route("api/admin/[controller]")]
[Authorize(Policy = "AdminOnly")]
public class QuestionsController : ControllerBase
{
    private readonly ISupportQuestionRepository _questionRepository;
    private readonly IAIRequestLogRepository _requestLogRepository;

    public QuestionsController(
        ISupportQuestionRepository questionRepository,
        IAIRequestLogRepository requestLogRepository)
    {
        _questionRepository = questionRepository;
        _requestLogRepository = requestLogRepository;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] PaginatedRequest request,
        [FromQuery] string? status = null,
        [FromQuery] string? date = null,
        CancellationToken cancellationToken = default)
    {
        request ??= new PaginatedRequest { Page = 1, PageSize = 10 };

        QuestionStatus? statusEnum = null;
        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<QuestionStatus>(status, true, out var parsedStatus))
        {
            statusEnum = parsedStatus;
        }

        DateTime? parsedDate = null;
        if (!string.IsNullOrWhiteSpace(date) && DateTime.TryParse(date, out var dt))
        {
            parsedDate = dt;
        }

        var (items, totalCount) = await _questionRepository.GetAllAsync(request.Page, request.PageSize, statusEnum, parsedDate, cancellationToken);
        var questionIds = items.Select(q => q.Id).ToList();
        var modelInfoDict = await _requestLogRepository.GetQuestionAIModelInfoAsync(questionIds, cancellationToken);

        var dtos = items.Select(q => {
            modelInfoDict.TryGetValue(q.Id, out var info);
            return new QuestionHistoryDto
            {
                Id = q.Id,
                EmployeeId = q.EmployeeId,
                EmployeeName = q.Employee != null ? q.Employee.FullName : null,
                EmployeeEmail = q.Employee != null ? q.Employee.Email : null,
                QuestionText = q.QuestionText,
                Status = q.Status.ToString(),
                AnsweredByAI = q.AnsweredByAI,
                ConfidenceScore = q.ConfidenceScore,
                ProviderName = info.ProviderName,
                ModelName = info.ModelName,
                CreatedAt = q.CreatedAt,
                CompletedAt = q.CompletedAt ?? q.CreatedAt
            };
        }).ToList();

        return Ok(new PaginatedResponse<QuestionHistoryDto>
        {
            Items = dtos,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var q = await _questionRepository.GetByIdAsync(id, cancellationToken);
        if (q == null) return NotFound();

        var modelInfoDict = await _requestLogRepository.GetQuestionAIModelInfoAsync(new[] { id }, cancellationToken);
        modelInfoDict.TryGetValue(id, out var info);

        return Ok(new
        {
            q.Id,
            q.QuestionText,
            Status = q.Status.ToString(),
            q.AnswerText,
            q.AnsweredByAI,
            q.Escalated,
            q.ConfidenceScore,
            ProviderName = info.ProviderName,
            ModelName = info.ModelName,
            q.ScenarioId,
            ScenarioName = q.Scenario?.Name,
            q.CreatedAt,
            q.CompletedAt,
            q.ProcessingTimeMs,
            EmployeeId = q.EmployeeId,
            EmployeeName = q.Employee?.FullName,
            EmployeeEmail = q.Employee?.Email
        });
    }
}
