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
    public void Manifest020SchemaFileParsesAsJson()
    {
        var schemaPath = Path.Combine(RepositoryRoot(), "schemas", "manifest", "0.2.0", "schema.json");
        var schema = JsonNode.Parse(File.ReadAllText(schemaPath)) as JsonObject;

        Assert.NotNull(schema);
        Assert.Equal("https://json-schema.org/draft/2020-12/schema", (string?)schema["$schema"]);
        Assert.Equal(WastelandForgeSchemaIds.Manifest020, (string?)schema["$id"]);
        Assert.Equal("0.2.0", (string?)schema["properties"]?["schemaVersion"]?["const"]);
        Assert.NotNull(schema["properties"]?["registries"]?["properties"]?["mcm"]);
        Assert.NotNull(schema["properties"]?["registries"]?["properties"]?["jipScripts"]);
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
    public void BuiltInCatalogResolvesPluginArtifactSchema()
    {
        Assert.True(WastelandForgeSchemaCatalog.TryGetById(WastelandForgeSchemaIds.PluginArtifact010, out var resource));
        Assert.NotNull(resource); Assert.Equal("plugin-artifact", resource.Kind); Assert.NotNull(JsonNode.Parse(WastelandForgeSchemaCatalog.ReadText(resource)));
    }

    [Fact]
    public void Manifest030DeclaresPluginArtifactsWithoutChangingOlderSchemas()
    {
        Assert.True(WastelandForgeSchemaCatalog.TryGetById(WastelandForgeSchemaIds.Manifest030, out var resource));
        var schema = JsonNode.Parse(WastelandForgeSchemaCatalog.ReadText(resource!)); Assert.NotNull(schema?["properties"]?["registries"]?["properties"]?["pluginArtifacts"]);
    }

    [Fact]
    public void GeckHandoff020SupportsExplicitGreenfieldScopeWithoutChanging010()
    {
        Assert.True(WastelandForgeSchemaCatalog.TryGetById(WastelandForgeSchemaIds.GeckHandoffManifest010, out var original));
        Assert.True(WastelandForgeSchemaCatalog.TryGetById(WastelandForgeSchemaIds.GeckHandoffManifest020, out var current));
        var originalSchema = JsonNode.Parse(WastelandForgeSchemaCatalog.ReadText(original!));
        var currentSchema = JsonNode.Parse(WastelandForgeSchemaCatalog.ReadText(current!));
        Assert.Null(originalSchema?["properties"]?["scope"]);
        Assert.Contains("greenfield", currentSchema?["properties"]?["scope"]?["enum"]!.AsArray().Select(node => node!.GetValue<string>())!);
        Assert.Equal(1, currentSchema?["properties"]?["sources"]?["minItems"]!.GetValue<int>());
    }

    [Fact]
    public void FomodSchemasAndManifest040AreRegistered()
    {
        Assert.True(WastelandForgeSchemaCatalog.TryGetById(WastelandForgeSchemaIds.Manifest040, out var manifest));
        var schema = JsonNode.Parse(WastelandForgeSchemaCatalog.ReadText(manifest!));
        Assert.NotNull(schema?["properties"]?["registries"]?["properties"]?["fomod"]);
        Assert.True(WastelandForgeSchemaCatalog.TryGetById(WastelandForgeSchemaIds.Fomod010, out var source));
        Assert.Equal("fomod", source!.Kind);
        Assert.True(WastelandForgeSchemaCatalog.TryGetById(WastelandForgeSchemaIds.FomodManifest010, out var evidence));
        Assert.Equal("fomod-manifest", evidence!.Kind);
    }

    [Fact]
    public void BuiltInCatalogResolvesPluginReviewEvidenceSchema()
    {
        Assert.True(WastelandForgeSchemaCatalog.TryGetById(WastelandForgeSchemaIds.PluginReviewEvidence010, out var resource));
        Assert.NotNull(resource); Assert.Equal("plugin-review-evidence", resource.Kind); Assert.NotNull(JsonNode.Parse(WastelandForgeSchemaCatalog.ReadText(resource)));
    }

    [Fact]
    public void BuiltInCatalogResolvesManifest020Schema()
    {
        var found = WastelandForgeSchemaCatalog.TryGetById(WastelandForgeSchemaIds.Manifest020, out var resource);

        Assert.True(found);
        Assert.NotNull(resource);
        Assert.Equal("manifest", resource.Kind);
        Assert.Equal("0.2.0", resource.Version);
        Assert.Equal("schemas/manifest/0.2.0/schema.json", resource.RelativePath);
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

    [Fact]
    public void BuiltInCatalogResolvesCombinedModPackageManifestSchema()
    {
        var found = WastelandForgeSchemaCatalog.TryGetById(WastelandForgeSchemaIds.ModPackageManifest010, out var resource);

        Assert.True(found);
        Assert.NotNull(resource);
        Assert.Equal("mod-package-manifest", resource.Kind);
        Assert.Equal("0.1.0", resource.Version);
        var schema = JsonNode.Parse(WastelandForgeSchemaCatalog.ReadText(resource)) as JsonObject;
        Assert.Equal(WastelandForgeSchemaIds.ModPackageManifest010, (string?)schema?["$id"]);
    }

    [Fact]
    public void BuiltInCatalogResolvesBsaPackPlanSchema()
    {
        Assert.True(WastelandForgeSchemaCatalog.TryGetById(WastelandForgeSchemaIds.BsaPackPlan010, out var resource));
        Assert.Equal("bsa-pack-plan", resource!.Kind);
        Assert.Contains("wastelandforge.bsa-pack-plan", WastelandForgeSchemaCatalog.ReadText(resource), StringComparison.Ordinal);
    }

    [Fact]
    public void BuiltInCatalogResolvesBsArchPreviewSchema()
    {
        Assert.True(WastelandForgeSchemaCatalog.TryGetById(WastelandForgeSchemaIds.BsArchPreview010, out var resource));
        Assert.Equal("bsa-bsarch-preview", resource!.Kind);
        Assert.Contains("previewSha256", WastelandForgeSchemaCatalog.ReadText(resource), StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(WastelandForgeSchemaIds.XEditAudit020, "xedit-audit")]
    [InlineData(WastelandForgeSchemaIds.XEditCheckReport010, "xedit-check-report")]
    public void BuiltInCatalogResolvesXEditCheckSchemas(string id, string kind)
    {
        Assert.True(WastelandForgeSchemaCatalog.TryGetById(id, out var resource));
        Assert.Equal(kind, resource!.Kind);
    }

    [Theory]
    [InlineData(WastelandForgeSchemaIds.BsArchExecution010, "bsarch-execution")]
    [InlineData(WastelandForgeSchemaIds.BsaOutputVerification010, "bsa-output-verification")]
    [InlineData(WastelandForgeSchemaIds.BsaPackageManifest010, "bsa-package-manifest")]
    public void BuiltInCatalogResolvesBsArchExecutionSchemas(string id, string kind)
    {
        Assert.True(WastelandForgeSchemaCatalog.TryGetById(id, out var resource));
        Assert.Equal(kind, resource!.Kind);
    }

    [Fact]
    public void JipScriptSchemaDefinesOpaqueBodyLines()
    {
        var schemaPath = Path.Combine(RepositoryRoot(), "schemas", "jip-scripts", "0.1.0", "schema.json");
        var schema = JsonNode.Parse(File.ReadAllText(schemaPath)) as JsonObject;

        Assert.NotNull(schema);
        var required = schema["$defs"]?["script"]?["required"]?.AsArray()
            .Select(node => (string?)node)
            .ToArray();
        Assert.NotNull(required);
        Assert.Contains("body", required);
        Assert.Equal("opaqueText", (string?)schema["$defs"]?["body"]?["properties"]?["lineMode"]?["const"]);
        Assert.Equal("^[^\\r\\n]*$", (string?)schema["$defs"]?["sourceLine"]?["properties"]?["text"]?["pattern"]);
    }

    [Fact]
    public void XEditAuditSchemaDefinesEvidenceOnlySafetyFlags()
    {
        var schemaPath = Path.Combine(RepositoryRoot(), "schemas", "xedit-audit", "0.1.0", "schema.json");
        var schema = JsonNode.Parse(File.ReadAllText(schemaPath)) as JsonObject;

        Assert.NotNull(schema);
        Assert.Equal(WastelandForgeSchemaIds.XEditAudit010, (string?)schema["$id"]);
        Assert.Equal("xedit-audit", (string?)schema["properties"]?["kind"]?["const"]);
        Assert.Equal("record-inspection", (string?)schema["$defs"]?["audit"]?["properties"]?["intent"]?["enum"]?[0]);
        Assert.Equal("script-report-evidence", (string?)schema["$defs"]?["audit"]?["properties"]?["mode"]?["const"]);
        Assert.Equal(false, (bool?)schema["$defs"]?["safety"]?["properties"]?["executesXEdit"]?["const"]);
        Assert.Equal(false, (bool?)schema["$defs"]?["safety"]?["properties"]?["mutatesPlugins"]?["const"]);
        Assert.Equal(false, (bool?)schema["$defs"]?["safety"]?["properties"]?["writesPatches"]?["const"]);
    }

    [Theory]
    [InlineData("dependencies", "0.1.0", WastelandForgeSchemaIds.Dependency010, "dependency")]
    [InlineData("dependencies", "0.2.0", WastelandForgeSchemaIds.Dependency020, "dependency")]
    [InlineData("capabilities", "0.1.0", WastelandForgeSchemaIds.Capability010, "capability")]
    [InlineData("capabilities", "0.2.0", WastelandForgeSchemaIds.Capability020, "capability")]
    [InlineData("assets", "0.1.0", WastelandForgeSchemaIds.Asset010, "asset")]
    [InlineData("mcm", "0.1.0", WastelandForgeSchemaIds.Mcm010, "mcm")]
    [InlineData("jip-scripts", "0.1.0", WastelandForgeSchemaIds.JipScript010, "jip-script")]
    [InlineData("jip-script-emission-manifest", "0.1.0", WastelandForgeSchemaIds.JipScriptEmissionManifest010, "wastelandforge.jip-script-emission-manifest")]
    [InlineData("xedit-audit", "0.1.0", WastelandForgeSchemaIds.XEditAudit010, "xedit-audit")]
    [InlineData("mcm-extender-output", "0.1.0", WastelandForgeSchemaIds.McmExtenderOutput010, null)]
    [InlineData("package-manifest", "0.1.0", WastelandForgeSchemaIds.PackageManifest010, "wastelandforge.package-manifest")]
    [InlineData("install-preview", "0.1.0", WastelandForgeSchemaIds.InstallPreview010, "wastelandforge.install-preview")]
    [InlineData("install-plan", "0.1.0", WastelandForgeSchemaIds.InstallPlan010, "wastelandforge.install-plan")]
    [InlineData("package-verification", "0.1.0", WastelandForgeSchemaIds.PackageVerification010, "wastelandforge.package-verification")]
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
    [InlineData("dialogue", "0.9.0", WastelandForgeSchemaIds.Dialogue090, "dialogue")]
    [InlineData("dialogue", "0.10.0", WastelandForgeSchemaIds.Dialogue0100, "dialogue")]
    [InlineData("dialogue", "0.11.0", WastelandForgeSchemaIds.Dialogue0110, "dialogue")]
    [InlineData("dialogue", "0.12.0", WastelandForgeSchemaIds.Dialogue0120, "dialogue")]
    [InlineData("dialogue", "0.13.0", WastelandForgeSchemaIds.Dialogue0130, "dialogue")]
    [InlineData("dialogue", "0.14.0", WastelandForgeSchemaIds.Dialogue0140, "dialogue")]
    [InlineData("dialogue", "0.15.0", WastelandForgeSchemaIds.Dialogue0150, "dialogue")]
    [InlineData("dialogue", "0.16.0", WastelandForgeSchemaIds.Dialogue0160, "dialogue")]
    [InlineData("dialogue", "0.17.0", WastelandForgeSchemaIds.Dialogue0170, "dialogue")]
    [InlineData("dialogue", "0.18.0", WastelandForgeSchemaIds.Dialogue0180, "dialogue")]
    [InlineData("dialogue", "0.19.0", WastelandForgeSchemaIds.Dialogue0190, "dialogue")]
    [InlineData("dialogue", "0.20.0", WastelandForgeSchemaIds.Dialogue0200, "dialogue")]
    [InlineData("dialogue", "0.21.0", WastelandForgeSchemaIds.Dialogue0210, "dialogue")]
    [InlineData("dialogue", "0.22.0", WastelandForgeSchemaIds.Dialogue0220, "dialogue")]
    [InlineData("dialogue", "0.23.0", WastelandForgeSchemaIds.Dialogue0230, "dialogue")]
    public void RegistrySchemaFilesParseAsJson(string directoryName, string version, string schemaId, string? expectedKind)
    {
        var schemaPath = Path.Combine(RepositoryRoot(), "schemas", directoryName, version, "schema.json");
        var schema = JsonNode.Parse(File.ReadAllText(schemaPath)) as JsonObject;

        Assert.NotNull(schema);
        Assert.Equal("https://json-schema.org/draft/2020-12/schema", (string?)schema["$schema"]);
        Assert.Equal(schemaId, (string?)schema["$id"]);
        if (expectedKind is not null)
        {
            Assert.Equal(expectedKind, (string?)schema["properties"]?["kind"]?["const"]);
        }
    }

    [Theory]
    [InlineData(WastelandForgeSchemaIds.Dependency010, "dependency", "0.1.0", "schemas/dependencies/0.1.0/schema.json")]
    [InlineData(WastelandForgeSchemaIds.Dependency020, "dependency", "0.2.0", "schemas/dependencies/0.2.0/schema.json")]
    [InlineData(WastelandForgeSchemaIds.Capability010, "capability", "0.1.0", "schemas/capabilities/0.1.0/schema.json")]
    [InlineData(WastelandForgeSchemaIds.Capability020, "capability", "0.2.0", "schemas/capabilities/0.2.0/schema.json")]
    [InlineData(WastelandForgeSchemaIds.Asset010, "asset", "0.1.0", "schemas/assets/0.1.0/schema.json")]
    [InlineData(WastelandForgeSchemaIds.Mcm010, "mcm", "0.1.0", "schemas/mcm/0.1.0/schema.json")]
    [InlineData(WastelandForgeSchemaIds.JipScript010, "jip-script", "0.1.0", "schemas/jip-scripts/0.1.0/schema.json")]
    [InlineData(WastelandForgeSchemaIds.JipScriptEmissionManifest010, "jip-script-emission-manifest", "0.1.0", "schemas/jip-script-emission-manifest/0.1.0/schema.json")]
    [InlineData(WastelandForgeSchemaIds.XEditAudit010, "xedit-audit", "0.1.0", "schemas/xedit-audit/0.1.0/schema.json")]
    [InlineData(WastelandForgeSchemaIds.McmExtenderOutput010, "mcm-extender-output", "0.1.0", "schemas/mcm-extender-output/0.1.0/schema.json")]
    [InlineData(WastelandForgeSchemaIds.PackageManifest010, "package-manifest", "0.1.0", "schemas/package-manifest/0.1.0/schema.json")]
    [InlineData(WastelandForgeSchemaIds.InstallPreview010, "install-preview", "0.1.0", "schemas/install-preview/0.1.0/schema.json")]
    [InlineData(WastelandForgeSchemaIds.InstallPlan010, "install-plan", "0.1.0", "schemas/install-plan/0.1.0/schema.json")]
    [InlineData(WastelandForgeSchemaIds.PackageVerification010, "package-verification", "0.1.0", "schemas/package-verification/0.1.0/schema.json")]
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
    [InlineData(WastelandForgeSchemaIds.Dialogue090, "dialogue", "0.9.0", "schemas/dialogue/0.9.0/schema.json")]
    [InlineData(WastelandForgeSchemaIds.Dialogue0100, "dialogue", "0.10.0", "schemas/dialogue/0.10.0/schema.json")]
    [InlineData(WastelandForgeSchemaIds.Dialogue0110, "dialogue", "0.11.0", "schemas/dialogue/0.11.0/schema.json")]
    [InlineData(WastelandForgeSchemaIds.Dialogue0120, "dialogue", "0.12.0", "schemas/dialogue/0.12.0/schema.json")]
    [InlineData(WastelandForgeSchemaIds.Dialogue0130, "dialogue", "0.13.0", "schemas/dialogue/0.13.0/schema.json")]
    [InlineData(WastelandForgeSchemaIds.Dialogue0140, "dialogue", "0.14.0", "schemas/dialogue/0.14.0/schema.json")]
    [InlineData(WastelandForgeSchemaIds.Dialogue0150, "dialogue", "0.15.0", "schemas/dialogue/0.15.0/schema.json")]
    [InlineData(WastelandForgeSchemaIds.Dialogue0160, "dialogue", "0.16.0", "schemas/dialogue/0.16.0/schema.json")]
    [InlineData(WastelandForgeSchemaIds.Dialogue0170, "dialogue", "0.17.0", "schemas/dialogue/0.17.0/schema.json")]
    [InlineData(WastelandForgeSchemaIds.Dialogue0180, "dialogue", "0.18.0", "schemas/dialogue/0.18.0/schema.json")]
    [InlineData(WastelandForgeSchemaIds.Dialogue0190, "dialogue", "0.19.0", "schemas/dialogue/0.19.0/schema.json")]
    [InlineData(WastelandForgeSchemaIds.Dialogue0200, "dialogue", "0.20.0", "schemas/dialogue/0.20.0/schema.json")]
    [InlineData(WastelandForgeSchemaIds.Dialogue0210, "dialogue", "0.21.0", "schemas/dialogue/0.21.0/schema.json")]
    [InlineData(WastelandForgeSchemaIds.Dialogue0220, "dialogue", "0.22.0", "schemas/dialogue/0.22.0/schema.json")]
    [InlineData(WastelandForgeSchemaIds.Dialogue0230, "dialogue", "0.23.0", "schemas/dialogue/0.23.0/schema.json")]
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
    [InlineData(WastelandForgeSchemaIds.Dependency020)]
    [InlineData(WastelandForgeSchemaIds.Capability010)]
    [InlineData(WastelandForgeSchemaIds.Capability020)]
    [InlineData(WastelandForgeSchemaIds.Asset010)]
    [InlineData(WastelandForgeSchemaIds.Mcm010)]
    [InlineData(WastelandForgeSchemaIds.JipScript010)]
    [InlineData(WastelandForgeSchemaIds.JipScriptEmissionManifest010)]
    [InlineData(WastelandForgeSchemaIds.XEditAudit010)]
    [InlineData(WastelandForgeSchemaIds.McmExtenderOutput010)]
    [InlineData(WastelandForgeSchemaIds.PackageManifest010)]
    [InlineData(WastelandForgeSchemaIds.InstallPreview010)]
    [InlineData(WastelandForgeSchemaIds.InstallPlan010)]
    [InlineData(WastelandForgeSchemaIds.PackageVerification010)]
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
    [InlineData(WastelandForgeSchemaIds.Dialogue090)]
    [InlineData(WastelandForgeSchemaIds.Dialogue0100)]
    [InlineData(WastelandForgeSchemaIds.Dialogue0110)]
    [InlineData(WastelandForgeSchemaIds.Dialogue0120)]
    [InlineData(WastelandForgeSchemaIds.Dialogue0130)]
    [InlineData(WastelandForgeSchemaIds.Dialogue0140)]
    [InlineData(WastelandForgeSchemaIds.Dialogue0150)]
    [InlineData(WastelandForgeSchemaIds.Dialogue0160)]
    [InlineData(WastelandForgeSchemaIds.Dialogue0170)]
    [InlineData(WastelandForgeSchemaIds.Dialogue0180)]
    [InlineData(WastelandForgeSchemaIds.Dialogue0190)]
    [InlineData(WastelandForgeSchemaIds.Dialogue0200)]
    [InlineData(WastelandForgeSchemaIds.Dialogue0210)]
    [InlineData(WastelandForgeSchemaIds.Dialogue0220)]
    [InlineData(WastelandForgeSchemaIds.Dialogue0230)]
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
