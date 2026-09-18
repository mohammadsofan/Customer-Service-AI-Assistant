using AIEmployeeSupport.Application.Interfaces;
using AIEmployeeSupport.Domain.Entities;
using AIEmployeeSupport.Domain.Enums;
using AIEmployeeSupport.Infrastructure.AI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AIEmployeeSupport.Infrastructure.Persistence.Repositories;

public class KnowledgeEmbeddingRepository : IKnowledgeEmbeddingRepository
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<KnowledgeEmbeddingRepository> _logger;

    public KnowledgeEmbeddingRepository(ApplicationDbContext context, ILogger<KnowledgeEmbeddingRepository> logger)
    {
        _context = context;
        _logger = logger;
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
            if (embedding.Embedding != null && embedding.Embedding.Length > 0)
            {
                existing.Embedding = embedding.Embedding;
            }
            existing.Status = embedding.Status;
            existing.UpdatedAt = DateTime.UtcNow;
        }
        else
        {
            _context.KnowledgeEmbeddings.Add(embedding);
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    public Task<IEnumerable<(KnowledgeEmbedding Embedding, double Similarity)>> SearchSimilarAsync(
        byte[] vector, int topK, double threshold, CancellationToken cancellationToken = default)
        => SearchSimilarAsync(vector, topK, threshold, null, cancellationToken);

    public async Task<IEnumerable<(KnowledgeEmbedding Embedding, double Similarity)>> SearchSimilarAsync(
        byte[] vector, int topK, double threshold, string? queryText, CancellationToken cancellationToken = default)
    {
        // ── Stage 1: Lightweight Candidate Retrieval ─────────────────────────
        // Query candidate ID, scenario ID, embedding vector, and scenario keywords
        // without loading scenario descriptions, categories, or resolution steps into EF Core memory.
        var swCandidate = System.Diagnostics.Stopwatch.StartNew();
        var candidates = await _context.KnowledgeEmbeddings
            .AsNoTracking()
            .Where(e => e.Status == EmbeddingStatus.Ready && e.Scenario.Status == ScenarioStatus.Active)
            .Select(e => new
            {
                e.Id,
                e.ScenarioId,
                e.Embedding,
                ScenarioKeywords = e.Scenario.ScenarioKeywords.Select(sk => sk.Keyword.Name).ToList()
            })
            .ToListAsync(cancellationToken);
        swCandidate.Stop();

        if (candidates.Count == 0)
        {
            _logger.LogInformation("[RAG Search] Zero active candidates found in database ({CandidateMs}ms).", swCandidate.ElapsedMilliseconds);
            return Enumerable.Empty<(KnowledgeEmbedding, double)>();
        }

        // ── Stage 1: Similarity Calculation & Top-K Selection ─────────────────
        var swSimilarity = System.Diagnostics.Stopwatch.StartNew();
        var queryVector = DeserializeVector(vector);
        var scored = new List<(Guid EmbeddingId, Guid ScenarioId, double Similarity)>(candidates.Count);

        var queryWords = string.IsNullOrWhiteSpace(queryText) 
            ? new HashSet<string>() 
            : new HashSet<string>(queryText.Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(w => w.ToLowerInvariant()));

        foreach (var candidate in candidates)
        {
            var candidateVector = DeserializeVector(candidate.Embedding);
            var similarity = CosineSimilarity(queryVector, candidateVector);

            // Compute lexical score if queryText is present
            double lexicalScore = 0;
            if (queryWords.Count > 0 && candidate.ScenarioKeywords.Count > 0)
            {
                var keywords = candidate.ScenarioKeywords.Select(k => k.ToLowerInvariant()).ToList();
                int matchCount = queryWords.Count(qw => keywords.Any(kw => kw.Contains(qw) || qw.Contains(kw)));
                lexicalScore = (double)matchCount / Math.Max(queryWords.Count, 1) * 0.15; // Max 15% boost
                similarity += lexicalScore;
            }

            Console.WriteLine($"[RAG DIAGNOSTICS] Scenario: {candidate.ScenarioId} | Cosine: {similarity - lexicalScore:F4} | Lexical: {lexicalScore:F4} | Final: {similarity:F4} | Threshold: {threshold:F4}");

            // Primary semantic path: candidate meets or exceeds threshold
            if (similarity >= threshold)
            {
                scored.Add((candidate.Id, candidate.ScenarioId, similarity));
            }
        }

        var topKMatches = scored
            .OrderByDescending(s => s.Similarity)
            .Take(topK)
            .ToList();
        swSimilarity.Stop();

        if (topKMatches.Count == 0)
        {
            _logger.LogInformation(
                "[RAG Search] Candidates: {CandidateCount} ({CandidateMs}ms) | Scored >= Threshold ({Threshold}): 0 ({ScoreMs}ms) | Hydrated: 0 | Total: {TotalMs}ms",
                candidates.Count, swCandidate.ElapsedMilliseconds,
                threshold, swSimilarity.ElapsedMilliseconds,
                swCandidate.ElapsedMilliseconds + swSimilarity.ElapsedMilliseconds);

            return Enumerable.Empty<(KnowledgeEmbedding, double)>();
        }

        // ── Stage 2: Hydrate Only Matching Top-K Scenarios ───────────────────
        // Eager-load full entity details ONLY for the matched Top-K scenarios.
        var swHydration = System.Diagnostics.Stopwatch.StartNew();
        var matchedIds = topKMatches.Select(m => m.EmbeddingId).ToList();

        var hydratedList = await _context.KnowledgeEmbeddings
            .AsNoTracking()
            .Include(e => e.Scenario)
                .ThenInclude(s => s.Category)
            .Include(e => e.Scenario)
                .ThenInclude(s => s.ResolutionSteps.OrderBy(r => r.StepOrder))
            .Where(e => matchedIds.Contains(e.Id))
            .ToListAsync(cancellationToken);

        var hydratedMap = hydratedList.ToDictionary(h => h.Id);

        var results = new List<(KnowledgeEmbedding Embedding, double Similarity)>(topKMatches.Count);
        foreach (var match in topKMatches)
        {
            if (hydratedMap.TryGetValue(match.EmbeddingId, out var embedding))
            {
                results.Add((embedding, match.Similarity));
            }
        }
        swHydration.Stop();

        var totalMs = swCandidate.ElapsedMilliseconds + swSimilarity.ElapsedMilliseconds + swHydration.ElapsedMilliseconds;
        _logger.LogInformation(
            "[RAG Search] Candidates: {CandidateCount} ({CandidateMs}ms) | Scored: {ScoredCount} ({ScoreMs}ms) | Hydrated: {HydratedCount} ({HydrateMs}ms) | Total: {TotalMs}ms",
            candidates.Count, swCandidate.ElapsedMilliseconds,
            scored.Count, swSimilarity.ElapsedMilliseconds,
            results.Count, swHydration.ElapsedMilliseconds,
            totalMs);

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

    public async Task InvalidateAllEmbeddingsAsync(CancellationToken cancellationToken = default)
    {
        await _context.Database.ExecuteSqlRawAsync(
            "UPDATE KnowledgeEmbeddings SET Status = 'Pending', UpdatedAt = GETUTCDATE()", 
            cancellationToken);
    }

    public async Task<AIEmployeeSupport.Application.DTOs.Knowledge.EmbeddingStatsDto> GetStatsAsync(CancellationToken cancellationToken = default)
    {
        var total = await _context.KnowledgeScenarios.CountAsync(s => s.Status == ScenarioStatus.Active || s.Status == ScenarioStatus.Draft, cancellationToken);
        var pending = await _context.KnowledgeEmbeddings.CountAsync(e => e.Status == EmbeddingStatus.Pending, cancellationToken);
        var ready = await _context.KnowledgeEmbeddings.CountAsync(e => e.Status == EmbeddingStatus.Ready, cancellationToken);
        var failed = await _context.KnowledgeEmbeddings.CountAsync(e => e.Status == EmbeddingStatus.Failed, cancellationToken);

        // Account for scenarios that don't have an embedding record yet
        var missingEmbeddings = total - (pending + ready + failed);
        if (missingEmbeddings > 0)
        {
            pending += missingEmbeddings;
        }

        return new AIEmployeeSupport.Application.DTOs.Knowledge.EmbeddingStatsDto
        {
            TotalScenarios = total,
            PendingEmbeddings = pending,
            ReadyEmbeddings = ready,
            FailedEmbeddings = failed
        };
    }
}
