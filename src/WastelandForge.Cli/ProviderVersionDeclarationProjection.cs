using System.Text.Json.Nodes;
using WastelandForge.Registry;

namespace WastelandForge.Cli;

internal static class ProviderVersionDeclarationProjection
{
    public static JsonObject ToJson(ProviderVersionDeclaration version) =>
        new()
        {
            ["scheme"] = version.Scheme,
            ["source"] = version.Source,
            ["status"] = version.Status,
            ["localVersionStatus"] = version.LocalVersionStatus,
            ["resolutionStatus"] = version.ResolutionStatus,
            ["notes"] = new JsonArray(version.Notes.Select(note => JsonValue.Create(note)).ToArray())
        };

    public static string Format(ProviderVersionDeclaration version) =>
        $"{version.Scheme} ({version.Status}; local={version.LocalVersionStatus}; resolution={version.ResolutionStatus})";
}
