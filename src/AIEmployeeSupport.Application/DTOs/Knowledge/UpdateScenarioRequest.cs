namespace AIEmployeeSupport.Application.DTOs.Knowledge;

public class UpdateScenarioRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
    public List<string> Keywords { get; set; } = new();
    public List<string> ResolutionSteps { get; set; } = new();
    public List<ResolutionStepInputDto>? Steps { get; set; }
}
