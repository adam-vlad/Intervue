using System.Collections.Concurrent;
using Intervue.Domain.Entities;
using Intervue.Domain.Repositories;

namespace Intervue.Infrastructure.Persistence;

/// <summary>
/// In-memory repository for Interview using ConcurrentDictionary.
/// Same pattern as InMemoryCvProfileRepository — thread-safe, no database.
/// </summary>
public class InMemoryInterviewRepository : IInterviewRepository
{
    private readonly ConcurrentDictionary<Guid, Interview> _store = new();

    public Task<Interview?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _store.TryGetValue(id, out var interview);
        return Task.FromResult(interview);
    }

    public Task<IReadOnlyList<Interview>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Interview> all = _store.Values.ToList().AsReadOnly();
        return Task.FromResult(all);
    }

    public Task AddAsync(Interview entity, CancellationToken cancellationToken = default)
    {
        _store.TryAdd(entity.Id, entity);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Interview entity, CancellationToken cancellationToken = default)
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
