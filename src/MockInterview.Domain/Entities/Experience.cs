using MockInterview.Domain.Common;

namespace MockInterview.Domain.Entities;

/// <summary>
/// A work experience entry from the CV (e.g., "Software Developer at Company X, 2 years").
/// </summary>
public class Experience : Entity<Guid>
{
    public string Role { get; private set; }
    public string Company { get; private set; }
    public int DurationMonths { get; private set; }
    public string? Description { get; private set; }

    private Experience(Guid id, string role, string company, int durationMonths, string? description)
        : base(id)
    {
        Role = role;
        Company = company;
        DurationMonths = durationMonths;
        Description = description;
    }

    public static Experience Create(string role, string company, int durationMonths, string? description = null)
    {
        Guard.AgainstNullOrWhiteSpace(role, nameof(role));
        Guard.AgainstNullOrWhiteSpace(company, nameof(company));
        Guard.InRange(durationMonths, 0, 600, nameof(durationMonths));

        return new Experience(Guid.NewGuid(), role, company, durationMonths, description);
    }
}
