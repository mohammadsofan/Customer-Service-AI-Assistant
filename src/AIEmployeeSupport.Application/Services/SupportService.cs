using AIEmployeeSupport.Application.DTOs.Common;
using AIEmployeeSupport.Application.DTOs.Support;
using AIEmployeeSupport.Application.Interfaces;
using AIEmployeeSupport.Application.Interfaces.Services;

namespace AIEmployeeSupport.Application.Services;

public class SupportService : ISupportService
{
    private readonly IRAGService _ragService;
    private readonly ISupportQuestionRepository _questionRepository;

    public SupportService(IRAGService ragService, ISupportQuestionRepository questionRepository)
    {
        _ragService = ragService;
        _questionRepository = questionRepository;
    }

    public async Task<QuestionResponse> SubmitQuestionAsync(Guid employeeId, SubmitQuestionRequest request, CancellationToken cancellationToken = default)
    {
        // RAGService handles the entire flow: embedding, searching, failover AI call, saving to DB
        return await _ragService.ProcessQuestionAsync(request.Problem, employeeId, cancellationToken);
    }

    public async Task<QuestionResponse?> GetQuestionByIdAsync(Guid questionId, CancellationToken cancellationToken = default)
    {
        var question = await _questionRepository.GetByIdAsync(questionId, cancellationToken);
        if (question == null) return null;

        return new QuestionResponse
        {
            Id = question.Id,
            Status = question.Status.ToString(),
            Answered = question.AnsweredByAI,
            Answer = question.AnswerText,
            Steps = new List<string>(), // Since steps aren't persisted separately on the question in this schema
            ConfidenceScore = question.ConfidenceScore,
            SourceScenario = question.Scenario?.Name,
            Escalated = question.Escalated
        };
    }

    public async Task<PaginatedResponse<QuestionHistoryDto>> GetQuestionHistoryAsync(Guid employeeId, PaginatedRequest request, CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await _questionRepository.GetByEmployeeIdAsync(employeeId, request.Page, request.PageSize, cancellationToken);
        
        var dtos = items.Select(q => new QuestionHistoryDto
        {
            Id = q.Id,
            QuestionText = q.QuestionText,
            Status = q.Status.ToString(),
            AnsweredByAI = q.AnsweredByAI,
            ConfidenceScore = q.ConfidenceScore,
            CreatedAt = q.CreatedAt,
            CompletedAt = q.CompletedAt ?? q.CreatedAt
        }).ToList();

        return new PaginatedResponse<QuestionHistoryDto>
        {
            Items = dtos,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }
}
