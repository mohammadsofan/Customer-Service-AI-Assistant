using AIEmployeeSupport.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AIEmployeeSupport.Infrastructure.Persistence.Configurations;

public class ResolutionStepConfiguration : IEntityTypeConfiguration<ResolutionStep>
{
    public void Configure(EntityTypeBuilder<ResolutionStep> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.StepOrder)
            .IsRequired();

        builder.Property(r => r.StepText)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(r => r.Description)
            .IsRequired(false)
            .HasMaxLength(4000);

        builder.HasIndex(r => new { r.ScenarioId, r.StepOrder });

        builder.Property(r => r.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(r => r.UpdatedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        builder.HasOne(r => r.Scenario)
            .WithMany(s => s.ResolutionSteps)
            .HasForeignKey(r => r.ScenarioId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
