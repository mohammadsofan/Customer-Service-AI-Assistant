namespace AIEmployeeSupport.Application.Interfaces.Services;

public class AIResponse
{
    public bool Answered { get; set; }
    public string? Summary { get; set; }
    public List<string> Steps { get; set; } = new();
    public string? Reason { get; set; }
    public string? RawResponse { get; set; }
}
