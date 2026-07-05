[CmdletBinding()]
param(
    [ValidateSet('Debug', 'Release')]
    [string] $Configuration = 'Release',

    [string] $RestoreRoot = '',

    [switch] $NoVerify
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

function Get-RepoPath {
    param(
        [Parameter(Mandatory = $true)]
        [string] $Path
    )

    if ([System.IO.Path]::IsPathRooted($Path)) {
        return [System.IO.Path]::GetFullPath($Path)
    }

    return [System.IO.Path]::GetFullPath((Join-Path $script:RepositoryRoot $Path))
}

function Invoke-DotNet {
    param(
        [Parameter(Mandatory = $true)]
        [string[]] $Arguments
    )

    & $script:DotNetPath @Arguments
    $exitCode = if ($null -eq $LASTEXITCODE) { 0 } else { [int] $LASTEXITCODE }
    if ($exitCode -ne 0) {
        throw "dotnet $($Arguments -join ' ') failed with exit code $exitCode."
    }
}

$scriptRoot = if ([string]::IsNullOrWhiteSpace($PSScriptRoot)) {
    Split-Path -Parent $PSCommandPath
}
else {
    $PSScriptRoot
}

$script:RepositoryRoot = (Resolve-Path -LiteralPath (Join-Path $scriptRoot '..')).Path
$projectPath = Join-Path (Join-Path $script:RepositoryRoot 'src') (Join-Path 'WastelandForge.Cli' 'WastelandForge.Cli.csproj')
$toolManifestPath = Join-Path (Join-Path $script:RepositoryRoot '.config') 'dotnet-tools.json'
$packageSourcePath = Join-Path (Join-Path (Join-Path $script:RepositoryRoot 'artifacts') 'local-tool') 'nupkg'

if ([string]::IsNullOrWhiteSpace($RestoreRoot)) {
    $RestoreRoot = Join-Path (Join-Path (Join-Path 'artifacts' 'local-tool') 'restore') 'gate-329'
}

$restoreRootPath = Get-RepoPath $RestoreRoot
$nuGetConfigPath = Join-Path $restoreRootPath 'NuGet.config'
$packageCachePath = Join-Path $restoreRootPath 'packages'

if (-not (Test-Path -LiteralPath $projectPath -PathType Leaf)) {
    Fail "WastelandForge CLI project was not found at '$projectPath'." 3
}

if (-not (Test-Path -LiteralPath $toolManifestPath -PathType Leaf)) {
    Fail "Forge local tool manifest was not found at '$toolManifestPath'." 3
}

$dotnetCommand = Get-Command dotnet -ErrorAction SilentlyContinue
if ($null -eq $dotnetCommand) {
    Fail 'The dotnet SDK was not found on PATH. Install the SDK pinned by global.json before restoring the Forge local tool.' 5
}

$script:DotNetPath = $dotnetCommand.Source
$previousNuGetPackages = $env:NUGET_PACKAGES
$exitCode = 0

Push-Location $script:RepositoryRoot
try {
    New-Item -ItemType Directory -Force -Path $restoreRootPath | Out-Null
    New-Item -ItemType Directory -Force -Path $packageCachePath | Out-Null

    Write-Host "Packing WastelandForge.Cli ($Configuration)..."
    Invoke-DotNet -Arguments @('pack', $projectPath, '-c', $Configuration)

    if (-not (Test-Path -LiteralPath $packageSourcePath -PathType Container)) {
        throw "Local package source was not created at '$packageSourcePath'."
    }

    $packageFiles = @(Get-ChildItem -LiteralPath $packageSourcePath -Filter 'WastelandForge.Cli.*.nupkg' -File)
    if ($packageFiles.Count -eq 0) {
        throw "No WastelandForge.Cli package was found under '$packageSourcePath'."
    }

    $escapedPackageSourcePath = [System.Security.SecurityElement]::Escape($packageSourcePath)
    $nuGetConfig = @"
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear />
    <add key="wastelandforge-local-tool" value="$escapedPackageSourcePath" />
  </packageSources>
</configuration>
"@
    Set-Content -LiteralPath $nuGetConfigPath -Value $nuGetConfig -Encoding utf8

    $env:NUGET_PACKAGES = $packageCachePath

    Write-Host 'Restoring Forge local tool from local package output...'
    Invoke-DotNet -Arguments @(
        'tool',
        'restore',
        '--tool-manifest',
        $toolManifestPath,
        '--configfile',
        $nuGetConfigPath,
        '--no-cache')

    if (-not $NoVerify) {
        Write-Host 'Verifying restored Forge local tool...'
        Invoke-DotNet -Arguments @('tool', 'run', 'forge', '--', '--version')
    }

    Write-Host "Forge local tool restore complete. Package cache: $packageCachePath"
}
catch {
    [Console]::Error.WriteLine($_.Exception.Message)
    $exitCode = 5
}
finally {
    $env:NUGET_PACKAGES = $previousNuGetPackages
    Pop-Location
}

exit $exitCode
