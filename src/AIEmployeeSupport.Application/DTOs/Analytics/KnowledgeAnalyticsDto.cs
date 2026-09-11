namespace AIEmployeeSupport.Application.DTOs.Analytics;

public class KnowledgeAnalyticsDto
{
    public Guid ScenarioId { get; set; }
    public string ScenarioName { get; set; } = string.Empty;
    public int RetrievalCount { get; set; }
    public double AvgSimilarityScore { get; set; }
    public DateTime? LastUsed { get; set; }
}
