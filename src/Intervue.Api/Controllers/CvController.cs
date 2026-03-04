using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Intervue.Api.Extensions;
using Intervue.Application.Features.Cv.ParseCv;
using Intervue.Application.Features.Cv.UploadCv;

namespace Intervue.Api.Controllers;

/// <summary>
/// Controller for CV-related endpoints: upload and parse.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/cv")]
public class CvController : ControllerBase
{
    private readonly IMediator _mediator;

    public CvController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Upload a CV PDF file. Extracts text and creates a CvProfile.
    /// </summary>
    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Upload(IFormFile file, CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
        {
            return BadRequest("A PDF file is required.");
        }

        // Read the file bytes
        using var ms = new MemoryStream();
        await file.CopyToAsync(ms, cancellationToken);
        var pdfBytes = ms.ToArray();

        var command = new UploadCvCommand(pdfBytes);
        var result = await _mediator.Send(command, cancellationToken);

        return result.ToActionResult();
    }

    /// <summary>
    /// Parse a previously uploaded CV using the LLM.
    /// Extracts technologies, experience, projects, and difficulty level.
    /// </summary>
    [HttpPost("parse")]
    public async Task<IActionResult> Parse([FromBody] ParseCvCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return result.ToActionResult();
    }
}
