using Intervue.Application.Common.Interfaces;
using UglyToad.PdfPig;

namespace Intervue.Infrastructure.Services;

/// <summary>
/// Implements IPdfExtractor using PdfPig library.
/// Takes raw PDF bytes and extracts all the text from every page.
/// </summary>
public class PdfPigExtractor : IPdfExtractor
{
    public string ExtractText(byte[] pdfBytes)
    {
        try
        {
            using var document = PdfDocument.Open(pdfBytes);

            // Extract text from each page and join with newlines
            var pages = document.GetPages();
            var textParts = pages.Select(page => page.Text);

            return string.Join(Environment.NewLine, textParts);
        }
        catch (Exception ex)
        {
            throw new InvalidDataException("Unable to extract text from PDF.", ex);
        }
    }
}
