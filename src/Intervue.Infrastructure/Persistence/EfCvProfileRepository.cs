using Microsoft.EntityFrameworkCore;
using Intervue.Domain.Entities;
using Intervue.Domain.Repositories;

namespace Intervue.Infrastructure.Persistence;

/// <summary>
/// Repository for CvProfile backed by PostgreSQL via EF Core.
/// </summary>
public class EfCvProfileRepository : ICvProfileRepository
{
    private readonly IntervueDbContext _db;

    public EfCvProfileRepository(IntervueDbContext db)
    {
        _db = db;
    }

    public async Task<CvProfile?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _db.CvProfiles
            .Include(c => c.Technologies)
            .Include(c => c.Experiences)
            .Include(c => c.Projects)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<CvProfile>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _db.CvProfiles
            .Include(c => c.Technologies)
            .Include(c => c.Experiences)
            .Include(c => c.Projects)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(CvProfile entity, CancellationToken cancellationToken = default)
    {
        await _db.CvProfiles.AddAsync(entity, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(CvProfile entity, CancellationToken cancellationToken = default)
    {
        // Entity is already tracked by EF Core from GetByIdAsync,
        // so just SaveChanges — change tracker detects modifications automatically.
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _db.CvProfiles.FindAsync([id], cancellationToken);
        if (entity is not null)
        {
            _db.CvProfiles.Remove(entity);
            await _db.SaveChangesAsync(cancellationToken);
        }
    }
}
