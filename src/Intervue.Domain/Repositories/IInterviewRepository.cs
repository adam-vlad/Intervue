using Intervue.Domain.Entities;

namespace Intervue.Domain.Repositories;

/// <summary>
/// Repository interface specific to Interview aggregate.
/// Extends the generic base with interview-specific queries.
/// </summary>
public interface IInterviewRepository : IRepository<Interview>
{
    Task<IReadOnlyList<Interview>> GetByCvProfileIdAsync(Guid cvProfileId, CancellationToken cancellationToken = default);
}
