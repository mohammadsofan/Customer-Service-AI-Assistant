namespace AIEmployeeSupport.Application.DTOs.AI;

public class AIModelDto
{
    public Guid Id { get; set; }
    public Guid ProviderId { get; set; }
    public string ProviderName { get; set; } = string.Empty;
    public string ModelName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}
