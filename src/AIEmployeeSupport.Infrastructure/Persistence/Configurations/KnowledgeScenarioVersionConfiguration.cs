using AIEmployeeSupport.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AIEmployeeSupport.Infrastructure.Persistence.Configurations;

public class KnowledgeScenarioVersionConfiguration : IEntityTypeConfiguration<KnowledgeScenarioVersion>
{
    public void Configure(EntityTypeBuilder<KnowledgeScenarioVersion> builder)
    {
        builder.HasKey(v => v.Id);

        builder.Property(v => v.Version)
            .IsRequired();

        builder.Property(v => v.Name)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(v => v.Description)
            .IsRequired()
            .HasMaxLength(4000);

        builder.Property(v => v.ResolutionStepsSnapshot)
            .IsRequired()
            .HasColumnType("nvarchar(max)");

        builder.Property(v => v.CreatedBy)
            .IsRequired();

        builder.Property(v => v.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        builder.HasOne(v => v.Scenario)
            .WithMany(s => s.Versions)
            .HasForeignKey(v => v.ScenarioId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
