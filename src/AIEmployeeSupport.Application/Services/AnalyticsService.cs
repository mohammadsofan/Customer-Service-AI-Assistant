using AIEmployeeSupport.Application.DTOs.Analytics;
using AIEmployeeSupport.Application.DTOs.Common;
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

    private static (DateTime? start, DateTime? end) NormalizeDateRange(DateTime? fromDate, DateTime? toDate)
    {
        DateTime? start = fromDate;
        DateTime? end = toDate;

        if (end.HasValue && end.Value.TimeOfDay == TimeSpan.Zero)
        {
            end = end.Value.Date.AddDays(1).AddTicks(-1);
        }

        return (start, end);
    }

    public async Task<OverviewAnalyticsDto> GetOverviewAsync(DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default)
    {
        var (start, end) = NormalizeDateRange(fromDate, toDate);
        var questionsTuple = await _questionRepository.GetAllAsync(1, int.MaxValue, null, start, end, cancellationToken);
        var questions = questionsTuple.Items.ToList();
        
        var today = DateTime.UtcNow.Date;
        
        var answered = questions.Count(q => q.Status == QuestionStatus.Answered && q.AnsweredByAI);
        var escalated = questions.Count(q => q.Escalated);
        var total = questions.Count;
        var processingTimes = questions.Where(q => q.ProcessingTimeMs.HasValue).Select(q => q.ProcessingTimeMs!.Value).ToList();
        var confidenceScores = questions.Where(q => q.ConfidenceScore.HasValue).Select(q => q.ConfidenceScore!.Value).ToList();

        return new OverviewAnalyticsDto
        {
            TotalQuestions = total,
            TodayQuestions = fromDate.HasValue || toDate.HasValue ? total : questions.Count(q => q.CreatedAt.Date == today),
            AnsweredCount = answered,
            NoAnswerCount = questions.Count(q => (!q.AnsweredByAI && q.Status != QuestionStatus.New) || q.Status == QuestionStatus.NoAnswer),
            EscalatedCount = escalated,
            AvgResponseTimeMs = processingTimes.Any() ? Math.Round(processingTimes.Average(), 2) : 0,
            AvgSimilarityScore = confidenceScores.Any() ? Math.Round(confidenceScores.Average(), 4) : 0,
            AnswerRate = total > 0 ? Math.Round((double)answered / total, 4) : 0
        };
    }

    public async Task<QuestionAnalyticsDto> GetQuestionAnalyticsAsync(DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default)
    {
        var (start, end) = NormalizeDateRange(fromDate, toDate);
        var questionsTuple = await _questionRepository.GetAllAsync(1, int.MaxValue, null, start, end, cancellationToken);
        var query = questionsTuple.Items.AsEnumerable();

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

    public async Task<PaginatedResponse<KnowledgeAnalyticsDto>> GetKnowledgeAnalyticsAsync(
        int page = 1, int pageSize = 10, DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var (start, end) = NormalizeDateRange(fromDate, toDate);
        var (questionsTuple, _) = await _questionRepository.GetAllAsync(1, int.MaxValue, null, start, end, cancellationToken);
        var scenarioStats = questionsTuple
            .Where(q => q.ScenarioId.HasValue)
            .GroupBy(q => q.ScenarioId.Value)
            .ToDictionary(
                g => g.Key,
                g => new
                {
                    Count = g.Count(),
                    AvgSimilarityScore = g.Any(q => q.ConfidenceScore.HasValue)
                        ? g.Where(q => q.ConfidenceScore.HasValue).Average(q => q.ConfidenceScore ?? 0)
                        : 0,
                    LastUsed = g.OrderByDescending(q => q.CreatedAt).FirstOrDefault()?.CreatedAt
                });

        var (scenarios, totalCount) = await _scenarioRepository.GetAllAsync(page, pageSize, null, null, cancellationToken);

        var items = scenarios.Select(s =>
        {
            var hasStats = scenarioStats.TryGetValue(s.Id, out var stats) && stats != null;
            return new KnowledgeAnalyticsDto
            {
                ScenarioId = s.Id,
                ScenarioName = s.Name,
                CategoryName = s.Category?.Name ?? "غير مصنف",
                RetrievalCount = hasStats ? stats!.Count : 0,
                AvgSimilarityScore = hasStats ? Math.Round(stats!.AvgSimilarityScore, 4) : 0,
                LastUsed = hasStats ? stats!.LastUsed : null
            };
        }).ToList();

        return new PaginatedResponse<KnowledgeAnalyticsDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<PaginatedResponse<CategoryAnalyticsDto>> GetCategoryAnalyticsAsync(
        int page = 1, int pageSize = 10, DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var categories = (await _categoryRepository.GetAllAsync(cancellationToken)).ToList();
        var (scenarios, _) = await _scenarioRepository.GetAllAsync(1, int.MaxValue, null, null, cancellationToken);

        var (start, end) = NormalizeDateRange(fromDate, toDate);
        var (questions, _) = await _questionRepository.GetAllAsync(1, int.MaxValue, null, start, end, cancellationToken);
        
        var scenariosList = scenarios.ToList();
        var questionsList = questions.ToList();
        var totalWithScenario = questionsList.Count(q => q.ScenarioId.HasValue);
        
        var allCategories = categories.Select(c =>
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
            allCategories.Add(new CategoryAnalyticsDto
            {
                CategoryId = Guid.Empty,
                CategoryName = "غير مصنف",
                ScenarioCount = unassignedScenarios.Count,
                QuestionCount = unassignedCount,
                Percentage = totalWithScenario > 0 ? Math.Round((double)unassignedCount / totalWithScenario * 100, 1) : 0
            });
        }

        var totalCount = allCategories.Count;
        var pagedItems = allCategories.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        return new PaginatedResponse<CategoryAnalyticsDto>
        {
            Items = pagedItems,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<UnansweredAnalyticsDto> GetUnansweredAnalyticsAsync(
        int page = 1, int pageSize = 10, string sortOrder = "desc", DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var (start, end) = NormalizeDateRange(fromDate, toDate);
        var unansweredQuestions = (await _questionRepository.GetUnansweredAsync(start, end, cancellationToken)).ToList();
        
        var query = unansweredQuestions
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
            });

        query = string.Equals(sortOrder, "asc", StringComparison.OrdinalIgnoreCase)
            ? query.OrderBy(x => x.LastAsked)
            : query.OrderByDescending(x => x.LastAsked);

        var list = query.ToList();
        var totalCount = list.Count;
        var pagedItems = list.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        return new UnansweredAnalyticsDto
        {
            Questions = pagedItems,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }
}
