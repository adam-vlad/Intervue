using Intervue.Domain.Common;

namespace Intervue.Domain.Entities;

/// <summary>
/// A project entry from the CV (e.g., "E-commerce website built with React and Node.js").
/// </summary>
public class Project : Entity<Guid>
{
    public string Name { get; private set; }
    public string? Description { get; private set; }
    public IReadOnlyList<string> TechnologiesUsed => _technologiesUsed.AsReadOnly();

    private readonly List<string> _technologiesUsed = new();

    private Project(Guid id, string name, string? description, List<string> technologiesUsed)
        : base(id)
    {
        Name = name;
        Description = description;
        _technologiesUsed = technologiesUsed;
    }

    public static Project Create(string name, string? description, List<string>? technologiesUsed = null)
    {
        Guard.AgainstNullOrWhiteSpace(name, nameof(name));

        return new Project(Guid.NewGuid(), name, description, technologiesUsed ?? new List<string>());
    }
}
