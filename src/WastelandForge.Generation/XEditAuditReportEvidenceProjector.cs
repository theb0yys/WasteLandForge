using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using WastelandForge.Core;

namespace WastelandForge.Generation;

public sealed class XEditAuditReportEvidenceProjector
{
    public const string Kind = "wastelandforge.xedit-audit-report-handoff";
    public const string FormatVersion = "1.0";

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    private readonly XEditAuditReportParser parser;

    public XEditAuditReportEvidenceProjector(XEditAuditReportParser? parser = null)
    {
        this.parser = parser ?? new XEditAuditReportParser();
    }

    public XEditAuditReportEvidenceProjection Project(string projectPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(projectPath);

        var parsed = parser.Parse(projectPath);
        var status = parsed.HasErrors ? "failed" : "passed";
        var summary = CreateSummary(parsed);
        return new XEditAuditReportEvidenceProjection(
            parsed.ProjectRoot,
            parsed.Target,
            parsed.ProjectId,
            status,
            parsed.Diagnostics,
            summary,
            parsed.Reports,
            CreateMachineJson(parsed, summary, status),
            CreateHumanText(parsed, summary, status));
    }

    private static XEditAuditReportEvidenceSummary CreateSummary(XEditAuditReportParserResult parsed) =>
        new(
            parsed.PlanEntries.Count,
            parsed.Reports.Count,
            parsed.Reports.Sum(report => report.Summary.Records),
            parsed.Reports.Sum(report => report.Summary.Findings),
            parsed.Reports.Sum(report => report.Summary.ErrorFindings),
            parsed.Reports.Sum(report => report.Summary.WarningFindings),
            parsed.Reports.Sum(report => report.Summary.NoteFindings),
            parsed.Diagnostics.ErrorCount,
            parsed.Diagnostics.WarningCount,
            parsed.Diagnostics.NoteCount);

    private static string CreateMachineJson(
        XEditAuditReportParserResult parsed,
        XEditAuditReportEvidenceSummary summary,
        string status)
    {
        var issues = new JsonArray();
        foreach (var issue in parsed.Diagnostics.Issues)
        {
            issues.Add(DiagnosticIssueJsonSerializer.ToJsonNode(issue));
        }

        var root = new JsonObject
        {
            ["formatVersion"] = FormatVersion,
            ["kind"] = Kind,
            ["target"] = parsed.Target,
            ["status"] = status,
            ["project"] = ToProjectJson(parsed),
            ["execution"] = new JsonObject
            {
                ["parsesSyntheticReports"] = true,
                ["executesXEdit"] = false,
                ["generatesReports"] = false,
                ["mutatesPlugins"] = false,
                ["writesPatches"] = false,
                ["writesGameData"] = false,
                ["appliesFindingsToPlugins"] = false,
                ["cliWired"] = false
            },
            ["summary"] = ToSummaryJson(summary),
            ["reports"] = new JsonArray(parsed.Reports.Select(ToReportJson).ToArray()),
            ["issues"] = issues,
            ["handoff"] = new JsonObject
            {
                ["machineReadable"] = true,
                ["humanReadable"] = true,
                ["parserCliWired"] = false,
                ["requiresHumanReview"] = true
            }
        };

        return root.ToJsonString(SerializerOptions);
    }

    private static JsonObject ToProjectJson(XEditAuditReportParserResult parsed)
    {
        var project = new JsonObject
        {
            ["root"] = parsed.ProjectRoot
        };
        if (parsed.ProjectId is not null)
        {
            project["id"] = parsed.ProjectId.ToString();
        }

        return project;
    }

    private static JsonObject ToSummaryJson(XEditAuditReportEvidenceSummary summary) =>
        new()
        {
            ["plannedAudits"] = summary.PlannedAudits,
            ["parsedReports"] = summary.ParsedReports,
            ["records"] = summary.Records,
            ["findings"] = summary.Findings,
            ["errorFindings"] = summary.ErrorFindings,
            ["warningFindings"] = summary.WarningFindings,
            ["noteFindings"] = summary.NoteFindings,
            ["diagnosticErrors"] = summary.DiagnosticErrors,
            ["diagnosticWarnings"] = summary.DiagnosticWarnings,
            ["diagnosticNotes"] = summary.DiagnosticNotes
        };

    private static JsonObject ToReportJson(XEditAuditReport report) =>
        new()
        {
            ["auditId"] = report.AuditId,
            ["reportPath"] = report.ReportPath,
            ["kind"] = report.Kind,
            ["intent"] = report.Intent,
            ["reportFormat"] = report.ReportFormat,
            ["source"] = new JsonObject
            {
                ["synthetic"] = report.Synthetic,
                ["usesRealPluginFixture"] = report.UsesRealPluginFixture
            },
            ["safety"] = new JsonObject
            {
                ["executesXEdit"] = report.Safety.ExecutesXEdit,
                ["mutatesPlugins"] = report.Safety.MutatesPlugins,
                ["writesPatches"] = report.Safety.WritesPatches
            },
            ["summary"] = new JsonObject
            {
                ["records"] = report.Summary.Records,
                ["findings"] = report.Summary.Findings,
                ["errorFindings"] = report.Summary.ErrorFindings,
                ["warningFindings"] = report.Summary.WarningFindings,
                ["noteFindings"] = report.Summary.NoteFindings
            },
            ["records"] = new JsonArray(report.Records.Select(ToRecordJson).ToArray()),
            ["findings"] = new JsonArray(report.Findings.Select(ToFindingJson).ToArray()),
            ["sourceLocation"] = ToLocationJson(report.Source)
        };

