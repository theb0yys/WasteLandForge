using System.Text.RegularExpressions;

namespace WastelandForge.Core;

public readonly partial record struct LogicalId
{
    private LogicalId(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static LogicalId Parse(string value)
    {
        if (!TryParse(value, out var logicalId))
        {
            throw new ArgumentException("Logical IDs must be dotted lowercase identifiers.", nameof(value));
        }

        return logicalId;
    }

    public static bool TryParse(string? value, out LogicalId logicalId)
    {
        if (!string.IsNullOrWhiteSpace(value) && LogicalIdPattern().IsMatch(value))
        {
            logicalId = new LogicalId(value);
            return true;
        }

        logicalId = default;
        return false;
    }

    public override string ToString() => Value;

    [GeneratedRegex("^[a-z][a-z0-9]*(?:\\.[a-z][a-z0-9]*)+$", RegexOptions.CultureInvariant)]
    private static partial Regex LogicalIdPattern();
}
