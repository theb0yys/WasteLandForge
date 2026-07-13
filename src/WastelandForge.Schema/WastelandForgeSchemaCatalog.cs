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
            WastelandForgeSchemaIds.Manifest020,
            "manifest",
            "0.2.0",
            "schemas/manifest/0.2.0/schema.json"),
        new(
            WastelandForgeSchemaIds.Manifest030,
            "manifest",
            "0.3.0",
            "schemas/manifest/0.3.0/schema.json"),
        new(WastelandForgeSchemaIds.Manifest040, "manifest", "0.4.0", "schemas/manifest/0.4.0/schema.json"),
        new(WastelandForgeSchemaIds.Manifest050, "manifest", "0.5.0", "schemas/manifest/0.5.0/schema.json"),
        new(WastelandForgeSchemaIds.GeckAuthoringIntent010, "geck-authoring-intent", "0.1.0", "schemas/geck-authoring-intent/0.1.0/schema.json"),
        new(WastelandForgeSchemaIds.GeckAuthoringPlan010, "geck-authoring-plan", "0.1.0", "schemas/geck-authoring-plan/0.1.0/schema.json"),
        new(WastelandForgeSchemaIds.Fomod010, "fomod", "0.1.0", "schemas/fomod/0.1.0/schema.json"),
        new(WastelandForgeSchemaIds.FomodManifest010, "fomod-manifest", "0.1.0", "schemas/fomod-manifest/0.1.0/schema.json"),
        new(
            WastelandForgeSchemaIds.Dependency010,
            "dependency",
            "0.1.0",
            "schemas/dependencies/0.1.0/schema.json"),
        new(
            WastelandForgeSchemaIds.Dependency020,
            "dependency",
            "0.2.0",
            "schemas/dependencies/0.2.0/schema.json"),
        new(
            WastelandForgeSchemaIds.Capability010,
            "capability",
            "0.1.0",
            "schemas/capabilities/0.1.0/schema.json"),
        new(
            WastelandForgeSchemaIds.Capability020,
            "capability",
            "0.2.0",
            "schemas/capabilities/0.2.0/schema.json"),
        new(
            WastelandForgeSchemaIds.Asset010,
            "asset",
            "0.1.0",
            "schemas/assets/0.1.0/schema.json"),
        new(
            WastelandForgeSchemaIds.PluginArtifact010,
            "plugin-artifact",
            "0.1.0",
            "schemas/plugin-artifacts/0.1.0/schema.json"),
        new(
            WastelandForgeSchemaIds.PluginReviewEvidence010,
            "plugin-review-evidence",
            "0.1.0",
            "schemas/plugin-review-evidence/0.1.0/schema.json"),
        new(
            WastelandForgeSchemaIds.Mcm010,
            "mcm",
            "0.1.0",
            "schemas/mcm/0.1.0/schema.json"),
        new(
            WastelandForgeSchemaIds.JipScript010,
            "jip-script",
            "0.1.0",
            "schemas/jip-scripts/0.1.0/schema.json"),
        new(
            WastelandForgeSchemaIds.JipScriptEmissionManifest010,
            "jip-script-emission-manifest",
            "0.1.0",
            "schemas/jip-script-emission-manifest/0.1.0/schema.json"),
        new(
            WastelandForgeSchemaIds.XEditAudit010,
            "xedit-audit",
            "0.1.0",
            "schemas/xedit-audit/0.1.0/schema.json"),
        new(WastelandForgeSchemaIds.XEditAudit020, "xedit-audit", "0.2.0", "schemas/xedit-audit/0.2.0/schema.json"),
        new(WastelandForgeSchemaIds.XEditCheckReport010, "xedit-check-report", "0.1.0", "schemas/xedit-check-report/0.1.0/schema.json"),
        new(
            WastelandForgeSchemaIds.McmExtenderOutput010,
            "mcm-extender-output",
            "0.1.0",
            "schemas/mcm-extender-output/0.1.0/schema.json"),
        new(
            WastelandForgeSchemaIds.PackageManifest010,
            "package-manifest",
            "0.1.0",
            "schemas/package-manifest/0.1.0/schema.json"),
        new(
            WastelandForgeSchemaIds.ModPackageManifest010,
            "mod-package-manifest",
            "0.1.0",
            "schemas/mod-package-manifest/0.1.0/schema.json"),
        new(WastelandForgeSchemaIds.BsaPackPlan010, "bsa-pack-plan", "0.1.0", "schemas/bsa-pack-plan/0.1.0/schema.json"),
        new(WastelandForgeSchemaIds.BsArchPreview010, "bsa-bsarch-preview", "0.1.0", "schemas/bsa-bsarch-preview/0.1.0/schema.json"),
        new(WastelandForgeSchemaIds.BsArchExecution010, "bsarch-execution", "0.1.0", "schemas/bsarch-execution/0.1.0/schema.json"),
        new(WastelandForgeSchemaIds.BsaOutputVerification010, "bsa-output-verification", "0.1.0", "schemas/bsa-output-verification/0.1.0/schema.json"),
        new(WastelandForgeSchemaIds.BsaPackageManifest010, "bsa-package-manifest", "0.1.0", "schemas/bsa-package-manifest/0.1.0/schema.json"),
        new(
            WastelandForgeSchemaIds.Mo2ExportManifest010,
            "mo2-export-manifest",
            "0.1.0",
            "schemas/mo2-export-manifest/0.1.0/schema.json"),
        new(
            WastelandForgeSchemaIds.Mo2ExportManifest020,
            "mo2-export-manifest",
            "0.2.0",
            "schemas/mo2-export-manifest/0.2.0/schema.json"),
        new(
            WastelandForgeSchemaIds.GeckHandoffManifest010,
            "geck-handoff-manifest",
            "0.1.0",
            "schemas/geck-handoff-manifest/0.1.0/schema.json"),
        new(
            WastelandForgeSchemaIds.GeckHandoffManifest020,
            "geck-handoff-manifest",
            "0.2.0",
            "schemas/geck-handoff-manifest/0.2.0/schema.json"),
        new(
            WastelandForgeSchemaIds.InstallPreview010,
            "install-preview",
            "0.1.0",
            "schemas/install-preview/0.1.0/schema.json"),
        new(
            WastelandForgeSchemaIds.InstallPlan010,
            "install-plan",
            "0.1.0",
            "schemas/install-plan/0.1.0/schema.json"),
        new(
            WastelandForgeSchemaIds.PackageVerification010,
            "package-verification",
            "0.1.0",
            "schemas/package-verification/0.1.0/schema.json"),
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
            "schemas/dialogue/0.5.0/schema.json"),
        new(
            WastelandForgeSchemaIds.Dialogue060,
            "dialogue",
            "0.6.0",
            "schemas/dialogue/0.6.0/schema.json"),
        new(
            WastelandForgeSchemaIds.Dialogue070,
            "dialogue",
            "0.7.0",
            "schemas/dialogue/0.7.0/schema.json"),
        new(
            WastelandForgeSchemaIds.Dialogue080,
            "dialogue",
            "0.8.0",
            "schemas/dialogue/0.8.0/schema.json"),
        new(
            WastelandForgeSchemaIds.Dialogue090,
            "dialogue",
            "0.9.0",
            "schemas/dialogue/0.9.0/schema.json"),
        new(
            WastelandForgeSchemaIds.Dialogue0100,
            "dialogue",
            "0.10.0",
            "schemas/dialogue/0.10.0/schema.json"),
        new(
            WastelandForgeSchemaIds.Dialogue0110,
            "dialogue",
            "0.11.0",
            "schemas/dialogue/0.11.0/schema.json"),
        new(
            WastelandForgeSchemaIds.Dialogue0120,
            "dialogue",
            "0.12.0",
            "schemas/dialogue/0.12.0/schema.json"),
        new(
            WastelandForgeSchemaIds.Dialogue0130,
            "dialogue",
            "0.13.0",
            "schemas/dialogue/0.13.0/schema.json"),
        new(
            WastelandForgeSchemaIds.Dialogue0140,
            "dialogue",
            "0.14.0",
            "schemas/dialogue/0.14.0/schema.json"),
        new(
            WastelandForgeSchemaIds.Dialogue0150,
            "dialogue",
            "0.15.0",
            "schemas/dialogue/0.15.0/schema.json"),
        new(
            WastelandForgeSchemaIds.Dialogue0160,
            "dialogue",
            "0.16.0",
            "schemas/dialogue/0.16.0/schema.json"),
        new(
            WastelandForgeSchemaIds.Dialogue0170,
            "dialogue",
            "0.17.0",
            "schemas/dialogue/0.17.0/schema.json"),
        new(
            WastelandForgeSchemaIds.Dialogue0180,
            "dialogue",
            "0.18.0",
            "schemas/dialogue/0.18.0/schema.json"),
        new(
            WastelandForgeSchemaIds.Dialogue0190,
            "dialogue",
            "0.19.0",
            "schemas/dialogue/0.19.0/schema.json"),
        new(
            WastelandForgeSchemaIds.Dialogue0200,
            "dialogue",
            "0.20.0",
            "schemas/dialogue/0.20.0/schema.json"),
        new(
            WastelandForgeSchemaIds.Dialogue0210,
            "dialogue",
            "0.21.0",
            "schemas/dialogue/0.21.0/schema.json"),
        new(
            WastelandForgeSchemaIds.Dialogue0220,
            "dialogue",
            "0.22.0",
            "schemas/dialogue/0.22.0/schema.json"),
        new(
            WastelandForgeSchemaIds.Dialogue0230,
            "dialogue",
            "0.23.0",
            "schemas/dialogue/0.23.0/schema.json")
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
        var escaped = segment.Replace("-", "_", StringComparison.Ordinal);
        return segment.Length > 0 && char.IsDigit(segment[0])
            ? "_" + escaped
            : escaped;
    }
}
