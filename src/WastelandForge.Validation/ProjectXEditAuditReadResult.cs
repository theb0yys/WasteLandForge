using WastelandForge.Core;

namespace WastelandForge.Validation;

public sealed record ProjectXEditAuditReadResult(
    string ProjectRoot,
    LogicalId? ProjectId,
    DiagnosticReport Diagnostics,
    IReadOnlyList<XEditAuditDefinition> Audits);

public sealed record XEditAuditDefinition(
    string Id,
    string Intent,
    string Mode,
    string ScriptLanguage,
    string ReportFormat,
    IReadOnlyList<XEditPluginTargetDefinition> TargetPlugins,
    IReadOnlyList<string> RecordTypes,
    IReadOnlyList<string> RequiredCapabilities,
    XEditAuditOutputDefinition Outputs,
    XEditAuditSafetyDefinition Safety,
    SourceLocation Source);

public sealed record XEditPluginTargetDefinition(
    string Name,
    string Role);

public sealed record XEditAuditOutputDefinition(
    string Script,
    string Report);

public sealed record XEditAuditSafetyDefinition(
    bool ExecutesXEdit,
    bool MutatesPlugins,
    bool WritesPatches,
    bool UsesRealPluginFixture);
