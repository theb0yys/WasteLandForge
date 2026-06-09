using System.Text.RegularExpressions;

namespace WastelandForge.Core;

public readonly partial record struct RuleId
{
    private RuleId(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static RuleId Parse(string value)
    {
        if (!TryParse(value, out var ruleId))
        {
            throw new ArgumentException("Rule IDs must use a reserved WastelandForge rule family, such as WF-SEM-014.", nameof(value));
        }

        return ruleId;
    }

    public static bool TryParse(string? value, out RuleId ruleId)
    {
        if (!string.IsNullOrWhiteSpace(value) && RuleIdPattern().IsMatch(value))
        {
            ruleId = new RuleId(value);
            return true;
        }

        ruleId = default;
        return false;
    }

    public override string ToString() => Value;

    [GeneratedRegex("^WF-(?:LOAD|SCHEMA|SEM|CAP|ASSET|GEN|BUILD|REL|GOV|SEC)-[0-9]{3}$", RegexOptions.CultureInvariant)]
    private static partial Regex RuleIdPattern();
}
