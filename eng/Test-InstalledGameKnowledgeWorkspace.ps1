[CmdletBinding()]
param(
    [string] $InstallerPath = 'artifacts/installer/inno/local/WastelandForge-Setup-local.exe',
    [string] $SyntheticExportPath = 'fixtures/fnv-game-knowledge/valid-synthetic-export.json',
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

function Invoke-Control([System.Windows.Automation.AutomationElement] $Control) {
    Wait-Until { $Control.Current.IsEnabled } "Installed UI control did not become enabled: $($Control.Current.AutomationId)" | Out-Null
    $Control.GetCurrentPattern([System.Windows.Automation.InvokePattern]::Pattern).Invoke()
}

function Set-Value([System.Windows.Automation.AutomationElement] $Control, [string] $Value) {
    $Control.GetCurrentPattern([System.Windows.Automation.ValuePattern]::Pattern).SetValue($Value)
}

function Select-ComboItem([System.Windows.Automation.AutomationElement] $ComboBox, [string] $Name) {
    $ComboBox.GetCurrentPattern([System.Windows.Automation.ExpandCollapsePattern]::Pattern).Expand()
    $condition = [System.Windows.Automation.PropertyCondition]::new([System.Windows.Automation.AutomationElement]::NameProperty, $Name)
    $item = $ComboBox.FindFirst([System.Windows.Automation.TreeScope]::Descendants, $condition)
    if ($null -eq $item) {
        $itemCondition = [System.Windows.Automation.AndCondition]::new(@(
            $condition,
            [System.Windows.Automation.PropertyCondition]::new([System.Windows.Automation.AutomationElement]::ControlTypeProperty, [System.Windows.Automation.ControlType]::ListItem)
        ))
        $item = [System.Windows.Automation.AutomationElement]::RootElement.FindFirst([System.Windows.Automation.TreeScope]::Descendants, $itemCondition)
    }
    if ($null -eq $item) { throw "Installed combo item was not found: $Name" }
    $item.GetCurrentPattern([System.Windows.Automation.SelectionItemPattern]::Pattern).Select()
    $ComboBox.GetCurrentPattern([System.Windows.Automation.ExpandCollapsePattern]::Pattern).Collapse()
}

function Select-DataGridRow([System.Windows.Automation.AutomationElement] $DataGrid, [int] $Index) {
    $condition = [System.Windows.Automation.PropertyCondition]::new([System.Windows.Automation.AutomationElement]::ControlTypeProperty, [System.Windows.Automation.ControlType]::DataItem)
    $items = $DataGrid.FindAll([System.Windows.Automation.TreeScope]::Descendants, $condition)
    if ($items.Count -le $Index) { throw "Installed game-knowledge row $Index was unavailable; row count was $($items.Count)." }
    $items[$Index].GetCurrentPattern([System.Windows.Automation.SelectionItemPattern]::Pattern).Select()
}

function Get-ProjectSnapshot([string] $Root) {
    return @(Get-ChildItem -LiteralPath $Root -File -Recurse | Sort-Object FullName | ForEach-Object {
        '{0}|{1}|{2}' -f [IO.Path]::GetRelativePath($Root, $_.FullName), $_.Length, (Get-FileHash -LiteralPath $_.FullName -Algorithm SHA256).Hash
    })
}

function Get-TrackedEditorProcessIds {
    return @(Get-Process -ErrorAction SilentlyContinue |
        Where-Object { $_.ProcessName -in @('GECK','FNVEdit','xEdit','ModOrganizer','ModOrganizer2','FalloutNV') } |
        Select-Object -ExpandProperty Id)
}

Add-Type -AssemblyName UIAutomationClient
Add-Type -AssemblyName UIAutomationTypes
Add-Type @'
using System;
using System.Runtime.InteropServices;
public static class WfGameKnowledgeWindow {
    [DllImport("user32.dll")] public static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int Width, int Height, bool Repaint);
}
'@

$installer = Resolve-RepositoryFile $InstallerPath 'Installer'
$syntheticExport = Resolve-RepositoryFile $SyntheticExportPath 'Synthetic game-knowledge export'
$runId = [Guid]::NewGuid().ToString('N')
$installRoot = Join-Path $env:LOCALAPPDATA "WastelandForge\Gate544-$runId"
$settingsRoot = Join-Path $env:TEMP "WastelandForge-Gate544-Settings-$runId"
$fixtureRoot = Join-Path $env:TEMP "WastelandForge-Gate544-Fixture-$runId"
$gameRoot = Join-Path $fixtureRoot 'Game'
$dataRoot = Join-Path $gameRoot 'Data'
$toolRoot = Join-Path $fixtureRoot 'Tools'
$projectRoot = Join-Path $fixtureRoot 'Project'
$masterPath = Join-Path $dataRoot 'FalloutNV.esm'
$providerPath = Join-Path $toolRoot 'FNVEdit.exe'
$process = $null
$installed = $false
$failure = $null

