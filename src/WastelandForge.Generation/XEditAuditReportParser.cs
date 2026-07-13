using System.Text.Json;
using System.Text.Json.Nodes;
using System.Security.Cryptography;
using Json.Schema;
using WastelandForge.Core;
using WastelandForge.Schema;
using WastelandForge.Validation;

namespace WastelandForge.Generation;

public sealed class XEditAuditReportParser
{
    public const string Target = XEditAuditAdapterPlanner.Target;
    public const string ReportKind = "wastelandforge.xedit-audit-report";
    public const string ReportParserRuleId = "WF-GEN-009";
    public const string CheckReportRuleId = "WF-GEN-015";
    public const string CheckFindingRuleId = "WF-SEM-045";
    private static readonly Lazy<JsonSchema> CheckReportSchema = new(() => { WastelandForgeSchemaCatalog.TryGetById(WastelandForgeSchemaIds.XEditCheckReport010, out var resource); return JsonSchema.FromText(WastelandForgeSchemaCatalog.ReadText(resource!), new BuildOptions { SchemaRegistry = new SchemaRegistry() }); });

    private static readonly HashSet<string> FindingSeverities = new(StringComparer.Ordinal)
    {
        "error",
        "warning",
        "note"
    };

    public XEditAuditReportParserResult Parse(string projectPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(projectPath);

        var plan = new XEditAuditAdapterPlanner().Plan(projectPath);
        var issues = new List<DiagnosticIssue>(plan.Diagnostics.Issues);
        if (plan.HasErrors)
        {
            return CreateResult(plan, issues, []);
        }

        var reports = new List<XEditAuditReport>();
        foreach (var audit in plan.Audits.OrderBy(audit => audit.ReportPath, StringComparer.Ordinal))
        {
            var fullPath = ResolveGeneratedReportPath(plan.ProjectRoot, audit, issues);
            if (fullPath is null)
            {
                continue;
            }

            if (!File.Exists(fullPath))
            {
                issues.Add(CreateReportIssue(
                    plan,
                    audit.ReportPath,
                    null,
                    "xEdit audit report is missing",
                    $"Expected synthetic xEdit audit report '{audit.ReportPath}' to exist before parsing.",
                    "Create or copy the synthetic JSON report fixture before invoking the report parser."));
                continue;
            }

            var before = issues.Count;
            var parsed = TryParseReport(plan, audit, fullPath, issues);
            if (parsed is not null && issues.Count == before)
            {
                reports.Add(parsed);
                if (StringComparer.Ordinal.Equals(audit.Intent, "check-for-errors"))
                {
                    foreach (var finding in parsed.Findings)
                    {
                        issues.Add(new DiagnosticIssue(RuleId.Parse(CheckFindingRuleId), DiagnosticSeverity.Error, "semantic", "xEdit Check found a record error", finding.Message, finding.Source, plan.ProjectId, suggestedFix: $"Inspect {finding.Plugin} {finding.RecordType} {finding.FormId ?? "(unknown FormID)"} in xEdit or GECK, correct the record manually, and rerun the audit.", docsUri: new Uri($"https://docs.wastelandforge.dev/rules/{CheckFindingRuleId}")));
                    }
                }
            }
        }

        return CreateResult(plan, issues, reports);
    }

    private static XEditAuditReportParserResult CreateResult(
        XEditAuditAdapterPlanResult plan,
        IReadOnlyList<DiagnosticIssue> issues,
        IReadOnlyList<XEditAuditReport> reports) =>
        new(
            plan.ProjectRoot,
            Target,
            plan.ProjectId,
            new DiagnosticReport(plan.ProjectId, issues),
            plan.Audits,
            reports);

