using Intervue.Domain.Common;

namespace Intervue.Domain.Entities;

/// <summary>
/// A technology/skill extracted from the CV (e.g., "C#", "React", "Docker").
/// </summary>
public class Technology : Entity<Guid>
{
    public string Name { get; private set; }
    public int YearsOfExperience { get; private set; }

    // Required by EF Core for database loading
    private Technology() : base(default!) { Name = default!; }

    private Technology(Guid id, string name, int yearsOfExperience) : base(id)
    {
        Name = name;
        YearsOfExperience = yearsOfExperience;
    }

    public static Technology Create(string name, int yearsOfExperience)
    {
        Guard.AgainstNullOrWhiteSpace(name, nameof(name));
        Guard.InRange(yearsOfExperience, 0, 50, nameof(yearsOfExperience));

        return new Technology(Guid.NewGuid(), name, yearsOfExperience);
    }
}
