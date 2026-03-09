using Intervue.Domain.Entities;

namespace Intervue.Application.Features.DTOs;

/// <summary>
/// Extension methods to map domain entities to DTOs.
/// Keeps mapping logic in one place.
/// </summary>
public static class MappingExtensions
{
    public static CvProfileDto ToDto(this CvProfile profile)
    {
        return new CvProfileDto(
            profile.Id,
            profile.RawText,
            profile.DifficultyLevel,
            profile.Education,
            profile.CreatedAt,
            profile.Technologies.Select(t => new TechnologyDto(t.Name, t.YearsOfExperience)).ToList(),
            profile.Experiences.Select(e => new ExperienceDto(e.Role, e.Company, e.DurationMonths, e.Description)).ToList(),
            profile.Projects.Select(p => new ProjectDto(p.Name, p.Description, p.TechnologiesUsed.ToList())).ToList());
    }

    public static InterviewDto ToDto(this Domain.Entities.Interview interview)
    {
        return new InterviewDto(
            interview.Id,
            interview.CvProfileId,
            interview.Status,
            interview.PromptProfile,
            interview.StartedAt,
            interview.CompletedAt,
            interview.Messages.Select(m => m.ToDto()).ToList(),
            interview.FeedbackReport?.ToDto());
    }

    public static InterviewMessageDto ToDto(this InterviewMessage message)
    {
        return new InterviewMessageDto(
            message.Id,
            message.Role,
            message.Content,
            message.SentAt);
    }

    public static FeedbackReportDto ToDto(this FeedbackReport report)
    {
        return new FeedbackReportDto(
            report.Id,
            report.OverallScore,
            report.CategoryScores.Select(s => new InterviewScoreDto(s.Category, s.Score)).ToList(),
            report.Strengths,
            report.Weaknesses,
            report.Suggestions,
            report.GeneratedAt);
    }
}
