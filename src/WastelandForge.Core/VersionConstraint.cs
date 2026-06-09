namespace WastelandForge.Core;

public sealed record VersionConstraint(
    SemanticVersion? MinInclusive = null,
    SemanticVersion? MaxExclusive = null)
{
    public static readonly VersionConstraint Any = new();

    public static VersionConstraint AtLeast(SemanticVersion minInclusive) => new(minInclusive);

    public bool Allows(SemanticVersion version)
    {
        ArgumentNullException.ThrowIfNull(version);

        if (MinInclusive is not null && version.CompareTo(MinInclusive) < 0)
        {
            return false;
        }

        if (MaxExclusive is not null && version.CompareTo(MaxExclusive) >= 0)
        {
            return false;
        }

        return true;
    }

    public override string ToString()
    {
        if (MinInclusive is null && MaxExclusive is null)
        {
            return "*";
        }

        if (MinInclusive is not null && MaxExclusive is not null)
        {
            return $">= {MinInclusive} < {MaxExclusive}";
        }

        return MinInclusive is not null ? $">= {MinInclusive}" : $"< {MaxExclusive}";
    }
}
