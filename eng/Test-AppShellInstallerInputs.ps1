[CmdletBinding()]
param(
    [string] $AppShellRoot = ''
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

function Fail {
    param(
        [Parameter(Mandatory = $true)]
        [string] $Message,

        [Parameter(Mandatory = $true)]
        [int] $Code
    )

    [Console]::Error.WriteLine($Message)
    exit $Code
}

function Get-FullPathFromRepository {
    param(
        [Parameter(Mandatory = $true)]
        [string] $Path
    )

    if ([System.IO.Path]::IsPathRooted($Path)) {
        return [System.IO.Path]::GetFullPath($Path)
    }

    return [System.IO.Path]::GetFullPath((Join-Path $script:RepositoryRoot $Path))
}

function Get-RelativePath {
    param(
        [Parameter(Mandatory = $true)]
        [string] $Root,

        [Parameter(Mandatory = $true)]
        [string] $Path
    )

    $rootPath = [System.IO.Path]::GetFullPath($Root)
    if (-not $rootPath.EndsWith([System.IO.Path]::DirectorySeparatorChar) -and
        -not $rootPath.EndsWith([System.IO.Path]::AltDirectorySeparatorChar)) {
        $rootPath = "$rootPath$([System.IO.Path]::DirectorySeparatorChar)"
    }

    $rootUri = [Uri] $rootPath
    $pathUri = [Uri] ([System.IO.Path]::GetFullPath($Path))
    return ([Uri]::UnescapeDataString($rootUri.MakeRelativeUri($pathUri).ToString())).Replace('\', '/')
}

function Assert-AllowedAppShellRoot {
    param(
        [Parameter(Mandatory = $true)]
        [string] $Path
    )

    $fullPath = Get-FullPathFromRepository -Path $Path
    $repositoryRootPath = [System.IO.Path]::GetFullPath($script:RepositoryRoot)
    if (-not $repositoryRootPath.EndsWith([System.IO.Path]::DirectorySeparatorChar) -and
        -not $repositoryRootPath.EndsWith([System.IO.Path]::AltDirectorySeparatorChar)) {
        $repositoryRootPath = "$repositoryRootPath$([System.IO.Path]::DirectorySeparatorChar)"
    }

    if (-not $fullPath.StartsWith($repositoryRootPath, [System.StringComparison]::OrdinalIgnoreCase)) {
        Fail "Refusing to validate installer input outside this repository: '$fullPath'." 6
    }

    $relativePath = Get-RelativePath -Root $script:RepositoryRoot -Path $fullPath
    $isDefaultDistribution = $relativePath.Equals('dist/app/WastelandForge.Desktop', [System.StringComparison]::OrdinalIgnoreCase)
    $isAppShellArtifact = $relativePath.StartsWith('artifacts/app-shell/', [System.StringComparison]::OrdinalIgnoreCase)

    if (-not $isDefaultDistribution -and -not $isAppShellArtifact) {
        Fail "Refusing to validate '$relativePath'. Use 'dist/app/WastelandForge.Desktop' or 'artifacts/app-shell/<name>'." 6
    }

    return $fullPath
}

function Get-Sha256 {
    param(
        [Parameter(Mandatory = $true)]
        [string] $Path
    )

    return (Get-FileHash -LiteralPath $Path -Algorithm SHA256).Hash.ToLowerInvariant()
}

function Assert-RequiredFile {
    param(
        [Parameter(Mandatory = $true)]
        [string] $Root,

        [Parameter(Mandatory = $true)]
        [string] $RelativePath
    )

    $path = Join-Path $Root ($RelativePath -replace '/', [System.IO.Path]::DirectorySeparatorChar)
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        Fail "Required installer input is missing: '$RelativePath'." 3
    }

    return $path
}

function Read-ChecksumEntries {
    param(
        [Parameter(Mandatory = $true)]
        [string] $Path
    )

    $entries = @{}
    $lineNumber = 0
    foreach ($line in [System.IO.File]::ReadLines($Path)) {
        $lineNumber++
        if ([string]::IsNullOrWhiteSpace($line)) {
            continue
        }

        $match = [regex]::Match($line, '^(?<hash>[0-9a-fA-F]{64})  (?<path>.+)$')
        if (-not $match.Success) {
            Fail "Malformed checksum entry at checksums.sha256 line $lineNumber." 5
        }

        $relativePath = $match.Groups['path'].Value.Replace('\', '/')
        if ($entries.ContainsKey($relativePath)) {
            Fail "Duplicate checksum entry for '$relativePath'." 5
        }

        $entries[$relativePath] = $match.Groups['hash'].Value.ToLowerInvariant()
    }

    return $entries
}

$scriptRoot = if ([string]::IsNullOrWhiteSpace($PSScriptRoot)) {
    Split-Path -Parent $PSCommandPath
}
else {
    $PSScriptRoot
}

$script:RepositoryRoot = (Resolve-Path -LiteralPath (Join-Path $scriptRoot '..')).Path

if ([string]::IsNullOrWhiteSpace($AppShellRoot)) {
    $AppShellRoot = Join-Path (Join-Path 'dist' 'app') 'WastelandForge.Desktop'
}

$appShellRootPath = Assert-AllowedAppShellRoot -Path $AppShellRoot

if (-not (Test-Path -LiteralPath $appShellRootPath -PathType Container)) {
    Fail "App-shell installer input folder was not found: '$appShellRootPath'." 3
}

$requiredPaths = @(
    'WastelandForge.exe',
    'ForgeBackend/forge.exe',
    'DemoProjects/ExampleMod/wastelandforge.json',
    'app-build-manifest.json',
    'checksums.sha256'
)

$resolvedRequiredFiles = @{}
foreach ($relativePath in $requiredPaths) {
    $resolvedRequiredFiles[$relativePath] = Assert-RequiredFile -Root $appShellRootPath -RelativePath $relativePath
}

$manifest = Get-Content -Raw -LiteralPath $resolvedRequiredFiles['app-build-manifest.json'] | ConvertFrom-Json

if ($manifest.schema -ne 'wastelandforge.local-app-shell-distribution.v1') {
    Fail "Unexpected app-build-manifest schema '$($manifest.schema)'." 5
}

if ($manifest.kind -ne 'wastelandforge.localAppShellDistribution') {
    Fail "Unexpected app-build-manifest kind '$($manifest.kind)'." 5
}

if ($manifest.boundaries.installerCreated -ne $false) {
    Fail 'App-shell manifest must report installerCreated=false before installer scaffolding consumes it.' 5
}

if ($manifest.boundaries.signing -ne $false -or
    $manifest.boundaries.updateChannel -ne $false -or
    $manifest.boundaries.releasePublication -ne $false -or
    $manifest.boundaries.aiRequired -ne $false) {
    Fail 'App-shell manifest boundary flags must keep signing, update channel, release publication, and AI disabled.' 5
}

$checksumEntries = Read-ChecksumEntries -Path $resolvedRequiredFiles['checksums.sha256']

foreach ($relativePath in $requiredPaths | Where-Object { $_ -ne 'checksums.sha256' }) {
    if (-not $checksumEntries.ContainsKey($relativePath)) {
        Fail "checksums.sha256 does not cover required input '$relativePath'." 5
    }

    $actualHash = Get-Sha256 -Path $resolvedRequiredFiles[$relativePath]
    if ($checksumEntries[$relativePath] -ne $actualHash) {
        Fail "Checksum mismatch for required input '$relativePath'." 5
    }
}

Write-Host 'WastelandForge app-shell installer input preflight passed.'
Write-Host "Input: $appShellRootPath"
Write-Host 'Boundary: no installer build, no signing, no update channel, no release publication, no runtime probes, no AI.'
