using MockInterview.Domain.Common;
using MockInterview.Domain.Enums;
using MockInterview.Domain.ValueObjects;

namespace MockInterview.Domain.Entities;

/// <summary>
/// Aggregate Root — represents a parsed CV profile.
/// Created when the user uploads a PDF. Populated when the LLM parses the text.
/// </summary>
public class CvProfile : AggregateRoot<Guid>
{
    public string RawText { get; private set; }
    public HashedPersonalData HashedPersonalData { get; private set; }
    public DifficultyLevel DifficultyLevel { get; private set; }
    public string? Education { get; private set; }
    public DateTime CreatedAt { get; private set; }

    // Private backing fields — only the aggregate root can modify these collections
    private readonly List<Technology> _technologies = new();
    private readonly List<Experience> _experiences = new();
    private readonly List<Project> _projects = new();

    // Public read-only access to the collections
    public IReadOnlyList<Technology> Technologies => _technologies.AsReadOnly();
    public IReadOnlyList<Experience> Experiences => _experiences.AsReadOnly();
    public IReadOnlyList<Project> Projects => _projects.AsReadOnly();

    private CvProfile(Guid id, string rawText, HashedPersonalData hashedPersonalData)
        : base(id)
    {
        RawText = rawText;
        HashedPersonalData = hashedPersonalData;
        DifficultyLevel = DifficultyLevel.Junior; // default until LLM determines it
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Factory method — creates a new CvProfile from raw text and hashed personal data.
    /// </summary>
    public static CvProfile Create(string rawText, HashedPersonalData hashedPersonalData)
    {
        Guard.AgainstNullOrWhiteSpace(rawText, nameof(rawText));
        Guard.AgainstNull(hashedPersonalData, nameof(hashedPersonalData));

        return new CvProfile(Guid.NewGuid(), rawText, hashedPersonalData);
    }

    /// <summary>
    /// Called after the LLM parses the CV text — populates technologies, experiences, projects, education, and level.
    /// </summary>
    public void SetParsedData(
        DifficultyLevel difficultyLevel,
        string? education,
        List<Technology> technologies,
        List<Experience> experiences,
        List<Project> projects)
    {
        DifficultyLevel = difficultyLevel;
        Education = education;

        _technologies.Clear();
        _technologies.AddRange(technologies);

        _experiences.Clear();
        _experiences.AddRange(experiences);

        _projects.Clear();
        _projects.AddRange(projects);
    }

    /// <summary>
    /// Returns the top N technologies by years of experience (for question generation).
    /// </summary>
    public IReadOnlyList<Technology> GetTopTechnologies(int count)
    {
        return _technologies
            .OrderByDescending(t => t.YearsOfExperience)
            .Take(count)
            .ToList()
            .AsReadOnly();
    }
}