    private static XEditAuditReport? TryParseReport(
        XEditAuditAdapterPlanResult plan,
        XEditAuditAdapterPlanEntry audit,
        string fullPath,
        List<DiagnosticIssue> issues)
    {
        var reportPath = ToDisplayPath(plan.ProjectRoot, fullPath);
        if (new FileInfo(fullPath).Length > 4 * 1024 * 1024)
        {
            issues.Add(CreateCheckIssue(plan, reportPath, null, "xEdit Check report is too large", "The report exceeds the 4 MiB ingestion limit."));
            return null;
        }
        JsonObject? root;
        try
        {
            root = JsonNode.Parse(File.ReadAllText(fullPath)) as JsonObject;
        }
        catch (JsonException exception)
        {
            issues.Add(CreateReportIssue(
                plan,
                reportPath,
                null,
                "xEdit audit report JSON is invalid",
                $"Could not parse synthetic xEdit audit report '{reportPath}': {exception.Message}",
                "Regenerate or replace the synthetic report fixture with valid JSON."));
            return null;
        }

        if (root is null)
        {
            issues.Add(CreateReportIssue(
                plan,
                reportPath,
                null,
                "xEdit audit report root is invalid",
                $"Expected synthetic xEdit audit report '{reportPath}' to contain a JSON object.",
                "Regenerate or replace the synthetic report fixture with an object root."));
            return null;
        }

        if (StringComparer.Ordinal.Equals(audit.Intent, "check-for-errors")) return TryParseCheckReport(plan, audit, root, reportPath, issues);

        var kind = ReadRequiredString(plan, root, "kind", reportPath, "/kind", issues);
        var auditId = ReadRequiredString(plan, root, "auditId", reportPath, "/auditId", issues);
        var intent = ReadRequiredString(plan, root, "intent", reportPath, "/intent", issues);
        var reportFormat = ReadRequiredString(plan, root, "reportFormat", reportPath, "/reportFormat", issues);
        if (kind is not null && !StringComparer.Ordinal.Equals(kind, ReportKind))
        {
            issues.Add(CreateReportIssue(
                plan,
                reportPath,
                "/kind",
                "xEdit audit report kind is invalid",
                $"Expected report kind '{ReportKind}', but found '{kind}'.",
                "Use the WastelandForge synthetic xEdit audit report contract."));
        }

        if (auditId is not null && !StringComparer.Ordinal.Equals(auditId, audit.AuditId))
        {
            issues.Add(CreateReportIssue(
                plan,
                reportPath,
                "/auditId",
                "xEdit audit report audit ID does not match plan",
                $"Expected report auditId '{audit.AuditId}', but found '{auditId}'.",
                "Use a synthetic report fixture generated for the matching xEdit audit contract."));
        }

        if (reportFormat is not null && !StringComparer.Ordinal.Equals(reportFormat, audit.ReportFormat))
        {
            issues.Add(CreateReportIssue(
                plan,
                reportPath,
                "/reportFormat",
                "xEdit audit report format does not match plan",
                $"Expected reportFormat '{audit.ReportFormat}', but found '{reportFormat}'.",
                "Use a synthetic report fixture matching the xEdit audit report format."));
        }

        var source = ReadRequiredObject(plan, root, "source", reportPath, "/source", issues);
        var synthetic = ReadRequiredBoolean(plan, source, "synthetic", reportPath, "/source/synthetic", issues);
        var usesRealPluginFixture = ReadRequiredBoolean(plan, source, "usesRealPluginFixture", reportPath, "/source/usesRealPluginFixture", issues);
        if (synthetic is false)
        {
            issues.Add(CreateReportIssue(
                plan,
                reportPath,
                "/source/synthetic",
                "xEdit audit report fixture is not synthetic",
                "Gate 222 only accepts synthetic xEdit audit report fixtures.",
                "Replace the report with a synthetic redistributable fixture."));
        }

        if (usesRealPluginFixture is true)
        {
            issues.Add(CreateReportIssue(
                plan,
                reportPath,
                "/source/usesRealPluginFixture",
                "xEdit audit report uses a real plugin fixture",
                "Gate 222 report parsing must not depend on real third-party plugin fixtures.",
                "Replace the report with synthetic redistributable evidence."));
        }

        var safety = ReadRequiredObject(plan, root, "safety", reportPath, "/safety", issues);
        var executesXEdit = ReadRequiredBoolean(plan, safety, "executesXEdit", reportPath, "/safety/executesXEdit", issues);
        var mutatesPlugins = ReadRequiredBoolean(plan, safety, "mutatesPlugins", reportPath, "/safety/mutatesPlugins", issues);
        var writesPatches = ReadRequiredBoolean(plan, safety, "writesPatches", reportPath, "/safety/writesPatches", issues);
        if (executesXEdit is true)
        {
            AddSafetyIssue(plan, reportPath, "/safety/executesXEdit", "xEdit audit report records xEdit execution", "Gate 222 does not accept executed xEdit reports.");
        }

        if (mutatesPlugins is true)
        {
            AddSafetyIssue(plan, reportPath, "/safety/mutatesPlugins", "xEdit audit report records plugin mutation", "Report parsing must not describe plugin mutation in Gate 222.");
        }

        if (writesPatches is true)
        {
            AddSafetyIssue(plan, reportPath, "/safety/writesPatches", "xEdit audit report records patch output", "Report parsing must not describe patch generation in Gate 222.");
        }

        var records = ReadRecords(plan, root, reportPath, issues);
        var findings = ReadFindings(plan, root, reportPath, issues);
        if (issues.Any(issue => StringComparer.Ordinal.Equals(issue.PrimaryLocation.File, reportPath)))
        {
            return null;
        }

        var summary = new XEditAuditReportSummary(
            records.Count,
            findings.Count,
            findings.Count(finding => StringComparer.Ordinal.Equals(finding.Severity, "error")),
            findings.Count(finding => StringComparer.Ordinal.Equals(finding.Severity, "warning")),
            findings.Count(finding => StringComparer.Ordinal.Equals(finding.Severity, "note")));

        return new XEditAuditReport(
            auditId ?? audit.AuditId,
            reportPath,
            kind ?? ReportKind,
            intent ?? audit.Intent,
            reportFormat ?? audit.ReportFormat,
            synthetic ?? false,
            usesRealPluginFixture ?? false,
            new XEditAuditReportSafety(executesXEdit ?? false, mutatesPlugins ?? false, writesPatches ?? false),
            summary,
            records,
            findings,
            new SourceLocation(reportPath));

        void AddSafetyIssue(XEditAuditAdapterPlanResult planResult, string path, string pointer, string title, string message)
        {
            issues.Add(CreateReportIssue(
                planResult,
                path,
                pointer,
                title,
                message,
                "Use synthetic report evidence that records no external execution, patch writing, or plugin mutation."));
        }
    }

