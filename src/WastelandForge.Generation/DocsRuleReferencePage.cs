namespace WastelandForge.Generation;

public sealed record DocsRuleReferencePage(
    string RuleFamilyId,
    string Prefix,
    string Title,
    string Scope,
    string Source,
    string JsonPath,
    string MarkdownPath,
    IReadOnlyList<string> KnownDiagnosticIds);
