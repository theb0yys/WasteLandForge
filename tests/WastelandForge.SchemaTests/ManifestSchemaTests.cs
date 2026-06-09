using System.Text.Json.Nodes;
using WastelandForge.Schema;

namespace WastelandForge.SchemaTests;

public sealed class ManifestSchemaTests
{
    [Fact]
    public void ManifestSchemaFileParsesAsJson()
    {
        var schemaPath = Path.Combine(RepositoryRoot(), "schemas", "manifest", "0.1.0", "schema.json");
        var schema = JsonNode.Parse(File.ReadAllText(schemaPath)) as JsonObject;

        Assert.NotNull(schema);
        Assert.Equal("https://json-schema.org/draft/2020-12/schema", (string?)schema["$schema"]);
        Assert.Equal(WastelandForgeSchemaIds.Manifest010, (string?)schema["$id"]);
        Assert.Equal("manifest", (string?)schema["properties"]?["kind"]?["const"]);
        Assert.Equal("falloutnv", (string?)schema["properties"]?["game"]?["const"]);
    }

    [Fact]
    public void BuiltInCatalogResolvesManifestSchema()
    {
        var found = WastelandForgeSchemaCatalog.TryGetById(WastelandForgeSchemaIds.Manifest010, out var resource);

        Assert.True(found);
        Assert.NotNull(resource);
        Assert.Equal("manifest", resource.Kind);
        Assert.Equal("0.1.0", resource.Version);
        Assert.Equal("schemas/manifest/0.1.0/schema.json", resource.RelativePath);
    }

    private static string RepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "WastelandForge.sln")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName ?? throw new InvalidOperationException("Could not locate repository root.");
    }
}
