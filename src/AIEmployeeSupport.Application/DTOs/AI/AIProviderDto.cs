namespace AIEmployeeSupport.Application.DTOs.AI;

public class AIProviderDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ProviderType { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int FallbackPriority { get; set; }
    public string MaskedApiKey { get; set; } = string.Empty;
    public int ModelCount { get; set; }
    public DateTime CreatedAt { get; set; }
}
