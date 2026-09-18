namespace AIEmployeeSupport.Application.DTOs.Knowledge;

public class UpdateScenarioRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
    public List<string>? Keywords { get; set; }
    public List<string>? ResolutionSteps { get; set; }
    public List<ResolutionStepInputDto>? Steps { get; set; }
}
