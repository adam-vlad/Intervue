using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Intervue.Domain.Entities;
using Intervue.Domain.ValueObjects;

namespace Intervue.Infrastructure.Configuration;

/// <summary>
/// Tells EF Core how to store FeedbackReport in the database.
/// CategoryScores (list of InterviewScore value objects) are stored as a JSON column.
/// </summary>
public class FeedbackReportConfiguration : IEntityTypeConfiguration<FeedbackReport>
{
    public void Configure(EntityTypeBuilder<FeedbackReport> builder)
    {
        builder.ToTable("feedback_reports");

        builder.HasKey(f => f.Id);
        builder.Property(f => f.Id).ValueGeneratedNever();

        builder.Property(f => f.OverallScore).IsRequired();
        builder.Property(f => f.Strengths).IsRequired();
        builder.Property(f => f.Weaknesses).IsRequired();
        builder.Property(f => f.Suggestions).IsRequired();
        builder.Property(f => f.GeneratedAt).IsRequired();

        // Store the list of InterviewScore value objects as JSON
        builder.Property(f => f.CategoryScores)
            .HasColumnType("jsonb");
    }
}
