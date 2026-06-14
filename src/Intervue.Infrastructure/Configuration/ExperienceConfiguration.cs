using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Intervue.Domain.Entities;

namespace Intervue.Infrastructure.Configuration;

/// <summary>
/// Tells EF Core how to store Experience in the database.
/// </summary>
public class ExperienceConfiguration : IEntityTypeConfiguration<Experience>
{
    public void Configure(EntityTypeBuilder<Experience> builder)
    {
        builder.ToTable("experiences");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();

        builder.Property(e => e.Role).HasMaxLength(500).IsRequired();
        builder.Property(e => e.Company).HasMaxLength(500).IsRequired();
        builder.Property(e => e.DurationMonths).IsRequired();
        builder.Property(e => e.Description);
    }
}
