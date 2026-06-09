namespace WastelandForge.Schema;

public static class WastelandForgeSchemaCatalog
{
    private static readonly SchemaResource[] BuiltInSchemas =
    [
        new(
            WastelandForgeSchemaIds.Manifest010,
            "manifest",
            "0.1.0",
            "schemas/manifest/0.1.0/schema.json")
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
}
