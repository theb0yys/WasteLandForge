namespace WastelandForge.Generation;

public sealed record JipScriptPackageOutputs(
    string Root,
    string PackageRoot,
    IReadOnlyList<string> Scripts,
    string PackageManifest,
    string InstallPlan,
    string BuildManifest,
    string Checksums);
