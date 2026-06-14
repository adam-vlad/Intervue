using FluentValidation;

namespace Intervue.Application.Features.Cv.UploadCv;

/// <summary>
/// Validates the UploadCvCommand before the handler runs.
/// Ensures the PDF bytes are not null or empty.
/// </summary>
public class UploadCvValidator : AbstractValidator<UploadCvCommand>
{
    public UploadCvValidator()
    {
        RuleFor(x => x.PdfBytes)
            .NotNull().WithMessage("PDF file is required.")
            .Must(bytes => bytes != null && bytes.Length > 0)
            .WithMessage("PDF file cannot be empty.");
    }
}
