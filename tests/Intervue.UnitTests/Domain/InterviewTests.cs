using FluentAssertions;
using Intervue.Domain.Common;
using Intervue.Domain.Entities;
using Intervue.Domain.Enums;
using Intervue.Domain.ValueObjects;

namespace Intervue.UnitTests.Domain;

/// <summary>
/// Unit tests for the Interview aggregate root.
/// Covers: Create, SetPromptProfile, Start, AddCandidateMessage, AddInterviewerMessage, Complete.
/// </summary>
public class InterviewTests
{
    private readonly Guid _validCvProfileId = Guid.NewGuid();

    // ── Create ────────────────────────────────────────────────────

    [Fact]
    public void Create_WithValidCvProfileId_ReturnsInterview()
    {
        var interview = Interview.Create(_validCvProfileId);

        interview.Should().NotBeNull();
        interview.Id.Should().NotBe(Guid.Empty);
        interview.CvProfileId.Should().Be(_validCvProfileId);
        interview.Status.Should().Be(InterviewStatus.NotStarted);
        interview.PromptProfile.Should().BeEmpty();
        interview.FeedbackReport.Should().BeNull();
        interview.Messages.Should().BeEmpty();
        interview.CompletedAt.Should().BeNull();
    }

    [Fact]
    public void Create_WithEmptyGuid_ThrowsDomainException()
    {
        var act = () => Interview.Create(Guid.Empty);

        act.Should().Throw<DomainException>()
           .WithMessage("*cvProfileId*");
    }

    // ── SetPromptProfile ──────────────────────────────────────────

