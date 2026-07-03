namespace WastelandForge.Generation;

public sealed record XEditAuditAdapterPlanEntry(
    string AuditId,
    string Intent,
    string Mode,
    string ScriptLanguage,
    string ReportFormat,
    IReadOnlyList<XEditAuditPluginTarget> TargetPlugins,
    IReadOnlyList<string> RecordTypes,
    IReadOnlyList<string> RequiredCapabilities,
    string ScriptPath,
    string ReportPath,
    bool ExecutesXEdit,
    bool MutatesPlugins,
    bool WritesPatches,
    bool UsesRealPluginFixture,
    WastelandForge.Core.SourceLocation Source);

public sealed record XEditAuditPluginTarget(
    string Name,
    string Role);
