using AIEmployeeSupport.Application.Interfaces;
using AIEmployeeSupport.Domain.Entities;
using AIEmployeeSupport.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace AIEmployeeSupport.Infrastructure.Persistence.Repositories;

public class KnowledgeEmbeddingRepository : IKnowledgeEmbeddingRepository
{
    private readonly ApplicationDbContext _context;

    public KnowledgeEmbeddingRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<KnowledgeEmbedding?> GetByScenarioIdAsync(Guid scenarioId, CancellationToken cancellationToken = default)
        => await _context.KnowledgeEmbeddings.AsNoTracking()
            .FirstOrDefaultAsync(e => e.ScenarioId == scenarioId, cancellationToken);

    public async Task UpsertAsync(KnowledgeEmbedding embedding, CancellationToken cancellationToken = default)
    {
        var existing = await _context.KnowledgeEmbeddings
            .FirstOrDefaultAsync(e => e.ScenarioId == embedding.ScenarioId, cancellationToken);

        if (existing != null)
        {
            existing.Content = embedding.Content;
            existing.Embedding = embedding.Embedding;
            existing.Status = embedding.Status;
            existing.UpdatedAt = DateTime.UtcNow;
            _context.KnowledgeEmbeddings.Update(existing);
        }
        else
        {
            _context.KnowledgeEmbeddings.Add(embedding);
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IEnumerable<(KnowledgeEmbedding Embedding, double Similarity)>> SearchSimilarAsync(
        byte[] vector, int topK, double threshold, CancellationToken cancellationToken = default)
    {
        // Load all ready embeddings from the database
        var embeddings = await _context.KnowledgeEmbeddings
            .AsNoTracking()
            .Include(e => e.Scenario)
                .ThenInclude(s => s.Category)
            .Include(e => e.Scenario)
                .ThenInclude(s => s.ResolutionSteps.OrderBy(r => r.StepOrder))
            .Where(e => e.Status == EmbeddingStatus.Ready)
            .ToListAsync(cancellationToken);

        // Deserialize the query vector
        var queryVector = DeserializeVector(vector);

        // Compute cosine similarity in C# application code
        var results = embeddings
            .Select(e => new
            {
                Embedding = e,
                Similarity = CosineSimilarity(queryVector, DeserializeVector(e.Embedding))
            })
            .Where(r => r.Similarity >= threshold)
            .OrderByDescending(r => r.Similarity)
            .Take(topK)
            .Select(r => (r.Embedding, r.Similarity))
            .ToList();

        return results;
    }

    private static float[] DeserializeVector(byte[] bytes)
    {
        var floats = new float[bytes.Length / sizeof(float)];
        Buffer.BlockCopy(bytes, 0, floats, 0, bytes.Length);
        return floats;
    }

    private static double CosineSimilarity(float[] a, float[] b)
    {
        if (a.Length != b.Length || a.Length == 0)
            return 0;

        double dotProduct = 0, magnitudeA = 0, magnitudeB = 0;
        for (int i = 0; i < a.Length; i++)
        {
            dotProduct += a[i] * b[i];
            magnitudeA += a[i] * a[i];
            magnitudeB += b[i] * b[i];
        }

        var magnitude = Math.Sqrt(magnitudeA) * Math.Sqrt(magnitudeB);
        return magnitude == 0 ? 0 : dotProduct / magnitude;
    }
}
