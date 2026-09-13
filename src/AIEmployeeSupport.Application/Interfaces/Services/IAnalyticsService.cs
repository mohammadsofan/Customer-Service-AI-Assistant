using AIEmployeeSupport.Application.DTOs.Analytics;
using AIEmployeeSupport.Application.DTOs.Common;

namespace AIEmployeeSupport.Application.Interfaces.Services;

public interface IAnalyticsService
{
    Task<OverviewAnalyticsDto> GetOverviewAsync(CancellationToken cancellationToken = default);
    Task<QuestionAnalyticsDto> GetQuestionAnalyticsAsync(DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default);
    Task<PaginatedResponse<KnowledgeAnalyticsDto>> GetKnowledgeAnalyticsAsync(int page = 1, int pageSize = 10, CancellationToken cancellationToken = default);
    Task<PaginatedResponse<CategoryAnalyticsDto>> GetCategoryAnalyticsAsync(int page = 1, int pageSize = 10, CancellationToken cancellationToken = default);
    Task<UnansweredAnalyticsDto> GetUnansweredAnalyticsAsync(int page = 1, int pageSize = 10, string sortOrder = "desc", CancellationToken cancellationToken = default);
}
