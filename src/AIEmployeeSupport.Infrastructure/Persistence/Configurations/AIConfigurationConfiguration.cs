using AIEmployeeSupport.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AIEmployeeSupport.Infrastructure.Persistence.Configurations;

public class AIConfigurationConfiguration : IEntityTypeConfiguration<AIConfiguration>
{
    public void Configure(EntityTypeBuilder<AIConfiguration> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Temperature)
            .IsRequired()
            .HasDefaultValue(0.7);

        builder.Property(c => c.MaxTokens)
            .IsRequired()
            .HasDefaultValue(1024);

        builder.Property(c => c.SimilarityThreshold)
            .IsRequired()
            .HasDefaultValue(0.7);

        builder.Property(c => c.TopK)
            .IsRequired()
            .HasDefaultValue(5);

        builder.Property(c => c.SystemPrompt)
            .IsRequired()
            .HasColumnType("nvarchar(max)");

        builder.Property(c => c.EnableAutoFailover)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(c => c.UpdatedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        builder.HasOne(c => c.ActiveProvider)
            .WithMany()
            .HasForeignKey(c => c.ActiveProviderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.ActiveModel)
            .WithMany()
            .HasForeignKey(c => c.ActiveModelId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
