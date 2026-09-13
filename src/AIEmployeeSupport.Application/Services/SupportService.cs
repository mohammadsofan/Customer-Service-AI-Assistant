using AIEmployeeSupport.Application.DTOs.Common;
using AIEmployeeSupport.Application.DTOs.Support;
using AIEmployeeSupport.Application.Interfaces;
using AIEmployeeSupport.Application.Interfaces.Services;
using AIEmployeeSupport.Domain.Enums;

namespace AIEmployeeSupport.Application.Services;

public class SupportService : ISupportService
{
    private readonly IRAGService _ragService;
    private readonly ISupportQuestionRepository _questionRepository;
    private readonly IKnowledgeScenarioRepository _scenarioRepository;

    public SupportService(
        IRAGService ragService,
        ISupportQuestionRepository questionRepository,
        IKnowledgeScenarioRepository scenarioRepository)
    {
        _ragService = ragService;
        _questionRepository = questionRepository;
        _scenarioRepository = scenarioRepository;
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
            EmployeeId = q.EmployeeId,
            EmployeeName = q.Employee != null ? q.Employee.FullName : null,
            EmployeeEmail = q.Employee != null ? q.Employee.Email : null,
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

    public async Task<IEnumerable<TopScenarioDto>> GetTopScenariosAsync(int count = 5, CancellationToken cancellationToken = default)
    {
        var (scenarios, _) = await _scenarioRepository.GetAllAsync(1, int.MaxValue, ScenarioStatus.Active, null, cancellationToken);
        var (allQuestions, _) = await _questionRepository.GetAllAsync(1, int.MaxValue, cancellationToken);

        var scenarioUsage = allQuestions
            .Where(q => q.ScenarioId.HasValue)
            .GroupBy(q => q.ScenarioId!.Value)
            .ToDictionary(g => g.Key, g => g.Count());

        return scenarios
            .Select(s => new TopScenarioDto
            {
                Id = s.Id,
                Name = s.Name,
                Description = s.Description,
                UsageCount = scenarioUsage.TryGetValue(s.Id, out var usage) ? usage : 0
            })
            .OrderByDescending(s => s.UsageCount)
            .ThenBy(s => s.Name)
            .Take(count)
            .ToList();
    }
}
