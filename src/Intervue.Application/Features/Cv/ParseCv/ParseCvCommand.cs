using MediatR;
using Intervue.Application.Common;
using Intervue.Application.Features.DTOs;

namespace Intervue.Application.Features.Cv.ParseCv;

/// <summary>
/// Command to parse a CV using the LLM. The LLM extracts structured data
/// (technologies, experience, projects, difficulty level) from the raw CV text.
/// </summary>
public record ParseCvCommand(Guid CvProfileId) : IRequest<Result<CvProfileDto>>;