    private static XEditAuditReport? TryParseCheckReport(XEditAuditAdapterPlanResult plan, XEditAuditAdapterPlanEntry audit, JsonObject root, string reportPath, List<DiagnosticIssue> issues)
    {
        using (var document = JsonDocument.Parse(root.ToJsonString()))
            if (!CheckReportSchema.Value.Evaluate(document.RootElement).IsValid) { issues.Add(CreateCheckIssue(plan, reportPath, null, "xEdit Check report schema is invalid", "The report does not satisfy xedit-check-report/0.1.0.")); return null; }
        if (!StringComparer.Ordinal.Equals(root["auditId"]!.GetValue<string>(), audit.AuditId)) { issues.Add(CreateCheckIssue(plan, reportPath, "/auditId", "xEdit Check report audit is stale", "The report auditId does not match the current declaration.")); return null; }
        var scriptPath = Path.GetFullPath(Path.Combine(plan.ProjectRoot, audit.ScriptPath.Replace('/', Path.DirectorySeparatorChar)));
        var allowedScripts = Path.GetFullPath(Path.Combine(plan.ProjectRoot, "generated", Target, "scripts"));
        if (!IsInsideOrEqual(allowedScripts, scriptPath) || !File.Exists(scriptPath) || (File.GetAttributes(scriptPath) & FileAttributes.ReparsePoint) != 0 || !StringComparer.Ordinal.Equals(Sha(scriptPath), root["script"]!["sha256"]!.GetValue<string>())) { issues.Add(CreateCheckIssue(plan, reportPath, "/script/sha256", "xEdit Check report script provenance is stale", "The report script digest does not match the current generated audit script.")); return null; }
        var target = audit.TargetPlugins.SingleOrDefault(plugin => plugin.Role == "subject");
        var plugins = PluginArtifactRegistryReader.Read(plan.ProjectRoot);
        var plugin = plugins.Plugins.SingleOrDefault(item => target is not null && StringComparer.OrdinalIgnoreCase.Equals(item.DataPath, target.Name));
        var subject = root["subject"]!.AsObject();
        if (plugin is null || !StringComparer.OrdinalIgnoreCase.Equals(subject["plugin"]!.GetValue<string>(), plugin.DataPath) || subject["length"]!.GetValue<long>() != plugin.Length || !StringComparer.Ordinal.Equals(subject["sha256"]!.GetValue<string>(), plugin.Sha256) || !StringComparer.Ordinal.Equals(subject["reviewStatus"]!.GetValue<string>(), plugin.ReviewStatus)) { issues.Add(CreateCheckIssue(plan, reportPath, "/subject", "xEdit Check report plugin provenance is stale", "The report subject does not match the current project plugin artifact.")); return null; }
        var findings = new List<XEditAuditReportFinding>();
        var records = new List<XEditAuditReportRecord>();
        var seen = new HashSet<string>(StringComparer.Ordinal);
        var array = root["findings"]!.AsArray();
        for (var index = 0; index < array.Count; index++)
        {
            var finding = array[index]!.AsObject(); var signature = finding["signature"]!.GetValue<string>(); var fixedId = finding["fixedFormId"]!.GetValue<string>(); var file = finding["recordFile"]!.GetValue<string>(); var message = finding["message"]!.GetValue<string>();
            if (!audit.RecordTypes.Contains(signature, StringComparer.Ordinal) || !StringComparer.OrdinalIgnoreCase.Equals(file, plugin.DataPath)) { issues.Add(CreateCheckIssue(plan, reportPath, $"/findings/{index}", "xEdit Check report finding is undeclared", "The finding plugin or signature is outside the current audit declaration.")); continue; }
            var key = $"{file}|{fixedId}|{signature}|{message}"; if (!seen.Add(key)) { issues.Add(CreateCheckIssue(plan, reportPath, $"/findings/{index}", "xEdit Check report finding is duplicated", "Duplicate record-error findings are refused.")); continue; }
            var source = new SourceLocation(reportPath, JsonPointer.Parse($"/findings/{index}")); var editorId = ReadOptionalString(finding, "editorId");
            records.Add(new(file, signature, editorId, fixedId, source));
            findings.Add(new($"xedit.check.{fixedId.ToLowerInvariant()}.{index}", "error", file, signature, editorId, fixedId, message, source));
        }
        if (issues.Any(issue => StringComparer.Ordinal.Equals(issue.RuleId.ToString(), CheckReportRuleId))) return null;
        findings = findings.OrderBy(item => item.Plugin, StringComparer.Ordinal).ThenBy(item => item.FormId, StringComparer.Ordinal).ThenBy(item => item.RecordType, StringComparer.Ordinal).ThenBy(item => item.Message, StringComparer.Ordinal).ToList();
        return new(audit.AuditId, reportPath, "wastelandforge.xedit-check-report", audit.Intent, audit.ReportFormat, false, true, new(false, false, false), new(root["recordsVisited"]!.GetValue<int>(), findings.Count, findings.Count, 0, 0), records, findings, new SourceLocation(reportPath));
    }

