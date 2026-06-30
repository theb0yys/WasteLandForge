namespace WastelandForge.Generation;

public sealed record MetadataReportOutputs(
    string Root,
    string ValidationReport,
    string DependencyReport,
    string CapabilityReport,
    string RunReport,
    string Manifest,
    string? Checksums);
