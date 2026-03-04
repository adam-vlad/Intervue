using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Intervue.Domain.Entities;
using Intervue.Domain.Enums;

namespace Intervue.Infrastructure.Configuration;

/// <summary>
/// Tells EF Core how to store Interview in the database.
/// </summary>
public class InterviewConfiguration : IEntityTypeConfiguration<Interview>
{
    public void Configure(EntityTypeBuilder<Interview> builder)
    {
        builder.ToTable("interviews");

        builder.HasKey(i => i.Id);
        builder.Property(i => i.Id).ValueGeneratedNever();

        builder.Property(i => i.CvProfileId).IsRequired();
        builder.Property(i => i.Status)
            .HasConversion<int>()
            .IsRequired();
        builder.Property(i => i.StartedAt).IsRequired();
        builder.Property(i => i.CompletedAt);

        // Messages — child entities stored in a separate table
        builder.HasMany(i => i.Messages)
            .WithOne()
            .HasForeignKey("InterviewId")
            .OnDelete(DeleteBehavior.Cascade);

        // FeedbackReport — owned entity (one-to-one, stored in a separate table)
        builder.HasOne(i => i.FeedbackReport)
            .WithOne()
            .HasForeignKey<FeedbackReport>("InterviewId")
            .OnDelete(DeleteBehavior.Cascade);
    }
}
