using MockInterview.Domain.Common;
using MockInterview.Domain.Enums;

namespace MockInterview.Domain.Entities;

/// <summary>
/// A single message in theinterview conversation.
/// Either the AI interviewer asks a question, or the candidate answers.
/// </summary>
public class InterviewMessage : Entity<Guid>
{
    public MessageRole Role { get; private set; }
    public string Content { get; private set; }
    public DateTime SentAt { get; private set; }

    private InterviewMessage(Guid id, MessageRole role, string content, DateTime sentAt)
        : base(id)
    {
        Role = role;
        Content = content;
        SentAt = sentAt;
    }

    public static InterviewMessage Create(MessageRole role, string content)
    {
        Guard.AgainstNullOrWhiteSpace(content, nameof(content));

        return new InterviewMessage(Guid.NewGuid(), role, content, DateTime.UtcNow);
    }
}
