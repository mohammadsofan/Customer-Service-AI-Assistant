namespace AIEmployeeSupport.Application.DTOs.AI;

public class ProviderTestResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public long LatencyMs { get; set; }
}
