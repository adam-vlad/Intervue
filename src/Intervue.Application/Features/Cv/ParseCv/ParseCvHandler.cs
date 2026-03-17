using System.Text.Json;
using System.Text.RegularExpressions;
using MediatR;
using Microsoft.Extensions.Logging;
using Intervue.Application.Common;
using Intervue.Application.Common.Interfaces;
using Intervue.Application.Common.Prompts;
using Intervue.Application.Features.DTOs;
using Intervue.Domain.Entities;
using Intervue.Domain.Enums;
using Intervue.Domain.Repositories;

namespace Intervue.Application.Features.Cv.ParseCv;

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
    private readonly ILogger<ParseCvHandler> _logger;

    public ParseCvHandler(ICvProfileRepository cvProfileRepository, ILlmClient llmClient, ILogger<ParseCvHandler> logger)
    {
        _cvProfileRepository = cvProfileRepository;
        _llmClient = llmClient;
        _logger = logger;
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

        // Step 2: Build the prompt for the LLM using PromptBuilder
        var systemPrompt = new PromptBuilder()
            .WithPersona(CvParsingPersona)
            .WithRules(CvParsingRules.All)
            .Build();

        var messages = new List<LlmMessage>
        {
            new("system", systemPrompt),
            new("user", cvProfile.RawText)
        };

        // Step 3: Call the LLM
        var llmResponse = await _llmClient.ChatAsync(messages, cancellationToken);

        // Step 4: Parse the JSON response from the LLM
        _logger.LogInformation("Raw LLM response for CV parsing:\n{LlmResponse}", llmResponse);

        var parsedCv = ParseLlmResponse(llmResponse);

        if (parsedCv is null)
        {
            _logger.LogError("Failed to parse LLM response into structured CV data. Raw response:\n{LlmResponse}", llmResponse);
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

    private ParsedCvData? ParseLlmResponse(string llmResponse)
    {
        try
        {
            var json = llmResponse;

            // Strip markdown code fences: ```json ... ``` or ``` ... ```
            var fenceMatch = Regex.Match(json, @"```(?:json)?\s*\n?(.*?)\n?\s*```", RegexOptions.Singleline);
            if (fenceMatch.Success)
            {
                json = fenceMatch.Groups[1].Value;
            }

            // Extract the outermost JSON object
            var jsonStart = json.IndexOf('{');
            var jsonEnd = json.LastIndexOf('}');

            if (jsonStart >= 0 && jsonEnd > jsonStart)
            {
                json = json.Substring(jsonStart, jsonEnd - jsonStart + 1);
            }

            // Normalize snake_case to camelCase for common fields
            json = json.Replace("years_of_experience", "yearsOfExperience")
                       .Replace("difficulty_level", "difficultyLevel")
                       .Replace("duration_months", "durationMonths")
                       .Replace("technologies_used", "technologiesUsed");

            // Fix quoted numbers: "yearsOfExperience": "3" → "yearsOfExperience": 3
            json = Regex.Replace(json, @"""(yearsOfExperience|durationMonths)"":\s*""(\d+)""", "\"$1\": $2");

            return JsonSerializer.Deserialize<ParsedCvData>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString
            });
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "LLM response JSON parsing failed in ParseCvHandler.");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Unexpected error while parsing LLM response in ParseCvHandler.");
            return null;
        }
    }

    // ── Persona used by PromptBuilder — the rules come from CvParsingRules ───

    private const string CvParsingPersona = """
        You are a CV parser. Analyze the following CV text and extract structured information.
        
        Return a JSON object with this structure:
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
