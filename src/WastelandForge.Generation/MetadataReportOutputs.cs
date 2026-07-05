namespace WastelandForge.Generation;

public sealed record MetadataReportOutputs(
    string Root,
    string ValidationReport,
    string DependencyReport,
    string CapabilityReport,
    string RunReport,
    string? BuildPlan,
    string? BuildPlanMarkdown,
    string? ReportIndex,
    string? ReportIndexMarkdown,
    string Manifest,
    string? Checksums);