    private static DiagnosticIssue CreateCheckIssue(XEditAuditAdapterPlanResult plan, string file, string? pointer, string title, string message) => new(RuleId.Parse(CheckReportRuleId), DiagnosticSeverity.Error, "generation", title, message, new SourceLocation(file, pointer is null ? null : JsonPointer.Parse(pointer)), plan.ProjectId, suggestedFix: "Regenerate the audit script and rerun it manually against the exact current plugin, then ingest the new report.", docsUri: new Uri($"https://docs.wastelandforge.dev/rules/{CheckReportRuleId}"));
    private static string Sha(string path) { using var stream = File.OpenRead(path); return Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant(); }

    private static IReadOnlyList<XEditAuditReportRecord> ReadRecords(
        XEditAuditAdapterPlanResult plan,
        JsonObject root,
        string reportPath,
        List<DiagnosticIssue> issues)
    {
        var records = ReadRequiredArray(plan, root, "records", reportPath, "/records", issues);
        if (records is null)
        {
            return [];
        }

        var parsed = new List<XEditAuditReportRecord>();
        for (var index = 0; index < records.Count; index++)
        {
            var pointer = $"/records/{index}";
            if (records[index] is not JsonObject record)
            {
                issues.Add(CreateReportIssue(
                    plan,
                    reportPath,
                    pointer,
                    "xEdit audit report record is invalid",
                    $"Expected record entry {index} to be a JSON object.",
                    "Use object entries in the synthetic records array."));
                continue;
            }

            var plugin = ReadRequiredString(plan, record, "plugin", reportPath, $"{pointer}/plugin", issues);
            var recordType = ReadRequiredString(plan, record, "recordType", reportPath, $"{pointer}/recordType", issues);
            if (plugin is null || recordType is null)
            {
                continue;
            }

            parsed.Add(new XEditAuditReportRecord(
                plugin,
                recordType,
                ReadOptionalString(record, "editorId"),
                ReadOptionalString(record, "formId"),
                new SourceLocation(reportPath, JsonPointer.Parse(pointer))));
        }

        return parsed;
    }

