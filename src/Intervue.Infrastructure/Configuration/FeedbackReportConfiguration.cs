using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Intervue.Domain.Entities;
using Intervue.Domain.ValueObjects;

namespace Intervue.Infrastructure.Configuration;

/// <summary>
/// Tells EF Core how to store FeedbackReport in the database.
/// CategoryScores (list of InterviewScore value objects) are stored as a JSON column.
/// A value converter serialises the list to a JSON string, making this config portable
/// across providers (PostgreSQL jsonb, InMemory, SQLite, etc.).
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

        // Store the list of InterviewScore value objects as JSON.
        // The value converter makes this portable across all EF Core providers.
        builder.Property(f => f.CategoryScores)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<List<InterviewScore>>(v, (JsonSerializerOptions?)null)
                     ?? new List<InterviewScore>())
            .HasColumnType("jsonb");
    }
}
