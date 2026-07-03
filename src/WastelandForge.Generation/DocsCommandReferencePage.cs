namespace WastelandForge.Generation;

public sealed record DocsCommandReferencePage(
    string CommandId,
    string CommandText,
    string Title,
    string CommandGroup,
    string SurfaceStatus,
    string Source,
    string ResearchSource,
    string JsonPath,
    string MarkdownPath,
    IReadOnlyList<string> Notes);
