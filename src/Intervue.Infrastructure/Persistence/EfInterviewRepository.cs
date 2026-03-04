using Microsoft.EntityFrameworkCore;
using Intervue.Domain.Entities;
using Intervue.Domain.Repositories;

namespace Intervue.Infrastructure.Persistence;

/// <summary>
/// Repository for Interview backed by PostgreSQL via EF Core.
/// </summary>
public class EfInterviewRepository : IInterviewRepository
{
    private readonly IntervueDbContext _db;

    public EfInterviewRepository(IntervueDbContext db)
    {
        _db = db;
    }

    public async Task<Interview?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _db.Interviews
            .Include(i => i.Messages.OrderBy(m => m.SentAt))
            .Include(i => i.FeedbackReport)
            .FirstOrDefaultAsync(i => i.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Interview>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _db.Interviews
            .Include(i => i.Messages.OrderBy(m => m.SentAt))
            .Include(i => i.FeedbackReport)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Interview entity, CancellationToken cancellationToken = default)
    {
        await _db.Interviews.AddAsync(entity, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Interview entity, CancellationToken cancellationToken = default)
    {
        // Entity is already tracked by EF Core from GetByIdAsync,
        // so just SaveChanges — change tracker detects modifications automatically.
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _db.Interviews.FindAsync([id], cancellationToken);
        if (entity is not null)
        {
            _db.Interviews.Remove(entity);
            await _db.SaveChangesAsync(cancellationToken);
        }
    }
}
