using AIEmployeeSupport.Application.Interfaces.Services;
using AIEmployeeSupport.Domain.Enums;

namespace AIEmployeeSupport.Infrastructure.AI;

public interface IAIProviderFactory
{
    IAIProviderClient CreateClient(ProviderType providerType, string apiKey);
}
