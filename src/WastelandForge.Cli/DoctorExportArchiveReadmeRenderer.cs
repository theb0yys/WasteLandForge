using System.Text;

namespace WastelandForge.Cli;

internal static class DoctorExportArchiveReadmeRenderer
{
    public static string Render(DoctorExportReport report, IReadOnlyList<DoctorExportArchiveSupplement> supplements)
    {
        ArgumentNullException.ThrowIfNull(report);
        ArgumentNullException.ThrowIfNull(supplements);

        var supplementPaths = supplements
            .Select(supplement => supplement.Path)
            .Order(StringComparer.Ordinal)
            .ToArray();
        var hasActionIndex = supplementPaths.Contains("actions/index.md", StringComparer.Ordinal);
        var hasBundleIndex = supplementPaths.Contains("bundle/index.md", StringComparer.Ordinal);
        var hasCapabilityIndex = supplementPaths.Contains("capabilities/index.md", StringComparer.Ordinal);
        var hasCataloguePolicyIndex = supplementPaths.Contains("catalogue-policy/index.md", StringComparer.Ordinal);
        var hasDiagnosticIndex = supplementPaths.Contains("diagnostics/index.md", StringComparer.Ordinal);
        var hasDoctorAreaIndex = supplementPaths.Contains("doctor-areas/index.md", StringComparer.Ordinal);
        var hasEvidenceIndex = supplementPaths.Contains("evidence/index.md", StringComparer.Ordinal);
        var hasHandoffSummary = supplementPaths.Contains("handoff-summary.md", StringComparer.Ordinal);
        var hasOpenQuestionIndex = supplementPaths.Contains("open-questions/index.md", StringComparer.Ordinal);
        var hasProviderIndex = supplementPaths.Contains("providers/index.md", StringComparer.Ordinal);
        var hasRedactionIndex = supplementPaths.Contains("redaction/index.md", StringComparer.Ordinal);
        var hasRequirementIndex = supplementPaths.Contains("requirements/index.md", StringComparer.Ordinal);
        var hasRequirementExplanationIndex = supplementPaths.Contains("requirement-explanations/index.md", StringComparer.Ordinal);
        var hasScanInputIndex = supplementPaths.Contains("scan-inputs/index.md", StringComparer.Ordinal);
        var hasSummaryIndex = supplementPaths.Contains("summary/index.md", StringComparer.Ordinal);
        var hasTriageIndex = supplementPaths.Contains("triage/index.md", StringComparer.Ordinal);
        var hasRequirementExplanations = supplementPaths.Any(path => path.StartsWith("requirement-explanations/", StringComparison.Ordinal));

        var builder = new StringBuilder();
        builder.AppendLine("# WastelandForge Doctor Handoff Bundle");
        builder.AppendLine();
        builder.AppendLine("Command: `doctor export`");
        builder.AppendLine($"Bundle: `{EscapeInline(report.Kind)}`");
        builder.AppendLine($"Offline: `{report.Offline.ToString().ToLowerInvariant()}`");
        builder.AppendLine($"AI optional: `{report.AiOptional.ToString().ToLowerInvariant()}`");
        builder.AppendLine($"Redaction: `{EscapeInline(report.Redaction.Mode)}`; paths `{EscapeInline(report.Redaction.Paths)}`");
        builder.AppendLine();

        builder.AppendLine("## Start Here");
        builder.AppendLine();
        builder.AppendLine("- `doctor-export.md` - redacted human-readable Doctor report.");
        builder.AppendLine("- `doctor-export.json` - redacted machine-readable Doctor report.");
        if (hasHandoffSummary)
        {
            builder.AppendLine("- `handoff-summary.md` - concise remediation, worklist, command, and archive-path handoff summary.");
        }

        if (hasTriageIndex)
        {
            builder.AppendLine("- `triage/index.md` - redacted blocking/review/action triage index.");
            builder.AppendLine("- `triage/index.json` - machine-readable blocking/review/action triage index.");
        }

        if (hasBundleIndex)
        {
            builder.AppendLine("- `bundle/index.md` - redacted bundle navigation index.");
            builder.AppendLine("- `bundle/index.json` - machine-readable bundle navigation index.");
        }

        if (hasActionIndex)
        {
            builder.AppendLine("- `actions/index.md` - redacted next-action handoff index.");
            builder.AppendLine("- `actions/index.json` - machine-readable next-action handoff index.");
        }

        if (hasCapabilityIndex)
        {
            builder.AppendLine("- `capabilities/index.md` - redacted capability readiness handoff index.");
            builder.AppendLine("- `capabilities/index.json` - machine-readable capability readiness handoff index.");
        }

        if (hasCataloguePolicyIndex)
        {
            builder.AppendLine("- `catalogue-policy/index.md` - redacted catalogue-policy open-question handoff index.");
            builder.AppendLine("- `catalogue-policy/index.json` - machine-readable catalogue-policy open-question handoff index.");
        }

        if (hasDiagnosticIndex)
        {
            builder.AppendLine("- `diagnostics/index.md` - redacted diagnostic handoff index.");
            builder.AppendLine("- `diagnostics/index.json` - machine-readable diagnostic handoff index.");
        }

        if (hasDoctorAreaIndex)
        {
            builder.AppendLine("- `doctor-areas/index.md` - redacted Doctor readiness area handoff index.");
            builder.AppendLine("- `doctor-areas/index.json` - machine-readable Doctor readiness area handoff index.");
        }

        if (hasEvidenceIndex)
        {
            builder.AppendLine("- `evidence/index.md` - redacted provider evidence handoff index.");
            builder.AppendLine("- `evidence/index.json` - machine-readable provider evidence handoff index.");
        }

        if (hasOpenQuestionIndex)
        {
            builder.AppendLine("- `open-questions/index.md` - redacted open-question handoff index.");
            builder.AppendLine("- `open-questions/index.json` - machine-readable open-question handoff index.");
        }

        if (hasProviderIndex)
        {
            builder.AppendLine("- `providers/index.md` - redacted provider readiness handoff index.");
            builder.AppendLine("- `providers/index.json` - machine-readable provider readiness handoff index.");
        }

        if (hasRedactionIndex)
        {
            builder.AppendLine("- `redaction/index.md` - redacted bundle redaction policy handoff index.");
            builder.AppendLine("- `redaction/index.json` - machine-readable bundle redaction policy handoff index.");
        }

        if (hasRequirementIndex)
        {
            builder.AppendLine("- `requirements/index.md` - redacted project requirement handoff index.");
            builder.AppendLine("- `requirements/index.json` - machine-readable project requirement handoff index.");
        }

        if (hasScanInputIndex)
        {
            builder.AppendLine("- `scan-inputs/index.md` - redacted capability scan input handoff index.");
            builder.AppendLine("- `scan-inputs/index.json` - machine-readable capability scan input handoff index.");
        }

        if (hasRequirementExplanationIndex)
        {
            builder.AppendLine("- `requirement-explanations/index.md` - redacted unavailable requirement explanation index.");
            builder.AppendLine("- `requirement-explanations/index.json` - machine-readable unavailable requirement explanation index.");
        }

        if (hasSummaryIndex)
        {
            builder.AppendLine("- `summary/index.md` - redacted bundle summary handoff index.");
            builder.AppendLine("- `summary/index.json` - machine-readable bundle summary handoff index.");
        }

        builder.AppendLine("- `doctor-bundle-manifest.json` - archive entry metadata.");
        builder.AppendLine("- `checksums.sha256` - SHA-256 checksums for archive payloads.");
        builder.AppendLine();

        builder.AppendLine("## Summary");
        builder.AppendLine();
        builder.AppendLine($"- Catalog: `{EscapeInline(report.Summary.Catalog.Id)} {EscapeInline(report.Summary.Catalog.Version)}`");
        builder.AppendLine(
            $"- Providers: {report.Summary.Providers.Total} total; {report.Summary.Providers.Probable} probable; {report.Summary.Providers.Missing} missing; {report.Summary.Providers.Unknown} unknown; {report.Summary.Providers.WrongScope} wrong-scope");
        builder.AppendLine(
            $"- Capabilities: {report.Summary.Capabilities.Total} total; {report.Summary.Capabilities.Probable} probable; {report.Summary.Capabilities.Missing} missing; {report.Summary.Capabilities.Unknown} unknown; {report.Summary.Capabilities.WrongScope} wrong-scope");
        builder.AppendLine(
            $"- Doctor areas: {report.Summary.Doctor.Areas} area(s); {report.Summary.Doctor.Ready} ready; {report.Summary.Doctor.ActionNeeded} action-needed; {report.Summary.Doctor.Unknown} unknown");
        if (report.Summary.Requirements is null)
        {
            builder.AppendLine("- Requirements: not included");
        }
        else
        {
            builder.AppendLine(
                $"- Requirements: {report.Summary.Requirements.Total} total; {report.Summary.Requirements.RequiredUnavailable} required unavailable; {report.Summary.Requirements.OptionalUnavailable} optional unavailable");
        }

        builder.AppendLine(
            $"- Diagnostics: {report.Summary.Diagnostics.Issues} issue(s); {report.Summary.Diagnostics.Errors} error(s); {report.Summary.Diagnostics.Warnings} warning(s); {report.Summary.Diagnostics.Notes} note(s)");
        builder.AppendLine();

        builder.AppendLine("## Bundle Entries");
        builder.AppendLine();
        builder.AppendLine("- `README.md` - this file.");
        builder.AppendLine("- `doctor-export.md`");
        builder.AppendLine("- `doctor-export.json`");
        foreach (var path in supplementPaths)
        {
            builder.Append("- `");
            builder.Append(EscapeInline(path));
            builder.AppendLine("`");
        }

        builder.AppendLine("- `doctor-bundle-manifest.json`");
        builder.AppendLine("- `checksums.sha256`");
        builder.AppendLine();

        builder.AppendLine("## Requirement Explanations");
        builder.AppendLine();
        if (hasRequirementExplanations)
        {
            builder.AppendLine("Unavailable project requirements have redacted per-requirement explanation entries under `requirement-explanations/`.");
            builder.AppendLine("Open `requirement-explanations/index.md` first, then follow the listed JSON or Markdown entry paths.");
            builder.AppendLine("Markdown entries include operator handoff checklists with placeholder commands; JSON entries keep the `capabilities explain` contract.");
        }
        else
        {
            builder.AppendLine("No per-requirement explanation entries are included in this bundle.");
        }

        builder.AppendLine();

        builder.AppendLine("## Boundaries");
        builder.AppendLine();
        builder.AppendLine("- Local game, Data, tool, project, and provider evidence paths are redacted.");
        builder.AppendLine("- This archive is deterministic local handoff evidence, not a release package.");
        builder.AppendLine("- Runtime probes, MO2 VFS launch checks, GECK automation, network checks, provider version checks, and AI calls are not performed by this bundle writer.");

        return builder.ToString();
    }

    private static string EscapeInline(string text) =>
        text.Replace("`", "\\`", StringComparison.Ordinal);
}
