using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Intervue.Domain.Entities;

namespace Intervue.Infrastructure.Configuration;

/// <summary>
/// Tells EF Core how to store Project in the database.
/// TechnologiesUsed is a list of strings — stored as a JSON column in PostgreSQL.
/// </summary>
public class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder.ToTable("projects");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedNever();

        builder.Property(p => p.Name).HasMaxLength(500).IsRequired();
        builder.Property(p => p.Description);

        // Store the List<string> as a JSON column (PostgreSQL supports this natively)
        builder.Property(p => p.TechnologiesUsed)
            .HasColumnType("jsonb");
    }
}
