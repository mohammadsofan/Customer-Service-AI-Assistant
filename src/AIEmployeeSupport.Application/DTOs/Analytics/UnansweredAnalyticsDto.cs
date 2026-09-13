namespace AIEmployeeSupport.Application.DTOs.Analytics;

public class UnansweredAnalyticsDto
{
    public List<UnansweredQuestionItem> Questions { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalCount / PageSize) : 0;
}

public class UnansweredQuestionItem
{
    public Guid Id { get; set; }
    public Guid? EmployeeId { get; set; }
    public string? EmployeeName { get; set; }
    public string? EmployeeEmail { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public int Frequency { get; set; }
    public DateTime FirstAsked { get; set; }
    public DateTime LastAsked { get; set; }
}
