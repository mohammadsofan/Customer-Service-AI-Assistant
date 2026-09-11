namespace AIEmployeeSupport.Application.DTOs.AI;

public class CreateAIModelRequest
{
    public Guid ProviderId { get; set; }
    public string ModelName { get; set; } = string.Empty;
}
