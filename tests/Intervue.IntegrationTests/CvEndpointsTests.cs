using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Moq;
using Intervue.Application.Common.Interfaces;
using Intervue.Application.Features.DTOs;

namespace Intervue.IntegrationTests;

/// <summary>
/// Integration tests for the CV endpoints (upload + parse).
/// Uses WebApplicationFactory with InMemory DB and mocked LLM.
/// </summary>
public class CvEndpointsTests : IClassFixture<IntervueWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly IntervueWebApplicationFactory _factory;

    public CvEndpointsTests(IntervueWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Upload_WithValidPdf_Returns201Created()
    {
        // Arrange — create a minimal valid PDF
        var pdfContent = CreateMinimalPdf();
        using var content = new MultipartFormDataContent();
        content.Add(new ByteArrayContent(pdfContent), "file", "test.pdf");

        // Act
        var response = await _client.PostAsync("/api/v1/cv/upload", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Upload_WithEmptyFile_Returns400BadRequest()
    {
        // Arrange — empty multipart
        using var content = new MultipartFormDataContent();

        // Act
        var response = await _client.PostAsync("/api/v1/cv/upload", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Upload_WithInvalidPdfBytes_Returns400BadRequest()
    {
        // Arrange
        using var content = new MultipartFormDataContent();
        content.Add(new ByteArrayContent(Encoding.ASCII.GetBytes("not a real pdf")), "file", "broken.pdf");

        // Act
        var response = await _client.PostAsync("/api/v1/cv/upload", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("Cv.InvalidPdf");
    }

    [Fact]
    public async Task Parse_WithNonExistentCvProfileId_Returns404NotFound()
    {
        // Arrange
        var command = new { CvProfileId = Guid.NewGuid() };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/cv/parse", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Parse_WithEmptyCvProfileId_Returns400BadRequest()
    {
        // Arrange
        var command = new { CvProfileId = Guid.Empty };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/cv/parse", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    /// <summary>Creates a minimal valid PDF file content.</summary>
    private static byte[] CreateMinimalPdf()
    {
        // Minimal PDF that PdfPig can read
        const string minimalPdf = @"%PDF-1.0
1 0 obj
<< /Type /Catalog /Pages 2 0 R >>
endobj
2 0 obj
<< /Type /Pages /Kids [3 0 R] /Count 1 >>
endobj
3 0 obj
<< /Type /Page /Parent 2 0 R /MediaBox [0 0 612 792] /Contents 4 0 R /Resources << /Font << /F1 5 0 R >> >> >>
endobj
4 0 obj
<< /Length 44 >>
stream
BT /F1 12 Tf 100 700 Td (Hello World) Tj ET
endstream
endobj
5 0 obj
<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>
endobj
xref
0 6
0000000000 65535 f 
0000000009 00000 n 
0000000058 00000 n 
0000000115 00000 n 
0000000266 00000 n 
0000000360 00000 n 
trailer
<< /Size 6 /Root 1 0 R >>
startxref
441
%%EOF";
        return Encoding.ASCII.GetBytes(minimalPdf);
    }
}
