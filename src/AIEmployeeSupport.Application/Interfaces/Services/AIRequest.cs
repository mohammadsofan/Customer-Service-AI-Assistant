namespace AIEmployeeSupport.Application.Interfaces.Services;

public class AIRequest
{
    public string QuestionText { get; set; } = string.Empty;
    public List<string> RetrievedKnowledge { get; set; } = new();
    public string SystemPrompt { get; set; } = string.Empty;
    public double Temperature { get; set; }
    public int MaxTokens { get; set; }
    public string ModelName { get; set; } = string.Empty;
}
