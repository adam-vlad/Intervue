using System.Text;
using System.Text.Json;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Intervue.Api.Extensions;
using Intervue.Application.Common.Constants;
using Intervue.Application.Common.Interfaces;
using Intervue.Application.Common.Prompts;
using Intervue.Application.Features.Interview.GenerateFeedback;
using Intervue.Application.Features.Interview.GetAllInterviews;
using Intervue.Application.Features.Interview.GetInterview;
using Intervue.Application.Features.Interview.SendMessage;
using Intervue.Application.Features.Interview.StartInterview;
using Intervue.Domain.Enums;
using Intervue.Domain.Repositories;

namespace Intervue.Api.Controllers;

/// <summary>
/// Controller for Interview-related endpoints: start, message, feedback, and get.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/interview")]
public class InterviewController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IInterviewRepository _interviewRepository;
    private readonly ICvProfileRepository _cvProfileRepository;
    private readonly ILlmClient _llmClient;

    public InterviewController(
        IMediator mediator,
        IInterviewRepository interviewRepository,
        ICvProfileRepository cvProfileRepository,
        ILlmClient llmClient)
    {
        _mediator = mediator;
        _interviewRepository = interviewRepository;
        _cvProfileRepository = cvProfileRepository;
        _llmClient = llmClient;
    }

    /// <summary>
    /// Start a new mock interview for a parsed CV profile.
    /// Generates the first question from the AI.
    /// </summary>
    [HttpPost("start")]
    public async Task<IActionResult> Start([FromBody] StartInterviewCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>
    /// Send a candidate's answer and receive a follow-up question from the AI.
    /// Returns the full updated interview so the client always has the complete conversation state.
    /// </summary>
    [HttpPost("message")]
    public async Task<IActionResult> SendMessage([FromBody] SendMessageCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>
    /// Stream a candidate's answer and the AI follow-up via Server-Sent Events.
    /// Saves both messages to the database. The client calls GET /{id} after [DONE] to refresh state.
    /// </summary>
    [HttpGet("{id:guid}/stream")]
    public async Task StreamMessage(Guid id, [FromQuery] string content, CancellationToken cancellationToken)
    {
        Response.ContentType = "text/event-stream";
        Response.Headers.CacheControl = "no-cache";
        Response.Headers["X-Accel-Buffering"] = "no";

        async Task SendDone()
        {
            await Response.WriteAsync("data: [DONE]\n\n", cancellationToken);
            await Response.Body.FlushAsync(cancellationToken);
        }

        if (string.IsNullOrWhiteSpace(content))
        {
            await SendDone();
            return;
        }

        var interview = await _interviewRepository.GetByIdAsync(id, cancellationToken);
        if (interview is null || interview.Status != InterviewStatus.InProgress)
        {
            await SendDone();
            return;
        }

        interview.AddCandidateMessage(content);

        var cvProfile = await _cvProfileRepository.GetByIdAsync(interview.CvProfileId, cancellationToken);
        var difficultyLevel = cvProfile?.DifficultyLevel ?? DifficultyLevel.Junior;

        var language = interview.PromptProfile.Contains("_ro_") ? "ro" : "en";
        var languageInstruction = language == "ro"
            ? "Conduct the entire interview in Romanian. All your questions and responses must be in Romanian. Never add translations in parentheses or include any text in another language."
            : "Conduct the entire interview in English. All your questions and responses must be in English.";

        var systemPrompt = new PromptBuilder()
            .WithPersona($"You are a professional technical interviewer conducting a mock interview. The candidate's level is {difficultyLevel}.")
            .WithRules(InterviewRules.GetRulesFor(difficultyLevel))
            .WithRules([new PromptRule(languageInstruction)])
            .Build();

        var llmMessages = new List<LlmMessage> { new(LlmRoles.System, systemPrompt) };
        foreach (var msg in interview.Messages)
        {
            var role = msg.Role == MessageRole.Interviewer ? LlmRoles.Assistant : LlmRoles.User;
            llmMessages.Add(new LlmMessage(role, msg.Content));
        }

        // Save candidate message before streaming starts
        await _interviewRepository.UpdateAsync(interview, cancellationToken);

        // Stream AI response token by token
        var fullResponse = new StringBuilder();
        await foreach (var token in _llmClient.StreamAsync(llmMessages, cancellationToken))
        {
            fullResponse.Append(token);
            var escaped = JsonSerializer.Serialize(token);
            await Response.WriteAsync($"data: {escaped}\n\n", cancellationToken);
            await Response.Body.FlushAsync(cancellationToken);
        }

        // Save AI response after streaming completes
        if (fullResponse.Length > 0 && !cancellationToken.IsCancellationRequested)
        {
            interview.AddInterviewerMessage(fullResponse.ToString());
            await _interviewRepository.UpdateAsync(interview, cancellationToken);
        }

        await SendDone();
    }

    /// <summary>
    /// Generate a feedback report for the interview.
    /// Requires at least 3 candidate responses. Completes the interview.
    /// </summary>
    [HttpPost("feedback")]
    public async Task<IActionResult> GenerateFeedback([FromBody] GenerateFeedbackCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>
    /// Get all interviews as lightweight summaries for the dashboard.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAllInterviews(CancellationToken cancellationToken)
    {
        var query = new GetAllInterviewsQuery();
        var result = await _mediator.Send(query, cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>
    /// Get an interview by Id, including all messages and feedback.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetInterview(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetInterviewQuery(id);
        var result = await _mediator.Send(query, cancellationToken);
        return result.ToActionResult();
    }
}
