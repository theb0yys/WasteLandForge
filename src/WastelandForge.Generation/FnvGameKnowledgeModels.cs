using System.Text.Json.Nodes;

namespace WastelandForge.Generation;

public enum FnvGameKnowledgeState
{
    NotConfigured,
    NotIndexed,
    PreparingExport,
    WaitingForExport,
    Importing,
    Ready,
    Stale,
    Blocked
}

public enum FnvGameKnowledgeExecutionState
{
    Prepared,
    ApprovalRequired,
    Starting,
    Running,
    ProcessExited,
    AuditingSideEffects,
    OutputReady,
    FailedClosed
}

public sealed record FnvGameKnowledgeRecord(
    string SourceFile,
    string Signature,
    string FixedFormId,
    string? LoadOrderFormId,
    string? EditorId,
    string? DisplayName,
    bool IsDeleted,
    JsonObject? Context)
{
    public string StableId => $"{SourceFile}|{Signature}|{FixedFormId}";
    public string ContextKind => Context?["kind"]?.GetValue<string>() ?? "none";
    public string ContextSummary => FnvGameKnowledgeCatalogue.DescribeContext(Context);
}

public sealed record FnvGameKnowledgePreparation(
    bool Success,
    FnvGameKnowledgeState State,
    string Message,
    string? RuleId,
    string? RunDirectory,
    string? ScriptPath,
    string? ManifestPath,
    string? RawExportPath,
    string? ApprovalToken,
    string Details,
    bool ExternalToolExecuted,
    bool GameDataWritten);

public sealed record FnvGameKnowledgeImportResult(
    bool Success,
    FnvGameKnowledgeState State,
    string Message,
    string? RuleId,
    string? IndexPath,
    int RecordCount,
    bool IndexReplaced,
    bool ExternalToolExecuted,
    bool GameDataWritten);

public sealed record FnvGameKnowledgeExecutionPreparation(
    bool Success,
    FnvGameKnowledgeExecutionState State,
    string Message,
    string? RuleId,
    string? RunDirectory,
    string? PlanPath,
    string? ApprovalToken,
    string? ExecutablePath,
    string? WorkingDirectory,
    IReadOnlyList<string> Arguments,
    string Details);

public sealed record FnvGameKnowledgeExecutionRequest(
    string RunDirectory,
    string ApprovalToken,
    string ExecutablePath,
    string WorkingDirectory,
    IReadOnlyList<string> Arguments);

public sealed record FnvGameKnowledgeProcessEvidence(
    bool ProcessStarted,
    int? ProcessId,
    DateTimeOffset StartedAtUtc,
    DateTimeOffset ExitedAtUtc,
    int? ExitCode,
    bool WaitCancelled,
    string? Failure);

public sealed record FnvGameKnowledgeExecutionResult(
    bool Success,
    FnvGameKnowledgeExecutionState State,
    string Message,
    string? RuleId,
    int? ProcessId,
    int? ExitCode,
    string? ReceiptPath,
    string? IndexPath,
    int RecordCount,
    IReadOnlyList<string> ChangedPaths);

public sealed record FnvGameKnowledgeSnapshot(
    FnvGameKnowledgeState State,
    string Message,
    string? RuleId,
    string? IndexPath,
    string? IndexSha256,
    DateTimeOffset? CreatedAtUtc,
    string? SourceClassification,
    string? StaleReason,
    string? LatestRunDirectory,
    IReadOnlyList<FnvGameKnowledgeRecord> Records)
{
    public static FnvGameKnowledgeSnapshot Empty(FnvGameKnowledgeState state, string message, string? ruleId = null) =>
        new(state, message, ruleId, null, null, null, null, null, null, []);
}

public sealed record FnvGameKnowledgeSearchResult(
    IReadOnlyList<FnvGameKnowledgeRecord> Records,
    int TotalMatches,
    bool Truncated,
    string Message);

public sealed record FnvGameKnowledgeReceiptResult(
    bool Success,
    string Message,
    string? RuleId,
    string? ReceiptPath,
    long Length,
    string? Sha256);

public sealed record FnvGameKnowledgeClearPreview(
    bool Success,
    string Message,
    string? RuleId,
    string? Token,
    string Root,
    IReadOnlyList<string> Paths);

public sealed record FnvGameKnowledgeClearResult(bool Success, string Message, string? RuleId);

public sealed record FnvGameKnowledgeLimits(
    long MaxExportBytes,
    int MaxRecords,
    int MaxTextLength,
    int MaxDiagnostics,
    int MaxSearchResults)
{
    public static FnvGameKnowledgeLimits Default { get; } = new(
        256L * 1024 * 1024,
        2_000_000,
        2048,
        100,
        500);
}
