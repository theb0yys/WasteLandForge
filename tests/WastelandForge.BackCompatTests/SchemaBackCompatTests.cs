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
    public void ManifestSchemaCatalogKeepsVersionedResource()
    {
        var found = WastelandForgeSchemaCatalog.TryGetById(WastelandForgeSchemaIds.Manifest010, out var resource);

        Assert.True(found);
        Assert.NotNull(resource);
        Assert.Equal("0.1.0", resource.Version);
        Assert.Equal("manifest", resource.Kind);
    }
}
