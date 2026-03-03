using System.Collections.Concurrent;
using Intervue.Domain.Entities;
using Intervue.Domain.Repositories;

namespace Intervue.Infrastructure.Persistence;

/// <summary>
/// In-memory repository for CvProfile using ConcurrentDictionary.
/// No database for now — data lives in memory and is lost when the app restarts.
/// ConcurrentDictionary is thread-safe for concurrent access.
/// </summary>
public class InMemoryCvProfileRepository : ICvProfileRepository
{
    private readonly ConcurrentDictionary<Guid, CvProfile> _store = new();

    public Task<CvProfile?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _store.TryGetValue(id, out var profile);
        return Task.FromResult(profile);
    }

    public Task<IReadOnlyList<CvProfile>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<CvProfile> all = _store.Values.ToList().AsReadOnly();
        return Task.FromResult(all);
    }

    public Task AddAsync(CvProfile entity, CancellationToken cancellationToken = default)
    {
        _store.TryAdd(entity.Id, entity);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(CvProfile entity, CancellationToken cancellationToken = default)
    {
        _store[entity.Id] = entity;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _store.TryRemove(id, out _);
        return Task.CompletedTask;
    }
}
