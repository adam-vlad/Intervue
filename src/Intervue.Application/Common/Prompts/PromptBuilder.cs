using System.Text;

namespace Intervue.Application.Common.Prompts;

/// <summary>
/// Fluent builder that composes a system prompt from a persona description and a set of <see cref="PromptRule"/> items.
/// Usage: <c>new PromptBuilder().WithPersona("...").WithRule(rule1).WithRules(rules).Build()</c>
/// </summary>
public sealed class PromptBuilder
{
    private string _persona = "You are a helpful assistant.";
    private readonly List<PromptRule> _rules = new();

    /// <summary>Sets the persona / role description that appears at the top of the prompt.</summary>
    public PromptBuilder WithPersona(string persona)
    {
        _persona = persona;
        return this;
    }

    /// <summary>Adds a single rule to the prompt.</summary>
    public PromptBuilder WithRule(PromptRule rule)
    {
        _rules.Add(rule);
        return this;
    }

    /// <summary>Adds multiple rules to the prompt.</summary>
    public PromptBuilder WithRules(IEnumerable<PromptRule> rules)
    {
        _rules.AddRange(rules);
        return this;
    }

    /// <summary>
    /// Builds the final prompt string.
    /// Format: persona paragraph, followed by numbered rules.
    /// </summary>
    public string Build()
    {
        var sb = new StringBuilder();
        sb.AppendLine(_persona);

        if (_rules.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine("Rules:");

            for (var i = 0; i < _rules.Count; i++)
            {
                sb.AppendLine($"{i + 1}. {_rules[i].Text}");
            }
        }

        return sb.ToString().TrimEnd();
    }
}
