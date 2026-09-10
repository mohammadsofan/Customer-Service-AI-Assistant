using AIEmployeeSupport.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AIEmployeeSupport.Infrastructure.Persistence.Configurations;

public class KnowledgeScenarioConfiguration : IEntityTypeConfiguration<KnowledgeScenario>
{
    public void Configure(EntityTypeBuilder<KnowledgeScenario> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Name)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(s => s.Description)
            .IsRequired()
            .HasMaxLength(4000);

        builder.Property(s => s.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.HasIndex(s => s.Status);

        builder.Property(s => s.CategoryId)
            .IsRequired();

        builder.Property(s => s.CreatedBy)
            .IsRequired();

        builder.Property(s => s.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(s => s.UpdatedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        builder.HasOne(s => s.Category)
            .WithMany(c => c.Scenarios)
            .HasForeignKey(s => s.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(s => s.ResolutionSteps)
            .WithOne(r => r.Scenario)
            .HasForeignKey(r => r.ScenarioId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(s => s.Embedding)
            .WithOne(e => e.Scenario)
            .HasForeignKey<KnowledgeEmbedding>(e => e.ScenarioId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(s => s.Versions)
            .WithOne(v => v.Scenario)
            .HasForeignKey(v => v.ScenarioId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
