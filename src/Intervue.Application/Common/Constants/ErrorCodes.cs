namespace Intervue.Application.Common.Constants;

/// <summary>
/// Centralized error codes used in Result failures across all handlers.
/// Prevents magic strings and ensures consistency.
/// </summary>
public static class ErrorCodes
{
    // ── CV-related errors ────────────────────────────────────────────
    public const string CvNotFound = "Cv.NotFound";
    public const string CvInvalidPdf = "Cv.InvalidPdf";
    public const string CvEmptyText = "Cv.EmptyText";
    public const string CvParseFailed = "Cv.ParseFailed";

    // ── Interview-related errors ─────────────────────────────────────
    public const string InterviewNotFound = "Interview.NotFound";
    public const string InterviewNotInProgress = "Interview.NotInProgress";

    // ── Feedback-related errors ──────────────────────────────────────
    public const string FeedbackParseFailed = "Feedback.ParseFailed";

    // ── Pipeline / cross-cutting errors ──────────────────────────────
    public const string DomainRuleViolation = "Domain.RuleViolation";
    public const string EntityNotFound = "Entity.NotFound";
    public const string UnexpectedError = "Unexpected.Error";
}
