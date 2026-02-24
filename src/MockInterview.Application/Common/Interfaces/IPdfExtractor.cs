namespace MockInterview.Application.Common.Interfaces;

/// <summary>
/// Contract for extracting text from a PDF file.
/// Infrastructure implements this using PdfPig.
/// </summary>
public interface IPdfExtractor
{
    /// <summary>
    /// Takes the raw bytes of a PDF and returns the extracted plain text.
    /// </summary>
    string ExtractText(byte[] pdfBytes);
}
