namespace AIEmployeeSupport.Application.DTOs.Knowledge;

public class ResolutionStepDto
{
    public Guid Id { get; set; }
    public int StepOrder { get; set; }
    public string StepText { get; set; } = string.Empty;
}
