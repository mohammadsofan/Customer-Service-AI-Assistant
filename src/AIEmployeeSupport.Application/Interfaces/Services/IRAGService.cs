using AIEmployeeSupport.Application.DTOs.Support;

namespace AIEmployeeSupport.Application.Interfaces.Services;

/// <summary>
/// Orchestrates the entire RAG (Retrieval-Augmented Generation) pipeline:
/// 1. Generate embedding for the question
/// 2. Search for similar scenarios in the knowledge base
/// 3. Build context from matched scenarios
/// 4. Generate AI answer using the context
/// 5. Handle failover if primary provider fails
/// </summary>
public interface IRAGService
{
    Task<QuestionResponse> ProcessQuestionAsync(string questionText, Guid employeeId, CancellationToken cancellationToken = default);
}
