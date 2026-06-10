[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [ValidateNotNullOrEmpty()]
    [string] $Lane,

    [Parameter(Mandatory = $true)]
    [ValidateNotNullOrEmpty()]
    [string] $OutputDirectory,

    [string] $RepositoryRoot
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

function Write-Utf8NoBom {
    param(
        [Parameter(Mandatory = $true)]
        [string] $Path,

        [Parameter(Mandatory = $true)]
        [string] $Content
    )

    $encoding = New-Object System.Text.UTF8Encoding($false)
    [System.IO.File]::WriteAllText($Path, $Content, $encoding)
}

function ConvertTo-RepositoryRelativePath {
    param(
        [Parameter(Mandatory = $true)]
        [string] $Path
    )

    $fullPath = (Resolve-Path -LiteralPath $Path).Path
    return [System.IO.Path]::GetRelativePath($script:ResolvedRepositoryRoot, $fullPath).Replace('\', '/')
}

if ([string]::IsNullOrWhiteSpace($RepositoryRoot)) {
    $RepositoryRoot = Join-Path $PSScriptRoot '..\..'
}

$script:ResolvedRepositoryRoot = (Resolve-Path -LiteralPath $RepositoryRoot).Path

if ([System.IO.Path]::IsPathRooted($OutputDirectory)) {
    $resolvedOutputDirectory = $OutputDirectory
}
else {
    $resolvedOutputDirectory = Join-Path $script:ResolvedRepositoryRoot $OutputDirectory
}

New-Item -ItemType Directory -Force -Path $resolvedOutputDirectory | Out-Null

$dotnetSdkVersion = (& dotnet --version).Trim()

$commit = $env:GITHUB_SHA
if ([string]::IsNullOrWhiteSpace($commit)) {
    try {
        $commit = (& git -C $script:ResolvedRepositoryRoot rev-parse HEAD).Trim()
    }
    catch {
        $commit = 'unknown'
    }
}

$ref = $env:GITHUB_REF
if ([string]::IsNullOrWhiteSpace($ref)) {
    $ref = 'local'
}

$manifest = [ordered] @{
    formatVersion = '0.1'
    kind = 'wastelandforge.ci-build-manifest'
    researchClassification = 'Inferred'
    gate = 'Gate 8'
    lane = $Lane
    generatedAtUtc = [DateTimeOffset]::UtcNow.ToString('O')
    repository = [ordered] @{
        commit = $commit
        ref = $ref
        repository = $env:GITHUB_REPOSITORY
        workflow = $env:GITHUB_WORKFLOW
        runId = $env:GITHUB_RUN_ID
        runAttempt = $env:GITHUB_RUN_ATTEMPT
    }
    dotnet = [ordered] @{
        sdkVersion = $dotnetSdkVersion
        globalJson = 'global.json'
        targetFramework = 'net10.0'
    }
    commands = @(
        'dotnet restore WastelandForge.sln',
        'dotnet build WastelandForge.sln -c Release --no-restore',
        'dotnet test WastelandForge.sln -c Release --no-build --no-restore -m:1 --logger trx'
    )
    notes = @(
        'This is a Gate 8 CI governance manifest.',
        'Release dry-run build manifests are emitted by forge release verify.'
    )
}

$manifestJson = $manifest | ConvertTo-Json -Depth 20
Write-Utf8NoBom -Path (Join-Path $resolvedOutputDirectory 'build-manifest.json') -Content ($manifestJson + [Environment]::NewLine)

$checksumFile = Join-Path $resolvedOutputDirectory 'checksums.sha256'
$files = Get-ChildItem -LiteralPath $resolvedOutputDirectory -File -Recurse |
    Where-Object { $_.FullName -ne $checksumFile } |
    Sort-Object FullName

$checksumLines = foreach ($file in $files) {
    $hash = Get-FileHash -LiteralPath $file.FullName -Algorithm SHA256
    '{0}  {1}' -f $hash.Hash.ToLowerInvariant(), (ConvertTo-RepositoryRelativePath -Path $file.FullName)
}

Write-Utf8NoBom -Path $checksumFile -Content (($checksumLines -join [Environment]::NewLine) + [Environment]::NewLine)
