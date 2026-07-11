[CmdletBinding()]
param(
    [string] $AppShellRoot = '',

    [string] $InstallerScript = '',

    [string] $OutputRoot = '',

    [string] $OutputBaseFilename = 'WastelandForge-Setup-local',

    [string] $InnoCompilerPath = '',

    [switch] $DetectOnly,

    [switch] $NoClean
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

function Assert-UnderRepository {
    param(
        [Parameter(Mandatory = $true)]
        [string] $Path,

        [Parameter(Mandatory = $true)]
        [string] $Purpose
    )

    $fullPath = Get-FullPathFromRepository -Path $Path
    $repositoryRootPath = [System.IO.Path]::GetFullPath($script:RepositoryRoot)
    if (-not $repositoryRootPath.EndsWith([System.IO.Path]::DirectorySeparatorChar) -and
        -not $repositoryRootPath.EndsWith([System.IO.Path]::AltDirectorySeparatorChar)) {
        $repositoryRootPath = "$repositoryRootPath$([System.IO.Path]::DirectorySeparatorChar)"
    }

    if (-not $fullPath.StartsWith($repositoryRootPath, [System.StringComparison]::OrdinalIgnoreCase)) {
        Fail "Refusing to use $Purpose outside this repository: '$fullPath'." 6
    }

    return $fullPath
}

function Assert-AllowedInstallerScript {
    param(
        [Parameter(Mandatory = $true)]
        [string] $Path
    )

    $fullPath = Assert-UnderRepository -Path $Path -Purpose 'installer script'
    $relativePath = Get-RelativePath -Root $script:RepositoryRoot -Path $fullPath
    if (-not $relativePath.StartsWith('installer/inno/', [System.StringComparison]::OrdinalIgnoreCase)) {
        Fail "Refusing to use installer script '$relativePath'. Use a script under 'installer/inno/'." 6
    }

    if (-not (Test-Path -LiteralPath $fullPath -PathType Leaf)) {
        Fail "Installer script was not found: '$fullPath'." 3
    }

    return $fullPath
}

function Assert-AllowedOutputRoot {
    param(
        [Parameter(Mandatory = $true)]
        [string] $Path
    )

    $fullPath = Assert-UnderRepository -Path $Path -Purpose 'installer output root'
    $relativePath = Get-RelativePath -Root $script:RepositoryRoot -Path $fullPath
    if (-not $relativePath.StartsWith('artifacts/installer/inno/', [System.StringComparison]::OrdinalIgnoreCase)) {
        Fail "Refusing to write installer output '$relativePath'. Use 'artifacts/installer/inno/<name>'." 6
    }

    return $fullPath
}

function Write-Utf8File {
    param(
        [Parameter(Mandatory = $true)]
        [string] $Path,

        [Parameter(Mandatory = $true)]
        [string] $Value
    )

    $encoding = [System.Text.UTF8Encoding]::new($false)
    [System.IO.File]::WriteAllText($Path, $Value, $encoding)
}

function Get-Sha256 {
    param(
        [Parameter(Mandatory = $true)]
        [string] $Path
    )

    return (Get-FileHash -LiteralPath $Path -Algorithm SHA256).Hash.ToLowerInvariant()
}

function Get-FileEntry {
    param(
        [Parameter(Mandatory = $true)]
        [System.IO.FileInfo] $File,

        [Parameter(Mandatory = $true)]
        [string] $Root
    )

    return [ordered] @{
        path = Get-RelativePath -Root $Root -Path $File.FullName
        bytes = $File.Length
        sha256 = Get-Sha256 -Path $File.FullName
    }
}

function Get-BuildTimestamp {
    $sourceDateEpoch = $env:SOURCE_DATE_EPOCH
    if (-not [string]::IsNullOrWhiteSpace($sourceDateEpoch)) {
        $epochSeconds = 0L
        if ([long]::TryParse($sourceDateEpoch, [ref] $epochSeconds)) {
            return [ordered] @{
                source = 'SOURCE_DATE_EPOCH'
                utc = [DateTimeOffset]::FromUnixTimeSeconds($epochSeconds).UtcDateTime.ToString('yyyy-MM-ddTHH:mm:ssZ')
            }
        }
    }

    return [ordered] @{
        source = 'utc-now'
        utc = [DateTimeOffset]::UtcNow.UtcDateTime.ToString('yyyy-MM-ddTHH:mm:ssZ')
    }
}

function Resolve-InnoCompiler {
    param(
        [string] $ExplicitPath
    )

    if (-not [string]::IsNullOrWhiteSpace($ExplicitPath)) {
        $fullPath = [System.IO.Path]::GetFullPath($ExplicitPath)
        if (Test-Path -LiteralPath $fullPath -PathType Leaf) {
            return $fullPath
        }

        Fail "Inno Setup compiler was not found at '$fullPath'." 5
    }

    $command = Get-Command iscc.exe -ErrorAction SilentlyContinue
    if ($null -ne $command) {
        return $command.Source
    }

    $command = Get-Command iscc -ErrorAction SilentlyContinue
    if ($null -ne $command) {
        return $command.Source
    }

    $candidates = @(
        'C:\Program Files (x86)\Inno Setup 6\ISCC.exe',
        'C:\Program Files\Inno Setup 6\ISCC.exe',
        'C:\Program Files (x86)\Inno Setup 5\ISCC.exe',
        'C:\Program Files\Inno Setup 5\ISCC.exe'
    )

    if (-not [string]::IsNullOrWhiteSpace($env:LOCALAPPDATA)) {
        $localPrograms = Join-Path $env:LOCALAPPDATA 'Programs'
        $candidates += @(
            (Join-Path (Join-Path $localPrograms 'Inno Setup 6') 'ISCC.exe'),
            (Join-Path (Join-Path $localPrograms 'Inno Setup 5') 'ISCC.exe')
        )
    }

    foreach ($candidate in $candidates) {
        if (Test-Path -LiteralPath $candidate -PathType Leaf) {
            return $candidate
        }
    }

    return ''
}

function Invoke-AppShellInputPreflight {
    param(
        [Parameter(Mandatory = $true)]
        [string] $Root
    )

    $preflightScript = Join-Path (Join-Path $script:RepositoryRoot 'eng') 'Test-AppShellInstallerInputs.ps1'
    if (-not (Test-Path -LiteralPath $preflightScript -PathType Leaf)) {
        Fail "App-shell installer input preflight script was not found at '$preflightScript'." 3
    }

    & pwsh -NoLogo -NoProfile -ExecutionPolicy Bypass -File $preflightScript -AppShellRoot $Root
    $exitCode = if ($null -eq $LASTEXITCODE) { 0 } else { [int] $LASTEXITCODE }
    if ($exitCode -ne 0) {
        Fail "App-shell installer input preflight failed with exit code $exitCode." $exitCode
    }
}

function Get-CompilerMetadata {
    param(
        [Parameter(Mandatory = $true)]
        [string] $Path
    )

    $item = Get-Item -LiteralPath $Path
    return [ordered] @{
        path = $item.FullName
        fileVersion = $item.VersionInfo.FileVersion
        productVersion = $item.VersionInfo.ProductVersion
    }
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

if ([string]::IsNullOrWhiteSpace($InstallerScript)) {
    $InstallerScript = Join-Path (Join-Path 'installer' 'inno') 'WastelandForge.iss'
}

if ([string]::IsNullOrWhiteSpace($OutputRoot)) {
    $OutputRoot = Join-Path (Join-Path (Join-Path 'artifacts' 'installer') 'inno') 'local'
}

if ([string]::IsNullOrWhiteSpace($OutputBaseFilename)) {
    Fail 'OutputBaseFilename must not be empty.' 2
}

if ($OutputBaseFilename.IndexOfAny([System.IO.Path]::GetInvalidFileNameChars()) -ge 0) {
    Fail "OutputBaseFilename contains invalid filename characters: '$OutputBaseFilename'." 2
}

$appShellRootPath = Assert-UnderRepository -Path $AppShellRoot -Purpose 'app-shell input root'
$installerScriptPath = Assert-AllowedInstallerScript -Path $InstallerScript
$outputRootPath = Assert-AllowedOutputRoot -Path $OutputRoot

Invoke-AppShellInputPreflight -Root $appShellRootPath

$compilerPath = Resolve-InnoCompiler -ExplicitPath $InnoCompilerPath
if ([string]::IsNullOrWhiteSpace($compilerPath)) {
    Write-Host 'Inno Setup compiler not found.'
    Write-Host 'Checked PATH plus standard machine and per-user Inno Setup 5/6 install locations.'
    Write-Host 'Boundary: no installer build, no signing, no timestamping, no update channel, no release publication.'
    if ($DetectOnly) {
        exit 0
    }

    Fail 'Install Inno Setup or pass -InnoCompilerPath before building a local unsigned installer.' 5
}

$compilerMetadata = Get-CompilerMetadata -Path $compilerPath
Write-Host "Inno Setup compiler found: $compilerPath"

if ($DetectOnly) {
    Write-Host 'DetectOnly requested; no installer build was run.'
    Write-Host 'Boundary: no installer build, no signing, no timestamping, no update channel, no release publication.'
    exit 0
}

if ((Test-Path -LiteralPath $outputRootPath) -and (-not $NoClean)) {
    Remove-Item -LiteralPath $outputRootPath -Recurse -Force
}

New-Item -ItemType Directory -Force -Path $outputRootPath | Out-Null

$compilerArguments = @(
    "/DAppShellSource=$appShellRootPath",
    "/DInstallerOutputDir=$outputRootPath",
    "/DInstallerOutputBaseFilename=$OutputBaseFilename",
    $installerScriptPath
)

Write-Host 'Building unsigned local WastelandForge installer...'
& $compilerPath @compilerArguments
$compilerExitCode = if ($null -eq $LASTEXITCODE) { 0 } else { [int] $LASTEXITCODE }
if ($compilerExitCode -ne 0) {
    Fail "Inno Setup compiler failed with exit code $compilerExitCode." 5
}

$installerPath = Join-Path $outputRootPath "$OutputBaseFilename.exe"
if (-not (Test-Path -LiteralPath $installerPath -PathType Leaf)) {
    Fail "Expected installer executable was not produced at '$installerPath'." 5
}

$manifestPath = Join-Path $outputRootPath 'installer-build-manifest.json'
$checksumsPath = Join-Path $outputRootPath 'checksums.sha256'
$readmePath = Join-Path $outputRootPath 'README.txt'

$readme = @"
WastelandForge unsigned local installer

Generated by eng/Build-AppShellInstaller.ps1 from this source repository.

Installer:
  $OutputBaseFilename.exe

Source app shell:
  $(Get-RelativePath -Root $script:RepositoryRoot -Path $appShellRootPath)

This Gate 344 output is a local unsigned installer build helper artifact. It
is not signed, not timestamped, not an update channel, not an attestation, and
not a published release.

Boundary:
  local unsigned installer only
  no signing
  no timestamping
  no attestation
  no update channel
  no release publication
  no external game tool execution
  no runtime probes
  no AI requirement
"@

Write-Utf8File -Path $readmePath -Value ($readme.TrimEnd() + [Environment]::NewLine)

$payloadFiles = @(
    Get-ChildItem -LiteralPath $outputRootPath -File -Recurse |
        Where-Object {
            -not $_.FullName.Equals($manifestPath, [System.StringComparison]::OrdinalIgnoreCase) -and
            -not $_.FullName.Equals($checksumsPath, [System.StringComparison]::OrdinalIgnoreCase)
        } |
        Sort-Object @{ Expression = { Get-RelativePath -Root $outputRootPath -Path $_.FullName } }
)

$manifest = [ordered] @{
    schema = 'wastelandforge.local-app-shell-installer.v1'
    kind = 'wastelandforge.localAppShellInstaller'
    gate = 344
    status = 'complete'
    build = [ordered] @{
        timestamp = Get-BuildTimestamp
        unsigned = $true
        signed = $false
        timestamped = $false
        releasePublished = $false
    }
    compiler = $compilerMetadata
    source = [ordered] @{
        repositoryRoot = $script:RepositoryRoot
        installerScript = Get-RelativePath -Root $script:RepositoryRoot -Path $installerScriptPath
        appShellRoot = Get-RelativePath -Root $script:RepositoryRoot -Path $appShellRootPath
        appShellManifest = 'app-build-manifest.json'
        appShellChecksums = 'checksums.sha256'
    }
    installer = [ordered] @{
        outputRoot = Get-RelativePath -Root $script:RepositoryRoot -Path $outputRootPath
        executable = "$OutputBaseFilename.exe"
    }
    boundaries = [ordered] @{
        installerCreated = $true
        signing = $false
        timestamping = $false
        attestation = $false
        updateChannel = $false
        releasePublication = $false
        nugetPublication = $false
        packageRestorePolicyChanged = $false
        generatedConsumerTemplateMutation = $false
        externalGameToolExecution = $false
        runtimeProbes = $false
        heatSourceAssetCommit = $false
        aiRequired = $false
    }
    outputs = @($payloadFiles | ForEach-Object { Get-FileEntry -File $_ -Root $outputRootPath })
}

Write-Utf8File -Path $manifestPath -Value (($manifest | ConvertTo-Json -Depth 12) + [Environment]::NewLine)

$checksumFiles = @(
    Get-ChildItem -LiteralPath $outputRootPath -File -Recurse |
        Where-Object { -not $_.FullName.Equals($checksumsPath, [System.StringComparison]::OrdinalIgnoreCase) } |
        Sort-Object @{ Expression = { Get-RelativePath -Root $outputRootPath -Path $_.FullName } }
)

$checksumLines = @($checksumFiles | ForEach-Object {
    $relativePath = Get-RelativePath -Root $outputRootPath -Path $_.FullName
    "$(Get-Sha256 -Path $_.FullName)  $relativePath"
})

Write-Utf8File -Path $checksumsPath -Value (($checksumLines -join [Environment]::NewLine) + [Environment]::NewLine)

Write-Host "WastelandForge unsigned local installer complete: $installerPath"
Write-Host 'Boundary: unsigned local installer only; no signing, timestamping, update channel, release publication, runtime probes, or AI.'
