using MockInterview.Domain.Common;

namespace MockInterview.Domain.ValueObjects;

/// <summary>
/// Value Object representing a skill level for a specific technology (e.g., "C# — Advanced").
/// </summary>
public sealed record SkillLevel
{
    public string Name { get; }
    public int YearsOfExperience { get; }

    public SkillLevel(string name, int yearsOfExperience)
    {
        Name = Guard.AgainstNullOrWhiteSpace(name, nameof(name));
        YearsOfExperience = Guard.InRange(yearsOfExperience, 0, 50, nameof(yearsOfExperience));
    }
}
