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
    }

    [Fact]
    public void RegistrySchemaIdsRemainImmutable()
    {
        Assert.Equal(
            "https://schemas.wastelandforge.dev/fnv/dependencies/0.1.0/schema.json",
            WastelandForgeSchemaIds.Dependency010);
        Assert.Equal(
            "https://schemas.wastelandforge.dev/fnv/capabilities/0.1.0/schema.json",
            WastelandForgeSchemaIds.Capability010);
        Assert.Equal(
            "https://schemas.wastelandforge.dev/fnv/assets/0.1.0/schema.json",
            WastelandForgeSchemaIds.Asset010);
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
    [InlineData(WastelandForgeSchemaIds.Dependency010, "dependency")]
    [InlineData(WastelandForgeSchemaIds.Capability010, "capability")]
    [InlineData(WastelandForgeSchemaIds.Asset010, "asset")]
    public void RegistrySchemaCatalogKeepsVersionedResources(string schemaId, string expectedKind)
    {
        var found = WastelandForgeSchemaCatalog.TryGetById(schemaId, out var resource);

        Assert.True(found);
        Assert.NotNull(resource);
        Assert.Equal("0.1.0", resource.Version);
        Assert.Equal(expectedKind, resource.Kind);
    }
}