try {
    $install = Start-Process -FilePath $installer -ArgumentList @('/VERYSILENT', '/SUPPRESSMSGBOXES', '/NORESTART', "/DIR=$installRoot") -Wait -PassThru -WindowStyle Hidden
    if ($install.ExitCode -ne 0) { throw "Installer exited with $($install.ExitCode)." }
    $installed = $true
    New-Item -ItemType Directory -Path $dataRoot,$toolRoot,(Join-Path $settingsRoot 'WastelandForge') -Force | Out-Null
    Copy-Item -LiteralPath (Join-Path $installRoot 'DemoProjects\ExampleMod') -Destination $projectRoot -Recurse
    [IO.File]::WriteAllBytes($masterPath, [Text.Encoding]::UTF8.GetBytes('synthetic FalloutNV.esm identity bytes'))
    Copy-Item -LiteralPath (Join-Path $installRoot 'ForgeBackend\forge.exe') -Destination $providerPath
    $masterHash = (Get-FileHash -LiteralPath $masterPath -Algorithm SHA256).Hash
    $providerHash = (Get-FileHash -LiteralPath $providerPath -Algorithm SHA256).Hash
    $projectBefore = Get-ProjectSnapshot $projectRoot
    $settings = [ordered]@{
        ProjectRoot = $projectRoot
        GameRoot = $gameRoot
        DataRoot = $dataRoot
        Mo2Path = ''
        Mo2ModsRoot = ''
        ToolPaths = [ordered]@{ geck = ''; xedit = $providerPath }
    }
    $settings | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath (Join-Path $settingsRoot 'WastelandForge\app-settings.json') -Encoding utf8NoBOM
    $trackedBefore = Get-TrackedEditorProcessIds

    $start = [Diagnostics.ProcessStartInfo]::new((Join-Path $installRoot 'WastelandForge.exe'))
    $start.UseShellExecute = $false
    $start.Environment['LOCALAPPDATA'] = $settingsRoot
    $process = [Diagnostics.Process]::Start($start)
    Wait-Until { $process.Refresh(); $process.MainWindowHandle -ne [IntPtr]::Zero } 'Installed WastelandForge window did not open.' | Out-Null
    $window = [System.Windows.Automation.AutomationElement]::FromHandle($process.MainWindowHandle)

    [WfGameKnowledgeWindow]::MoveWindow($process.MainWindowHandle, 40, 40, 960, 640, $true) | Out-Null
    Select-ComboItem (Require-Control $window 'BuildWorkspaceComboBox') 'Game Knowledge'
    foreach ($controlId in @('GameKnowledgeTabItem','GameKnowledgeStateTextBlock','PrepareGameKnowledgeExportButton','ImportGameKnowledgeExportButton','GameKnowledgeSearchTextBox','GameKnowledgeResultsDataGrid','GameKnowledgeDetailsTextBox')) {
        $visible = $false
        $deadline = [DateTime]::UtcNow.AddSeconds(10)
        do {
            $control = Require-Control $window $controlId
            $visible = -not $control.Current.IsOffscreen -and $control.Current.BoundingRectangle.Width -gt 0 -and $control.Current.BoundingRectangle.Height -gt 0
            if (-not $visible) { Start-Sleep -Milliseconds 200 }
        } while (-not $visible -and [DateTime]::UtcNow -lt $deadline)
        if (-not $visible) {
            $tab = Require-Control $window 'GameKnowledgeTabItem'
            throw "Game Knowledge control is not accessible at 960x640 after layout settled: $controlId; window=$($window.Current.BoundingRectangle); tab=$($tab.Current.BoundingRectangle); control=$($control.Current.BoundingRectangle); offscreen=$($control.Current.IsOffscreen)."
        }
    }

    [WfGameKnowledgeWindow]::MoveWindow($process.MainWindowHandle, 40, 40, 1180, 760, $true) | Out-Null
    Start-Sleep -Milliseconds 300
    Invoke-Control (Require-Control $window 'PrepareGameKnowledgeExportButton')
    Wait-Until { (Require-Control $window 'GameKnowledgeStateTextBlock').Current.Name -eq 'WaitingForExport' } 'Installed export preparation did not reach WaitingForExport.' | Out-Null
    $cacheRoot = Join-Path $settingsRoot 'WastelandForge\game-knowledge\fnv'
    $runRoot = Wait-Until { Get-ChildItem -LiteralPath (Join-Path $cacheRoot 'runs') -Directory -ErrorAction SilentlyContinue | Sort-Object LastWriteTimeUtc -Descending | Select-Object -First 1 -ExpandProperty FullName } 'Prepared private export run was not created.'
    foreach ($name in @('WastelandForgeFNVGameKnowledge.pas','run-manifest.json')) { if (-not (Test-Path -LiteralPath (Join-Path $runRoot $name) -PathType Leaf)) { throw "Prepared export bundle omitted $name." } }
    Copy-Item -LiteralPath $syntheticExport -Destination (Join-Path $runRoot 'raw-export.json')
    Invoke-Control (Require-Control $window 'ImportGameKnowledgeExportButton')
    Wait-Until { (Require-Control $window 'GameKnowledgeStateTextBlock').Current.Name -eq 'Ready' } 'Installed synthetic import did not reach Ready.' | Out-Null

    Set-Value (Require-Control $window 'GameKnowledgeSearchTextBox') 'SyntheticRoadCell'
    $results = Require-Control $window 'GameKnowledgeResultsDataGrid'
    Wait-Until {
        $condition = [System.Windows.Automation.PropertyCondition]::new([System.Windows.Automation.AutomationElement]::ControlTypeProperty, [System.Windows.Automation.ControlType]::DataItem)
        $results.FindAll([System.Windows.Automation.TreeScope]::Descendants, $condition).Count -eq 1
    } 'Installed exact game-knowledge search did not return one row.' | Out-Null
    Select-DataGridRow $results 0
    Invoke-Control (Require-Control $window 'CreateGameKnowledgeReceiptButton')
    Wait-Until { (Require-Control $window 'GameKnowledgeStatusTextBlock').Current.Name -like '*receipt created*' } 'Installed receipt creation did not complete.' | Out-Null
    Select-ComboItem (Require-Control $window 'GameKnowledgeIntentKindComboBox') 'Cell'
    Invoke-Control (Require-Control $window 'UseGameKnowledgeInIntentButton')
    Wait-Until { (Require-Control $window 'GeckIntentStatusTextBlock').Current.Name -like '*provisional resolution*' } 'Installed provisional GECK Intent Builder handoff did not complete.' | Out-Null
    $projectAfter = Get-ProjectSnapshot $projectRoot
    if (($projectAfter -join "`n") -ne ($projectBefore -join "`n")) { throw 'Game Knowledge workflow changed canonical project source before preview/apply.' }

    Select-ComboItem (Require-Control $window 'BuildWorkspaceComboBox') 'Basic Mod Builder'
    Select-ComboItem (Require-Control $window 'BuildWorkspaceComboBox') 'Game Knowledge'
    Invoke-Control (Require-Control $window 'PreviewClearGameKnowledgeButton')
    Invoke-Control (Require-Control $window 'ConfirmClearGameKnowledgeButton')
    Wait-Until { (Require-Control $window 'GameKnowledgeStateTextBlock').Current.Name -eq 'NotIndexed' } 'Installed private-cache clear did not return to NotIndexed.' | Out-Null
    if (Test-Path -LiteralPath $cacheRoot) { throw 'Installed private-cache clear left the owned FNV cache root behind.' }
    if ((Get-FileHash -LiteralPath $masterPath -Algorithm SHA256).Hash -ne $masterHash) { throw 'Installed workflow changed the synthetic master bytes.' }
    if ((Get-FileHash -LiteralPath $providerPath -Algorithm SHA256).Hash -ne $providerHash) { throw 'Installed workflow changed the synthetic provider bytes.' }
    $trackedAfter = Get-TrackedEditorProcessIds
    if (@($trackedAfter | Where-Object { $_ -notin $trackedBefore }).Count -ne 0) { throw 'Installed workflow created an editor, mod-manager, or game process.' }
    Write-Host 'Gate 544 installed Game Knowledge regression passed at 960x640 and 1180x760.'
}
catch {
    $failure = $_
    Write-Host ("Gate 544 installed Game Knowledge regression failed: " + $_.Exception.Message)
}
finally {
    if ($null -ne $process -and -not $process.HasExited) { $process.Kill($true); $process.WaitForExit(5000) | Out-Null }
    if ($installed -and (Test-Path -LiteralPath (Join-Path $installRoot 'unins000.exe'))) {
        Start-Process -FilePath (Join-Path $installRoot 'unins000.exe') -ArgumentList @('/VERYSILENT', '/SUPPRESSMSGBOXES', '/NORESTART') -Wait -WindowStyle Hidden
    }
    foreach ($path in @($settingsRoot, $fixtureRoot)) {
        if (Test-Path -LiteralPath $path) {
            $resolved = (Resolve-Path -LiteralPath $path).Path
            $tempPrefix = [IO.Path]::GetFullPath($env:TEMP).TrimEnd('\') + '\'
            if (-not $resolved.StartsWith($tempPrefix, [StringComparison]::OrdinalIgnoreCase)) { throw "Refusing unsafe Gate 544 cleanup: $resolved" }
            Remove-Item -LiteralPath $resolved -Recurse -Force
        }
    }
    if (Test-Path -LiteralPath $installRoot) { throw "Gate 544 installed regression did not clean installation root: $installRoot" }
}

if ($null -ne $failure) { throw $failure }
