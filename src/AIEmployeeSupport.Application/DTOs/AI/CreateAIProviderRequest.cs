namespace AIEmployeeSupport.Application.DTOs.AI;

public class CreateAIProviderRequest
{
    public string Name { get; set; } = string.Empty;
    public string ProviderType { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string? BaseUrl { get; set; }
    public int? FallbackPriority { get; set; }
}