    private static IReadOnlyList<XEditAuditReportFinding> ReadFindings(
        XEditAuditAdapterPlanResult plan,
        JsonObject root,
        string reportPath,
        List<DiagnosticIssue> issues)
    {
        var findings = ReadRequiredArray(plan, root, "findings", reportPath, "/findings", issues);
        if (findings is null)
        {
            return [];
        }

        var parsed = new List<XEditAuditReportFinding>();
        for (var index = 0; index < findings.Count; index++)
        {
            var pointer = $"/findings/{index}";
            if (findings[index] is not JsonObject finding)
            {
                issues.Add(CreateReportIssue(
                    plan,
                    reportPath,
                    pointer,
                    "xEdit audit report finding is invalid",
                    $"Expected finding entry {index} to be a JSON object.",
                    "Use object entries in the synthetic findings array."));
                continue;
            }

            var id = ReadRequiredString(plan, finding, "id", reportPath, $"{pointer}/id", issues);
            var severity = ReadRequiredString(plan, finding, "severity", reportPath, $"{pointer}/severity", issues);
            var plugin = ReadRequiredString(plan, finding, "plugin", reportPath, $"{pointer}/plugin", issues);
            var recordType = ReadRequiredString(plan, finding, "recordType", reportPath, $"{pointer}/recordType", issues);
            var message = ReadRequiredString(plan, finding, "message", reportPath, $"{pointer}/message", issues);
            if (severity is not null && !FindingSeverities.Contains(severity))
            {
                issues.Add(CreateReportIssue(
                    plan,
                    reportPath,
                    $"{pointer}/severity",
                    "xEdit audit report finding severity is invalid",
                    $"Expected finding severity to be error, warning, or note, but found '{severity}'.",
                    "Use the synthetic xEdit audit report finding severity vocabulary."));
            }

            if (id is null || severity is null || plugin is null || recordType is null || message is null)
            {
                continue;
            }

            parsed.Add(new XEditAuditReportFinding(
                id,
                severity,
                plugin,
                recordType,
                ReadOptionalString(finding, "editorId"),
                ReadOptionalString(finding, "formId"),
                message,
                new SourceLocation(reportPath, JsonPointer.Parse(pointer))));
        }

        return parsed;
    }

    private static string? ResolveGeneratedReportPath(
        string projectRoot,
        XEditAuditAdapterPlanEntry audit,
        List<DiagnosticIssue> issues)
    {
        var allowedRoot = Path.GetFullPath(Path.Combine(projectRoot, "generated", Target, "reports"));
        var fullPath = Path.GetFullPath(Path.Combine(
            projectRoot,
            audit.ReportPath.Replace('/', Path.DirectorySeparatorChar)));
        if (IsInsideOrEqual(allowedRoot, fullPath))
        {
            return fullPath;
        }

        issues.Add(new DiagnosticIssue(
            RuleId.Parse(ReportParserRuleId),
            DiagnosticSeverity.Error,
            "generation",
            "xEdit audit report path escaped generated report root",
            $"Expected report path '{audit.ReportPath}' to stay under generated/{Target}/reports.",
            audit.Source,
            suggestedFix: $"Use a report path under generated/{Target}/reports.",
            docsUri: new Uri($"https://docs.wastelandforge.dev/rules/{ReportParserRuleId}")));
        return null;
    }

