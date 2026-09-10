using AIEmployeeSupport.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AIEmployeeSupport.Infrastructure.Persistence.Configurations;

public class ScenarioKeywordConfiguration : IEntityTypeConfiguration<ScenarioKeyword>
{
    public void Configure(EntityTypeBuilder<ScenarioKeyword> builder)
    {
        builder.HasKey(sk => new { sk.ScenarioId, sk.KeywordId });

        builder.HasOne(sk => sk.Scenario)
            .WithMany(s => s.ScenarioKeywords)
            .HasForeignKey(sk => sk.ScenarioId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(sk => sk.Keyword)
            .WithMany(k => k.ScenarioKeywords)
            .HasForeignKey(sk => sk.KeywordId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
