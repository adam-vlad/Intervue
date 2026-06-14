using FluentAssertions;
using Moq;
using Intervue.Application.Common.Interfaces;
using Intervue.Application.Features.Cv.UploadCv;
using Intervue.Domain.Entities;
using Intervue.Domain.Repositories;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Intervue.UnitTests.Handlers;

/// <summary>
/// Unit tests for UploadCvHandler.
/// Mocks: IPdfExtractor, IHashingService, ICvProfileRepository.
/// </summary>
public class UploadCvHandlerTests
{
    private readonly Mock<IPdfExtractor> _pdfExtractor = new();
    private readonly Mock<IHashingService> _hashingService = new();
    private readonly Mock<ICvProfileRepository> _cvProfileRepository = new();
    private readonly UploadCvHandler _sut;

    public UploadCvHandlerTests()
    {
        _sut = new UploadCvHandler(
            _pdfExtractor.Object,
            _hashingService.Object,
            _cvProfileRepository.Object,
            NullLoggerFactory.Instance.CreateLogger<UploadCvHandler>());
    }

    [Fact]
    public async Task Handle_WithValidPdf_ReturnsCreatedWithGuid()
    {
        // Arrange
        var pdfBytes = new byte[] { 1, 2, 3 };
        var command = new UploadCvCommand(pdfBytes);

        _pdfExtractor.Setup(x => x.ExtractText(pdfBytes)).Returns("John Doe - Software Engineer");
        _hashingService.Setup(x => x.Hash(It.IsAny<string>())).Returns("abc123hash");
        _cvProfileRepository.Setup(x => x.AddAsync(It.IsAny<CvProfile>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBe(Guid.Empty);

        _pdfExtractor.Verify(x => x.ExtractText(pdfBytes), Times.Once);
        _hashingService.Verify(x => x.Hash("John Doe - Software Engineer"), Times.Once);
        _cvProfileRepository.Verify(x => x.AddAsync(It.IsAny<CvProfile>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenPdfTextIsEmpty_ReturnsValidationError()
    {
        // Arrange
        var pdfBytes = new byte[] { 1, 2, 3 };
        var command = new UploadCvCommand(pdfBytes);

        _pdfExtractor.Setup(x => x.ExtractText(pdfBytes)).Returns(string.Empty);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.Code == "Cv.EmptyText");

        _cvProfileRepository.Verify(x => x.AddAsync(It.IsAny<CvProfile>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenPdfTextIsWhitespace_ReturnsValidationError()
    {
        // Arrange
        var command = new UploadCvCommand(new byte[] { 1 });
        _pdfExtractor.Setup(x => x.ExtractText(It.IsAny<byte[]>())).Returns("   ");

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.Code == "Cv.EmptyText");
    }

    [Fact]
    public async Task Handle_WhenPdfExtractorThrows_ReturnsInvalidPdfValidationError()
    {
        // Arrange
        var command = new UploadCvCommand(new byte[] { 1, 2, 3 });
        _pdfExtractor.Setup(x => x.ExtractText(It.IsAny<byte[]>())).Throws(new InvalidDataException("Invalid PDF"));

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.Code == "Cv.InvalidPdf");
        _cvProfileRepository.Verify(x => x.AddAsync(It.IsAny<CvProfile>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
