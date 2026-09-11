using AIEmployeeSupport.Domain.Entities;

namespace AIEmployeeSupport.Application.Interfaces;

public interface IKnowledgeEmbeddingRepository
{
    Task<KnowledgeEmbedding?> GetByScenarioIdAsync(Guid scenarioId, CancellationToken cancellationToken = default);
    Task UpsertAsync(KnowledgeEmbedding embedding, CancellationToken cancellationToken = default);
    Task<IEnumerable<(KnowledgeEmbedding Embedding, double Similarity)>> SearchSimilarAsync(
        byte[] vector,
        int topK,
        double threshold,
        CancellationToken cancellationToken = default);
}
