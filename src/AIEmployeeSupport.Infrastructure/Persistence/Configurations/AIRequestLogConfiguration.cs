using AIEmployeeSupport.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AIEmployeeSupport.Infrastructure.Persistence.Configurations;

public class AIRequestLogConfiguration : IEntityTypeConfiguration<AIRequestLog>
{
    public void Configure(EntityTypeBuilder<AIRequestLog> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.ErrorMessage)
            .HasMaxLength(2000);

        builder.Property(r => r.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        builder.HasIndex(r => r.CreatedAt);

        builder.HasOne<SupportQuestion>()
            .WithMany()
            .HasForeignKey(r => r.QuestionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
