using MediatR;
using Microsoft.Extensions.Logging;
using Intervue.Application.Common;
using Intervue.Application.Common.Constants;
using Intervue.Application.Common.Interfaces;
using Intervue.Domain.Entities;
using Intervue.Domain.Repositories;
using Intervue.Domain.ValueObjects;

namespace Intervue.Application.Features.Cv.UploadCv;

/// <summary>
/// Handles UploadCvCommand:
/// 1. Extracts text from PDF using PdfPig
/// 2. Hashes personal data for privacy
/// 3. Creates a CvProfile entity
/// 4. Saves it to the repository
/// 5. Returns the new CvProfile Id
/// </summary>
public class UploadCvHandler : IRequestHandler<UploadCvCommand, Result<Guid>>
{
    private readonly IPdfExtractor _pdfExtractor;
    private readonly IHashingService _hashingService;
    private readonly ICvProfileRepository _cvProfileRepository;
    private readonly ILogger<UploadCvHandler> _logger;

    public UploadCvHandler(
        IPdfExtractor pdfExtractor,
        IHashingService hashingService,
        ICvProfileRepository cvProfileRepository,
        ILogger<UploadCvHandler> logger)
    {
        _pdfExtractor = pdfExtractor;
        _hashingService = hashingService;
        _cvProfileRepository = cvProfileRepository;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(UploadCvCommand request, CancellationToken cancellationToken)
    {
        string rawText;
        try
        {
            // Step 1: Extract text from PDF
            rawText = _pdfExtractor.ExtractText(request.PdfBytes);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to extract text from uploaded PDF ({ByteCount} bytes).", request.PdfBytes.Length);
            return Result<Guid>.Fail(
                Error.Validation(ErrorCodes.CvInvalidPdf, "The uploaded file is not a valid or readable PDF."));
        }

        if (string.IsNullOrWhiteSpace(rawText))
        {
            return Result<Guid>.Fail(Error.Validation(ErrorCodes.CvEmptyText, "No text could be extracted from the PDF."));
        }

        // Step 2: Hash the raw text as personal data identifier
        var hash = _hashingService.Hash(rawText);
        var hashedData = new HashedPersonalData(hash);

        // Step 3: Create the CvProfile domain entity
        var cvProfile = CvProfile.Create(rawText, hashedData);

        // Step 4: Save to repository
        await _cvProfileRepository.AddAsync(cvProfile, cancellationToken);

        _logger.LogInformation("CV profile created with Id {CvProfileId}.", cvProfile.Id);

        // Step 5: Return the Id
        return Result<Guid>.Created(cvProfile.Id);
    }
}
