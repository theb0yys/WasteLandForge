using System.Text.Json.Nodes;

namespace WastelandForge.Desktop;

internal enum DiagnosticWorkspaceRoute { None, ValidationReport, Capabilities, ProjectOutputs, ReleaseCandidate }

internal sealed record DiagnosticExplanation(
    string RuleId,
    string Command,
    string FamilyId,
    string Scope,
    string ValidationStage,
    string? RuleTitle,
    string? RuleSummary,
    IReadOnlyList<string> RecoveryCommands,
    IReadOnlyList<string> Boundaries,
    DiagnosticWorkspaceRoute Route);

internal sealed record DiagnosticExplanationRead(bool Success, string Message, DiagnosticExplanation? Explanation);

internal sealed class DiagnosticExplanationRequestGate
{
    private int revision;
    public int Advance() => ++revision;
    public bool IsCurrent(int request) => request == revision;
}

internal static class DiagnosticRemediation
{
    public static DiagnosticExplanationRead Parse(string json, string expectedRuleId)
    {
        try
        {
            var root = JsonNode.Parse(json)?.AsObject() ?? throw new InvalidOperationException("Explanation root is not an object.");
            if (Text(root, "command") != "explain diagnostic") throw new InvalidOperationException("Explanation command identity is invalid.");
            var ruleId = Text(root["subject"], "ruleId");
            if (!StringComparer.Ordinal.Equals(ruleId, expectedRuleId)) throw new InvalidOperationException("Explanation rule identity does not match the selected diagnostic.");
            var family = root["family"]?.AsObject() ?? throw new InvalidOperationException("Explanation family is missing.");
            var familyId = Text(family, "id");
            var scope = Text(family, "scope");
            var validationStage = Text(family, "validationStage");
            if (string.IsNullOrWhiteSpace(familyId) || string.IsNullOrWhiteSpace(scope) || string.IsNullOrWhiteSpace(validationStage)) throw new InvalidOperationException("Explanation family metadata is incomplete.");
            var rule = root["rule"]?.AsObject();
            var recovery = Strings(family["recoveryCommands"]);
            var boundaries = Strings(root["boundaries"]);
            return new(true, "Explanation loaded from forge.exe.", new(
                ruleId,
                $"forge explain diagnostic {ruleId} --format json",
                familyId,
                scope,
                validationStage,
                OptionalText(rule, "title"),
                OptionalText(rule, "summary"),
                recovery,
                boundaries,
                RouteFor(ruleId)));
        }
        catch (Exception ex) when (ex is System.Text.Json.JsonException or InvalidOperationException)
        {
            return new(false, "Explanation unavailable: " + ex.Message, null);
        }
    }

    public static DiagnosticWorkspaceRoute RouteFor(string ruleId)
    {
        if (Starts(ruleId, "WF-LOAD-") || Starts(ruleId, "WF-SCHEMA-") || Starts(ruleId, "WF-SEM-") || Starts(ruleId, "WF-ASSET-")) return DiagnosticWorkspaceRoute.ValidationReport;
        if (Starts(ruleId, "WF-CAP-")) return DiagnosticWorkspaceRoute.Capabilities;
        if (Starts(ruleId, "WF-GEN-") || Starts(ruleId, "WF-BUILD-")) return DiagnosticWorkspaceRoute.ProjectOutputs;
        if (Starts(ruleId, "WF-REL-") || Starts(ruleId, "WF-GOV-") || Starts(ruleId, "WF-SEC-")) return DiagnosticWorkspaceRoute.ReleaseCandidate;
        return DiagnosticWorkspaceRoute.None;
    }

    private static bool Starts(string value, string prefix) => value.StartsWith(prefix, StringComparison.Ordinal);
    private static string Text(JsonNode? node, string property) => node?[property]?.GetValue<string>() ?? "";
    private static string? OptionalText(JsonNode? node, string property) { var value = Text(node, property); return string.IsNullOrWhiteSpace(value) ? null : value; }
    private static IReadOnlyList<string> Strings(JsonNode? node) => node?.AsArray().Select(item => item?.GetValue<string>()).Where(item => !string.IsNullOrWhiteSpace(item)).Cast<string>().ToArray() ?? [];
}
