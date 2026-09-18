namespace AIEmployeeSupport.Application.DTOs.Knowledge;

public class RagDiagnosticRequest
{
    public string Question { get; set; } = string.Empty;
}

public class RagDiagnosticResult
{
    public string EmbeddingModel { get; set; } = string.Empty;
    public int EmbeddingDimension { get; set; }
    public double Threshold { get; set; }
    public System.Collections.Generic.List<RagCandidate> Candidates { get; set; } = new();
    public string SelectedScenarioId { get; set; } = string.Empty;
    public string SelectedScenarioName { get; set; } = string.Empty;
    public string FinalAnswer { get; set; } = string.Empty;
}

public class RagCandidate
{
    public string ScenarioId { get; set; } = string.Empty;
    public string ScenarioName { get; set; } = string.Empty;
    public double CosineSimilarity { get; set; }
    public double LexicalScore { get; set; }
    public double FinalScore { get; set; }
}
