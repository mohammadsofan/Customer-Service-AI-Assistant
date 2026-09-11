using AIEmployeeSupport.Application.DTOs.Analytics;
using AIEmployeeSupport.Application.Interfaces;
using AIEmployeeSupport.Application.Interfaces.Services;
using AIEmployeeSupport.Domain.Enums;

namespace AIEmployeeSupport.Application.Services;

public class AnalyticsService : IAnalyticsService
{
    private readonly ISupportQuestionRepository _questionRepository;
    private readonly IKnowledgeScenarioRepository _scenarioRepository;
    private readonly IAIRequestLogRepository _requestLogRepository;

    public AnalyticsService(
        ISupportQuestionRepository questionRepository,
        IKnowledgeScenarioRepository scenarioRepository,
        IAIRequestLogRepository requestLogRepository)
    {
        _questionRepository = questionRepository;
        _scenarioRepository = scenarioRepository;
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
                RetrievalCount = relatedQuestions.Count,
                AvgSimilarityScore = relatedQuestions.Any(q => q.ConfidenceScore.HasValue) 
                    ? relatedQuestions.Where(q => q.ConfidenceScore.HasValue).Average(q => q.ConfidenceScore.Value) 
                    : 0,
                LastUsed = relatedQuestions.OrderByDescending(q => q.CreatedAt).FirstOrDefault()?.CreatedAt
            };
        }).ToList();
    }

    public async Task<UnansweredAnalyticsDto> GetUnansweredAnalyticsAsync(CancellationToken cancellationToken = default)
    {
        var unansweredQuestions = await _questionRepository.GetUnansweredAsync(cancellationToken);
        
        var grouped = unansweredQuestions
            .GroupBy(q => q.QuestionText.ToLowerInvariant().Trim())
            .Select(g => new UnansweredQuestionItem
            {
                Id = g.First().Id,
                QuestionText = g.First().QuestionText,
                Frequency = g.Count(),
                FirstAsked = g.Min(q => q.CreatedAt),
                LastAsked = g.Max(q => q.CreatedAt)
            })
            .OrderByDescending(x => x.Frequency)
            .ToList();

        return new UnansweredAnalyticsDto
        {
            Questions = grouped
        };
    }
}
