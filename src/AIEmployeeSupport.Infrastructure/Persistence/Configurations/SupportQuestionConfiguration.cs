using AIEmployeeSupport.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AIEmployeeSupport.Infrastructure.Persistence.Configurations;

public class SupportQuestionConfiguration : IEntityTypeConfiguration<SupportQuestion>
{
    public void Configure(EntityTypeBuilder<SupportQuestion> builder)
    {
        builder.HasKey(q => q.Id);

        builder.Property(q => q.QuestionText)
            .IsRequired()
            .HasMaxLength(4000);

        builder.Property(q => q.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(q => q.AnswerText)
            .HasColumnType("nvarchar(max)");

        builder.Property(q => q.EmployeeId)
            .IsRequired();

        builder.HasIndex(q => q.EmployeeId);
        builder.HasIndex(q => q.CreatedAt);

        builder.Property(q => q.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        builder.HasOne(q => q.Employee)
            .WithMany()
            .HasForeignKey(q => q.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(q => q.Scenario)
            .WithMany()
            .HasForeignKey(q => q.ScenarioId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
