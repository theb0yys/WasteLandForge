using WastelandForge.Schema;

namespace WastelandForge.BackCompatTests;

public sealed class SchemaBackCompatTests
{
    [Fact]
    public void ManifestSchemaIdRemainsImmutable()
    {
        Assert.Equal(
            "https://schemas.wastelandforge.dev/fnv/manifest/0.1.0/schema.json",
            WastelandForgeSchemaIds.Manifest010);
        Assert.Equal(
            "https://schemas.wastelandforge.dev/fnv/manifest/0.2.0/schema.json",
            WastelandForgeSchemaIds.Manifest020);
    }

    [Fact]
    public void RegistrySchemaIdsRemainImmutable()
    {
        Assert.Equal(
            "https://schemas.wastelandforge.dev/fnv/dependencies/0.1.0/schema.json",
            WastelandForgeSchemaIds.Dependency010);
        Assert.Equal(
            "https://schemas.wastelandforge.dev/fnv/dependencies/0.2.0/schema.json",
            WastelandForgeSchemaIds.Dependency020);
        Assert.Equal(
            "https://schemas.wastelandforge.dev/fnv/capabilities/0.1.0/schema.json",
            WastelandForgeSchemaIds.Capability010);
        Assert.Equal(
            "https://schemas.wastelandforge.dev/fnv/capabilities/0.2.0/schema.json",
            WastelandForgeSchemaIds.Capability020);
        Assert.Equal(
            "https://schemas.wastelandforge.dev/fnv/assets/0.1.0/schema.json",
            WastelandForgeSchemaIds.Asset010);
        Assert.Equal(
            "https://schemas.wastelandforge.dev/fnv/mcm/0.1.0/schema.json",
            WastelandForgeSchemaIds.Mcm010);
        Assert.Equal(
            "https://schemas.wastelandforge.dev/fnv/jip-scripts/0.1.0/schema.json",
            WastelandForgeSchemaIds.JipScript010);
        Assert.Equal(
            "https://schemas.wastelandforge.dev/fnv/xedit-audit/0.1.0/schema.json",
            WastelandForgeSchemaIds.XEditAudit010);
        Assert.Equal(
            "https://schemas.wastelandforge.dev/fnv/mcm-extender-output/0.1.0/schema.json",
            WastelandForgeSchemaIds.McmExtenderOutput010);
        Assert.Equal(
            "https://schemas.wastelandforge.dev/fnv/package-manifest/0.1.0/schema.json",
            WastelandForgeSchemaIds.PackageManifest010);
        Assert.Equal(
            "https://schemas.wastelandforge.dev/fnv/install-preview/0.1.0/schema.json",
            WastelandForgeSchemaIds.InstallPreview010);
        Assert.Equal(
            "https://schemas.wastelandforge.dev/fnv/install-plan/0.1.0/schema.json",
            WastelandForgeSchemaIds.InstallPlan010);
        Assert.Equal(
            "https://schemas.wastelandforge.dev/fnv/package-verification/0.1.0/schema.json",
            WastelandForgeSchemaIds.PackageVerification010);
        Assert.Equal(
            "https://schemas.wastelandforge.dev/fnv/quests/0.1.0/schema.json",
            WastelandForgeSchemaIds.Quest010);
        Assert.Equal(
            "https://schemas.wastelandforge.dev/fnv/quests/0.2.0/schema.json",
            WastelandForgeSchemaIds.Quest020);
        Assert.Equal(
            "https://schemas.wastelandforge.dev/fnv/quests/0.3.0/schema.json",
            WastelandForgeSchemaIds.Quest030);
        Assert.Equal(
            "https://schemas.wastelandforge.dev/fnv/quests/0.4.0/schema.json",
            WastelandForgeSchemaIds.Quest040);
        Assert.Equal(
            "https://schemas.wastelandforge.dev/fnv/quests/0.5.0/schema.json",
            WastelandForgeSchemaIds.Quest050);
        Assert.Equal(
            "https://schemas.wastelandforge.dev/fnv/quests/0.6.0/schema.json",
            WastelandForgeSchemaIds.Quest060);
        Assert.Equal(
            "https://schemas.wastelandforge.dev/fnv/dialogue/0.1.0/schema.json",
            WastelandForgeSchemaIds.Dialogue010);
        Assert.Equal(
            "https://schemas.wastelandforge.dev/fnv/dialogue/0.2.0/schema.json",
            WastelandForgeSchemaIds.Dialogue020);
        Assert.Equal(
            "https://schemas.wastelandforge.dev/fnv/dialogue/0.3.0/schema.json",
            WastelandForgeSchemaIds.Dialogue030);
        Assert.Equal(
            "https://schemas.wastelandforge.dev/fnv/dialogue/0.4.0/schema.json",
            WastelandForgeSchemaIds.Dialogue040);
        Assert.Equal(
            "https://schemas.wastelandforge.dev/fnv/dialogue/0.5.0/schema.json",
            WastelandForgeSchemaIds.Dialogue050);
        Assert.Equal(
            "https://schemas.wastelandforge.dev/fnv/dialogue/0.6.0/schema.json",
            WastelandForgeSchemaIds.Dialogue060);
        Assert.Equal(
            "https://schemas.wastelandforge.dev/fnv/dialogue/0.7.0/schema.json",
            WastelandForgeSchemaIds.Dialogue070);
        Assert.Equal(
            "https://schemas.wastelandforge.dev/fnv/dialogue/0.8.0/schema.json",
            WastelandForgeSchemaIds.Dialogue080);
        Assert.Equal(
            "https://schemas.wastelandforge.dev/fnv/dialogue/0.9.0/schema.json",
            WastelandForgeSchemaIds.Dialogue090);
        Assert.Equal(
            "https://schemas.wastelandforge.dev/fnv/dialogue/0.10.0/schema.json",
            WastelandForgeSchemaIds.Dialogue0100);
        Assert.Equal(
            "https://schemas.wastelandforge.dev/fnv/dialogue/0.11.0/schema.json",
            WastelandForgeSchemaIds.Dialogue0110);
        Assert.Equal(
            "https://schemas.wastelandforge.dev/fnv/dialogue/0.12.0/schema.json",
            WastelandForgeSchemaIds.Dialogue0120);
        Assert.Equal(
            "https://schemas.wastelandforge.dev/fnv/dialogue/0.13.0/schema.json",
            WastelandForgeSchemaIds.Dialogue0130);
        Assert.Equal(
            "https://schemas.wastelandforge.dev/fnv/dialogue/0.14.0/schema.json",
            WastelandForgeSchemaIds.Dialogue0140);
        Assert.Equal(
            "https://schemas.wastelandforge.dev/fnv/dialogue/0.15.0/schema.json",
            WastelandForgeSchemaIds.Dialogue0150);
        Assert.Equal(
            "https://schemas.wastelandforge.dev/fnv/dialogue/0.16.0/schema.json",
            WastelandForgeSchemaIds.Dialogue0160);
        Assert.Equal(
            "https://schemas.wastelandforge.dev/fnv/dialogue/0.17.0/schema.json",
            WastelandForgeSchemaIds.Dialogue0170);
        Assert.Equal(
            "https://schemas.wastelandforge.dev/fnv/dialogue/0.18.0/schema.json",
            WastelandForgeSchemaIds.Dialogue0180);
        Assert.Equal(
            "https://schemas.wastelandforge.dev/fnv/dialogue/0.19.0/schema.json",
            WastelandForgeSchemaIds.Dialogue0190);
        Assert.Equal(
            "https://schemas.wastelandforge.dev/fnv/dialogue/0.20.0/schema.json",
            WastelandForgeSchemaIds.Dialogue0200);
        Assert.Equal(
            "https://schemas.wastelandforge.dev/fnv/dialogue/0.21.0/schema.json",
            WastelandForgeSchemaIds.Dialogue0210);
        Assert.Equal(
            "https://schemas.wastelandforge.dev/fnv/dialogue/0.22.0/schema.json",
            WastelandForgeSchemaIds.Dialogue0220);
        Assert.Equal(
            "https://schemas.wastelandforge.dev/fnv/dialogue/0.23.0/schema.json",
            WastelandForgeSchemaIds.Dialogue0230);
    }

