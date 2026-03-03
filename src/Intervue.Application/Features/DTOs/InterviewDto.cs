using Intervue.Domain.Enums;

namespace Intervue.Application.Features.DTOs;

/// <summary>
/// Data Transfer Object for Interview — includes all messages and optional feedback.
/// </summary>
public record InterviewDto(
    Guid Id,
    Guid CvProfileId,
    InterviewStatus Status,
    DateTime StartedAt,
    DateTime? CompletedAt,
    List<InterviewMessageDto> Messages,
    FeedbackReportDto? FeedbackReport);
