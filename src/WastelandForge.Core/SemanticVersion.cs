using System.Text.RegularExpressions;

namespace WastelandForge.Core;

public sealed partial record SemanticVersion : IComparable<SemanticVersion>
{
    public SemanticVersion(
        int major,
        int minor,
        int patch,
        string? prerelease = null,
        string? buildMetadata = null)
    {
        if (major < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(major), "Major version must be non-negative.");
        }

        if (minor < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(minor), "Minor version must be non-negative.");
        }

        if (patch < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(patch), "Patch version must be non-negative.");
        }

        if (prerelease is not null && string.IsNullOrWhiteSpace(prerelease))
        {
            throw new ArgumentException("Prerelease label must not be blank when provided.", nameof(prerelease));
        }

        if (buildMetadata is not null && string.IsNullOrWhiteSpace(buildMetadata))
        {
            throw new ArgumentException("Build metadata must not be blank when provided.", nameof(buildMetadata));
        }

        Major = major;
        Minor = minor;
        Patch = patch;
        Prerelease = prerelease;
        BuildMetadata = buildMetadata;
    }

    public int Major { get; }

    public int Minor { get; }

    public int Patch { get; }

    public string? Prerelease { get; }

    public string? BuildMetadata { get; }

    public static SemanticVersion Parse(string value)
    {
        if (!TryParse(value, out var version))
        {
            throw new ArgumentException("Version must be a semantic version in major.minor.patch form.", nameof(value));
        }

        return version!;
    }

    public static bool TryParse(string? value, out SemanticVersion? version)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            version = null;
            return false;
        }

        var match = SemanticVersionPattern().Match(value);
        if (!match.Success)
        {
            version = null;
            return false;
        }

        version = new SemanticVersion(
            int.Parse(match.Groups["major"].Value),
            int.Parse(match.Groups["minor"].Value),
            int.Parse(match.Groups["patch"].Value),
            EmptyToNull(match.Groups["prerelease"].Value),
            EmptyToNull(match.Groups["build"].Value));
        return true;
    }

    public int CompareTo(SemanticVersion? other)
    {
        if (other is null)
        {
            return 1;
        }

        var coreComparison = Major.CompareTo(other.Major);
        if (coreComparison != 0)
        {
            return coreComparison;
        }

        coreComparison = Minor.CompareTo(other.Minor);
        if (coreComparison != 0)
        {
            return coreComparison;
        }

        coreComparison = Patch.CompareTo(other.Patch);
        if (coreComparison != 0)
        {
            return coreComparison;
        }

        if (Prerelease is null && other.Prerelease is null)
        {
            return 0;
        }

        if (Prerelease is null)
        {
            return 1;
        }

        if (other.Prerelease is null)
        {
            return -1;
        }

        return StringComparer.Ordinal.Compare(Prerelease, other.Prerelease);
    }

    public override string ToString()
    {
        var version = $"{Major}.{Minor}.{Patch}";
        if (Prerelease is not null)
        {
            version += $"-{Prerelease}";
        }

        if (BuildMetadata is not null)
        {
            version += $"+{BuildMetadata}";
        }

        return version;
    }

    private static string? EmptyToNull(string value) => value.Length == 0 ? null : value;

    [GeneratedRegex("^(?<major>0|[1-9][0-9]*)\\.(?<minor>0|[1-9][0-9]*)\\.(?<patch>0|[1-9][0-9]*)(?:-(?<prerelease>[0-9A-Za-z.-]+))?(?:\\+(?<build>[0-9A-Za-z.-]+))?$", RegexOptions.CultureInvariant)]
    private static partial Regex SemanticVersionPattern();
}
