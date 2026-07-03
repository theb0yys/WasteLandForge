using System.Globalization;
using WastelandForge.Core;

namespace WastelandForge.Registry;

public sealed record ProviderVersionParseResult(
    string Scheme,
    string RawValue,
    bool Parsed,
    string? NormalizedValue,
    IReadOnlyList<long> NumericComponents,
    string? FailureReason)
{
    public static ProviderVersionParseResult Success(
        string scheme,
        string rawValue,
        string normalizedValue,
        IReadOnlyList<long> numericComponents) =>
        new(scheme, rawValue, true, normalizedValue, numericComponents, null);

    public static ProviderVersionParseResult Failure(
        string scheme,
        string rawValue,
        string failureReason) =>
        new(scheme, rawValue, false, null, [], failureReason);
}

public static class ProviderVersionParser
{
    public static ProviderVersionParseResult Parse(string? scheme, string? rawValue)
    {
        var normalizedScheme = scheme?.Trim() ?? string.Empty;
        var preservedRawValue = rawValue ?? string.Empty;
        var value = preservedRawValue.Trim();

        if (normalizedScheme.Length == 0)
        {
            return ProviderVersionParseResult.Failure(normalizedScheme, preservedRawValue, "missing-scheme");
        }

        if (value.Length == 0)
        {
            return ProviderVersionParseResult.Failure(normalizedScheme, preservedRawValue, "missing-value");
        }

        return normalizedScheme switch
        {
            "semver" => ParseSemver(normalizedScheme, preservedRawValue, value),
            "integer" => ParseInteger(normalizedScheme, preservedRawValue, value),
            "scaled-integer" => ParseScaledInteger(normalizedScheme, preservedRawValue, value),
            _ => ProviderVersionParseResult.Failure(normalizedScheme, preservedRawValue, "unsupported-scheme")
        };
    }

    private static ProviderVersionParseResult ParseSemver(string scheme, string rawValue, string value)
    {
        if (!SemanticVersion.TryParse(value, out var version))
        {
            return ProviderVersionParseResult.Failure(scheme, rawValue, "invalid-semver");
        }

        return ProviderVersionParseResult.Success(
            scheme,
            rawValue,
            version!.ToString(),
            [version.Major, version.Minor, version.Patch]);
    }

    private static ProviderVersionParseResult ParseInteger(string scheme, string rawValue, string value)
    {
        if (!TryParseNonNegativeInteger(value, out var integer))
        {
            return ProviderVersionParseResult.Failure(scheme, rawValue, "invalid-integer");
        }

        return ProviderVersionParseResult.Success(
            scheme,
            rawValue,
            integer.ToString(CultureInfo.InvariantCulture),
            [integer]);
    }

    private static ProviderVersionParseResult ParseScaledInteger(string scheme, string rawValue, string value)
    {
        if (!TryParseNonNegativeInteger(value, out var integer))
        {
            return ProviderVersionParseResult.Failure(scheme, rawValue, "invalid-scaled-integer");
        }

        var major = integer / 100;
        var minor = integer % 100;

        return ProviderVersionParseResult.Success(
            scheme,
            rawValue,
            string.Create(CultureInfo.InvariantCulture, $"{major}.{minor:00}"),
            [major, minor]);
    }

    private static bool TryParseNonNegativeInteger(string value, out long integer) =>
        long.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out integer) && integer >= 0;
}