    [Theory]
    [InlineData("Junior_v1")]
    [InlineData("Senior_v1")]
    [InlineData("Mid_v2")]
    public void SetPromptProfile_WithValidValue_SetsProperty(string profile)
    {
        var interview = Interview.Create(_validCvProfileId);

        interview.SetPromptProfile(profile);

        interview.PromptProfile.Should().Be(profile);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void SetPromptProfile_WithInvalidValue_ThrowsDomainException(string? profile)
    {
        var interview = Interview.Create(_validCvProfileId);

        var act = () => interview.SetPromptProfile(profile!);

        act.Should().Throw<DomainException>()
           .WithMessage("*promptProfile*");
    }

    // ── Start ─────────────────────────────────────────────────────

    [Fact]
    public void Start_WhenNotStarted_TransitionsToInProgress()
    {
        var interview = Interview.Create(_validCvProfileId);

        interview.Start("Hello, let's begin! What is dependency injection?");

        interview.Status.Should().Be(InterviewStatus.InProgress);
        interview.Messages.Should().HaveCount(1);
        interview.Messages[0].Role.Should().Be(MessageRole.Interviewer);
        interview.Messages[0].Content.Should().Contain("dependency injection");
    }

    [Fact]
    public void Start_WhenAlreadyInProgress_ThrowsDomainException()
    {
        var interview = CreateStartedInterview();

        var act = () => interview.Start("Another question?");

        act.Should().Throw<DomainException>()
           .WithMessage("*NotStarted*");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Start_WithEmptyQuestion_ThrowsDomainException(string? question)
    {
        var interview = Interview.Create(_validCvProfileId);

        var act = () => interview.Start(question!);

        act.Should().Throw<DomainException>();
    }

    // ── AddCandidateMessage ───────────────────────────────────────

    [Fact]
    public void AddCandidateMessage_AfterInterviewerMessage_Succeeds()
    {
        var interview = CreateStartedInterview();

        interview.AddCandidateMessage("DI is a design pattern for loose coupling.");

        interview.Messages.Should().HaveCount(2);
        interview.Messages[1].Role.Should().Be(MessageRole.Candidate);
    }

    [Fact]
    public void AddCandidateMessage_TwiceInARow_ThrowsDomainException()
    {
        var interview = CreateStartedInterview();
        interview.AddCandidateMessage("First answer");

        var act = () => interview.AddCandidateMessage("Second answer");

        act.Should().Throw<DomainException>()
           .WithMessage("*two candidate messages*");
    }

    [Fact]
    public void AddCandidateMessage_WhenNotStarted_ThrowsDomainException()
    {
        var interview = Interview.Create(_validCvProfileId);

        var act = () => interview.AddCandidateMessage("Some answer");

        act.Should().Throw<DomainException>()
           .WithMessage("*InProgress*");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void AddCandidateMessage_WithEmptyContent_ThrowsDomainException(string? content)
    {
        var interview = CreateStartedInterview();

        var act = () => interview.AddCandidateMessage(content!);

        act.Should().Throw<DomainException>();
    }

    // ── AddInterviewerMessage ─────────────────────────────────────

    [Fact]
    public void AddInterviewerMessage_AfterCandidateMessage_Succeeds()
    {
        var interview = CreateStartedInterview();
        interview.AddCandidateMessage("My answer");

        interview.AddInterviewerMessage("Follow-up question");

        interview.Messages.Should().HaveCount(3);
        interview.Messages[2].Role.Should().Be(MessageRole.Interviewer);
    }

    [Fact]
    public void AddInterviewerMessage_TwiceInARow_ThrowsDomainException()
    {
        var interview = CreateStartedInterview();

        // Interview started with an interviewer message, so adding another should fail
        var act = () => interview.AddInterviewerMessage("Another question");

        act.Should().Throw<DomainException>()
           .WithMessage("*two interviewer messages*");
    }

    [Fact]
    public void AddInterviewerMessage_WhenNotStarted_ThrowsDomainException()
    {
        var interview = Interview.Create(_validCvProfileId);

        var act = () => interview.AddInterviewerMessage("Question");

        act.Should().Throw<DomainException>()
           .WithMessage("*InProgress*");
    }

    // ── Complete ──────────────────────────────────────────────────

    [Fact]
    public void Complete_WithEnoughMessages_TransitionsToCompleted()
    {
        var interview = CreateInterviewWithMessages(3);
        var feedback = CreateFeedbackReport();

        interview.Complete(feedback);

        interview.Status.Should().Be(InterviewStatus.Completed);
        interview.CompletedAt.Should().NotBeNull();
        interview.FeedbackReport.Should().Be(feedback);
    }

    [Fact]
    public void Complete_WithTooFewCandidateMessages_ThrowsDomainException()
    {
        var interview = CreateInterviewWithMessages(2);
        var feedback = CreateFeedbackReport();

        var act = () => interview.Complete(feedback);

        act.Should().Throw<DomainException>()
           .WithMessage("*3 candidate responses*");
    }

    [Fact]
    public void Complete_WhenNotInProgress_ThrowsDomainException()
    {
        var interview = Interview.Create(_validCvProfileId);
        var feedback = CreateFeedbackReport();

        var act = () => interview.Complete(feedback);

        act.Should().Throw<DomainException>()
           .WithMessage("*InProgress*");
    }

    [Fact]
    public void Complete_WithNullFeedback_ThrowsDomainException()
    {
        var interview = CreateInterviewWithMessages(3);

        var act = () => interview.Complete(null!);

        act.Should().Throw<DomainException>()
           .WithMessage("*feedbackReport*");
    }

    [Fact]
    public void Complete_WhenAlreadyCompleted_ThrowsDomainException()
    {
        var interview = CreateInterviewWithMessages(3);
        interview.Complete(CreateFeedbackReport());

        var act = () => interview.Complete(CreateFeedbackReport());

        act.Should().Throw<DomainException>()
           .WithMessage("*InProgress*");
    }

    // ── Message alternation integration ───────────────────────────

    [Fact]
    public void FullConversationFlow_AlternatesCorrectly()
    {
        var interview = Interview.Create(_validCvProfileId);
        interview.SetPromptProfile("Junior_v1");
        interview.Start("Q1: What is C#?");

        interview.AddCandidateMessage("A1");
        interview.AddInterviewerMessage("Q2");
        interview.AddCandidateMessage("A2");
        interview.AddInterviewerMessage("Q3");
        interview.AddCandidateMessage("A3");

        interview.Messages.Should().HaveCount(6);
        interview.Messages[0].Role.Should().Be(MessageRole.Interviewer);
        interview.Messages[1].Role.Should().Be(MessageRole.Candidate);
        interview.Messages[2].Role.Should().Be(MessageRole.Interviewer);
        interview.Messages[3].Role.Should().Be(MessageRole.Candidate);
        interview.Messages[4].Role.Should().Be(MessageRole.Interviewer);
        interview.Messages[5].Role.Should().Be(MessageRole.Candidate);
    }

    // ── Helpers ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─

    private Interview CreateStartedInterview()
    {
        var interview = Interview.Create(_validCvProfileId);
        interview.Start("First question?");
        return interview;
    }

    /// <summary>Creates a started interview with the specified number of candidate messages (and matching interviewer follow-ups).</summary>
    private Interview CreateInterviewWithMessages(int candidateMessageCount)
    {
        var interview = Interview.Create(_validCvProfileId);
        interview.Start("Q1?");

        for (int i = 0; i < candidateMessageCount; i++)
        {
            interview.AddCandidateMessage($"Answer {i + 1}");
            if (i < candidateMessageCount - 1) // Don't add follow-up after last answer
                interview.AddInterviewerMessage($"Q{i + 2}?");
        }

        return interview;
    }

    private static FeedbackReport CreateFeedbackReport()
    {
        var scores = new List<InterviewScore>
        {
            new("Technical Knowledge", 80),
            new("Problem Solving", 75)
        };

        return FeedbackReport.Create(78, scores, "Good knowledge", "Needs practice", "Study more");
    }
}
