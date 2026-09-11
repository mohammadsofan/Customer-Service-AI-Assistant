using AIEmployeeSupport.Application.Interfaces.Services;
using AIEmployeeSupport.Domain.Enums;

namespace AIEmployeeSupport.Application.Interfaces.Services;

public interface IAIProviderFactory
{
    IAIProviderClient CreateClient(ProviderType providerType, string apiKey, string? baseUrl = null);
}
