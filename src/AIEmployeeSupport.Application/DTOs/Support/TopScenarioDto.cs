using System;

namespace AIEmployeeSupport.Application.DTOs.Support;

public class TopScenarioDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int UsageCount { get; set; }
}