    private static string? ReadRequiredString(
        XEditAuditAdapterPlanResult plan,
        JsonObject? obj,
        string propertyName,
        string reportPath,
        string pointer,
        List<DiagnosticIssue> issues)
    {
        if (obj?[propertyName] is JsonValue value &&
            value.TryGetValue<string>(out var text) &&
            !string.IsNullOrWhiteSpace(text))
        {
            return text;
        }

        issues.Add(CreateReportIssue(
            plan,
            reportPath,
            pointer,
            "xEdit audit report field is missing or invalid",
            $"Expected required string field '{propertyName}' at '{pointer}'.",
            "Use the synthetic xEdit audit report contract."));
        return null;
    }

    private static string? ReadOptionalString(JsonObject obj, string propertyName)
    {
        if (obj[propertyName] is JsonValue value &&
            value.TryGetValue<string>(out var text) &&
            !string.IsNullOrWhiteSpace(text))
        {
            return text;
        }

        return null;
    }

    private static bool? ReadRequiredBoolean(
        XEditAuditAdapterPlanResult plan,
        JsonObject? obj,
        string propertyName,
        string reportPath,
        string pointer,
        List<DiagnosticIssue> issues)
    {
        if (obj?[propertyName] is JsonValue value && value.TryGetValue<bool>(out var flag))
        {
            return flag;
        }

        issues.Add(CreateReportIssue(
            plan,
            reportPath,
            pointer,
            "xEdit audit report field is missing or invalid",
            $"Expected required boolean field '{propertyName}' at '{pointer}'.",
            "Use the synthetic xEdit audit report contract."));
        return null;
    }

    private static JsonObject? ReadRequiredObject(
        XEditAuditAdapterPlanResult plan,
        JsonObject root,
        string propertyName,
        string reportPath,
        string pointer,
        List<DiagnosticIssue> issues)
    {
        if (root[propertyName] is JsonObject obj)
        {
            return obj;
        }

        issues.Add(CreateReportIssue(
            plan,
            reportPath,
            pointer,
            "xEdit audit report object is missing or invalid",
            $"Expected required object field '{propertyName}' at '{pointer}'.",
            "Use the synthetic xEdit audit report contract."));
        return null;
    }

    private static JsonArray? ReadRequiredArray(
        XEditAuditAdapterPlanResult plan,
        JsonObject root,
        string propertyName,
        string reportPath,
        string pointer,
        List<DiagnosticIssue> issues)
    {
        if (root[propertyName] is JsonArray array)
        {
            return array;
        }

        issues.Add(CreateReportIssue(
            plan,
            reportPath,
            pointer,
            "xEdit audit report array is missing or invalid",
            $"Expected required array field '{propertyName}' at '{pointer}'.",
            "Use the synthetic xEdit audit report contract."));
        return null;
    }

    private static DiagnosticIssue CreateReportIssue(
        XEditAuditAdapterPlanResult plan,
        string file,
        string? pointer,
        string title,
        string message,
        string suggestedFix) =>
        new(
            RuleId.Parse(ReportParserRuleId),
            DiagnosticSeverity.Error,
            "generation",
            title,
            message,
            new SourceLocation(file, pointer is null ? null : JsonPointer.Parse(pointer)),
            plan.ProjectId,
            suggestedFix: suggestedFix,
            docsUri: new Uri($"https://docs.wastelandforge.dev/rules/{ReportParserRuleId}"));

    private static string ToDisplayPath(string root, string path) =>
        Path.GetRelativePath(root, path).Replace('\\', '/');

    private static bool IsInsideOrEqual(string root, string candidate)
    {
        var normalizedRoot = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var normalizedCandidate = Path.GetFullPath(candidate).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        return StringComparer.OrdinalIgnoreCase.Equals(normalizedRoot, normalizedCandidate) ||
            normalizedCandidate.StartsWith(normalizedRoot + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) ||
            normalizedCandidate.StartsWith(normalizedRoot + Path.AltDirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);
    }
}
