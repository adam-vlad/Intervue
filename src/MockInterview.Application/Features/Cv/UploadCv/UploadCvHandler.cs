using MediatR;
using MockInterview.Application.Common;
using MockInterview.Application.Common.Interfaces;
using MockInterview.Domain.Entities;
using MockInterview.Domain.Repositories;
using MockInterview.Domain.ValueObjects;

namespace MockInterview.Application.Features.Cv.UploadCv;

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

    public UploadCvHandler(
        IPdfExtractor pdfExtractor,
        IHashingService hashingService,
        ICvProfileRepository cvProfileRepository)
    {
        _pdfExtractor = pdfExtractor;
        _hashingService = hashingService;
        _cvProfileRepository = cvProfileRepository;
    }

    public async Task<Result<Guid>> Handle(UploadCvCommand request, CancellationToken cancellationToken)
    {
        // Step 1: Extract text from PDF
        var rawText = _pdfExtractor.ExtractText(request.PdfBytes);

        if (string.IsNullOrWhiteSpace(rawText))
        {
            return Result<Guid>.Fail(Error.Validation("Cv.EmptyText", "No text could be extracted from the PDF."));
        }

        // Step 2: Hash the raw text as personal data identifier
        var hash = _hashingService.Hash(rawText);
        var hashedData = new HashedPersonalData(hash);

        // Step 3: Create the CvProfile domain entity
        var cvProfile = CvProfile.Create(rawText, hashedData);

        // Step 4: Save to repository
        await _cvProfileRepository.AddAsync(cvProfile, cancellationToken);

        // Step 5: Return the Id
        return Result<Guid>.Created(cvProfile.Id);
    }
}
