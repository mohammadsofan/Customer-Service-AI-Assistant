namespace AIEmployeeSupport.Application.DTOs.Knowledge;

public class EmbeddingStatsDto
{
    public int TotalScenarios { get; set; }
    public int PendingEmbeddings { get; set; }
    public int ReadyEmbeddings { get; set; }
    public int FailedEmbeddings { get; set; }
}
