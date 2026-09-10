namespace AIEmployeeSupport.Domain.Enums;

public enum AuditAction
{
    ScenarioCreated,
    ScenarioUpdated,
    ScenarioActivated,
    ScenarioDeactivated,
    ScenarioArchived,
    KeywordAdded,
    KeywordRemoved,
    ResolutionStepChanged,
    CategoryChanged,
    AIProviderCreated,
    AIProviderUpdated,
    AIProviderActivated,
    AIModelChanged,
    AIConfigurationChanged,
    EmployeeCreated,
    EmployeeDeactivated,
    AIProviderFailoverTriggered
}
