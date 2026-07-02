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
        var doctor = CapabilityDoctorPlanner.Build(catalog, providers, capabilities);
        var summary = new CapabilityScanSummary(
            providers.Length,
            capabilities.Length,
            CountStatus(providers, CapabilityScanStatuses.Probable),
            CountStatus(providers, CapabilityScanStatuses.Missing),
            CountStatus(providers, CapabilityScanStatuses.Unknown),
            CountStatus(providers, CapabilityScanStatuses.WrongScope),
            CountStatus(capabilities, CapabilityScanStatuses.Probable),
            CountStatus(capabilities, CapabilityScanStatuses.Missing),
            CountStatus(capabilities, CapabilityScanStatuses.Unknown),
            CountStatus(capabilities, CapabilityScanStatuses.WrongScope));

        return new CapabilityScanReport(catalog, inputs, summary, providers, capabilities, doctor);
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
            "provider.game.falloutnv" => ProbePathSet(provider, "root-file", "root", inputs.GameRoot, ["FalloutNV.exe"], inputs.DataRoot, "data-managed"),
            "provider.runtime.xnvse" => ProbePathSet(provider, "root-file", "root", inputs.GameRoot, ["nvse_loader.exe"], inputs.DataRoot, "data-managed"),
            "provider.runtime.jip_ln" => ProbePathSet(provider, "data-file", "data-managed", inputs.DataRoot, [Path.Combine("NVSE", "Plugins", "jip_nvse.dll")], inputs.GameRoot, "root"),
            "provider.runtime.jip_pp_ln" => UnknownProvider(provider, "data-file", "data-managed", "JIP PP LN file alias policy remains open in the built-in catalogue."),
            "provider.runtime.johnnyguitar" => ProbePathSet(provider, "data-file", "data-managed", inputs.DataRoot, [Path.Combine("NVSE", "Plugins", "JohnnyGuitarNVSE.dll")], inputs.GameRoot, "root"),
            "provider.runtime.showoff" => ProbePathSet(provider, "data-file", "data-managed", inputs.DataRoot, [Path.Combine("NVSE", "Plugins", "ShowOffNVSE.dll")], inputs.GameRoot, "root"),
            "provider.runtime.uio" => ProbePathSet(provider, "data-file", "data-managed", inputs.DataRoot, [Path.Combine("UIO", "Public")], inputs.GameRoot, "root"),
            "provider.runtime.mcm" => ProbePathSet(provider, "data-file", "data-managed", inputs.DataRoot, [Path.Combine("Menus", "Prefabs", "MCM")], inputs.GameRoot, "root"),
            "provider.runtime.mcm_extender" => ProbePathSet(provider, "data-file", "data-managed", inputs.DataRoot, [Path.Combine("Menus", "Prefabs", "MCMExtender")], inputs.GameRoot, "root"),
            "provider.runtime.knvse" => ProbePathSet(provider, "data-file", "data-managed", inputs.DataRoot, [Path.Combine("NVSE", "Plugins", "kNVSE.dll")], inputs.GameRoot, "root"),
            "provider.editor.geck" => ProbeExecutable(provider, "editor", inputs, ["GECK.exe"], includeGameRoot: true),
            "provider.editor.geck_extender" => UnknownProvider(provider, "executable-tool", "mixed", "GECK Extender has mixed-scope install evidence; Gate 58 does not define a safe file marker."),
            "provider.editor.hot_reload" => ProbePathSet(provider, "data-file", "data-managed", inputs.DataRoot, [Path.Combine("NVSE", "Plugins", "hot_reload.dll")], inputs.GameRoot, "root"),
            "provider.tool.xedit" => ProbeExecutable(provider, "tool", inputs, ["FNVEdit.exe", "xEdit.exe"], includeGameRoot: false),
            "provider.tool.mo2" => ProbeExecutable(provider, "tool", inputs, ["ModOrganizer.exe", "ModOrganizer2.exe"], includeGameRoot: false),
            _ => UnknownProvider(provider, "unknown", provider.InstallScope, "No Gate 58 detector is configured for this provider.")
        };

    private static ProviderScanResult ProbePathSet(
        ProviderDefinition provider,
        string detectorKind,
        string scope,
        string? root,
        IReadOnlyList<string> relativePaths,
        string? wrongScopeRoot = null,
        string? wrongScope = null)
    {
        var evidence = ProbeExpectedPathSet(detectorKind, scope, root, relativePaths);
        if (evidence.Any(item => StringComparer.Ordinal.Equals(item.Status, CapabilityScanStatuses.Probable)))
        {
            return new ProviderScanResult(
                provider,
                CapabilityScanStatuses.Probable,
                evidence);
        }

        var wrongScopeEvidence = ProbeWrongScopePathSet(detectorKind, scope, wrongScopeRoot, wrongScope, relativePaths);
        if (wrongScopeEvidence.Any(item => StringComparer.Ordinal.Equals(item.Status, CapabilityScanStatuses.WrongScope)))
        {
            return new ProviderScanResult(
                provider,
                CapabilityScanStatuses.WrongScope,
                evidence.Concat(wrongScopeEvidence).ToArray());
        }

        return new ProviderScanResult(
            provider,
            evidence.All(item => StringComparer.Ordinal.Equals(item.Status, CapabilityScanStatuses.Missing))
                ? CapabilityScanStatuses.Missing
                : CapabilityScanStatuses.Unknown,
            evidence);
    }

    private static IReadOnlyList<CapabilityScanEvidence> ProbeExpectedPathSet(
        string detectorKind,
        string scope,
        string? root,
        IReadOnlyList<string> relativePaths)
    {
        if (string.IsNullOrWhiteSpace(root))
        {
            return [new CapabilityScanEvidence(detectorKind, scope, CapabilityScanStatuses.Unknown, null, $"No {scope} path was provided.")];
        }

        if (!Directory.Exists(root))
        {
            return [new CapabilityScanEvidence(detectorKind, scope, CapabilityScanStatuses.Missing, root, $"The {scope} path does not exist.")];
        }

        return relativePaths.Select(relativePath =>
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
    }

    private static IReadOnlyList<CapabilityScanEvidence> ProbeWrongScopePathSet(
        string detectorKind,
        string expectedScope,
        string? wrongScopeRoot,
        string? wrongScope,
        IReadOnlyList<string> relativePaths)
    {
        if (string.IsNullOrWhiteSpace(wrongScopeRoot) || string.IsNullOrWhiteSpace(wrongScope) || !Directory.Exists(wrongScopeRoot))
        {
            return [];
        }

        return relativePaths
            .Select(relativePath =>
            {
                var fullPath = Path.Combine(wrongScopeRoot, relativePath);
                var exists = File.Exists(fullPath) || Directory.Exists(fullPath);
                return exists
                    ? new CapabilityScanEvidence(
                        detectorKind,
                        wrongScope,
                        CapabilityScanStatuses.WrongScope,
                        fullPath,
                        $"Expected marker was found in {wrongScope} scope, but this provider expects {expectedScope} scope.")
                    : null;
            })
            .OfType<CapabilityScanEvidence>()
            .ToArray();
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
            : providerStatuses.Any(item => item.EndsWith($":{CapabilityScanStatuses.WrongScope}", StringComparison.Ordinal))
                ? CapabilityScanStatuses.WrongScope
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
