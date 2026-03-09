using FluentAssertions;
using Intervue.Application.Common.Prompts;
using Intervue.Domain.Enums;

namespace Intervue.UnitTests.Prompts;

/// <summary>
/// Unit tests for <see cref="PromptBuilder"/>, <see cref="InterviewRules"/>,
/// <see cref="CvParsingRules"/> and <see cref="FeedbackRules"/>.
/// </summary>
public class PromptBuilderTests
{
    // ── PromptBuilder basics ────────────────────────────────────────

    [Fact]
    public void Build_WithNoRules_ShouldStillContainPersona()
    {
        // Arrange
        var builder = new PromptBuilder()
            .WithPersona("You are a helpful assistant.");

        // Act
        var prompt = builder.Build();

        // Assert
        prompt.Should().Contain("You are a helpful assistant.");
    }

    [Fact]
    public void Build_WithDefaultPersona_ShouldContainDefaultText()
    {
        // Arrange & Act
        var prompt = new PromptBuilder().Build();

        // Assert
        prompt.Should().Contain("You are a helpful assistant.");
    }

    [Fact]
    public void Build_WithRules_ShouldContainEachRuleText()
    {
        // Arrange
        var rule1 = new PromptRule("Ask follow-up questions.");
        var rule2 = new PromptRule("Be professional and encouraging.");

        var builder = new PromptBuilder()
            .WithPersona("You are an interviewer.")
            .WithRule(rule1)
            .WithRule(rule2);

        // Act
        var prompt = builder.Build();

        // Assert
        prompt.Should().Contain(rule1.Text);
        prompt.Should().Contain(rule2.Text);
    }

    [Fact]
    public void Build_WithRules_ShouldContainNumberedRules()
    {
        // Arrange
        var rules = new[]
        {
            new PromptRule("First rule."),
            new PromptRule("Second rule."),
            new PromptRule("Third rule.")
        };

        var prompt = new PromptBuilder()
            .WithPersona("Persona.")
            .WithRules(rules)
            .Build();

        // Assert
        prompt.Should().Contain("1. First rule.");
        prompt.Should().Contain("2. Second rule.");
        prompt.Should().Contain("3. Third rule.");
    }

    [Fact]
    public void Build_WithRules_ShouldContainRulesHeader()
    {
        // Arrange & Act
        var prompt = new PromptBuilder()
            .WithPersona("Persona.")
            .WithRule(new PromptRule("Some rule."))
            .Build();

        // Assert
        prompt.Should().Contain("Rules:");
    }

    [Fact]
    public void Build_WithNoRules_ShouldNotContainRulesHeader()
    {
        // Arrange & Act
        var prompt = new PromptBuilder()
            .WithPersona("Just a persona.")
            .Build();

        // Assert
        prompt.Should().NotContain("Rules:");
    }

    // ── InterviewRules.GetRulesFor ──────────────────────────────────

    [Fact]
    public void GetRulesFor_Junior_ShouldNotContainSystemDesignRules()
    {
        // Arrange & Act
        var rules = InterviewRules.GetRulesFor(DifficultyLevel.Junior);

        // Assert
        rules.Should().NotContain(InterviewRules.SystemDesign);
        rules.Should().NotContain(InterviewRules.EdgeCases);
        rules.Should().NotContain(InterviewRules.JustifyDecisions);
    }

    [Fact]
    public void GetRulesFor_Junior_ShouldContainHintFriendlyRules()
    {
        // Arrange & Act
        var rules = InterviewRules.GetRulesFor(DifficultyLevel.Junior);

        // Assert
        rules.Should().Contain(InterviewRules.GiveHints);
        rules.Should().Contain(InterviewRules.FocusOnFundamentals);
        rules.Should().Contain(InterviewRules.BeginnerFriendlyLanguage);
    }

    [Fact]
    public void GetRulesFor_Senior_ShouldContainSystemDesignAndEdgeCaseRules()
    {
        // Arrange & Act
        var rules = InterviewRules.GetRulesFor(DifficultyLevel.Senior);

        // Assert
        rules.Should().Contain(InterviewRules.SystemDesign);
        rules.Should().Contain(InterviewRules.EdgeCases);
        rules.Should().Contain(InterviewRules.JustifyDecisions);
    }