    [Fact]
    public void ManifestSchemaCatalogKeepsVersionedResource()
    {
        var found = WastelandForgeSchemaCatalog.TryGetById(WastelandForgeSchemaIds.Manifest010, out var resource);

        Assert.True(found);
        Assert.NotNull(resource);
        Assert.Equal("0.1.0", resource.Version);
        Assert.Equal("manifest", resource.Kind);
    }

    [Theory]
    [InlineData(WastelandForgeSchemaIds.Dependency010, "dependency", "0.1.0")]
    [InlineData(WastelandForgeSchemaIds.Dependency020, "dependency", "0.2.0")]
    [InlineData(WastelandForgeSchemaIds.Capability010, "capability", "0.1.0")]
    [InlineData(WastelandForgeSchemaIds.Capability020, "capability", "0.2.0")]
    [InlineData(WastelandForgeSchemaIds.Asset010, "asset", "0.1.0")]
    [InlineData(WastelandForgeSchemaIds.Mcm010, "mcm", "0.1.0")]
    [InlineData(WastelandForgeSchemaIds.JipScript010, "jip-script", "0.1.0")]
    [InlineData(WastelandForgeSchemaIds.XEditAudit010, "xedit-audit", "0.1.0")]
    [InlineData(WastelandForgeSchemaIds.GeckAuthoringObservations010, "geck-authoring-observations", "0.1.0")]
    [InlineData(WastelandForgeSchemaIds.FnvGameKnowledgeExport010, "fnv-game-knowledge-export", "0.1.0")]
    [InlineData(WastelandForgeSchemaIds.FnvGameKnowledgeExport020, "fnv-game-knowledge-export", "0.2.0")]
    [InlineData(WastelandForgeSchemaIds.FnvGameKnowledgeIndex010, "fnv-game-knowledge-index", "0.1.0")]
    [InlineData(WastelandForgeSchemaIds.FnvGameKnowledgeIndex020, "fnv-game-knowledge-index", "0.2.0")]
    [InlineData(WastelandForgeSchemaIds.FnvGameKnowledgeReceipt010, "fnv-game-knowledge-receipt", "0.1.0")]
    [InlineData(WastelandForgeSchemaIds.FnvGameKnowledgeReceipt020, "fnv-game-knowledge-receipt", "0.2.0")]
    [InlineData(WastelandForgeSchemaIds.FnvGameKnowledgeExecutionPlan010, "fnv-game-knowledge-execution-plan", "0.1.0")]
    [InlineData(WastelandForgeSchemaIds.FnvGameKnowledgeExecutionReceipt010, "fnv-game-knowledge-execution-receipt", "0.1.0")]
    [InlineData(WastelandForgeSchemaIds.McmExtenderOutput010, "mcm-extender-output", "0.1.0")]
    [InlineData(WastelandForgeSchemaIds.PackageManifest010, "package-manifest", "0.1.0")]
    [InlineData(WastelandForgeSchemaIds.InstallPreview010, "install-preview", "0.1.0")]
    [InlineData(WastelandForgeSchemaIds.InstallPlan010, "install-plan", "0.1.0")]
    [InlineData(WastelandForgeSchemaIds.PackageVerification010, "package-verification", "0.1.0")]
    [InlineData(WastelandForgeSchemaIds.Quest010, "quest", "0.1.0")]
    [InlineData(WastelandForgeSchemaIds.Quest020, "quest", "0.2.0")]
    [InlineData(WastelandForgeSchemaIds.Quest030, "quest", "0.3.0")]
    [InlineData(WastelandForgeSchemaIds.Quest040, "quest", "0.4.0")]
    [InlineData(WastelandForgeSchemaIds.Quest050, "quest", "0.5.0")]
    [InlineData(WastelandForgeSchemaIds.Quest060, "quest", "0.6.0")]
    [InlineData(WastelandForgeSchemaIds.Dialogue010, "dialogue", "0.1.0")]
    [InlineData(WastelandForgeSchemaIds.Dialogue020, "dialogue", "0.2.0")]
    [InlineData(WastelandForgeSchemaIds.Dialogue030, "dialogue", "0.3.0")]
    [InlineData(WastelandForgeSchemaIds.Dialogue040, "dialogue", "0.4.0")]
    [InlineData(WastelandForgeSchemaIds.Dialogue050, "dialogue", "0.5.0")]
    [InlineData(WastelandForgeSchemaIds.Dialogue060, "dialogue", "0.6.0")]
    [InlineData(WastelandForgeSchemaIds.Dialogue070, "dialogue", "0.7.0")]
    [InlineData(WastelandForgeSchemaIds.Dialogue080, "dialogue", "0.8.0")]
    [InlineData(WastelandForgeSchemaIds.Dialogue090, "dialogue", "0.9.0")]
    [InlineData(WastelandForgeSchemaIds.Dialogue0100, "dialogue", "0.10.0")]
    [InlineData(WastelandForgeSchemaIds.Dialogue0110, "dialogue", "0.11.0")]
    [InlineData(WastelandForgeSchemaIds.Dialogue0120, "dialogue", "0.12.0")]
    [InlineData(WastelandForgeSchemaIds.Dialogue0130, "dialogue", "0.13.0")]
    [InlineData(WastelandForgeSchemaIds.Dialogue0140, "dialogue", "0.14.0")]
    [InlineData(WastelandForgeSchemaIds.Dialogue0150, "dialogue", "0.15.0")]
    [InlineData(WastelandForgeSchemaIds.Dialogue0160, "dialogue", "0.16.0")]
    [InlineData(WastelandForgeSchemaIds.Dialogue0170, "dialogue", "0.17.0")]
    [InlineData(WastelandForgeSchemaIds.Dialogue0180, "dialogue", "0.18.0")]
    [InlineData(WastelandForgeSchemaIds.Dialogue0190, "dialogue", "0.19.0")]
    [InlineData(WastelandForgeSchemaIds.Dialogue0200, "dialogue", "0.20.0")]
    [InlineData(WastelandForgeSchemaIds.Dialogue0210, "dialogue", "0.21.0")]
    [InlineData(WastelandForgeSchemaIds.Dialogue0220, "dialogue", "0.22.0")]
    [InlineData(WastelandForgeSchemaIds.Dialogue0230, "dialogue", "0.23.0")]
    public void RegistrySchemaCatalogKeepsVersionedResources(string schemaId, string expectedKind, string expectedVersion)
    {
        var found = WastelandForgeSchemaCatalog.TryGetById(schemaId, out var resource);

        Assert.True(found);
        Assert.NotNull(resource);
        Assert.Equal(expectedVersion, resource.Version);
        Assert.Equal(expectedKind, resource.Kind);
    }
}
