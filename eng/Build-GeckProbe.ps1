[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [string] $XnvseSourceRoot,
    [string] $OutputRoot = 'generated/geck-probe'
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$expectedCommit = '694cdde6cbfa5e75afa661df587c73e8f0f6f441'
$expectedTree = 'e80453c217027f47979d2dfca03665de0a93fa6f'
$componentId = 'Microsoft.VisualStudio.Component.VC.14.44.17.14.x86.x64'
$vcToolsVersion = '14.44.35207'
$repositoryRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$projectPath = Join-Path $repositoryRoot 'integrations\geck-probe\WastelandForge.GeckProbe.vcxproj'
$sourceRoot = [IO.Path]::GetFullPath($XnvseSourceRoot)
$outputRootPath = if ([IO.Path]::IsPathRooted($OutputRoot)) {
    [IO.Path]::GetFullPath($OutputRoot)
} else {
    [IO.Path]::GetFullPath((Join-Path $repositoryRoot $OutputRoot))
}
$expectedOutputRoot = [IO.Path]::GetFullPath((Join-Path $repositoryRoot 'generated\geck-probe'))
if (-not $outputRootPath.Equals($expectedOutputRoot, [StringComparison]::OrdinalIgnoreCase)) {
    throw "Refusing GECK probe output outside generated/geck-probe: $outputRootPath"
}

function Invoke-Git([string[]] $Arguments) {
    $result = & git -C $sourceRoot @Arguments
    if ($LASTEXITCODE -ne 0) {
        throw "git $($Arguments -join ' ') failed for xNVSE source root."
    }
    return ($result -join "`n").Trim()
}

if (-not (Test-Path -LiteralPath (Join-Path $sourceRoot '.git'))) {
    throw "xNVSE source root must be a Git checkout: $sourceRoot"
}
$actualCommit = Invoke-Git @('rev-parse', 'HEAD')
$actualTree = Invoke-Git @('rev-parse', 'HEAD^{tree}')
$sourceStatus = Invoke-Git @('status', '--porcelain=v1', '--untracked-files=all')
if ($actualCommit -ne $expectedCommit) {
    throw "xNVSE commit mismatch. Expected $expectedCommit, got $actualCommit."
}
if ($actualTree -ne $expectedTree) {
    throw "xNVSE tree mismatch. Expected $expectedTree, got $actualTree."
}
if ($sourceStatus.Length -ne 0) {
    throw "xNVSE checkout is not clean:`n$sourceStatus"
}

$vswhere = 'C:\Program Files (x86)\Microsoft Visual Studio\Installer\vswhere.exe'
if (-not (Test-Path -LiteralPath $vswhere -PathType Leaf)) {
    throw 'Visual Studio Installer vswhere.exe was not found.'
}
$visualStudioRoot = (& $vswhere -latest -products * -requires $componentId -property installationPath).Trim()
if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace($visualStudioRoot)) {
    throw "Required MSVC v143 component is not installed: $componentId"
}
$msbuild = Join-Path $visualStudioRoot 'MSBuild\Current\Bin\MSBuild.exe'
$toolRoot = Join-Path $visualStudioRoot "VC\Tools\MSVC\$vcToolsVersion"
$compiler = Join-Path $toolRoot 'bin\Hostx64\x86\cl.exe'
$dumpbin = Join-Path $toolRoot 'bin\Hostx64\x86\dumpbin.exe'
$platformToolset = Join-Path $visualStudioRoot 'MSBuild\Microsoft\VC\v170\Platforms\Win32\PlatformToolsets\v143\Toolset.props'
foreach ($requiredPath in @($msbuild, $compiler, $dumpbin, $platformToolset, $projectPath)) {
    if (-not (Test-Path -LiteralPath $requiredPath -PathType Leaf)) {
        throw "Required Gate 527 build input is missing: $requiredPath"
    }
}

$ownedSource = Join-Path $repositoryRoot 'integrations\geck-probe\GeckProbe.cpp'
$forbiddenPatterns = @(
    'TESForm', 'TESObjectREFR', 'SetOpcodeBase', 'RegisterCommand',
    'QueryInterface', 'CreateProcess', 'ShellExecute', 'WinHttp', 'WinInet',
    'URLDownload', 'SendInput', 'keybd_event', 'mouse_event'
)
$ownedText = [IO.File]::ReadAllText($ownedSource)
foreach ($pattern in $forbiddenPatterns) {
    if ($ownedText.Contains($pattern, [StringComparison]::Ordinal)) {
        throw "Forge-owned probe source contains forbidden API token: $pattern"
    }
}

