using AIEmployeeSupport.Application.DTOs.Analytics;
using AIEmployeeSupport.Application.Interfaces;
using AIEmployeeSupport.Application.Interfaces.Services;
using AIEmployeeSupport.Domain.Enums;

namespace AIEmployeeSupport.Application.Services;

public class AnalyticsService : IAnalyticsService
{
    private readonly ISupportQuestionRepository _questionRepository;
    private readonly IKnowledgeScenarioRepository _scenarioRepository;
    private readonly IKnowledgeCategoryRepository _categoryRepository;
    private readonly IAIRequestLogRepository _requestLogRepository;

    public AnalyticsService(
        ISupportQuestionRepository questionRepository,
        IKnowledgeScenarioRepository scenarioRepository,
        IKnowledgeCategoryRepository categoryRepository,
        IAIRequestLogRepository requestLogRepository)
    {
        _questionRepository = questionRepository;
        _scenarioRepository = scenarioRepository;
        _categoryRepository = categoryRepository;
        _requestLogRepository = requestLogRepository;
    }

    public async Task<OverviewAnalyticsDto> GetOverviewAsync(CancellationToken cancellationToken = default)
    {
        var questionsTuple = await _questionRepository.GetAllAsync(1, int.MaxValue, cancellationToken);
        var questions = questionsTuple.Items.ToList();
        
        var today = DateTime.UtcNow.Date;
        
        var answered = questions.Count(q => q.Status == QuestionStatus.Answered && q.AnsweredByAI);
        var escalated = questions.Count(q => q.Escalated);
        var total = questions.Count;
        
        return new OverviewAnalyticsDto
        {
            TotalQuestions = total,
            TodayQuestions = questions.Count(q => q.CreatedAt.Date == today),
            AnsweredCount = answered,
            NoAnswerCount = questions.Count(q => !q.AnsweredByAI && q.Status != QuestionStatus.New),
            EscalatedCount = escalated,
            AvgResponseTimeMs = questions.Where(q => q.ProcessingTimeMs.HasValue).Average(q => q.ProcessingTimeMs) ?? 0,
            AvgSimilarityScore = questions.Where(q => q.ConfidenceScore.HasValue).Average(q => q.ConfidenceScore) ?? 0,
            AnswerRate = total > 0 ? (double)answered / total : 0
        };
    }

    public async Task<QuestionAnalyticsDto> GetQuestionAnalyticsAsync(DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default)
    {
        var questionsTuple = await _questionRepository.GetAllAsync(1, int.MaxValue, cancellationToken);
        var query = questionsTuple.Items.AsEnumerable();

        if (fromDate.HasValue) query = query.Where(q => q.CreatedAt >= fromDate.Value);
        if (toDate.HasValue) query = query.Where(q => q.CreatedAt <= toDate.Value);

        return new QuestionAnalyticsDto
        {
            Questions = query.Select(q => new QuestionAnalyticsItem
            {
                Id = q.Id,
                EmployeeId = q.EmployeeId,
                EmployeeName = q.Employee != null ? q.Employee.FullName : null,
                EmployeeEmail = q.Employee != null ? q.Employee.Email : null,
                QuestionText = q.QuestionText,
                Status = q.Status.ToString(),
                AnsweredByAI = q.AnsweredByAI,
                ConfidenceScore = q.ConfidenceScore,
                ProcessingTimeMs = q.ProcessingTimeMs,
                ScenarioName = q.Scenario != null ? q.Scenario.Name : null,
                CreatedAt = q.CreatedAt,
                CompletedAt = q.CompletedAt
            }).ToList()
        };
    }

