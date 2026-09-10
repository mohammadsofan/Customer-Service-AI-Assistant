using AIEmployeeSupport.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AIEmployeeSupport.Infrastructure.Persistence.Configurations;

public class KnowledgeEmbeddingConfiguration : IEntityTypeConfiguration<KnowledgeEmbedding>
{
    public void Configure(EntityTypeBuilder<KnowledgeEmbedding> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Content)
            .IsRequired()
            .HasColumnType("nvarchar(max)");

        // Store serialized float array as varbinary(max)
        // Vector similarity computed in C# application layer (cosine similarity), not in SQL
        builder.Property(e => e.Embedding)
            .IsRequired()
            .HasColumnType("varbinary(max)");

        builder.Property(e => e.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(e => e.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(e => e.UpdatedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        builder.HasOne(e => e.Scenario)
            .WithOne(s => s.Embedding)
            .HasForeignKey<KnowledgeEmbedding>(e => e.ScenarioId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