if (Test-Path -LiteralPath $outputRootPath) {
    Remove-Item -LiteralPath $outputRootPath -Recurse -Force
}
$binaryRoot = Join-Path $outputRootPath 'bin'
$intermediateRoot = Join-Path $outputRootPath 'obj'
New-Item -ItemType Directory -Force -Path $binaryRoot, $intermediateRoot | Out-Null

$buildLog = Join-Path $outputRootPath 'msbuild.log'
$arguments = @(
    $projectPath,
    '/nologo',
    '/m:1',
    '/t:Rebuild',
    '/v:minimal',
    '/p:Configuration=Release GECK',
    '/p:Platform=Win32',
    "/p:XnvseSourceRoot=$sourceRoot",
    "/p:VCToolsVersion=$vcToolsVersion",
    "/p:OutDir=$binaryRoot\",
    "/p:IntDir=$intermediateRoot\",
    "/flp:logfile=$buildLog;verbosity=normal;encoding=UTF-8"
)
$processInfo = [Diagnostics.ProcessStartInfo]::new($msbuild)
$processInfo.UseShellExecute = $false
$processInfo.RedirectStandardOutput = $true
$processInfo.RedirectStandardError = $true
$processInfo.Environment.Clear()
$processEnvironment = [Environment]::GetEnvironmentVariables('Process')
$pathValue = if (-not [string]::IsNullOrWhiteSpace([string] $processEnvironment['PATH'])) {
    [string] $processEnvironment['PATH']
} else {
    [string] $processEnvironment['Path']
}
$environmentNames = [Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
foreach ($name in $processEnvironment.Keys) {
    $textName = [string] $name
    if ($textName.Equals('Path', [StringComparison]::OrdinalIgnoreCase)) { continue }
    if ($environmentNames.Add($textName)) {
        $processInfo.Environment[$textName] = [string] $processEnvironment[$name]
    }
}
$processInfo.Environment['Path'] = $pathValue
foreach ($argument in $arguments) {
    $processInfo.ArgumentList.Add($argument)
}
$process = [Diagnostics.Process]::Start($processInfo)
$standardOutput = $process.StandardOutput.ReadToEndAsync()
$standardError = $process.StandardError.ReadToEndAsync()
$process.WaitForExit()
$outputText = $standardOutput.GetAwaiter().GetResult()
$errorText = $standardError.GetAwaiter().GetResult()
if (-not [string]::IsNullOrWhiteSpace($outputText)) { Write-Host $outputText.TrimEnd() }
if (-not [string]::IsNullOrWhiteSpace($errorText)) { Write-Host $errorText.TrimEnd() }
if ($process.ExitCode -ne 0) {
    throw "GECK probe build failed with exit code $($process.ExitCode)."
}

$probeDll = Join-Path $binaryRoot 'WastelandForge.GeckProbe.dll'
if (-not (Test-Path -LiteralPath $probeDll -PathType Leaf)) {
    throw "GECK probe DLL was not produced: $probeDll"
}
$headersReport = Join-Path $outputRootPath 'dumpbin-headers.txt'
$exportsReport = Join-Path $outputRootPath 'dumpbin-exports.txt'
$dependentsReport = Join-Path $outputRootPath 'dumpbin-dependents.txt'
(& $dumpbin /headers $probeDll) | Set-Content -LiteralPath $headersReport -Encoding utf8NoBOM
if ($LASTEXITCODE -ne 0) { throw 'dumpbin /headers failed.' }
(& $dumpbin /exports $probeDll) | Set-Content -LiteralPath $exportsReport -Encoding utf8NoBOM
if ($LASTEXITCODE -ne 0) { throw 'dumpbin /exports failed.' }
(& $dumpbin /dependents $probeDll) | Set-Content -LiteralPath $dependentsReport -Encoding utf8NoBOM
if ($LASTEXITCODE -ne 0) { throw 'dumpbin /dependents failed.' }

$headersText = Get-Content -LiteralPath $headersReport -Raw
if ($headersText -notmatch '(?im)^\s*14C machine \(x86\)') {
    throw 'Static verification did not identify an x86 DLL.'
}
$exportsText = Get-Content -LiteralPath $exportsReport -Raw
$expectedExports = @('NVSEPlugin_Load', 'NVSEPlugin_Query')
$actualExports = @(
    [regex]::Matches($exportsText, '(?im)^\s+\d+\s+[0-9A-F]+\s+[0-9A-F]+\s+(NVSEPlugin_[A-Za-z]+)\s*$') |
        ForEach-Object { $_.Groups[1].Value } |
        Sort-Object
)
if ([string]::Join("`n", $actualExports) -ne [string]::Join("`n", $expectedExports)) {
    throw "Unexpected exported symbols: $($actualExports -join ', ')"
}
$dependentsText = Get-Content -LiteralPath $dependentsReport -Raw
$actualDependents = @(
    [regex]::Matches($dependentsText, '(?im)^\s+([A-Z0-9._-]+\.dll)\s*$') |
        ForEach-Object { $_.Groups[1].Value.ToUpperInvariant() } |
        Sort-Object -Unique
)
$allowedDependents = @('ADVAPI32.DLL', 'KERNEL32.DLL')
$unexpectedDependents = @($actualDependents | Where-Object { $_ -notin $allowedDependents })
if ($unexpectedDependents.Count -ne 0) {
    throw "Unexpected DLL dependencies: $($unexpectedDependents -join ', ')"
}

function Get-Evidence([string] $Path, [string] $Base) {
    $item = Get-Item -LiteralPath $Path
    [ordered]@{
        path = [IO.Path]::GetRelativePath($Base, $item.FullName).Replace('\', '/')
        length = $item.Length
        sha256 = (Get-FileHash -LiteralPath $item.FullName -Algorithm SHA256).Hash.ToLowerInvariant()
    }
}

$apiHeader = Join-Path $sourceRoot 'nvse\nvse\PluginAPI.h'
$versionHeader = Join-Path $sourceRoot 'nvse\nvse\nvse_version.h'
$manifest = [ordered]@{
    formatVersion = '0.1'
    kind = 'wastelandforge.geck-host-probe-build'
    probeVersion = '0.1.0'
    configuration = 'Release GECK'
    platform = 'Win32'
    platformToolset = 'v143'
    externalSource = [ordered]@{
        name = 'xNVSE'
        version = '6.4.4'
        commit = $actualCommit
        tree = $actualTree
        clean = $true
        checkoutRoot = $sourceRoot
        inputs = @(
            Get-Evidence $apiHeader $sourceRoot
            Get-Evidence $versionHeader $sourceRoot
        )
    }
    toolchain = [ordered]@{
        componentId = $componentId
        visualStudioRoot = $visualStudioRoot
        vcToolsVersion = $vcToolsVersion
        compiler = Get-Evidence $compiler $visualStudioRoot
        msbuild = Get-Evidence $msbuild $visualStudioRoot
    }
    ownedInputs = @(
        Get-Evidence $ownedSource $repositoryRoot
        Get-Evidence $projectPath $repositoryRoot
        Get-Evidence (Join-Path $repositoryRoot 'integrations\geck-probe\exports.def') $repositoryRoot
    )
    output = Get-Evidence $probeDll $outputRootPath
    verification = [ordered]@{
        machine = 'x86'
        exports = $actualExports
        dependents = $actualDependents
        reports = @(
            Get-Evidence $headersReport $outputRootPath
            Get-Evidence $exportsReport $outputRootPath
            Get-Evidence $dependentsReport $outputRootPath
        )
        forbiddenOwnedSourceTokens = $forbiddenPatterns
        forbiddenOwnedSourceTokensFound = @()
    }
    launchAuthorized = $false
    approvedProbeRunId = 'build-only-unapproved'
    mo2Staged = $false
    geckExecuted = $false
    runtimeObservationProduced = $false
    recordApisUsed = $false
    pluginMutation = $false
    physicalDataWritten = $false
    bundledThirdPartySource = $false
    bundledThirdPartyBinaries = $false
}
$utf8 = [Text.UTF8Encoding]::new($false)
$manifestPath = Join-Path $outputRootPath 'build-manifest.json'
[IO.File]::WriteAllText($manifestPath, ($manifest | ConvertTo-Json -Depth 12) + "`n", $utf8)

Write-Host "Gate 527 GECK host probe build complete: $probeDll"
Write-Host "SHA-256: $($manifest.output.sha256)"
Write-Host 'Launch authorized: false'
