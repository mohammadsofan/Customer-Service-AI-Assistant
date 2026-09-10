using AIEmployeeSupport.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AIEmployeeSupport.Infrastructure.Persistence.Configurations;

public class KnowledgeKeywordConfiguration : IEntityTypeConfiguration<KnowledgeKeyword>
{
    public void Configure(EntityTypeBuilder<KnowledgeKeyword> builder)
    {
        builder.HasKey(k => k.Id);

        builder.Property(k => k.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(k => k.Name)
            .IsUnique();

        builder.Property(k => k.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");
    }
}
