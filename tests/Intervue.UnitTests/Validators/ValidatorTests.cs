using FluentAssertions;
using FluentValidation.TestHelper;
using Intervue.Application.Features.Cv.UploadCv;
using Intervue.Application.Features.Cv.ParseCv;
using Intervue.Application.Features.Interview.StartInterview;
using Intervue.Application.Features.Interview.SendMessage;
using Intervue.Application.Features.Interview.GetInterview;
using Intervue.Application.Features.Interview.GenerateFeedback;

namespace Intervue.UnitTests.Validators;

/// <summary>
/// Unit tests for all FluentValidation validators.
/// Each validator is tested for valid and invalid inputs.
/// </summary>
public class ValidatorTests
{
    // ── UploadCvValidator ──────────────────────────────────────────

    private readonly UploadCvValidator _uploadCvValidator = new();

    [Fact]
    public void UploadCvValidator_WithValidPdf_PassesValidation()
    {
        var command = new UploadCvCommand(new byte[] { 1, 2, 3 });
        var result = _uploadCvValidator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void UploadCvValidator_WithNullBytes_FailsValidation()
    {
        var command = new UploadCvCommand(null!);
        var result = _uploadCvValidator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.PdfBytes);
    }

    [Fact]
    public void UploadCvValidator_WithEmptyBytes_FailsValidation()
    {
        var command = new UploadCvCommand(Array.Empty<byte>());
        var result = _uploadCvValidator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.PdfBytes);
    }

    // ── ParseCvValidator ───────────────────────────────────────────

    private readonly ParseCvValidator _parseCvValidator = new();

    [Fact]
    public void ParseCvValidator_WithValidId_PassesValidation()
    {
        var command = new ParseCvCommand(Guid.NewGuid());
        var result = _parseCvValidator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void ParseCvValidator_WithEmptyId_FailsValidation()
    {
        var command = new ParseCvCommand(Guid.Empty);
        var result = _parseCvValidator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.CvProfileId)
              .WithErrorMessage("CvProfileId is required.");
    }

    // ── StartInterviewValidator ────────────────────────────────────

    private readonly StartInterviewValidator _startInterviewValidator = new();

    [Fact]
    public void StartInterviewValidator_WithValidId_PassesValidation()
    {
        var command = new StartInterviewCommand(Guid.NewGuid());
        var result = _startInterviewValidator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void StartInterviewValidator_WithEmptyId_FailsValidation()
    {
        var command = new StartInterviewCommand(Guid.Empty);
        var result = _startInterviewValidator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.CvProfileId)
              .WithErrorMessage("CvProfileId is required.");
    }

    // ── SendMessageValidator ───────────────────────────────────────

    private readonly SendMessageValidator _sendMessageValidator = new();

    [Fact]
    public void SendMessageValidator_WithValidData_PassesValidation()
    {
        var command = new SendMessageCommand(Guid.NewGuid(), "My answer to the question");
        var result = _sendMessageValidator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void SendMessageValidator_WithEmptyInterviewId_FailsValidation()
    {
        var command = new SendMessageCommand(Guid.Empty, "Valid content");
        var result = _sendMessageValidator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.InterviewId)
              .WithErrorMessage("InterviewId is required.");
    }

    [Fact]
    public void SendMessageValidator_WithEmptyContent_FailsValidation()
    {
        var command = new SendMessageCommand(Guid.NewGuid(), string.Empty);
        var result = _sendMessageValidator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Content)
              .WithErrorMessage("Message content is required.");
    }

    [Fact]
    public void SendMessageValidator_WithContentExceeding5000Chars_FailsValidation()
    {
        var longContent = new string('x', 5001);
        var command = new SendMessageCommand(Guid.NewGuid(), longContent);
        var result = _sendMessageValidator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Content)
              .WithErrorMessage("Message content must not exceed 5000 characters.");
    }

    [Fact]
    public void SendMessageValidator_WithContentExactly5000Chars_PassesValidation()
    {
        var exactContent = new string('x', 5000);
        var command = new SendMessageCommand(Guid.NewGuid(), exactContent);
        var result = _sendMessageValidator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    // ── GetInterviewValidator ──────────────────────────────────────

    private readonly GetInterviewValidator _getInterviewValidator = new();

    [Fact]
    public void GetInterviewValidator_WithValidId_PassesValidation()
    {
        var query = new GetInterviewQuery(Guid.NewGuid());
        var result = _getInterviewValidator.TestValidate(query);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void GetInterviewValidator_WithEmptyId_FailsValidation()
    {
        var query = new GetInterviewQuery(Guid.Empty);
        var result = _getInterviewValidator.TestValidate(query);
        result.ShouldHaveValidationErrorFor(x => x.InterviewId)
              .WithErrorMessage("InterviewId is required.");
    }

    // ── GenerateFeedbackValidator ──────────────────────────────────

    private readonly GenerateFeedbackValidator _generateFeedbackValidator = new();

    [Fact]
    public void GenerateFeedbackValidator_WithValidId_PassesValidation()
    {
        var command = new GenerateFeedbackCommand(Guid.NewGuid());
        var result = _generateFeedbackValidator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void GenerateFeedbackValidator_WithEmptyId_FailsValidation()
    {
        var command = new GenerateFeedbackCommand(Guid.Empty);
        var result = _generateFeedbackValidator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.InterviewId)
              .WithErrorMessage("InterviewId is required.");
    }
}
