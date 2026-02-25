using MockInterview.Domain.Enums;

namespace MockInterview.Application.Features.DTOs;

/// <summary>
/// Data Transfer Object for CvProfile — sent to the client as JSON.
/// Contains only the data the client needs, not internal domain details.
/// </summary>
public record CvProfileDto(
    Guid Id,
    string RawText,
    DifficultyLevel DifficultyLevel,
    string? Education,
    DateTime CreatedAt,
    List<TechnologyDto> Technologies,
    List<ExperienceDto> Experiences,
    List<ProjectDto> Projects);

public record TechnologyDto(string Name, int YearsOfExperience);

public record ExperienceDto(string Role, string Company, int DurationMonths, string? Description);

public record ProjectDto(string Name, string? Description, List<string> TechnologiesUsed);
