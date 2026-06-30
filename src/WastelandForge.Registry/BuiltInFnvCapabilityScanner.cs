namespace WastelandForge.Registry;

public sealed class BuiltInFnvCapabilityScanner
{
    private static readonly string[] DetectorFamilies = ["root-file", "data-file", "executable-tool"];

    public CapabilityScanReport Scan(CapabilityScanOptions options)
    {
        var catalog = BuiltInFnvCapabilityCatalog.Create();
        var inputs = NormalizeInputs(options);
        var providers = catalog.Providers.Select(provider => ScanProvider(provider, inputs)).ToArray();
        var capabilities = catalog.Capabilities.Select(capability => ScanCapability(capability, providers)).ToArray();
        var summary = new CapabilityScanSummary(
            providers.Length,
            capabilities.Length,
            CountStatus(providers, CapabilityScanStatuses.Probable),
            CountStatus(providers, CapabilityScanStatuses.Missing),
            CountStatus(providers, CapabilityScanStatuses.Unknown),
            CountStatus(capabilities, CapabilityScanStatuses.Probable),
            CountStatus(capabilities, CapabilityScanStatuses.Missing),
            CountStatus(capabilities, CapabilityScanStatuses.Unknown));

        return new CapabilityScanReport(catalog, inputs, summary, providers, capabilities);
    }

    private static CapabilityScanInputs NormalizeInputs(CapabilityScanOptions options)
    {
        var gameRoot = NormalizePath(options.GameRoot);
        var dataRoot = NormalizePath(options.DataRoot);
        if (dataRoot is null && gameRoot is not null)
        {
            dataRoot = Path.Combine(gameRoot, "Data");
        }

        return new CapabilityScanInputs(
            gameRoot,
            dataRoot,
            options.ToolPaths.Select(NormalizeRequiredPath).ToArray(),
            DetectorFamilies,
            RuntimeProbesEnabled: false,
            Mo2VfsEnabled: false);
    }

    private static ProviderScanResult ScanProvider(ProviderDefinition provider, CapabilityScanInputs inputs) =>
        provider.Id switch
        {
            "provider.game.falloutnv" => ProbePathSet(provider, "root-file", "root", inputs.GameRoot, ["FalloutNV.exe"]),
            "provider.runtime.xnvse" => ProbePathSet(provider, "root-file", "root", inputs.GameRoot, ["nvse_loader.exe"]),
            "provider.runtime.jip_ln" => ProbePathSet(provider, "data-file", "data-managed", inputs.DataRoot, [Path.Combine("NVSE", "Plugins", "jip_nvse.dll")]),
            "provider.runtime.jip_pp_ln" => UnknownProvider(provider, "data-file", "data-managed", "JIP PP LN file alias policy remains open in the built-in catalogue."),
            "provider.runtime.johnnyguitar" => ProbePathSet(provider, "data-file", "data-managed", inputs.DataRoot, [Path.Combine("NVSE", "Plugins", "JohnnyGuitarNVSE.dll")]),
            "provider.runtime.showoff" => ProbePathSet(provider, "data-file", "data-managed", inputs.DataRoot, [Path.Combine("NVSE", "Plugins", "ShowOffNVSE.dll")]),
            "provider.runtime.uio" => ProbePathSet(provider, "data-file", "data-managed", inputs.DataRoot, [Path.Combine("UIO", "Public")]),
            "provider.runtime.mcm" => ProbePathSet(provider, "data-file", "data-managed", inputs.DataRoot, [Path.Combine("Menus", "Prefabs", "MCM")]),
            "provider.runtime.mcm_extender" => ProbePathSet(provider, "data-file", "data-managed", inputs.DataRoot, [Path.Combine("Menus", "Prefabs", "MCMExtender")]),
            "provider.runtime.knvse" => ProbePathSet(provider, "data-file", "data-managed", inputs.DataRoot, [Path.Combine("NVSE", "Plugins", "kNVSE.dll")]),
            "provider.editor.geck" => ProbeExecutable(provider, "editor", inputs, ["GECK.exe"], includeGameRoot: true),
            "provider.editor.geck_extender" => UnknownProvider(provider, "executable-tool", "mixed", "GECK Extender has mixed-scope install evidence; Gate 58 does not define a safe file marker."),
            "provider.editor.hot_reload" => ProbePathSet(provider, "data-file", "data-managed", inputs.DataRoot, [Path.Combine("NVSE", "Plugins", "hot_reload.dll")]),
            "provider.tool.xedit" => ProbeExecutable(provider, "tool", inputs, ["FNVEdit.exe", "xEdit.exe"], includeGameRoot: false),
            "provider.tool.mo2" => ProbeExecutable(provider, "tool", inputs, ["ModOrganizer.exe", "ModOrganizer2.exe"], includeGameRoot: false),
            _ => UnknownProvider(provider, "unknown", provider.InstallScope, "No Gate 58 detector is configured for this provider.")
        };

