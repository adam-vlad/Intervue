using Intervue.Domain.Enums;

namespace Intervue.Application.Features.DTOs;

/// <summary>
/// Data Transfer Object for a single interview message.
/// </summary>
public record InterviewMessageDto(
    Guid Id,
    MessageRole Role,
    string Content,
    DateTime SentAt);
