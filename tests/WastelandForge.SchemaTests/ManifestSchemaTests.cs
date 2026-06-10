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
    [InlineData("dependencies", "0.1.0", WastelandForgeSchemaIds.Dependency010, "dependency")]
    [InlineData("capabilities", "0.1.0", WastelandForgeSchemaIds.Capability010, "capability")]
    [InlineData("assets", "0.1.0", WastelandForgeSchemaIds.Asset010, "asset")]
    [InlineData("quests", "0.1.0", WastelandForgeSchemaIds.Quest010, "quest")]
    [InlineData("quests", "0.2.0", WastelandForgeSchemaIds.Quest020, "quest")]
    [InlineData("quests", "0.3.0", WastelandForgeSchemaIds.Quest030, "quest")]
    [InlineData("quests", "0.4.0", WastelandForgeSchemaIds.Quest040, "quest")]
    [InlineData("quests", "0.5.0", WastelandForgeSchemaIds.Quest050, "quest")]
    [InlineData("quests", "0.6.0", WastelandForgeSchemaIds.Quest060, "quest")]
    [InlineData("dialogue", "0.1.0", WastelandForgeSchemaIds.Dialogue010, "dialogue")]
    [InlineData("dialogue", "0.2.0", WastelandForgeSchemaIds.Dialogue020, "dialogue")]
    [InlineData("dialogue", "0.3.0", WastelandForgeSchemaIds.Dialogue030, "dialogue")]
    [InlineData("dialogue", "0.4.0", WastelandForgeSchemaIds.Dialogue040, "dialogue")]
    [InlineData("dialogue", "0.5.0", WastelandForgeSchemaIds.Dialogue050, "dialogue")]
    [InlineData("dialogue", "0.6.0", WastelandForgeSchemaIds.Dialogue060, "dialogue")]
    [InlineData("dialogue", "0.7.0", WastelandForgeSchemaIds.Dialogue070, "dialogue")]
    [InlineData("dialogue", "0.8.0", WastelandForgeSchemaIds.Dialogue080, "dialogue")]
    public void RegistrySchemaFilesParseAsJson(string directoryName, string version, string schemaId, string expectedKind)
    {
        var schemaPath = Path.Combine(RepositoryRoot(), "schemas", directoryName, version, "schema.json");
        var schema = JsonNode.Parse(File.ReadAllText(schemaPath)) as JsonObject;

        Assert.NotNull(schema);
        Assert.Equal("https://json-schema.org/draft/2020-12/schema", (string?)schema["$schema"]);
        Assert.Equal(schemaId, (string?)schema["$id"]);
        Assert.Equal(expectedKind, (string?)schema["properties"]?["kind"]?["const"]);
    }

    [Theory]
    [InlineData(WastelandForgeSchemaIds.Dependency010, "dependency", "0.1.0", "schemas/dependencies/0.1.0/schema.json")]
    [InlineData(WastelandForgeSchemaIds.Capability010, "capability", "0.1.0", "schemas/capabilities/0.1.0/schema.json")]
    [InlineData(WastelandForgeSchemaIds.Asset010, "asset", "0.1.0", "schemas/assets/0.1.0/schema.json")]
    [InlineData(WastelandForgeSchemaIds.Quest010, "quest", "0.1.0", "schemas/quests/0.1.0/schema.json")]
    [InlineData(WastelandForgeSchemaIds.Quest020, "quest", "0.2.0", "schemas/quests/0.2.0/schema.json")]
    [InlineData(WastelandForgeSchemaIds.Quest030, "quest", "0.3.0", "schemas/quests/0.3.0/schema.json")]
    [InlineData(WastelandForgeSchemaIds.Quest040, "quest", "0.4.0", "schemas/quests/0.4.0/schema.json")]
    [InlineData(WastelandForgeSchemaIds.Quest050, "quest", "0.5.0", "schemas/quests/0.5.0/schema.json")]
    [InlineData(WastelandForgeSchemaIds.Quest060, "quest", "0.6.0", "schemas/quests/0.6.0/schema.json")]
    [InlineData(WastelandForgeSchemaIds.Dialogue010, "dialogue", "0.1.0", "schemas/dialogue/0.1.0/schema.json")]
    [InlineData(WastelandForgeSchemaIds.Dialogue020, "dialogue", "0.2.0", "schemas/dialogue/0.2.0/schema.json")]
    [InlineData(WastelandForgeSchemaIds.Dialogue030, "dialogue", "0.3.0", "schemas/dialogue/0.3.0/schema.json")]
    [InlineData(WastelandForgeSchemaIds.Dialogue040, "dialogue", "0.4.0", "schemas/dialogue/0.4.0/schema.json")]
    [InlineData(WastelandForgeSchemaIds.Dialogue050, "dialogue", "0.5.0", "schemas/dialogue/0.5.0/schema.json")]
    [InlineData(WastelandForgeSchemaIds.Dialogue060, "dialogue", "0.6.0", "schemas/dialogue/0.6.0/schema.json")]
    [InlineData(WastelandForgeSchemaIds.Dialogue070, "dialogue", "0.7.0", "schemas/dialogue/0.7.0/schema.json")]
    [InlineData(WastelandForgeSchemaIds.Dialogue080, "dialogue", "0.8.0", "schemas/dialogue/0.8.0/schema.json")]
    public void BuiltInCatalogResolvesRegistrySchemas(string schemaId, string expectedKind, string expectedVersion, string expectedPath)
    {
        var found = WastelandForgeSchemaCatalog.TryGetById(schemaId, out var resource);

        Assert.True(found);
        Assert.NotNull(resource);
        Assert.Equal(expectedKind, resource.Kind);
        Assert.Equal(expectedVersion, resource.Version);
        Assert.Equal(expectedPath, resource.RelativePath);
    }

    [Theory]
    [InlineData(WastelandForgeSchemaIds.Dependency010)]
    [InlineData(WastelandForgeSchemaIds.Capability010)]
    [InlineData(WastelandForgeSchemaIds.Asset010)]
    [InlineData(WastelandForgeSchemaIds.Quest010)]
    [InlineData(WastelandForgeSchemaIds.Quest020)]
    [InlineData(WastelandForgeSchemaIds.Quest030)]
    [InlineData(WastelandForgeSchemaIds.Quest040)]
    [InlineData(WastelandForgeSchemaIds.Quest050)]
    [InlineData(WastelandForgeSchemaIds.Quest060)]
    [InlineData(WastelandForgeSchemaIds.Dialogue010)]
    [InlineData(WastelandForgeSchemaIds.Dialogue020)]
    [InlineData(WastelandForgeSchemaIds.Dialogue030)]
    [InlineData(WastelandForgeSchemaIds.Dialogue040)]
    [InlineData(WastelandForgeSchemaIds.Dialogue050)]
    [InlineData(WastelandForgeSchemaIds.Dialogue060)]
    [InlineData(WastelandForgeSchemaIds.Dialogue070)]
    [InlineData(WastelandForgeSchemaIds.Dialogue080)]
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
