namespace AIEmployeeSupport.Application.DTOs.Knowledge;

public class ScenarioListDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int KeywordCount { get; set; }
    public int StepCount { get; set; }
    public DateTime CreatedAt { get; set; }
}
