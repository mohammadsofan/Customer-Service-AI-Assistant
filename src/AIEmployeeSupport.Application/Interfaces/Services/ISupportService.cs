using AIEmployeeSupport.Application.DTOs.Common;
using AIEmployeeSupport.Application.DTOs.Support;

namespace AIEmployeeSupport.Application.Interfaces.Services;

public interface ISupportService
{
    Task<QuestionResponse> SubmitQuestionAsync(Guid employeeId, SubmitQuestionRequest request, CancellationToken cancellationToken = default);
    Task<QuestionResponse?> GetQuestionByIdAsync(Guid questionId, Guid currentUserId, bool isAdmin, CancellationToken cancellationToken = default);
    Task<PaginatedResponse<QuestionHistoryDto>> GetQuestionHistoryAsync(Guid employeeId, PaginatedRequest request, CancellationToken cancellationToken = default);
    Task<IEnumerable<TopScenarioDto>> GetTopScenariosAsync(int count = 5, CancellationToken cancellationToken = default);
}