    private static JsonObject ToRecordJson(XEditAuditReportRecord record)
    {
        var json = new JsonObject
        {
            ["plugin"] = record.Plugin,
            ["recordType"] = record.RecordType,
            ["sourceLocation"] = ToLocationJson(record.Source)
        };
        AddOptional(json, "editorId", record.EditorId);
        AddOptional(json, "formId", record.FormId);
        return json;
    }

    private static JsonObject ToFindingJson(XEditAuditReportFinding finding)
    {
        var json = new JsonObject
        {
            ["id"] = finding.Id,
            ["severity"] = finding.Severity,
            ["plugin"] = finding.Plugin,
            ["recordType"] = finding.RecordType,
            ["message"] = finding.Message,
            ["sourceLocation"] = ToLocationJson(finding.Source)
        };
        AddOptional(json, "editorId", finding.EditorId);
        AddOptional(json, "formId", finding.FormId);
        return json;
    }

    private static JsonObject ToLocationJson(SourceLocation location)
    {
        var json = new JsonObject
        {
            ["file"] = location.File
        };
        AddOptional(json, "pointer", location.Pointer?.ToString());
        if (location.Line is not null)
        {
            json["line"] = location.Line.Value;
        }

        if (location.Column is not null)
        {
            json["column"] = location.Column.Value;
        }

        return json;
    }

    private static string CreateHumanText(
        XEditAuditReportParserResult parsed,
        XEditAuditReportEvidenceSummary summary,
        string status)
    {
        var builder = new StringBuilder();
        AppendLine(builder, "WastelandForge xEdit audit report handoff");
        AppendLine(builder, $"Status: {status}");
        AppendLine(builder, $"Target: {parsed.Target}");
        AppendLine(builder, $"Project: {parsed.ProjectId?.ToString() ?? "(unknown)"}");
        AppendLine(builder, "xEdit execution: not run");
        AppendLine(builder, "Report generation: not performed");
        AppendLine(builder, "Plugin mutation: not performed");
        AppendLine(builder, "Patch writing: not performed");
        AppendLine(builder, "CLI wiring: not exposed");
        AppendLine(builder);
        AppendLine(builder, "Summary:");
        AppendLine(builder, $"- Planned audits: {summary.PlannedAudits}");
        AppendLine(builder, $"- Parsed reports: {summary.ParsedReports}");
        AppendLine(builder, $"- Records: {summary.Records}");
        AppendLine(builder, $"- Findings: {summary.Findings} (errors {summary.ErrorFindings}, warnings {summary.WarningFindings}, notes {summary.NoteFindings})");
        AppendLine(builder, $"- Diagnostics: errors {summary.DiagnosticErrors}, warnings {summary.DiagnosticWarnings}, notes {summary.DiagnosticNotes}");
        AppendLine(builder);
        AppendReports(builder, parsed.Reports);
        AppendIssues(builder, parsed.Diagnostics.Issues);
        return builder.ToString();
    }

    private static void AppendReports(StringBuilder builder, IReadOnlyList<XEditAuditReport> reports)
    {
        AppendLine(builder, "Reports:");
        if (reports.Count == 0)
        {
            AppendLine(builder, "- None");
            AppendLine(builder);
            return;
        }

        foreach (var report in reports)
        {
            AppendLine(builder, $"- {report.ReportPath}");
            AppendLine(builder, $"  Audit: {report.AuditId}");
            AppendLine(builder, $"  Intent: {report.Intent}");
            AppendLine(builder, $"  Report format: {report.ReportFormat}");
            AppendLine(builder, $"  Synthetic: {FormatBoolean(report.Synthetic)}");
            AppendLine(builder, $"  Uses real plugin fixture: {FormatBoolean(report.UsesRealPluginFixture)}");
            AppendLine(builder, $"  Records: {report.Summary.Records}");
            AppendLine(builder, $"  Findings: {report.Summary.Findings} (errors {report.Summary.ErrorFindings}, warnings {report.Summary.WarningFindings}, notes {report.Summary.NoteFindings})");
            foreach (var finding in report.Findings)
            {
                AppendLine(builder, $"  - {finding.Severity}: {finding.Id} [{finding.Plugin} {finding.RecordType}] {finding.Message}");
            }
        }

        AppendLine(builder);
    }

    private static void AppendIssues(StringBuilder builder, IReadOnlyList<DiagnosticIssue> issues)
    {
        AppendLine(builder, "Issues:");
        if (issues.Count == 0)
        {
            AppendLine(builder, "- None");
            return;
        }

        foreach (var issue in issues)
        {
            var pointer = issue.PrimaryLocation.Pointer?.ToString();
            var location = pointer is null
                ? issue.PrimaryLocation.File
                : $"{issue.PrimaryLocation.File}{pointer}";
            AppendLine(builder, $"- {issue.RuleId} {issue.Severity}: {issue.Title} ({location})");
            AppendLine(builder, $"  {issue.Message}");
            if (!string.IsNullOrWhiteSpace(issue.SuggestedFix))
            {
                AppendLine(builder, $"  Fix: {issue.SuggestedFix}");
            }
        }
    }

    private static void AddOptional(JsonObject json, string propertyName, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            json[propertyName] = value;
        }
    }

    private static void AppendLine(StringBuilder builder, string? text = null)
    {
        if (text is not null)
        {
            builder.Append(text);
        }

        builder.Append('\n');
    }

    private static string FormatBoolean(bool value) => value ? "yes" : "no";
}
