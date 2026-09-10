using AIEmployeeSupport.Domain.Enums;

namespace AIEmployeeSupport.Domain.Entities;

public class AIProvider
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public ProviderType ProviderType { get; set; }
    public string EncryptedApiKey { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int FallbackPriority { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public ICollection<AIModel> Models { get; set; } = new List<AIModel>();
}
