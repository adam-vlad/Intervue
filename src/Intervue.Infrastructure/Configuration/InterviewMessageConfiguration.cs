using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Intervue.Domain.Entities;
using Intervue.Domain.Enums;

namespace Intervue.Infrastructure.Configuration;

/// <summary>
/// Tells EF Core how to store InterviewMessage in the database.
/// </summary>
public class InterviewMessageConfiguration : IEntityTypeConfiguration<InterviewMessage>
{
    public void Configure(EntityTypeBuilder<InterviewMessage> builder)
    {
        builder.ToTable("interview_messages");

        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).ValueGeneratedNever();

        builder.Property(m => m.Role)
            .HasConversion<int>()
            .IsRequired();
        builder.Property(m => m.Content).IsRequired();
        builder.Property(m => m.SentAt).IsRequired();
    }
}
