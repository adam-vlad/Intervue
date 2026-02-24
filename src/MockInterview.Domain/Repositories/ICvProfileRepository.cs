using MockInterview.Domain.Entities;

namespace MockInterview.Domain.Repositories;

/// <summary>
/// Repository interface specific to CvProfile aggregate.
/// Can be extended with CV-specific queries later.
/// </summary>
public interface ICvProfileRepository : IRepository<CvProfile>
{
}
