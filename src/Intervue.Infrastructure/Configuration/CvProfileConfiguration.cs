using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Intervue.Domain.Entities;
using Intervue.Domain.Enums;
using Intervue.Domain.ValueObjects;

namespace Intervue.Infrastructure.Configuration;

/// <summary>
/// Tells EF Core how to store CvProfile in the database.
/// Maps properties to columns, sets up relationships to child entities.
/// </summary>
public class CvProfileConfiguration : IEntityTypeConfiguration<CvProfile>
{
    public void Configure(EntityTypeBuilder<CvProfile> builder)
    {
        builder.ToTable("cv_profiles");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).ValueGeneratedNever();

        builder.Property(c => c.RawText).IsRequired();
        builder.Property(c => c.DifficultyLevel)
            .HasConversion<int>()
            .IsRequired();
        builder.Property(c => c.Education);
        builder.Property(c => c.CreatedAt).IsRequired();

        // HashedPersonalData is a Value Object — store it as an "owned type" (embedded columns)
        builder.OwnsOne(c => c.HashedPersonalData, hpd =>
        {
            hpd.Property(h => h.Hash)
                .HasColumnName("hashed_personal_data")
                .IsRequired();
        });

        // Technologies — child entities stored in a separate table
        builder.HasMany(c => c.Technologies)
            .WithOne()
            .HasForeignKey("CvProfileId")
            .OnDelete(DeleteBehavior.Cascade);

        // Experiences — child entities stored in a separate table
        builder.HasMany(c => c.Experiences)
            .WithOne()
            .HasForeignKey("CvProfileId")
            .OnDelete(DeleteBehavior.Cascade);

        // Projects — child entities stored in a separate table
        builder.HasMany(c => c.Projects)
            .WithOne()
            .HasForeignKey("CvProfileId")
            .OnDelete(DeleteBehavior.Cascade);
    }
}
