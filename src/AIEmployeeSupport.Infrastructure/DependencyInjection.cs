using AIEmployeeSupport.Application.Interfaces;
using AIEmployeeSupport.Application.Interfaces.Services;
using AIEmployeeSupport.Infrastructure.AI;
using AIEmployeeSupport.Infrastructure.Persistence;
using AIEmployeeSupport.Infrastructure.Persistence.Repositories;
using AIEmployeeSupport.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AIEmployeeSupport.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Database
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

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
        services.AddHttpClient("AIProviderClient", client =>
        {
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
            client.DefaultRequestHeaders.Accept.ParseAdd("application/json, text/plain, */*");
        });
        services.AddSingleton<IAIProviderFactory, AIProviderFactory>();

        // Embedding Service
        services.AddHttpClient("EmbeddingClient");
        services.AddScoped<IEmbeddingService, EmbeddingService>();

        // Background Services
        services.AddHostedService<AIEmployeeSupport.Infrastructure.Services.EmbeddingBackgroundService>();

        return services;
    }
}
