namespace WastelandForge.Registry;

public sealed record ProviderVersionParsedEvidence(
    string ProviderId,
    string SourceKind,
    string Scheme,
    string RawValue,
    bool Parsed,
    string? NormalizedValue,
    IReadOnlyList<long> NumericComponents,
    string? FailureReason,
    string Provenance)
{
    public static ProviderVersionParsedEvidence FromParseResult(
        string providerId,
        string sourceKind,
        string provenance,
        ProviderVersionParseResult parseResult) =>
        new(
            providerId,
            sourceKind,
            parseResult.Scheme,
            parseResult.RawValue,
            parseResult.Parsed,
            parseResult.NormalizedValue,
            parseResult.NumericComponents,
            parseResult.FailureReason,
            provenance);
}

public static class ProviderVersionEvidenceSourceKinds
{
    public const string RuntimeProbe = "runtime-probe";
    public const string FileMetadata = "file-metadata";
    public const string Synthetic = "synthetic";
}