    [Fact]
    public void GetRulesFor_Senior_ShouldNotContainJuniorSpecificRules()
    {
        // Arrange & Act
        var rules = InterviewRules.GetRulesFor(DifficultyLevel.Senior);

        // Assert
        rules.Should().NotContain(InterviewRules.GiveHints);
        rules.Should().NotContain(InterviewRules.FocusOnFundamentals);
        rules.Should().NotContain(InterviewRules.BeginnerFriendlyLanguage);
    }

    [Fact]
    public void GetRulesFor_Mid_ShouldContainTradeOffAndDepthRules()
    {
        // Arrange & Act
        var rules = InterviewRules.GetRulesFor(DifficultyLevel.Mid);

        // Assert
        rules.Should().Contain(InterviewRules.AskAboutTradeOffs);
        rules.Should().Contain(InterviewRules.ExploreDepth);
    }

    [Theory]
    [InlineData(DifficultyLevel.Junior)]
    [InlineData(DifficultyLevel.Mid)]
    [InlineData(DifficultyLevel.Senior)]
    public void GetRulesFor_AllLevels_ShouldContainCommonRules(DifficultyLevel level)
    {
        // Arrange & Act
        var rules = InterviewRules.GetRulesFor(level);

        // Assert
        rules.Should().Contain(InterviewRules.FollowUp);
        rules.Should().Contain(InterviewRules.PressForExamples);
        rules.Should().Contain(InterviewRules.MoveOnIfGood);
        rules.Should().Contain(InterviewRules.OneQuestionAtATime);
        rules.Should().Contain(InterviewRules.ProfessionalTone);
        rules.Should().Contain(InterviewRules.OutputFormatQuestionOnly);
    }

    // ── Built prompt contains each rule's text ──────────────────────

    [Theory]
    [InlineData(DifficultyLevel.Junior)]
    [InlineData(DifficultyLevel.Mid)]
    [InlineData(DifficultyLevel.Senior)]
    public void BuiltPrompt_ShouldContainAllRuleTexts(DifficultyLevel level)
    {
        // Arrange
        var rules = InterviewRules.GetRulesFor(level);

        var prompt = new PromptBuilder()
            .WithPersona("You are an interviewer.")
            .WithRules(rules)
            .Build();

        // Assert — each rule's text must appear in the final prompt
        foreach (var rule in rules)
        {
            prompt.Should().Contain(rule.Text);
        }
    }

    // ── CvParsingRules ──────────────────────────────────────────────

    [Fact]
    public void CvParsingRules_All_ShouldContainExpectedRules()
    {
        // Assert
        CvParsingRules.All.Should().Contain(CvParsingRules.ReturnOnlyJson);
        CvParsingRules.All.Should().Contain(CvParsingRules.UseExactSchema);
        CvParsingRules.All.Should().Contain(CvParsingRules.EstimateExperience);
        CvParsingRules.All.Should().Contain(CvParsingRules.EstimateDuration);
        CvParsingRules.All.Should().Contain(CvParsingRules.DetermineDifficulty);
    }

    [Fact]
    public void CvParsingRules_BuiltPrompt_ShouldContainAllRuleTexts()
    {
        // Arrange & Act
        var prompt = new PromptBuilder()
            .WithPersona("You are a CV parser.")
            .WithRules(CvParsingRules.All)
            .Build();

        // Assert
        foreach (var rule in CvParsingRules.All)
        {
            prompt.Should().Contain(rule.Text);
        }
    }

    // ── FeedbackRules ───────────────────────────────────────────────

    [Fact]
    public void FeedbackRules_All_ShouldContainExpectedRules()
    {
        // Assert
        FeedbackRules.All.Should().Contain(FeedbackRules.ReturnOnlyJson);
        FeedbackRules.All.Should().Contain(FeedbackRules.UseExactSchema);
        FeedbackRules.All.Should().Contain(FeedbackRules.ScoreRange);
        FeedbackRules.All.Should().Contain(FeedbackRules.FourCategories);
        FeedbackRules.All.Should().Contain(FeedbackRules.StrengthsFormat);
        FeedbackRules.All.Should().Contain(FeedbackRules.WeaknessesFormat);
        FeedbackRules.All.Should().Contain(FeedbackRules.SuggestionsFormat);
    }

    [Fact]
    public void FeedbackRules_BuiltPrompt_ShouldContainAllRuleTexts()
    {
        // Arrange & Act
        var prompt = new PromptBuilder()
            .WithPersona("You are an evaluator.")
            .WithRules(FeedbackRules.All)
            .Build();

        // Assert
        foreach (var rule in FeedbackRules.All)
        {
            prompt.Should().Contain(rule.Text);
        }
    }
}