    public async Task<IEnumerable<KnowledgeAnalyticsDto>> GetKnowledgeAnalyticsAsync(CancellationToken cancellationToken = default)
    {
        var questionsTuple = await _questionRepository.GetAllAsync(1, int.MaxValue, cancellationToken);
        var scenariosTuple = await _scenarioRepository.GetAllAsync(1, int.MaxValue, null, null, cancellationToken);
        
        var questionsWithScenario = questionsTuple.Items.Where(q => q.ScenarioId.HasValue).ToList();
        
        return scenariosTuple.Items.Select(s => 
        {
            var relatedQuestions = questionsWithScenario.Where(q => q.ScenarioId == s.Id).ToList();
            return new KnowledgeAnalyticsDto
            {
                ScenarioId = s.Id,
                ScenarioName = s.Name,
                CategoryName = s.Category?.Name ?? "غير مصنف",
                RetrievalCount = relatedQuestions.Count,
                AvgSimilarityScore = relatedQuestions.Any(q => q.ConfidenceScore.HasValue) 
                    ? relatedQuestions.Where(q => q.ConfidenceScore.HasValue).Average(q => q.ConfidenceScore ?? 0) 
                    : 0,
                LastUsed = relatedQuestions.OrderByDescending(q => q.CreatedAt).FirstOrDefault()?.CreatedAt
            };
        }).ToList();
    }

    public async Task<IEnumerable<CategoryAnalyticsDto>> GetCategoryAnalyticsAsync(CancellationToken cancellationToken = default)
    {
        var categories = (await _categoryRepository.GetAllAsync(cancellationToken)).ToList();
        var (scenarios, _) = await _scenarioRepository.GetAllAsync(1, int.MaxValue, null, null, cancellationToken);
        var (questions, _) = await _questionRepository.GetAllAsync(1, int.MaxValue, cancellationToken);
        
        var scenariosList = scenarios.ToList();
        var questionsList = questions.ToList();
        var totalWithScenario = questionsList.Count(q => q.ScenarioId.HasValue);
        
        var result = categories.Select(c =>
        {
            var catScenarioIds = scenariosList.Where(s => s.CategoryId == c.Id).Select(s => s.Id).ToHashSet();
            var count = questionsList.Count(q => q.ScenarioId.HasValue && catScenarioIds.Contains(q.ScenarioId.Value));
            return new CategoryAnalyticsDto
            {
                CategoryId = c.Id,
                CategoryName = c.Name,
                ScenarioCount = catScenarioIds.Count,
                QuestionCount = count,
                Percentage = totalWithScenario > 0 ? Math.Round((double)count / totalWithScenario * 100, 1) : 0
            };
        }).OrderByDescending(x => x.QuestionCount).ThenByDescending(x => x.ScenarioCount).ToList();

        var knownCategoryIds = categories.Select(c => c.Id).ToHashSet();
        var unassignedScenarios = scenariosList.Where(s => s.CategoryId == Guid.Empty || !knownCategoryIds.Contains(s.CategoryId)).Select(s => s.Id).ToHashSet();
        if (unassignedScenarios.Count > 0)
        {
            var unassignedCount = questionsList.Count(q => q.ScenarioId.HasValue && unassignedScenarios.Contains(q.ScenarioId.Value));
            result.Add(new CategoryAnalyticsDto
            {
                CategoryId = Guid.Empty,
                CategoryName = "غير مصنف",
                ScenarioCount = unassignedScenarios.Count,
                QuestionCount = unassignedCount,
                Percentage = totalWithScenario > 0 ? Math.Round((double)unassignedCount / totalWithScenario * 100, 1) : 0
            });
        }

        return result;
    }

    public async Task<UnansweredAnalyticsDto> GetUnansweredAnalyticsAsync(CancellationToken cancellationToken = default)
    {
        var unansweredQuestions = await _questionRepository.GetUnansweredAsync(cancellationToken);
        
        var grouped = unansweredQuestions
            .GroupBy(q => q.QuestionText.ToLowerInvariant().Trim())
            .Select(g => 
            {
                var latest = g.OrderByDescending(q => q.CreatedAt).First();
                return new UnansweredQuestionItem
                {
                    Id = latest.Id,
                    EmployeeId = latest.EmployeeId,
                    EmployeeName = latest.Employee != null ? latest.Employee.FullName : null,
                    EmployeeEmail = latest.Employee != null ? latest.Employee.Email : null,
                    QuestionText = latest.QuestionText,
                    Frequency = g.Count(),
                    FirstAsked = g.Min(q => q.CreatedAt),
                    LastAsked = g.Max(q => q.CreatedAt)
                };
            })
            .OrderByDescending(x => x.LastAsked)
            .ToList();

        return new UnansweredAnalyticsDto
        {
            Questions = grouped
        };
    }
}
