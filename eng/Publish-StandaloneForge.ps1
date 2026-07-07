[CmdletBinding()]
param(
    [ValidateSet('Debug', 'Release')]
    [string] $Configuration = 'Release',

    [string] $RuntimeIdentifier = '',

    [string] $OutputRoot = '',

    [switch] $NoClean,

    [switch] $SelfContained,

    [switch] $SkipSmoke
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

function Assert-AllowedOutputRoot {
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
        Fail "Refusing to publish outside this repository: '$fullPath'." 6
    }

    $relativePath = Get-RelativePath -Root $script:RepositoryRoot -Path $fullPath
    $isDefaultDistribution = $relativePath.Equals('dist/local/forge', [System.StringComparison]::OrdinalIgnoreCase)
    $isStandaloneArtifact = $relativePath.StartsWith('artifacts/standalone-forge/', [System.StringComparison]::OrdinalIgnoreCase)

    if (-not $isDefaultDistribution -and -not $isStandaloneArtifact) {
        Fail "Refusing to clean or write '$relativePath'. Use 'dist/local/forge' or 'artifacts/standalone-forge/<name>'." 6
    }

    return $fullPath
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

$scriptRoot = if ([string]::IsNullOrWhiteSpace($PSScriptRoot)) {
    Split-Path -Parent $PSCommandPath
}
else {
    $PSScriptRoot
}

$script:RepositoryRoot = (Resolve-Path -LiteralPath (Join-Path $scriptRoot '..')).Path
$projectPath = Join-Path (Join-Path $script:RepositoryRoot 'src') (Join-Path 'WastelandForge.Cli' 'WastelandForge.Cli.csproj')

if ([string]::IsNullOrWhiteSpace($OutputRoot)) {
    $OutputRoot = Join-Path (Join-Path 'dist' 'local') 'forge'
}

if (-not (Test-Path -LiteralPath $projectPath -PathType Leaf)) {
    Fail "WastelandForge CLI project was not found at '$projectPath'." 3
}

$dotnetCommand = Get-Command dotnet -ErrorAction SilentlyContinue
if ($null -eq $dotnetCommand) {
    Fail 'The dotnet SDK was not found on PATH. Install the SDK pinned by global.json before publishing standalone Forge.' 5
}

$script:DotNetPath = $dotnetCommand.Source
$outputRootPath = Assert-AllowedOutputRoot -Path $OutputRoot
$manifestPath = Join-Path $outputRootPath 'build-manifest.json'
$readmePath = Join-Path $outputRootPath 'README.txt'
$checksumsPath = Join-Path $outputRootPath 'checksums.sha256'
$executablePath = Join-Path $outputRootPath 'forge.exe'
$exitCode = 0

Push-Location $script:RepositoryRoot
try {
    if ((Test-Path -LiteralPath $outputRootPath) -and (-not $NoClean)) {
        Remove-Item -LiteralPath $outputRootPath -Recurse -Force
    }

    New-Item -ItemType Directory -Force -Path $outputRootPath | Out-Null

    if ($SelfContained -and [string]::IsNullOrWhiteSpace($RuntimeIdentifier)) {
        $RuntimeIdentifier = 'win-x64'
    }

    $runtimeLabel = if ([string]::IsNullOrWhiteSpace($RuntimeIdentifier)) { 'current-platform-apphost' } else { $RuntimeIdentifier }
    $selfContainedValue = if ($SelfContained) { 'true' } else { 'false' }
    $publishArguments = @(
        'publish',
        $projectPath,
        '-c',
        $Configuration,
        '--self-contained',
        $selfContainedValue,
        '--no-restore',
        '-o',
        $outputRootPath,
        '/p:UseAppHost=true',
        '/p:DebugType=embedded',
        '/p:ContinuousIntegrationBuild=true')

    if (-not [string]::IsNullOrWhiteSpace($RuntimeIdentifier)) {
        $publishArguments += @('-r', $RuntimeIdentifier)
    }

    if ($SelfContained) {
        $publishArguments += @('/p:PublishSingleFile=true', '/p:IncludeNativeLibrariesForSelfExtract=true')
    }

    Write-Host "Publishing standalone Forge CLI ($Configuration, $runtimeLabel, self-contained: $SelfContained)..."
    Invoke-DotNet -Arguments $publishArguments

    $defaultExecutablePath = Join-Path $outputRootPath 'WastelandForge.Cli.exe'
    if ((Test-Path -LiteralPath $defaultExecutablePath -PathType Leaf) -and
        (-not $defaultExecutablePath.Equals($executablePath, [System.StringComparison]::OrdinalIgnoreCase))) {
        if (Test-Path -LiteralPath $executablePath -PathType Leaf) {
            Remove-Item -LiteralPath $executablePath -Force
        }

        Move-Item -LiteralPath $defaultExecutablePath -Destination $executablePath
    }

    if (-not (Test-Path -LiteralPath $executablePath -PathType Leaf)) {
        Fail "Expected standalone executable was not produced at '$executablePath'." 5
    }

    $smokeVersion = 'skipped'
    if (-not $SkipSmoke) {
        Write-Host 'Verifying standalone forge.exe...'
        $versionOutput = & $executablePath '--version'
        $smokeExitCode = if ($null -eq $LASTEXITCODE) { 0 } else { [int] $LASTEXITCODE }
        if ($smokeExitCode -ne 0) {
            Fail "Standalone forge.exe --version failed with exit code $smokeExitCode." 5
        }

        $smokeVersion = (($versionOutput | Out-String).Trim())
    }

    $readme = @"
WastelandForge standalone Forge CLI

Generated by eng/Publish-StandaloneForge.ps1 from this source repository.

Executable:
  forge.exe

Try:
  .\forge.exe help
  .\forge.exe validate <project> --format plain --no-input
  .\forge.exe capabilities list --format plain --no-input

Generated Forge projects expect a forge command. To satisfy that prerequisite
without NuGet publication, add this folder to PATH or set FORGE_COMMAND to this
forge.exe path.

This default Gate 338 scaffold is framework-dependent and expects the .NET
runtime used by WastelandForge. Use -SelfContained only after the repository has
already been restored for the selected runtime identifier; this helper still
uses --no-restore.

Boundary:
  no installer
  no package restore
  no NuGet publication
  no signing or attestation
  no update channel
  no WastelandForge.exe app-shell packaging
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
        schema = 'wastelandforge.local-standalone-forge-distribution.v1'
        kind = 'wastelandforge.localStandaloneForgeDistribution'
        gate = 338
        status = 'complete'
        build = [ordered] @{
            configuration = $Configuration
            runtimeIdentifier = $runtimeLabel
            selfContained = [bool] $SelfContained
            frameworkDependent = -not [bool] $SelfContained
            publishSingleFile = [bool] $SelfContained
            noRestore = $true
            timestamp = Get-BuildTimestamp
        }
        source = [ordered] @{
            repositoryRoot = $script:RepositoryRoot
            project = Get-RelativePath -Root $script:RepositoryRoot -Path $projectPath
        }
        command = [ordered] @{
            executable = 'forge.exe'
            versionSmoke = $smokeVersion
        }
        boundaries = [ordered] @{
            installerCreated = $false
            packageRestore = $false
            nugetPublication = $false
            signing = $false
            attestation = $false
            updateChannel = $false
            generatedConsumerTemplateMutation = $false
            appShellPackaging = $false
            externalGameToolExecution = $false
            runtimeProbes = $false
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

    Write-Host "Standalone Forge distribution complete: $outputRootPath"
    Write-Host "Run: $executablePath help"
}
catch {
    [Console]::Error.WriteLine($_.Exception.Message)
    $exitCode = 5
}
finally {
    Pop-Location
}

exit $exitCode
