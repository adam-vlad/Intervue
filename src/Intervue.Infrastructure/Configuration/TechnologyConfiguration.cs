using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Intervue.Domain.Entities;

namespace Intervue.Infrastructure.Configuration;

/// <summary>
/// Tells EF Core how to store Technology in the database.
/// </summary>
public class TechnologyConfiguration : IEntityTypeConfiguration<Technology>
{
    public void Configure(EntityTypeBuilder<Technology> builder)
    {
        builder.ToTable("technologies");

        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).ValueGeneratedNever();

        builder.Property(t => t.Name).HasMaxLength(200).IsRequired();
        builder.Property(t => t.YearsOfExperience).IsRequired();
    }
}
