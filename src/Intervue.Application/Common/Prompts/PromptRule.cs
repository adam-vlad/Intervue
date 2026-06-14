namespace Intervue.Application.Common.Prompts;

/// <summary>
/// Represents a single rule or instruction that can be injected into an LLM prompt.
/// Each rule is a self-contained piece of guidance (e.g. "ask follow-up questions", "return only JSON").
/// </summary>
public sealed record PromptRule(string Text);
