using WastelandForge.Registry;

namespace WastelandForge.SemanticTests;

public sealed class ProviderVersionParserTests
{
    [Fact]
    public void SemverSchemeParsesSyntheticRawValue()
    {
        var result = ProviderVersionParser.Parse("semver", "6.4.7");

        Assert.True(result.Parsed);
        Assert.Equal("semver", result.Scheme);
        Assert.Equal("6.4.7", result.RawValue);
        Assert.Equal("6.4.7", result.NormalizedValue);
        Assert.Equal([6, 4, 7], result.NumericComponents);
        Assert.Null(result.FailureReason);
    }

    [Fact]
    public void SemverSchemeRejectsReleaseArchiveLabels()
    {
        var result = ProviderVersionParser.Parse("semver", "nvse_6_4_7.7z");

        Assert.False(result.Parsed);
        Assert.Equal("nvse_6_4_7.7z", result.RawValue);
        Assert.Null(result.NormalizedValue);
        Assert.Empty(result.NumericComponents);
        Assert.Equal("invalid-semver", result.FailureReason);
    }

    [Fact]
    public void IntegerSchemeParsesSyntheticRuntimeStyleValue()
    {
        var result = ProviderVersionParser.Parse("integer", "647");

        Assert.True(result.Parsed);
        Assert.Equal("integer", result.Scheme);
        Assert.Equal("647", result.RawValue);
        Assert.Equal("647", result.NormalizedValue);
        Assert.Equal([647], result.NumericComponents);
        Assert.Null(result.FailureReason);
    }

    [Fact]
    public void ScaledIntegerSchemeParsesSyntheticGetPluginVersionValue()
    {
        var result = ProviderVersionParser.Parse("scaled-integer", "180");

        Assert.True(result.Parsed);
        Assert.Equal("scaled-integer", result.Scheme);
        Assert.Equal("180", result.RawValue);
        Assert.Equal("1.80", result.NormalizedValue);
        Assert.Equal([1, 80], result.NumericComponents);
        Assert.Null(result.FailureReason);
    }

    [Fact]
    public void ScaledIntegerSchemePreservesRawValueWhenParsingFails()
    {
        var result = ProviderVersionParser.Parse("scaled-integer", "57.30");

        Assert.False(result.Parsed);
        Assert.Equal("57.30", result.RawValue);
        Assert.Null(result.NormalizedValue);
        Assert.Empty(result.NumericComponents);
        Assert.Equal("invalid-scaled-integer", result.FailureReason);
    }

    [Fact]
    public void UnsupportedSchemePreservesRawValue()
    {
        var result = ProviderVersionParser.Parse("custom", "57.30");

        Assert.False(result.Parsed);
        Assert.Equal("custom", result.Scheme);
        Assert.Equal("57.30", result.RawValue);
        Assert.Null(result.NormalizedValue);
        Assert.Empty(result.NumericComponents);
        Assert.Equal("unsupported-scheme", result.FailureReason);
    }
}
