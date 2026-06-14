using Intervue.Domain.Enums;

namespace Intervue.Application.Features.DTOs;

/// <summary>
/// Lightweight DTO for interviews in list views — excludes messages and full feedback.
/// </summary>
public record InterviewSummaryDto(
    Guid Id,
    Guid CvProfileId,
    InterviewStatus Status,
    DateTime StartedAt,
    DateTime? CompletedAt,
    int MessageCount,
    bool HasFeedback,
    int? OverallScore);
