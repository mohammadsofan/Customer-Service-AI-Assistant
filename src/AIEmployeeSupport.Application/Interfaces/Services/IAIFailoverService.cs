using AIEmployeeSupport.Application.Interfaces.Services;

namespace AIEmployeeSupport.Application.Interfaces.Services;

public interface IAIFailoverService
{
    Task<AIResponse> GenerateAnswerWithFailoverAsync(AIRequest request, CancellationToken token = default);
    Task<string?> RewriteQueryWithFailoverAsync(string questionText, CancellationToken token = default);
}
