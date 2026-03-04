using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Intervue.Api.Extensions;
using Intervue.Application.Features.Interview.GenerateFeedback;
using Intervue.Application.Features.Interview.GetInterview;
using Intervue.Application.Features.Interview.SendMessage;
using Intervue.Application.Features.Interview.StartInterview;

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

    public InterviewController(IMediator mediator)
    {
        _mediator = mediator;
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
    /// </summary>
    [HttpPost("message")]
    public async Task<IActionResult> SendMessage([FromBody] SendMessageCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return result.ToActionResult();
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
