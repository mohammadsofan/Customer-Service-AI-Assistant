namespace AIEmployeeSupport.Application.DTOs.Knowledge;

public class KeywordDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public int ScenarioCount { get; set; }
}
