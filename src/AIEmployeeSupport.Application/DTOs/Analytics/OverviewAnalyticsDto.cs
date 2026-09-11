namespace AIEmployeeSupport.Application.DTOs.Analytics;

public class OverviewAnalyticsDto
{
    public int TotalQuestions { get; set; }
    public int TodayQuestions { get; set; }
    public int AnsweredCount { get; set; }
    public int NoAnswerCount { get; set; }
    public int EscalatedCount { get; set; }
    public double AvgResponseTimeMs { get; set; }
    public double AvgSimilarityScore { get; set; }
    public double AnswerRate { get; set; }
}
