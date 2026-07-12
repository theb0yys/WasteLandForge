[CmdletBinding()]
param(
    [string] $InstallerPath = 'artifacts/installer/inno/local/WastelandForge-Setup-local.exe',
    [int] $TimeoutSeconds = 90
)

$ErrorActionPreference = 'Stop'
$repositoryRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))

function Resolve-RepositoryFile([string] $Path, [string] $Label) {
    $candidate = if ([IO.Path]::IsPathRooted($Path)) { [IO.Path]::GetFullPath($Path) } else { [IO.Path]::GetFullPath((Join-Path $repositoryRoot $Path)) }
    if (-not $candidate.StartsWith($repositoryRoot.TrimEnd('\') + '\', [StringComparison]::OrdinalIgnoreCase)) { throw "$Label must stay under the repository: $candidate" }
    if (-not (Test-Path -LiteralPath $candidate -PathType Leaf)) { throw "$Label was not found: $candidate" }
    return $candidate
}

function Wait-Until([scriptblock] $Condition, [string] $Failure, [int] $Seconds = $TimeoutSeconds) {
    $deadline = [DateTime]::UtcNow.AddSeconds($Seconds)
    do {
        $result = & $Condition
        if ($result) { return $result }
        Start-Sleep -Milliseconds 200
    } while ([DateTime]::UtcNow -lt $deadline)
    throw $Failure
}

function Find-Control([System.Windows.Automation.AutomationElement] $Root, [string] $AutomationId) {
    $condition = [System.Windows.Automation.PropertyCondition]::new([System.Windows.Automation.AutomationElement]::AutomationIdProperty, $AutomationId)
    return $Root.FindFirst([System.Windows.Automation.TreeScope]::Descendants, $condition)
}

function Require-Control([System.Windows.Automation.AutomationElement] $Root, [string] $AutomationId) {
    $control = Find-Control $Root $AutomationId
    if ($null -eq $control) { throw "Installed UI control was not found: $AutomationId" }
    return $control
}

function Set-Value([System.Windows.Automation.AutomationElement] $Control, [string] $Value) {
    $Control.GetCurrentPattern([System.Windows.Automation.ValuePattern]::Pattern).SetValue($Value)
}

function Invoke-Control([System.Windows.Automation.AutomationElement] $Control) {
    $Control.GetCurrentPattern([System.Windows.Automation.InvokePattern]::Pattern).Invoke()
}

function Select-Tab([System.Windows.Automation.AutomationElement] $Window, [string] $Name) {
    $condition = [System.Windows.Automation.PropertyCondition]::new([System.Windows.Automation.AutomationElement]::NameProperty, $Name)
    $tab = $Window.FindFirst([System.Windows.Automation.TreeScope]::Descendants, $condition)
    if ($null -eq $tab) { throw "Installed tab was not found: $Name" }
    $tab.GetCurrentPattern([System.Windows.Automation.SelectionItemPattern]::Pattern).Select()
}

function Descendant-Text([System.Windows.Automation.AutomationElement] $Control) {
    return (($Control.FindAll([System.Windows.Automation.TreeScope]::Descendants, [System.Windows.Automation.Condition]::TrueCondition) | ForEach-Object { $_.Current.Name }) -join "`n")
}

function Find-FirstControlType([System.Windows.Automation.AutomationElement] $Root, [System.Windows.Automation.ControlType] $ControlType) {
    $condition = [System.Windows.Automation.PropertyCondition]::new([System.Windows.Automation.AutomationElement]::ControlTypeProperty, $ControlType)
    return $Root.FindFirst([System.Windows.Automation.TreeScope]::Descendants, $condition)
}

Add-Type -AssemblyName UIAutomationClient
Add-Type -AssemblyName UIAutomationTypes
Add-Type @'
using System;
using System.Runtime.InteropServices;
public static class WfNativeWindow {
    [DllImport("user32.dll")] public static extern bool SetForegroundWindow(IntPtr hWnd);
}
'@

$installer = Resolve-RepositoryFile $InstallerPath 'Installer'
$runId = [Guid]::NewGuid().ToString('N')
$installRoot = Join-Path $env:LOCALAPPDATA "WastelandForge\Gate462-$runId"
$settingsRoot = Join-Path $env:TEMP "WastelandForge-Gate462-Settings-$runId"
$readyRoot = Join-Path $env:TEMP "WastelandForge-Gate462-Ready-$runId"
$blockedRoot = Join-Path $env:TEMP "WastelandForge-Gate462-Blocked-$runId"
$process = $null
$installed = $false

try {
    $install = Start-Process -FilePath $installer -ArgumentList @('/VERYSILENT', '/SUPPRESSMSGBOXES', '/NORESTART', "/DIR=$installRoot") -Wait -PassThru -WindowStyle Hidden
    if ($install.ExitCode -ne 0) { throw "Installer exited with $($install.ExitCode)." }
    $installed = $true
    Copy-Item -LiteralPath (Join-Path $installRoot 'DemoProjects\CombinedModExample') -Destination $readyRoot -Recurse
    Copy-Item -LiteralPath (Join-Path $installRoot 'DemoProjects\CombinedModExample') -Destination $blockedRoot -Recurse
    $blockedManifest = Join-Path $blockedRoot 'wastelandforge.json'
    $blocked = Get-Content -LiteralPath $blockedManifest -Raw | ConvertFrom-Json
    $blocked.id = 'INVALID ID'
    $blocked | ConvertTo-Json -Depth 20 | Set-Content -LiteralPath $blockedManifest -Encoding utf8NoBOM

    $start = [Diagnostics.ProcessStartInfo]::new((Join-Path $installRoot 'WastelandForge.exe'))
    $start.UseShellExecute = $false
    $start.Environment['LOCALAPPDATA'] = $settingsRoot
    $process = [Diagnostics.Process]::Start($start)
    Wait-Until { $process.Refresh(); $process.MainWindowHandle -ne [IntPtr]::Zero } 'Installed WastelandForge window did not open.' | Out-Null
    $window = [System.Windows.Automation.AutomationElement]::FromHandle($process.MainWindowHandle)
    Select-Tab $window 'Release Candidate'

    $projectPath = Require-Control $window 'ProjectPathTextBox'
    $run = Require-Control $window 'RunReleaseCandidateButton'
    $state = Require-Control $window 'ReleaseCandidateStateTextBlock'
    Set-Value $projectPath $readyRoot
    Invoke-Control $run
    Wait-Until { $state.Current.Name -eq 'Candidate ready' } "Ready project did not reach Candidate ready; current state: $($state.Current.Name)" | Out-Null

    foreach ($controlId in @('OpenCandidatePackageFolderButton', 'OpenCandidatePackageArchiveButton', 'OpenCandidateReleaseEvidenceButton', 'OpenCandidateReleaseHandoffButton')) {
        if (-not (Require-Control $window $controlId).Current.IsEnabled) { throw "Ready evidence action was disabled: $controlId" }
    }
    $stageGrid = Require-Control $window 'ReleaseCandidateStagesDataGrid'
    $stageText = Descendant-Text $stageGrid
    foreach ($expected in @('Validate', 'Combined package', 'Release verification', 'Passed')) {
        if (-not $stageText.Contains($expected, [StringComparison]::Ordinal)) { throw "Ready stage grid did not expose '$expected'." }
    }

    Add-Content -LiteralPath (Join-Path $readyRoot 'src\registries\mcm\main.json') -Value ' ' -Encoding utf8NoBOM
    [WfNativeWindow]::SetForegroundWindow($process.MainWindowHandle) | Out-Null
    Wait-Until { $state.Current.Name -eq 'Stale' } 'Changed source did not mark the installed candidate projection stale.' | Out-Null
    if ((Require-Control $window 'OpenCandidatePackageArchiveButton').Current.IsEnabled) { throw 'Stale package action remained enabled.' }

    Set-Value $projectPath $blockedRoot
    Invoke-Control $run
    Wait-Until { $state.Current.Name -eq 'Blocked' } 'Invalid project did not reach Blocked.' | Out-Null
    $diagnosticText = Descendant-Text (Require-Control $window 'ReleaseCandidateDiagnosticsDataGrid')
    if (-not $diagnosticText.Contains('WF-SCHEMA-', [StringComparison]::Ordinal)) { throw 'Blocked diagnostics did not expose the exact WF-SCHEMA rule identifier.' }
    if (-not (Descendant-Text $stageGrid).Contains('Not run', [StringComparison]::Ordinal)) { throw 'Blocked stage grid did not expose later stages as Not run.' }

    $diagnosticGrid = Require-Control $window 'ReleaseCandidateDiagnosticsDataGrid'
    $diagnosticRow = Find-FirstControlType $diagnosticGrid ([System.Windows.Automation.ControlType]::DataItem)
    if ($null -eq $diagnosticRow) { throw 'Blocked diagnostic row was not selectable.' }
    $diagnosticRow.GetCurrentPattern([System.Windows.Automation.SelectionItemPattern]::Pattern).Select()
    $explain = Require-Control $window 'ExplainReleaseCandidateDiagnosticButton'
    Wait-Until { $explain.Current.IsEnabled } 'Explain Diagnostic did not enable for the selected issue.' | Out-Null
    Invoke-Control $explain
    $explainStatus = Require-Control $window 'DiagnosticExplanationStatusTextBlock'
    Wait-Until { $explainStatus.Current.Name -eq 'Ready' } 'Canonical diagnostic explanation did not reach Ready.' | Out-Null
    $explanationText = (Require-Control $window 'DiagnosticExplanationTextBox').GetCurrentPattern([System.Windows.Automation.ValuePattern]::Pattern).Current.Value
    foreach ($expected in @('forge explain diagnostic WF-SCHEMA-', 'Family: WF-SCHEMA-*', 'Validation stage: schema validation', 'Recovery commands (display only):', 'Boundaries:')) {
        if (-not $explanationText.Contains($expected, [StringComparison]::Ordinal)) { throw "Explanation did not expose '$expected'." }
    }
    $originalContext = (Require-Control $window 'DiagnosticOriginalContextTextBlock').Current.Name
    if (-not $originalContext.Contains('WF-SCHEMA-', [StringComparison]::Ordinal)) { throw 'Original exact diagnostic context was not preserved separately.' }

    $goToWorkspace = Require-Control $window 'GoToDiagnosticWorkspaceButton'
    if (-not $goToWorkspace.Current.IsEnabled) { throw 'Schema diagnostic workspace route was disabled.' }
    Invoke-Control $goToWorkspace
    $validationTab = Require-Control $window 'ValidationReportTabItem'
    if (-not $validationTab.GetCurrentPattern([System.Windows.Automation.SelectionItemPattern]::Pattern).Current.IsSelected) { throw 'Schema diagnostic did not route to Validation Report.' }

    Select-Tab $window 'Release Candidate'
    Add-Content -LiteralPath (Join-Path $blockedRoot 'src\registries\mcm\main.json') -Value ' ' -Encoding utf8NoBOM
    [WfNativeWindow]::SetForegroundWindow($process.MainWindowHandle) | Out-Null
    Wait-Until { $state.Current.Name -eq 'Stale' } 'Changed blocked source did not stale the remediation projection.' | Out-Null
    if ($goToWorkspace.Current.IsEnabled) { throw 'Stale diagnostic workspace route remained enabled.' }

    Write-Host 'Installed Release Candidate UI regression passed.'
    Write-Host 'Ready, evidence actions, stale projection, exact diagnostics, canonical explanation, deterministic routing, and stage short-circuiting verified.'
}
finally {
    if ($null -ne $process -and -not $process.HasExited) { Stop-Process -Id $process.Id -Force }
    if ($installed -and (Test-Path -LiteralPath (Join-Path $installRoot 'unins000.exe'))) {
        Start-Process -FilePath (Join-Path $installRoot 'unins000.exe') -ArgumentList @('/VERYSILENT', '/SUPPRESSMSGBOXES', '/NORESTART') -Wait -WindowStyle Hidden
    }
    foreach ($path in @($readyRoot, $blockedRoot, $settingsRoot)) {
        if (Test-Path -LiteralPath $path) {
            $resolved = (Resolve-Path -LiteralPath $path).Path
            $tempPrefix = [IO.Path]::GetFullPath($env:TEMP).TrimEnd('\') + '\'
            if (-not $resolved.StartsWith($tempPrefix, [StringComparison]::OrdinalIgnoreCase)) { throw "Refusing unsafe temporary cleanup: $resolved" }
            Remove-Item -LiteralPath $resolved -Recurse -Force
        }
    }
    Start-Sleep -Milliseconds 300
    if (Test-Path -LiteralPath $installRoot) { throw "Installed regression did not clean installation root: $installRoot" }
}
