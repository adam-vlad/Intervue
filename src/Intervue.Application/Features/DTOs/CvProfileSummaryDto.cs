using Intervue.Domain.Enums;

namespace Intervue.Application.Features.DTOs;

/// <summary>
/// Lightweight DTO for CV profiles in list views — excludes heavy fields like RawText.
/// </summary>
public record CvProfileSummaryDto(
    Guid Id,
    DifficultyLevel DifficultyLevel,
    string? Education,
    DateTime CreatedAt,
    List<string> Technologies);
