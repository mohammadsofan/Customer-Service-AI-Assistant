using AIEmployeeSupport.Application.DTOs.Analytics;

namespace AIEmployeeSupport.Application.Interfaces.Services;

public interface IAnalyticsService
{
    Task<OverviewAnalyticsDto> GetOverviewAsync(CancellationToken cancellationToken = default);
    Task<QuestionAnalyticsDto> GetQuestionAnalyticsAsync(DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<KnowledgeAnalyticsDto>> GetKnowledgeAnalyticsAsync(CancellationToken cancellationToken = default);
    Task<UnansweredAnalyticsDto> GetUnansweredAnalyticsAsync(CancellationToken cancellationToken = default);
}
