using Microsoft.EntityFrameworkCore;
using Intervue.Domain.Entities;

namespace Intervue.Infrastructure.Persistence;

/// <summary>
/// The main database context — EF Core uses this to talk to PostgreSQL.
/// Think of it as the "bridge" between your C# code and the database tables.
/// </summary>
public class IntervueDbContext : DbContext
{
    // Each DbSet = one table in the database
    public DbSet<CvProfile> CvProfiles => Set<CvProfile>();
    public DbSet<Interview> Interviews => Set<Interview>();

    public IntervueDbContext(DbContextOptions<IntervueDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// This is where we tell EF Core how to map our C# classes to database tables.
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Apply all configurations from separate files in this assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(IntervueDbContext).Assembly);
    }
}
