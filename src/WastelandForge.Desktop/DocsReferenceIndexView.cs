using System.Text.Json;

namespace WastelandForge.Desktop;

internal sealed record DocsReferenceEntryView(
    string Id,
    string Title,
    string Kind,
    string Source,
    string Detail,
    string MarkdownPath);

internal sealed record DocsReferenceSectionView(
    string Id,
    string Title,
    IReadOnlyList<DocsReferenceEntryView> Entries);

internal sealed record DocsReferenceIndexView(
    string ProjectId,
    IReadOnlyList<DocsReferenceSectionView> Sections)
{
    public int EntryCount => Sections.Sum(section => section.Entries.Count);
}

internal static class DocsReferenceIndexViewParser
{
    public static DocsReferenceIndexView Parse(string json)
    {
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;
        if (!StringComparer.Ordinal.Equals(GetRequiredString(root, "kind"), "wastelandforge.docs.reference-index"))
        {
            throw new InvalidOperationException("Unexpected docs reference index kind.");
        }

        if (!StringComparer.Ordinal.Equals(GetRequiredString(root, "outputRoot"), "generated/docs"))
        {
            throw new InvalidOperationException("Unexpected docs reference index output root.");
        }

        var project = root.GetProperty("project");
        var projectId = GetRequiredString(project, "id");
        var markdownPaths = ParseMarkdownPaths(root);
        var sections = root.GetProperty("sections")
            .EnumerateArray()
            .Select(section => ParseSection(section, markdownPaths))
            .ToArray();
        if (sections.Length == 0)
        {
            throw new InvalidOperationException("Docs reference index contains no sections.");
        }

        return new DocsReferenceIndexView(projectId, sections);
    }

    private static DocsReferenceSectionView ParseSection(
        JsonElement element,
        IReadOnlyDictionary<(string SectionId, string EntryId), string> markdownPaths)
    {
        var id = GetRequiredString(element, "id");
        var entries = element.GetProperty("entries")
            .EnumerateArray()
            .Select(entry => ParseEntry(id, entry, markdownPaths))
            .ToArray();
        return new DocsReferenceSectionView(
            id,
            GetRequiredString(element, "title"),
            entries);
    }

    private static DocsReferenceEntryView ParseEntry(
        string sectionId,
        JsonElement element,
        IReadOnlyDictionary<(string SectionId, string EntryId), string> markdownPaths)
    {
        var id = GetRequiredString(element, "id");
        if (!markdownPaths.TryGetValue((sectionId, id), out var markdownPath))
        {
            throw new InvalidOperationException($"Docs reference entry '{id}' has no generated Markdown mapping.");
        }

        var version = GetOptionalString(element, "version");
        var description = GetOptionalString(element, "description");
        var detail = string.Join(
            " | ",
            new[] { version, description }
                .Where(value => !string.IsNullOrWhiteSpace(value)));
        return new DocsReferenceEntryView(
            id,
            GetRequiredString(element, "title"),
            GetRequiredString(element, "kind"),
            GetRequiredString(element, "source"),
            detail,
            markdownPath);
    }

    private static IReadOnlyDictionary<(string SectionId, string EntryId), string> ParseMarkdownPaths(JsonElement root)
    {
        var mappings = new Dictionary<(string SectionId, string EntryId), string>();
        AddMappings(root, mappings, "schemas", "schemaReferences", "schemaId");
        AddMappings(root, mappings, "registries", "registryReferences", "registryId");
        AddMappings(root, mappings, "ruleFamilies", "ruleReferences", "ruleFamilyId");
        AddMappings(root, mappings, "capabilities", "capabilityReferences", "capabilityId");
        AddMappings(root, mappings, "providers", "providerReferences", "providerId");
        AddMappings(root, mappings, "commands", "commandReferences", "commandId");
        return mappings;
    }

    private static void AddMappings(
        JsonElement root,
        Dictionary<(string SectionId, string EntryId), string> mappings,
        string sectionId,
        string arrayName,
        string idName)
    {
        foreach (var item in root.GetProperty(arrayName).EnumerateArray())
        {
            var key = (sectionId, GetRequiredString(item, idName));
            if (!mappings.TryAdd(key, GetRequiredString(item, "markdown")))
            {
                throw new InvalidOperationException($"Duplicate docs reference mapping for '{key.Item2}'.");
            }
        }
    }

    private static string GetRequiredString(JsonElement element, string propertyName) =>
        GetOptionalString(element, propertyName) is { Length: > 0 } value
            ? value
            : throw new InvalidOperationException($"Docs reference index property '{propertyName}' is missing.");

    private static string? GetOptionalString(JsonElement element, string propertyName) =>
        element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String
            ? property.GetString()
            : null;
}
