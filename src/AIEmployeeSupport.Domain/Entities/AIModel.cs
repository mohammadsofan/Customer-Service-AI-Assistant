namespace AIEmployeeSupport.Domain.Entities;

public class AIModel
{
    public Guid Id { get; set; }
    public Guid ProviderId { get; set; }
    public string ModelName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public AIProvider Provider { get; set; } = null!;
}
