using AIEmployeeSupport.Application.Interfaces;
using AIEmployeeSupport.Application.Interfaces.Services;
using AIEmployeeSupport.Infrastructure.AI;
using AIEmployeeSupport.Infrastructure.Persistence.Repositories;
using AIEmployeeSupport.Infrastructure.Security;
using Microsoft.Extensions.DependencyInjection;

namespace AIEmployeeSupport.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        // Repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IKnowledgeScenarioRepository, KnowledgeScenarioRepository>();
        services.AddScoped<IKnowledgeCategoryRepository, KnowledgeCategoryRepository>();
        services.AddScoped<IKnowledgeKeywordRepository, KnowledgeKeywordRepository>();
        services.AddScoped<IResolutionStepRepository, ResolutionStepRepository>();
        services.AddScoped<IKnowledgeEmbeddingRepository, KnowledgeEmbeddingRepository>();
        services.AddScoped<ISupportQuestionRepository, SupportQuestionRepository>();
        services.AddScoped<IAIProviderRepository, AIProviderRepository>();
        services.AddScoped<IAIModelRepository, AIModelRepository>();
        services.AddScoped<IAIConfigurationRepository, AIConfigurationRepository>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();
        services.AddScoped<IAIRequestLogRepository, AIRequestLogRepository>();
        services.AddScoped<IKnowledgeScenarioVersionRepository, KnowledgeScenarioVersionRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Services
        services.AddSingleton<IEncryptionService, EncryptionService>();

        // AI Provider Factory & Http Client
        services.AddHttpClient("AIProviderClient");
        services.AddSingleton<IAIProviderFactory, AIProviderFactory>();

        return services;
    }
}
