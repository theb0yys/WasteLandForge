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

    [Fact]
    public void BuiltInCatalogReadsEmbeddedManifestSchema()
    {
        var found = WastelandForgeSchemaCatalog.TryGetById(WastelandForgeSchemaIds.Manifest010, out var resource);

        Assert.True(found);
        Assert.NotNull(resource);
        var text = WastelandForgeSchemaCatalog.ReadText(resource);
        var schema = JsonNode.Parse(text) as JsonObject;

        Assert.NotNull(schema);
        Assert.Equal(WastelandForgeSchemaIds.Manifest010, (string?)schema["$id"]);
    }

    [Theory]
    [InlineData("dependencies", WastelandForgeSchemaIds.Dependency010, "dependency")]
    [InlineData("capabilities", WastelandForgeSchemaIds.Capability010, "capability")]
    [InlineData("assets", WastelandForgeSchemaIds.Asset010, "asset")]
    public void RegistrySchemaFilesParseAsJson(string directoryName, string schemaId, string expectedKind)
    {
        var schemaPath = Path.Combine(RepositoryRoot(), "schemas", directoryName, "0.1.0", "schema.json");
        var schema = JsonNode.Parse(File.ReadAllText(schemaPath)) as JsonObject;

        Assert.NotNull(schema);
        Assert.Equal("https://json-schema.org/draft/2020-12/schema", (string?)schema["$schema"]);
        Assert.Equal(schemaId, (string?)schema["$id"]);
        Assert.Equal(expectedKind, (string?)schema["properties"]?["kind"]?["const"]);
    }

    [Theory]
    [InlineData(WastelandForgeSchemaIds.Dependency010, "dependency", "schemas/dependencies/0.1.0/schema.json")]
    [InlineData(WastelandForgeSchemaIds.Capability010, "capability", "schemas/capabilities/0.1.0/schema.json")]
    [InlineData(WastelandForgeSchemaIds.Asset010, "asset", "schemas/assets/0.1.0/schema.json")]
    public void BuiltInCatalogResolvesRegistrySchemas(string schemaId, string expectedKind, string expectedPath)
    {
        var found = WastelandForgeSchemaCatalog.TryGetById(schemaId, out var resource);

        Assert.True(found);
        Assert.NotNull(resource);
        Assert.Equal(expectedKind, resource.Kind);
        Assert.Equal("0.1.0", resource.Version);
        Assert.Equal(expectedPath, resource.RelativePath);
    }

    [Theory]
    [InlineData(WastelandForgeSchemaIds.Dependency010)]
    [InlineData(WastelandForgeSchemaIds.Capability010)]
    [InlineData(WastelandForgeSchemaIds.Asset010)]
    public void BuiltInCatalogReadsEmbeddedRegistrySchemas(string schemaId)
    {
        var found = WastelandForgeSchemaCatalog.TryGetById(schemaId, out var resource);

        Assert.True(found);
        Assert.NotNull(resource);
        var text = WastelandForgeSchemaCatalog.ReadText(resource);
        var schema = JsonNode.Parse(text) as JsonObject;

        Assert.NotNull(schema);
        Assert.Equal(schemaId, (string?)schema["$id"]);
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
