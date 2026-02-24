using MockInterview.Domain.Entities;

namespace MockInterview.Domain.Repositories;

/// <summary>
/// Repository interface specific to Interview aggregate.
/// Can be extended with interview-specific queries later.
/// </summary>
public interface IInterviewRepository : IRepository<Interview>
{
}
