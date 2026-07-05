$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$forgeArguments = @($args)
$scriptRoot = if ([string]::IsNullOrWhiteSpace($PSScriptRoot)) {
    Split-Path -Parent $PSCommandPath
}
else {
    $PSScriptRoot
}

$repositoryRoot = (Resolve-Path -LiteralPath (Join-Path $scriptRoot '..')).Path
$projectPath = Join-Path $repositoryRoot 'src\WastelandForge.Cli\WastelandForge.Cli.csproj'

if (-not (Test-Path -LiteralPath $projectPath -PathType Leaf)) {
    Write-Error "WastelandForge CLI project was not found at '$projectPath'."
    exit 3
}

$dotnetCommand = Get-Command dotnet -ErrorAction SilentlyContinue
if ($null -eq $dotnetCommand) {
    Write-Error 'The dotnet SDK was not found on PATH. Install the SDK pinned by global.json before using the source-built Forge runner.'
    exit 5
}

$exitCode = 0
Push-Location $repositoryRoot
try {
    & $dotnetCommand.Source run --project $projectPath -- @forgeArguments
    $exitCode = if ($null -eq $LASTEXITCODE) { 0 } else { [int] $LASTEXITCODE }
}
finally {
    Pop-Location
}

exit $exitCode
