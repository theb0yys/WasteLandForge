namespace WastelandForge.Schema;

public static class WastelandForgeSchemaCatalog
{
    private static readonly SchemaResource[] BuiltInSchemas =
    [
        new(
            WastelandForgeSchemaIds.Manifest010,
            "manifest",
            "0.1.0",
            "schemas/manifest/0.1.0/schema.json"),
        new(
            WastelandForgeSchemaIds.Dependency010,
            "dependency",
            "0.1.0",
            "schemas/dependencies/0.1.0/schema.json"),
        new(
            WastelandForgeSchemaIds.Capability010,
            "capability",
            "0.1.0",
            "schemas/capabilities/0.1.0/schema.json"),
        new(
            WastelandForgeSchemaIds.Asset010,
            "asset",
            "0.1.0",
            "schemas/assets/0.1.0/schema.json"),
        new(
            WastelandForgeSchemaIds.Quest010,
            "quest",
            "0.1.0",
            "schemas/quests/0.1.0/schema.json"),
        new(
            WastelandForgeSchemaIds.Quest020,
            "quest",
            "0.2.0",
            "schemas/quests/0.2.0/schema.json"),
        new(
            WastelandForgeSchemaIds.Quest030,
            "quest",
            "0.3.0",
            "schemas/quests/0.3.0/schema.json"),
        new(
            WastelandForgeSchemaIds.Quest040,
            "quest",
            "0.4.0",
            "schemas/quests/0.4.0/schema.json"),
        new(
            WastelandForgeSchemaIds.Quest050,
            "quest",
            "0.5.0",
            "schemas/quests/0.5.0/schema.json"),
        new(
            WastelandForgeSchemaIds.Quest060,
            "quest",
            "0.6.0",
            "schemas/quests/0.6.0/schema.json"),
        new(
            WastelandForgeSchemaIds.Dialogue010,
            "dialogue",
            "0.1.0",
            "schemas/dialogue/0.1.0/schema.json"),
        new(
            WastelandForgeSchemaIds.Dialogue020,
            "dialogue",
            "0.2.0",
            "schemas/dialogue/0.2.0/schema.json"),
        new(
            WastelandForgeSchemaIds.Dialogue030,
            "dialogue",
            "0.3.0",
            "schemas/dialogue/0.3.0/schema.json"),
        new(
            WastelandForgeSchemaIds.Dialogue040,
            "dialogue",
            "0.4.0",
            "schemas/dialogue/0.4.0/schema.json"),
        new(
            WastelandForgeSchemaIds.Dialogue050,
            "dialogue",
            "0.5.0",
            "schemas/dialogue/0.5.0/schema.json")
    ];

    public static IReadOnlyList<SchemaResource> BuiltIn => BuiltInSchemas;

    public static bool TryGetById(string id, out SchemaResource? resource)
    {
        foreach (var candidate in BuiltInSchemas)
        {
            if (StringComparer.Ordinal.Equals(candidate.Id, id))
            {
                resource = candidate;
                return true;
            }
        }

        resource = null;
        return false;
    }

    public static string ReadText(SchemaResource resource)
    {
        ArgumentNullException.ThrowIfNull(resource);

        var assembly = typeof(WastelandForgeSchemaCatalog).Assembly;
        var suffix = ToManifestResourceSuffix(resource.RelativePath);
        var resourceName = assembly
            .GetManifestResourceNames()
            .SingleOrDefault(name => name.EndsWith(suffix, StringComparison.Ordinal));

        if (resourceName is null)
        {
            throw new InvalidOperationException($"Built-in schema resource '{resource.RelativePath}' was not found.");
        }

        using var stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Built-in schema resource '{resource.RelativePath}' could not be opened.");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    private static string ToManifestResourceSuffix(string relativePath)
    {
        var pathSegments = relativePath.Split(['/', '\\'], StringSplitOptions.RemoveEmptyEntries);
        return string.Join(
            ".",
            pathSegments.SelectMany(segment => segment
                .Split('.', StringSplitOptions.RemoveEmptyEntries)
                .Select(EscapeResourceIdentifier)));
    }

    private static string EscapeResourceIdentifier(string segment)
    {
        return segment.Length > 0 && char.IsDigit(segment[0])
            ? "_" + segment
            : segment;
    }
}
