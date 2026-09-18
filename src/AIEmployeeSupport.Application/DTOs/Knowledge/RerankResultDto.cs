namespace AIEmployeeSupport.Application.DTOs.Knowledge;

public class RerankResultDto
{
    public Guid? selectedScenarioId { get; set; }
    public double confidence { get; set; }
}
