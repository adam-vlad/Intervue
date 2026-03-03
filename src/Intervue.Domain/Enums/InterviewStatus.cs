namespace Intervue.Domain.Enums;

/// <summary>
/// The lifecycle of an interview: not started → in progress → completed.
/// </summary>
public enum InterviewStatus
{
    NotStarted = 0,
    InProgress = 1,
    Completed = 2
}