    private static ProviderScanResult ProbePathSet(
        ProviderDefinition provider,
        string detectorKind,
        string scope,
        string? root,
        IReadOnlyList<string> relativePaths)
    {
        if (string.IsNullOrWhiteSpace(root))
        {
            return new ProviderScanResult(
                provider,
                CapabilityScanStatuses.Unknown,
                [new CapabilityScanEvidence(detectorKind, scope, CapabilityScanStatuses.Unknown, null, $"No {scope} path was provided.")]);
        }

        if (!Directory.Exists(root))
        {
            return new ProviderScanResult(
                provider,
                CapabilityScanStatuses.Missing,
                [new CapabilityScanEvidence(detectorKind, scope, CapabilityScanStatuses.Missing, root, $"The {scope} path does not exist.")]);
        }

        var evidence = relativePaths.Select(relativePath =>
        {
            var fullPath = Path.Combine(root, relativePath);
            var exists = File.Exists(fullPath) || Directory.Exists(fullPath);
            return new CapabilityScanEvidence(
                detectorKind,
                scope,
                exists ? CapabilityScanStatuses.Probable : CapabilityScanStatuses.Missing,
                fullPath,
                exists ? "Expected marker exists." : "Expected marker is missing.");
        }).ToArray();

        return new ProviderScanResult(
            provider,
            evidence.Any(item => StringComparer.Ordinal.Equals(item.Status, CapabilityScanStatuses.Probable))
                ? CapabilityScanStatuses.Probable
                : CapabilityScanStatuses.Missing,
            evidence);
    }

    private static ProviderScanResult ProbeExecutable(
        ProviderDefinition provider,
        string scope,
        CapabilityScanInputs inputs,
        IReadOnlyList<string> executableNames,
        bool includeGameRoot)
    {
        var candidatePaths = new List<string>();
        if (includeGameRoot && inputs.GameRoot is not null)
        {
            candidatePaths.AddRange(executableNames.Select(name => Path.Combine(inputs.GameRoot, name)));
        }

        foreach (var toolPath in inputs.ToolPaths)
        {
            if (Directory.Exists(toolPath))
            {
                candidatePaths.AddRange(executableNames.Select(name => Path.Combine(toolPath, name)));
            }
            else
            {
                candidatePaths.Add(toolPath);
            }
        }

        if (candidatePaths.Count == 0)
        {
            return new ProviderScanResult(
                provider,
                CapabilityScanStatuses.Unknown,
                [new CapabilityScanEvidence("executable-tool", scope, CapabilityScanStatuses.Unknown, null, "No executable tool path was provided.")]);
        }

        var evidence = candidatePaths
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Select(path =>
            {
                var fileNameMatches = executableNames.Contains(Path.GetFileName(path), StringComparer.OrdinalIgnoreCase);
                var exists = fileNameMatches && File.Exists(path);
                return new CapabilityScanEvidence(
                    "executable-tool",
                    scope,
                    exists ? CapabilityScanStatuses.Probable : CapabilityScanStatuses.Missing,
                    path,
                    exists ? "Expected executable exists." : "Expected executable is missing.");
            })
            .ToArray();

        return new ProviderScanResult(
            provider,
            evidence.Any(item => StringComparer.Ordinal.Equals(item.Status, CapabilityScanStatuses.Probable))
                ? CapabilityScanStatuses.Probable
                : CapabilityScanStatuses.Missing,
            evidence);
    }

    private static ProviderScanResult UnknownProvider(
        ProviderDefinition provider,
        string detectorKind,
        string scope,
        string message) =>
        new(
            provider,
            CapabilityScanStatuses.Unknown,
            [new CapabilityScanEvidence(detectorKind, scope, CapabilityScanStatuses.Unknown, null, message)]);

    private static CapabilityScanResult ScanCapability(
        CapabilityDefinition capability,
        IReadOnlyList<ProviderScanResult> providers)
    {
        var providerStatuses = providers
            .Where(provider => capability.SatisfiedBy.Contains(provider.Provider.Id, StringComparer.Ordinal))
            .Select(provider => $"{provider.Provider.Id}:{provider.Status}")
            .Order(StringComparer.Ordinal)
            .ToArray();

        var status = providerStatuses.Any(item => item.EndsWith($":{CapabilityScanStatuses.Probable}", StringComparison.Ordinal))
            ? CapabilityScanStatuses.Probable
            : providerStatuses.Length > 0 && providerStatuses.All(item => item.EndsWith($":{CapabilityScanStatuses.Missing}", StringComparison.Ordinal))
                ? CapabilityScanStatuses.Missing
                : CapabilityScanStatuses.Unknown;

        return new CapabilityScanResult(capability, status, providerStatuses);
    }

    private static int CountStatus(IEnumerable<ProviderScanResult> results, string status) =>
        results.Count(result => StringComparer.Ordinal.Equals(result.Status, status));

    private static int CountStatus(IEnumerable<CapabilityScanResult> results, string status) =>
        results.Count(result => StringComparer.Ordinal.Equals(result.Status, status));

    private static string? NormalizePath(string? path) =>
        string.IsNullOrWhiteSpace(path) ? null : NormalizeRequiredPath(path);

    private static string NormalizeRequiredPath(string path) =>
        Path.GetFullPath(path);
}
