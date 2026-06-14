using MediatR;
using Intervue.Application.Common;

namespace Intervue.Application.Features.Cv.UploadCv;

/// <summary>
/// Command to upload a CV PDF file. Contains the raw PDF bytes.
/// Returns the new CvProfile's Id (Guid).
/// </summary>
public record UploadCvCommand(byte[] PdfBytes) : IRequest<Result<Guid>>;
