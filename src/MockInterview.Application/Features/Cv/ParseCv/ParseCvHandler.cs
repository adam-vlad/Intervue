using System.Text.Json;
using MediatR;
using MockInterview.Application.Common;
using MockInterview.Application.Common.Interfaces;
using MockInterview.Application.Features.DTOs;
using MockInterview.Domain.Entities;
using MockInterview.Domain.Enums;
using MockInterview.Domain.Repositories;

namespace MockInterview.Application.Features.Cv.ParseCv;

/// <summary>
/// Handles ParseCvCommand:
/// 1. Gets the CvProfile from repository
/// 2. Sends the raw text to the LLM with a structured prompt
/// 3. Parses the LLM's JSON response
/// 4. Updates the CvProfile entity with parsed data
/// 5. Returns the updated CvProfileDto
/// </summary>
public class ParseCvHandler : IRequestHandler<ParseCvCommand, Result<CvProfileDto>>
{
    private readonly ICvProfileRepository _cvProfileRepository;
    private readonly ILlmClient _llmClient;

    public ParseCvHandler(ICvProfileRepository cvProfileRepository, ILlmClient llmClient)
    {
        _cvProfileRepository = cvProfileRepository;
        _llmClient = llmClient;
    }

    public async Task<Result<CvProfileDto>> Handle(ParseCvCommand request, CancellationToken cancellationToken)
    {
        // Step 1: Get the CvProfile
        var cvProfile = await _cvProfileRepository.GetByIdAsync(request.CvProfileId, cancellationToken);

        if (cvProfile is null)
        {
            return Result<CvProfileDto>.Fail(
                Error.NotFound("Cv.NotFound", $"CV profile with id '{request.CvProfileId}' was not found."));
        }

        // Step 2: Build the prompt for the LLM
        var messages = new List<LlmMessage>
        {
            new("system", CvParsingPrompt),
            new("user", cvProfile.RawText)
        };

        // Step 3: Call the LLM
        var llmResponse = await _llmClient.ChatAsync(messages, cancellationToken);

        // Step 4: Parse the JSON response from the LLM
        var parsedCv = ParseLlmResponse(llmResponse);

        if (parsedCv is null)
        {
            return Result<CvProfileDto>.Fail(
                Error.Failure("Cv.ParseFailed", "Failed to parse the LLM response into structured CV data."));
        }

        // Step 5: Convert parsed data to domain entities
        var technologies = parsedCv.Technologies
            .Select(t => Technology.Create(t.Name, t.YearsOfExperience))
            .ToList();

        var experiences = parsedCv.Experiences
            .Select(e => Experience.Create(e.Role, e.Company, e.DurationMonths, e.Description))
            .ToList();

        var projects = parsedCv.Projects
            .Select(p => Project.Create(p.Name, p.Description, p.TechnologiesUsed))
            .ToList();

        var difficultyLevel = Enum.TryParse<DifficultyLevel>(parsedCv.DifficultyLevel, true, out var level)
            ? level
            : DifficultyLevel.Junior;

        // Step 6: Update the CvProfile entity
        cvProfile.SetParsedData(difficultyLevel, parsedCv.Education, technologies, experiences, projects);

        // Step 7: Save changes
        await _cvProfileRepository.UpdateAsync(cvProfile, cancellationToken);

        // Step 8: Return the DTO
        return Result<CvProfileDto>.Ok(cvProfile.ToDto());
    }

    private static ParsedCvData? ParseLlmResponse(string llmResponse)
    {
        try
        {
            // Try to extract JSON from the response (LLM might wrap it in markdown code blocks)
            var json = llmResponse;

            var jsonStart = json.IndexOf('{');
            var jsonEnd = json.LastIndexOf('}');

            if (jsonStart >= 0 && jsonEnd > jsonStart)
            {
                json = json.Substring(jsonStart, jsonEnd - jsonStart + 1);
            }

            return JsonSerializer.Deserialize<ParsedCvData>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch
        {
            return null;
        }
    }

    // ── Prompt engineering — this is what tells the LLM how to parse the CV ───

    private const string CvParsingPrompt = """
        You are a CV parser. Analyze the following CV text and extract structured information.
        
        Return ONLY a valid JSON object with this exact structure (no markdown, no explanation):
        {
          "difficultyLevel": "Junior" or "Mid" or "Senior",
          "education": "degree and university or null",
          "technologies": [
            { "name": "technology name", "yearsOfExperience": number }
          ],
          "experiences": [
            { "role": "job title", "company": "company name", "durationMonths": number, "description": "brief description or null" }
          ],
          "projects": [
            { "name": "project name", "description": "brief description or null", "technologiesUsed": ["tech1", "tech2"] }
          ]
        }
        
        Rules:
        - Estimate yearsOfExperience based on context if not explicitly stated (minimum 1)
        - Estimate durationMonths from dates if available
        - Set difficultyLevel based on total experience: <2 years = Junior, 2-5 years = Mid, >5 years = Senior
        - Return valid JSON only, no extra text
        """;

    // ── Internal DTO for deserializing the LLM's JSON response ───

    private class ParsedCvData
    {
        public string DifficultyLevel { get; set; } = "Junior";
        public string? Education { get; set; }
        public List<ParsedTechnology> Technologies { get; set; } = new();
        public List<ParsedExperience> Experiences { get; set; } = new();
        public List<ParsedProject> Projects { get; set; } = new();
    }

    private class ParsedTechnology
    {
        public string Name { get; set; } = string.Empty;
        public int YearsOfExperience { get; set; } = 1;
    }

    private class ParsedExperience
    {
        public string Role { get; set; } = string.Empty;
        public string Company { get; set; } = string.Empty;
        public int DurationMonths { get; set; }
        public string? Description { get; set; }
    }

    private class ParsedProject
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public List<string> TechnologiesUsed { get; set; } = new();
    }
}
