[CmdletBinding()]
param(
    [ValidateSet('Debug', 'Release')]
    [string] $Configuration = 'Release',

    [string] $RuntimeIdentifier = '',

    [string] $OutputRoot = '',

    [string] $BackendRoot = '',

    [string] $HeatSourceRoot = '',

    [switch] $NoClean,

    [switch] $SelfContained,

    [switch] $SkipBackendPublish,

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
    $isDefaultDistribution = $relativePath.Equals('dist/app/WastelandForge.Desktop', [System.StringComparison]::OrdinalIgnoreCase)
    $isAppShellArtifact = $relativePath.StartsWith('artifacts/app-shell/', [System.StringComparison]::OrdinalIgnoreCase)

    if (-not $isDefaultDistribution -and -not $isAppShellArtifact) {
        Fail "Refusing to clean or write '$relativePath'. Use 'dist/app/WastelandForge.Desktop' or 'artifacts/app-shell/<name>'." 6
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
$projectPath = Join-Path (Join-Path $script:RepositoryRoot 'src') (Join-Path 'WastelandForge.Desktop' 'WastelandForge.Desktop.csproj')
$backendPublishScript = Join-Path (Join-Path $script:RepositoryRoot 'eng') 'Publish-StandaloneForge.ps1'

if ([string]::IsNullOrWhiteSpace($OutputRoot)) {
    $OutputRoot = Join-Path (Join-Path 'dist' 'app') 'WastelandForge.Desktop'
}

if ([string]::IsNullOrWhiteSpace($BackendRoot)) {
    $BackendRoot = Join-Path (Join-Path 'dist' 'local') 'forge'
}

if (-not (Test-Path -LiteralPath $projectPath -PathType Leaf)) {
    Fail "WastelandForge Desktop project was not found at '$projectPath'." 3
}

if (-not (Test-Path -LiteralPath $backendPublishScript -PathType Leaf)) {
    Fail "Standalone Forge publish helper was not found at '$backendPublishScript'." 3
}

if (-not [string]::IsNullOrWhiteSpace($HeatSourceRoot) -and
    -not (Test-Path -LiteralPath $HeatSourceRoot -PathType Container)) {
    Fail "HeatSourceRoot was provided but does not exist: '$HeatSourceRoot'." 3
}

$dotnetCommand = Get-Command dotnet -ErrorAction SilentlyContinue
if ($null -eq $dotnetCommand) {
    Fail 'The dotnet SDK was not found on PATH. Install the SDK pinned by global.json before publishing the app shell.' 5
}

$script:DotNetPath = $dotnetCommand.Source
$outputRootPath = Assert-AllowedOutputRoot -Path $OutputRoot
$backendRootPath = Get-FullPathFromRepository -Path $BackendRoot
$manifestPath = Join-Path $outputRootPath 'app-build-manifest.json'
$readmePath = Join-Path $outputRootPath 'README.txt'
$checksumsPath = Join-Path $outputRootPath 'checksums.sha256'
$executablePath = Join-Path $outputRootPath 'WastelandForge.exe'
$backendExecutablePath = Join-Path (Join-Path $outputRootPath 'ForgeBackend') 'forge.exe'
$demoProjectPath = Join-Path (Join-Path $outputRootPath 'DemoProjects') (Join-Path 'ExampleMod' 'wastelandforge.json')
$exitCode = 0

Push-Location $script:RepositoryRoot
try {
    if ((Test-Path -LiteralPath $outputRootPath) -and (-not $NoClean)) {
        Remove-Item -LiteralPath $outputRootPath -Recurse -Force
    }

    New-Item -ItemType Directory -Force -Path $outputRootPath | Out-Null

    if (-not $SkipBackendPublish) {
        $backendArguments = @(
            '-NoLogo',
            '-NoProfile',
            '-ExecutionPolicy',
            'Bypass',
            '-File',
            $backendPublishScript,
            '-Configuration',
            $Configuration,
            '-OutputRoot',
            $backendRootPath)

        Write-Host 'Publishing bundled Forge backend first...'
        & pwsh @backendArguments
        $backendExitCode = if ($null -eq $LASTEXITCODE) { 0 } else { [int] $LASTEXITCODE }
        if ($backendExitCode -ne 0) {
            Fail "Standalone Forge backend publish failed with exit code $backendExitCode." 5
        }
    }

    if (-not (Test-Path -LiteralPath (Join-Path $backendRootPath 'forge.exe') -PathType Leaf)) {
        Fail "Expected backend forge.exe was not found at '$backendRootPath'." 5
    }

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

    if (-not [string]::IsNullOrWhiteSpace($HeatSourceRoot)) {
        $publishArguments += @("/p:HeatSourceRoot=$HeatSourceRoot")
    }

    Write-Host "Publishing WastelandForge app shell ($Configuration, $runtimeLabel, self-contained: $SelfContained)..."
    Invoke-DotNet -Arguments $publishArguments

    if (-not (Test-Path -LiteralPath $executablePath -PathType Leaf)) {
        Fail "Expected WastelandForge.exe was not produced at '$executablePath'." 5
    }

    if (-not (Test-Path -LiteralPath $backendExecutablePath -PathType Leaf)) {
        Fail "Expected bundled backend forge.exe was not produced at '$backendExecutablePath'." 5
    }

    if (-not (Test-Path -LiteralPath $demoProjectPath -PathType Leaf)) {
        Fail "Expected bundled demo project was not produced at '$demoProjectPath'." 5
    }

    $backendVersion = 'skipped'
    if (-not $SkipSmoke) {
        Write-Host 'Verifying bundled Forge backend...'
        $versionOutput = & $backendExecutablePath '--version'
        $smokeExitCode = if ($null -eq $LASTEXITCODE) { 0 } else { [int] $LASTEXITCODE }
        if ($smokeExitCode -ne 0) {
            Fail "Bundled ForgeBackend forge.exe --version failed with exit code $smokeExitCode." 5
        }

        $backendVersion = (($versionOutput | Out-String).Trim())
    }

    $readme = @"
WastelandForge Windows app shell

Generated by eng/Publish-AppShell.ps1 from this source repository.

Executable:
  WastelandForge.exe

Bundled backend:
  ForgeBackend\forge.exe

Bundled demo source:
  DemoProjects\ExampleMod

Try:
  .\WastelandForge.exe
  .\ForgeBackend\forge.exe --version
  .\ForgeBackend\forge.exe help

This Gate 341 local app-shell publish helper is a developer handoff folder. It
is not an installer, not a signed release artifact, not an update channel, and
not a public archive package.

Boundary:
  no installer
  no signing or attestation
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
        schema = 'wastelandforge.local-app-shell-distribution.v1'
        kind = 'wastelandforge.localAppShellDistribution'
        gate = 341
        status = 'complete'
        build = [ordered] @{
            configuration = $Configuration
            runtimeIdentifier = $runtimeLabel
            selfContained = [bool] $SelfContained
            frameworkDependent = -not [bool] $SelfContained
            publishSingleFile = $false
            noRestore = $true
            timestamp = Get-BuildTimestamp
        }
        source = [ordered] @{
            repositoryRoot = $script:RepositoryRoot
            project = Get-RelativePath -Root $script:RepositoryRoot -Path $projectPath
            backendRoot = Get-RelativePath -Root $script:RepositoryRoot -Path $backendRootPath
        }
        app = [ordered] @{
            executable = 'WastelandForge.exe'
            backendExecutable = 'ForgeBackend/forge.exe'
            bundledDemoProject = 'DemoProjects/ExampleMod'
            backendVersionSmoke = $backendVersion
        }
        heat = [ordered] @{
            heatSourceRootProvided = -not [string]::IsNullOrWhiteSpace($HeatSourceRoot)
            rawHeatAssetsCommitted = $false
            publicReleaseApproved = $false
        }
        boundaries = [ordered] @{
            installerCreated = $false
            archiveCreated = $false
            signing = $false
            attestation = $false
            updateChannel = $false
            releasePublication = $false
            nugetPublication = $false
            packageRestorePolicyChanged = $false
            generatedConsumerTemplateMutation = $false
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

    Write-Host "WastelandForge app shell distribution complete: $outputRootPath"
    Write-Host "Run: $executablePath"
}
catch {
    [Console]::Error.WriteLine($_.Exception.Message)
    $exitCode = 5
}
finally {
    Pop-Location
}

exit $exitCode
