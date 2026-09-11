using AIEmployeeSupport.Application.Interfaces.Services;
using AIEmployeeSupport.Application.Services;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace AIEmployeeSupport.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Register all FluentValidation validators from this assembly
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        // Register application services
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IEmployeeService, EmployeeService>();
        services.AddScoped<IAuditService, AuditService>();
        services.AddScoped<IKnowledgeService, KnowledgeService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IKeywordService, KeywordService>();
        services.AddScoped<IRAGService, RAGService>();
        services.AddScoped<IAIFailoverService, AIFailoverService>();
        services.AddScoped<IAIProviderService, AIProviderService>();
        services.AddScoped<IAIModelService, AIModelService>();
        services.AddScoped<IAIConfigurationService, AIConfigurationService>();
        services.AddScoped<IAnalyticsService, AnalyticsService>();
        services.AddScoped<ISupportService, SupportService>();

        return services;
    }
}
