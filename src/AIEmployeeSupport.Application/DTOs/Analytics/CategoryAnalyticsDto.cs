using System;

namespace AIEmployeeSupport.Application.DTOs.Analytics;

public class CategoryAnalyticsDto
{
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public int ScenarioCount { get; set; }
    public int QuestionCount { get; set; }
    public double Percentage { get; set; }
}
