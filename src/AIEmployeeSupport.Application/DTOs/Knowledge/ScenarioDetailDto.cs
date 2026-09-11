namespace AIEmployeeSupport.Application.DTOs.Knowledge;

public class ScenarioDetailDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public List<string> Keywords { get; set; } = new();
    public List<ResolutionStepDto> ResolutionSteps { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
