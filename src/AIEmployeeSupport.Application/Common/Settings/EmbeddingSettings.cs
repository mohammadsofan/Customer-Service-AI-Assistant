namespace AIEmployeeSupport.Application.Common.Settings;

public class EmbeddingSettings
{
    public const string SectionName = "EmbeddingSettings";
    public string Provider { get; set; } = "OpenAI";
    public string ApiKey { get; set; } = string.Empty;
    public string ModelName { get; set; } = "text-embedding-3-small";
}
