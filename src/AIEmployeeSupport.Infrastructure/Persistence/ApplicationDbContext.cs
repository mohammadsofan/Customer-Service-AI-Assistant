using AIEmployeeSupport.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AIEmployeeSupport.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<KnowledgeCategory> KnowledgeCategories => Set<KnowledgeCategory>();
    public DbSet<KnowledgeKeyword> KnowledgeKeywords => Set<KnowledgeKeyword>();
    public DbSet<KnowledgeScenario> KnowledgeScenarios => Set<KnowledgeScenario>();
    public DbSet<KnowledgeScenarioVersion> KnowledgeScenarioVersions => Set<KnowledgeScenarioVersion>();
    public DbSet<ScenarioKeyword> ScenarioKeywords => Set<ScenarioKeyword>();
    public DbSet<ResolutionStep> ResolutionSteps => Set<ResolutionStep>();
    public DbSet<KnowledgeEmbedding> KnowledgeEmbeddings => Set<KnowledgeEmbedding>();
    public DbSet<SupportQuestion> SupportQuestions => Set<SupportQuestion>();
    public DbSet<AIProvider> AIProviders => Set<AIProvider>();
    public DbSet<AIModel> AIModels => Set<AIModel>();
    public DbSet<AIConfiguration> AIConfigurations => Set<AIConfiguration>();
    public DbSet<AIRequestLog> AIRequestLogs => Set<AIRequestLog>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all IEntityTypeConfiguration<T> from this assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
