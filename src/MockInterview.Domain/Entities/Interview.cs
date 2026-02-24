using MockInterview.Domain.Common;
using MockInterview.Domain.Enums;

namespace MockInterview.Domain.Entities;

/// <summary>
/// Aggregate Root — represents a mock interview session.
/// Manages the conversation flow: start → messages → complete with feedback.
/// </summary>
public class Interview : AggregateRoot<Guid>
{
    public Guid CvProfileId { get; private set; }
    public InterviewStatus Status { get; private set; }
    public FeedbackReport? FeedbackReport { get; private set; }
    public DateTime StartedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }

    private readonly List<InterviewMessage> _messages = new();
    public IReadOnlyList<InterviewMessage> Messages => _messages.AsReadOnly();

    private Interview(Guid id, Guid cvProfileId) : base(id)
    {
        CvProfileId = cvProfileId;
        Status = InterviewStatus.NotStarted;
        StartedAt = DateTime.UtcNow;
    }

    /// <summary>Factory method — creates a new interview linked to a CV profile.</summary>
    public static Interview Create(Guid cvProfileId)
    {
        Guard.AgainstEmpty(cvProfileId, nameof(cvProfileId));

        return new Interview(Guid.NewGuid(), cvProfileId);
    }

    /// <summary>Starts the interview with the first AI question. Transitions NotStarted → InProgress.</summary>
    public void Start(string firstQuestion)
    {
        if (Status != InterviewStatus.NotStarted)
            throw new DomainException("Interview can only be started when status is NotStarted.");

        Guard.AgainstNullOrWhiteSpace(firstQuestion, nameof(firstQuestion));

        Status = InterviewStatus.InProgress;
        _messages.Add(InterviewMessage.Create(MessageRole.Interviewer, firstQuestion));
    }

    /// <summary>Adds a candidate's answer to the conversation.</summary>
    public void AddCandidateMessage(string content)
    {
        EnsureInProgress();
        Guard.AgainstNullOrWhiteSpace(content, nameof(content));

        // Last message should be from Interviewer (candidate replies to a question)
        if (_messages.Count > 0 && _messages[^1].Role == MessageRole.Candidate)
            throw new DomainException("Cannot add two candidate messages in a row. Wait for interviewer response.");

        _messages.Add(InterviewMessage.Create(MessageRole.Candidate, content));
    }

    /// <summary>Adds the AI interviewer's follow-up question.</summary>
    public void AddInterviewerMessage(string content)
    {
        EnsureInProgress();
        Guard.AgainstNullOrWhiteSpace(content, nameof(content));

        // Last message should be from Candidate (interviewer follows up after an answer)
        if (_messages.Count > 0 && _messages[^1].Role == MessageRole.Interviewer)
            throw new DomainException("Cannot add two interviewer messages in a row. Wait for candidate response.");

        _messages.Add(InterviewMessage.Create(MessageRole.Interviewer, content));
    }

    /// <summary>
    /// Completes the interview with a feedback report. 
    /// Requires at least 3 candidate messages. Transitions InProgress → Completed.
    /// </summary>
    public void Complete(FeedbackReport feedbackReport)
    {
        EnsureInProgress();
        Guard.AgainstNull(feedbackReport, nameof(feedbackReport));

        var candidateMessageCount = _messages.Count(m => m.Role == MessageRole.Candidate);
        if (candidateMessageCount < 3)
            throw new DomainException(
                $"At least 3 candidate responses are required before completing. Got: {candidateMessageCount}.");

        Status = InterviewStatus.Completed;
        CompletedAt = DateTime.UtcNow;
        FeedbackReport = feedbackReport;
    }

    private void EnsureInProgress()
    {
        if (Status != InterviewStatus.InProgress)
            throw new DomainException($"Interview must be InProgress. Current status: {Status}.");
    }
}
