namespace AIEmployeeSupport.Application.Common.Settings;

public class EncryptionSettings
{
    public const string SectionName = "EncryptionSettings";
    public string Key { get; set; } = string.Empty;
}
