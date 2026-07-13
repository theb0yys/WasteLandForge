[CmdletBinding()]
param(
    [string] $OutputRoot = 'generated/geck-probe'
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$repositoryRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$outputRootPath = if ([IO.Path]::IsPathRooted($OutputRoot)) {
    [IO.Path]::GetFullPath($OutputRoot)
} else {
    [IO.Path]::GetFullPath((Join-Path $repositoryRoot $OutputRoot))
}
$expectedOutputRoot = [IO.Path]::GetFullPath((Join-Path $repositoryRoot 'generated\geck-probe'))
if (-not $outputRootPath.Equals($expectedOutputRoot, [StringComparison]::OrdinalIgnoreCase)) {
    throw "Refusing GECK probe verification outside generated/geck-probe: $outputRootPath"
}

$manifestPath = Join-Path $outputRootPath 'build-manifest.json'
if (-not (Test-Path -LiteralPath $manifestPath -PathType Leaf)) {
    throw "GECK probe build manifest is missing: $manifestPath"
}
$manifest = Get-Content -LiteralPath $manifestPath -Raw | ConvertFrom-Json
if ($manifest.kind -ne 'wastelandforge.geck-host-probe-build' -or $manifest.formatVersion -ne '0.1') {
    throw 'GECK probe build manifest identity is invalid.'
}
if ($manifest.configuration -ne 'Release GECK' -or $manifest.platform -ne 'Win32' -or $manifest.platformToolset -ne 'v143') {
    throw 'GECK probe build configuration is invalid.'
}
if ($manifest.externalSource.version -ne '6.4.4' -or
    $manifest.externalSource.commit -ne '694cdde6cbfa5e75afa661df587c73e8f0f6f441' -or
    $manifest.externalSource.tree -ne 'e80453c217027f47979d2dfca03665de0a93fa6f' -or
    $manifest.externalSource.clean -ne $true) {
    throw 'Pinned xNVSE provenance is invalid.'
}

$falseFlags = @(
    'launchAuthorized', 'mo2Staged', 'geckExecuted', 'runtimeObservationProduced',
    'recordApisUsed', 'pluginMutation', 'physicalDataWritten',
    'bundledThirdPartySource', 'bundledThirdPartyBinaries'
)
foreach ($flag in $falseFlags) {
    if ($manifest.$flag -ne $false) {
        throw "GECK probe build manifest must keep $flag false."
    }
}
if ($manifest.approvedProbeRunId -ne 'build-only-unapproved') {
    throw 'Build-only probe run identity is invalid.'
}

$probeDll = Join-Path $outputRootPath $manifest.output.path
$probeItem = Get-Item -LiteralPath $probeDll -ErrorAction Stop
$probeHash = (Get-FileHash -LiteralPath $probeDll -Algorithm SHA256).Hash.ToLowerInvariant()
if ($probeItem.Length -ne $manifest.output.length -or $probeHash -ne $manifest.output.sha256) {
    throw 'GECK probe DLL length or digest does not match the build manifest.'
}

$stream = [IO.File]::OpenRead($probeDll)
try {
    $reader = [IO.BinaryReader]::new($stream)
    if ($reader.ReadUInt16() -ne 0x5A4D) { throw 'GECK probe is not a PE file.' }
    $stream.Position = 0x3C
    $peOffset = $reader.ReadInt32()
    $stream.Position = $peOffset
    if ($reader.ReadUInt32() -ne 0x00004550) { throw 'GECK probe PE signature is invalid.' }
    if ($reader.ReadUInt16() -ne 0x014C) { throw 'GECK probe is not x86.' }
} finally {
    if ($null -ne $reader) { $reader.Dispose() } else { $stream.Dispose() }
}

$expectedExports = @('NVSEPlugin_Load', 'NVSEPlugin_Query')
$actualExports = @($manifest.verification.exports)
if ([string]::Join("`n", $actualExports) -ne [string]::Join("`n", $expectedExports)) {
    throw "GECK probe exports are invalid: $($actualExports -join ', ')"
}
$allowedDependents = @('ADVAPI32.DLL', 'KERNEL32.DLL')
$unexpectedDependents = @($manifest.verification.dependents | Where-Object { $_ -notin $allowedDependents })
if ($unexpectedDependents.Count -ne 0) {
    throw "GECK probe dependencies are invalid: $($unexpectedDependents -join ', ')"
}

$sourceRoot = Join-Path $repositoryRoot 'integrations\geck-probe'
$sourceFiles = @(Get-ChildItem -LiteralPath $sourceRoot -File | Select-Object -ExpandProperty Name | Sort-Object)
$expectedSourceFiles = @('exports.def', 'GeckProbe.cpp', 'README.md', 'WastelandForge.GeckProbe.vcxproj')
if ([string]::Join("`n", $sourceFiles) -ne [string]::Join("`n", $expectedSourceFiles)) {
    throw "Unexpected GECK probe source files: $($sourceFiles -join ', ')"
}
$ownedText = Get-Content -LiteralPath (Join-Path $sourceRoot 'GeckProbe.cpp') -Raw
foreach ($pattern in @($manifest.verification.forbiddenOwnedSourceTokens)) {
    if ($ownedText.Contains($pattern, [StringComparison]::Ordinal)) {
        throw "Forge-owned probe source contains forbidden API token: $pattern"
    }
}

[xml] $project = Get-Content -LiteralPath (Join-Path $sourceRoot 'WastelandForge.GeckProbe.vcxproj') -Raw
$namespace = [Xml.XmlNamespaceManager]::new($project.NameTable)
$namespace.AddNamespace('msb', 'http://schemas.microsoft.com/developer/msbuild/2003')
$configurationNodes = @($project.SelectNodes('//msb:ProjectConfiguration', $namespace))
$toolset = $project.SelectSingleNode('//msb:PlatformToolset', $namespace).InnerText
$runtime = $project.SelectSingleNode('//msb:RuntimeLibrary', $namespace).InnerText
$targetMachine = $project.SelectSingleNode('//msb:TargetMachine', $namespace).InnerText
$definitions = $project.SelectSingleNode('//msb:PreprocessorDefinitions', $namespace).InnerText
if ($configurationNodes.Count -ne 1 -or $configurationNodes[0].Include -ne 'Release GECK|Win32' -or
    $toolset -ne 'v143' -or $runtime -ne 'MultiThreaded' -or $targetMachine -ne 'MachineX86' -or
    -not $definitions.Contains('WF_GECK_PROBE_LAUNCH_AUTHORIZED=0', [StringComparison]::Ordinal)) {
    throw 'GECK probe native project does not preserve the Gate 526 build-only contract.'
}

Write-Host 'Gate 527 GECK host probe static regression passed.'
