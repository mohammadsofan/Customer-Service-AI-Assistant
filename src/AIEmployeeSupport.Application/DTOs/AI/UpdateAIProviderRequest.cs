namespace AIEmployeeSupport.Application.DTOs.AI;

public class UpdateAIProviderRequest
{
    public string Name { get; set; } = string.Empty;
    public string? ApiKey { get; set; }
    public string? BaseUrl { get; set; }
    public int FallbackPriority { get; set; }
}
