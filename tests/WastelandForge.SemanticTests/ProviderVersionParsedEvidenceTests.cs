using WastelandForge.Registry;

namespace WastelandForge.SemanticTests;

public sealed class ProviderVersionParsedEvidenceTests
{
    [Fact]
    public void FromParseResultCreatesParsedSyntheticEvidence()
    {
        var parseResult = ProviderVersionParser.Parse("scaled-integer", "180");

        var evidence = ProviderVersionParsedEvidence.FromParseResult(
            "provider.runtime.showoff",
            ProviderVersionEvidenceSourceKinds.Synthetic,
            "synthetic://provider-version/showoff",
            parseResult);

        Assert.Equal("provider.runtime.showoff", evidence.ProviderId);
        Assert.Equal(ProviderVersionEvidenceSourceKinds.Synthetic, evidence.SourceKind);
        Assert.Equal("scaled-integer", evidence.Scheme);
        Assert.Equal("180", evidence.RawValue);
        Assert.True(evidence.Parsed);
        Assert.Equal("1.80", evidence.NormalizedValue);
        Assert.Equal([1, 80], evidence.NumericComponents);
        Assert.Null(evidence.FailureReason);
        Assert.Equal("synthetic://provider-version/showoff", evidence.Provenance);
    }

    [Fact]
    public void FromParseResultPreservesFailedRawEvidence()
    {
        var parseResult = ProviderVersionParser.Parse("scaled-integer", "57.30");

        var evidence = ProviderVersionParsedEvidence.FromParseResult(
            "provider.runtime.jip_ln",
            ProviderVersionEvidenceSourceKinds.Synthetic,
            "synthetic://provider-version/jip-ln",
            parseResult);

        Assert.Equal("provider.runtime.jip_ln", evidence.ProviderId);
        Assert.Equal(ProviderVersionEvidenceSourceKinds.Synthetic, evidence.SourceKind);
        Assert.Equal("scaled-integer", evidence.Scheme);
        Assert.Equal("57.30", evidence.RawValue);
        Assert.False(evidence.Parsed);
        Assert.Null(evidence.NormalizedValue);
        Assert.Empty(evidence.NumericComponents);
        Assert.Equal("invalid-scaled-integer", evidence.FailureReason);
        Assert.Equal("synthetic://provider-version/jip-ln", evidence.Provenance);
    }
}
