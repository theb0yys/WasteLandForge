[CmdletBinding()]
param(
    [string] $Destination = 'D:\downlods\Development folder\dev\FalloutNV\WastelandForge.Desktop',

    [ValidateSet('Debug', 'Release')]
    [string] $Configuration = 'Release',

    [string] $RuntimeIdentifier = 'win-x64',

    [switch] $SkipBackup
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$scriptRoot = if ([string]::IsNullOrWhiteSpace($PSScriptRoot)) {
    Split-Path -Parent $PSCommandPath
}
else {
    $PSScriptRoot
}

$repositoryRoot = (Resolve-Path -LiteralPath (Join-Path $scriptRoot '..')).Path
$solutionPath = Join-Path $repositoryRoot 'WastelandForge.sln'
$publishScript = Join-Path $scriptRoot 'Publish-AppShell.ps1'
$publishRoot = Join-Path $repositoryRoot 'dist\app\WastelandForge.Desktop'
$destinationPath = [System.IO.Path]::GetFullPath($Destination)
$timestamp = [DateTime]::UtcNow.ToString('yyyyMMdd-HHmmss')
$stagingPath = "$destinationPath.update-$timestamp"
$backupPath = "$destinationPath.backup-$timestamp"
$movedExistingInstall = $false

function Assert-PublishedApp {
    param([Parameter(Mandatory = $true)][string] $Root)

    $app = Join-Path $Root 'WastelandForge.exe'
    $backend = Join-Path $Root 'ForgeBackend\forge.exe'

    if (-not (Test-Path -LiteralPath $app -PathType Leaf)) {
        throw "Published desktop executable was not found at '$app'."
    }

    if (-not (Test-Path -LiteralPath $backend -PathType Leaf)) {
        throw "Published backend executable was not found at '$backend'."
    }
}

function Stop-InstalledApp {
    param([Parameter(Mandatory = $true)][string] $Root)

    $normalizedRoot = [System.IO.Path]::GetFullPath($Root).TrimEnd('\') + '\'
    $processes = Get-CimInstance Win32_Process -Filter "Name = 'WastelandForge.exe'" -ErrorAction SilentlyContinue

    foreach ($process in $processes) {
        $executablePath = [string] $process.ExecutablePath
        if (-not [string]::IsNullOrWhiteSpace($executablePath) -and
            $executablePath.StartsWith($normalizedRoot, [System.StringComparison]::OrdinalIgnoreCase)) {
            Stop-Process -Id $process.ProcessId -Force -ErrorAction Stop
        }
    }
}

Push-Location $repositoryRoot
try {
    if (-not (Test-Path -LiteralPath $solutionPath -PathType Leaf)) {
        throw "Solution was not found at '$solutionPath'."
    }

    if (-not (Test-Path -LiteralPath $publishScript -PathType Leaf)) {
        throw "Publish helper was not found at '$publishScript'."
    }

    if ($destinationPath.Equals([System.IO.Path]::GetFullPath($publishRoot), [System.StringComparison]::OrdinalIgnoreCase)) {
        throw 'Destination cannot be the repository publish staging directory.'
    }

    Write-Host "Restoring $RuntimeIdentifier dependencies..."
    & dotnet restore $solutionPath -r $RuntimeIdentifier
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet restore failed with exit code $LASTEXITCODE."
    }

    Write-Host 'Publishing the latest desktop app and bundled Forge backend...'
    & $publishScript -Configuration $Configuration -RuntimeIdentifier $RuntimeIdentifier -SelfContained
    if ($LASTEXITCODE -ne 0) {
        throw "Publish-AppShell.ps1 failed with exit code $LASTEXITCODE."
    }

    Assert-PublishedApp -Root $publishRoot
    Stop-InstalledApp -Root $destinationPath

    if (Test-Path -LiteralPath $stagingPath) {
        Remove-Item -LiteralPath $stagingPath -Recurse -Force
    }

    New-Item -ItemType Directory -Path $stagingPath -Force | Out-Null
    Copy-Item -LiteralPath (Join-Path $publishRoot '*') -Destination $stagingPath -Recurse -Force
    Assert-PublishedApp -Root $stagingPath

    if (Test-Path -LiteralPath $destinationPath) {
        if ($SkipBackup) {
            Remove-Item -LiteralPath $destinationPath -Recurse -Force
        }
        else {
            if (Test-Path -LiteralPath $backupPath) {
                Remove-Item -LiteralPath $backupPath -Recurse -Force
            }

            Move-Item -LiteralPath $destinationPath -Destination $backupPath
            $movedExistingInstall = $true
        }
    }

    Move-Item -LiteralPath $stagingPath -Destination $destinationPath
    Assert-PublishedApp -Root $destinationPath

    Write-Host "Updated WastelandForge at: $destinationPath"
    if ($movedExistingInstall) {
        Write-Host "Previous installation backed up at: $backupPath"
    }
}
catch {
    if (Test-Path -LiteralPath $stagingPath) {
        Remove-Item -LiteralPath $stagingPath -Recurse -Force -ErrorAction SilentlyContinue
    }

    if ($movedExistingInstall -and
        -not (Test-Path -LiteralPath $destinationPath) -and
        (Test-Path -LiteralPath $backupPath)) {
        Move-Item -LiteralPath $backupPath -Destination $destinationPath -ErrorAction SilentlyContinue
    }

    throw
}
finally {
    Pop-Location
}
