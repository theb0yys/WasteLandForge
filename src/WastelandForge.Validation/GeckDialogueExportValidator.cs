using WastelandForge.Core;

namespace WastelandForge.Validation;

public sealed class GeckDialogueExportValidator
{
    public IReadOnlyList<DiagnosticIssue> Validate(string exportPath, LogicalId? projectId = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(exportPath);

        if (!File.Exists(exportPath))
        {
            return
            [
                CreateIssue(
                    "WF-LOAD-009",
                    "GECK dialogue export was not found",
                    $"GECK dialogue export file '{exportPath}' could not be found.",
                    exportPath,
                    projectId,
                    "Export quest dialogue from GECK to a text file, then pass that file to --geck-dialogue-export.",
                    $"wf:load:009:{exportPath}")
            ];
        }

        string text;
        try
        {
            text = File.ReadAllText(exportPath);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or NotSupportedException or ArgumentException)
        {
            return
            [
                CreateIssue(
                    "WF-LOAD-010",
                    "GECK dialogue export could not be read",
                    $"GECK dialogue export file '{exportPath}' could not be read as text: {ex.Message}",
                    exportPath,
                    projectId,
                    "Check that the export path points to a readable text file produced by GECK.",
                    $"wf:load:010:{exportPath}")
            ];
        }

        if (text.Contains('\0'))
        {
            return
            [
                CreateIssue(
                    "WF-LOAD-010",
                    "GECK dialogue export could not be read",
                    $"GECK dialogue export file '{exportPath}' appears to be binary rather than text.",
                    exportPath,
                    projectId,
                    "Export quest dialogue from GECK as text before passing it to --geck-dialogue-export.",
                    $"wf:load:010:{exportPath}")
            ];
        }

        if (string.IsNullOrWhiteSpace(text))
        {
            return
            [
                CreateIssue(
                    "WF-LOAD-011",
                    "GECK dialogue export is empty",
                    $"GECK dialogue export file '{exportPath}' is empty.",
                    exportPath,
                    projectId,
                    "Export a quest with dialogue rows from GECK, or choose the non-empty export file.",
                    $"wf:load:011:{exportPath}")
            ];
        }

        return [];
    }

    private static DiagnosticIssue CreateIssue(
        string ruleId,
        string title,
        string message,
        string file,
        LogicalId? projectId,
        string suggestedFix,
        string fingerprint)
    {
        return new DiagnosticIssue(
            RuleId.Parse(ruleId),
            DiagnosticSeverity.Error,
            "load",
            title,
            message,
            new SourceLocation(file),
            projectId,
            suggestedFix: suggestedFix,
            docsUri: new Uri($"https://docs.wastelandforge.dev/rules/{ruleId}"),
            fingerprint: fingerprint);
    }
}
