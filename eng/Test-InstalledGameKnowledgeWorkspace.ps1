[CmdletBinding()]
param(
    [string] $InstallerPath = 'artifacts/installer/inno/local/WastelandForge-Setup-local.exe',
    [string] $FakeProviderProjectPath = 'eng/stubs/WastelandForge.FakeFNVEdit/WastelandForge.FakeFNVEdit.csproj',
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
    $nameCondition = [System.Windows.Automation.PropertyCondition]::new([System.Windows.Automation.AutomationElement]::NameProperty, $Name)
    $itemCondition = [System.Windows.Automation.AndCondition]::new(@(
        $nameCondition,
        [System.Windows.Automation.PropertyCondition]::new([System.Windows.Automation.AutomationElement]::ControlTypeProperty, [System.Windows.Automation.ControlType]::ListItem)
    ))
    $item = Wait-Until {
        $candidate = $ComboBox.FindFirst([System.Windows.Automation.TreeScope]::Descendants, $nameCondition)
        if ($null -eq $candidate) {
            $candidate = [System.Windows.Automation.AutomationElement]::RootElement.FindFirst([System.Windows.Automation.TreeScope]::Descendants, $itemCondition)
        }
        $candidate
    } "Installed combo item was not found: $Name" 10
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
$fakeProviderProject = Resolve-RepositoryFile $FakeProviderProjectPath 'Synthetic FNVEdit provider project'
$runId = [Guid]::NewGuid().ToString('N')
$installRoot = Join-Path $env:LOCALAPPDATA "WastelandForge\Gate550-$runId"
$settingsRoot = Join-Path $env:TEMP "WastelandForge-Gate550-Settings-$runId"
$fixtureRoot = Join-Path $env:TEMP "WastelandForge-Gate550-Fixture-$runId"
$gameRoot = Join-Path $fixtureRoot 'Game'
$dataRoot = Join-Path $gameRoot 'Data'
$toolRoot = Join-Path $fixtureRoot 'Tools'
$userStateRoot = Join-Path $fixtureRoot 'UserState\FalloutNV'
$documentsRoot = Join-Path $fixtureRoot 'Documents\My Games\FalloutNV'
$projectRoot = Join-Path $fixtureRoot 'Project'
$masterPath = Join-Path $dataRoot 'FalloutNV.esm'
$providerPath = Join-Path $toolRoot 'FNVEdit.exe'
$iniPath = Join-Path $documentsRoot 'Fallout.ini'
$process = $null
$installed = $false
$failure = $null

try {
    $install = Start-Process -FilePath $installer -ArgumentList @('/VERYSILENT', '/SUPPRESSMSGBOXES', '/NORESTART', "/DIR=$installRoot") -Wait -PassThru -WindowStyle Hidden
    if ($install.ExitCode -ne 0) { throw "Installer exited with $($install.ExitCode)." }
    $installed = $true
    New-Item -ItemType Directory -Path $dataRoot,$toolRoot,$userStateRoot,$documentsRoot,(Join-Path $settingsRoot 'WastelandForge') -Force | Out-Null
    Copy-Item -LiteralPath (Join-Path $installRoot 'DemoProjects\ExampleMod') -Destination $projectRoot -Recurse
    [IO.File]::WriteAllBytes($masterPath, [Text.Encoding]::UTF8.GetBytes('synthetic FalloutNV.esm identity bytes'))
    [IO.File]::WriteAllText((Join-Path $userStateRoot 'plugins.txt'), 'synthetic user load-order state', [Text.UTF8Encoding]::new($false))
    [IO.File]::WriteAllText($iniPath, "[General]`r`nbUseThreadedAI=1`r`n", [Text.UTF8Encoding]::new($false))
    $publish = Start-Process -FilePath 'dotnet' -ArgumentList @('publish', $fakeProviderProject, '-c', 'Release', '-o', $toolRoot, '-p:UseAppHost=true', '-m:1', '/nodeReuse:false') -Wait -PassThru -NoNewWindow
    if ($publish.ExitCode -ne 0 -or -not (Test-Path -LiteralPath $providerPath -PathType Leaf)) { throw "Synthetic FNVEdit provider publish failed with exit code $($publish.ExitCode)." }
    $masterHash = (Get-FileHash -LiteralPath $masterPath -Algorithm SHA256).Hash
    $providerHash = (Get-FileHash -LiteralPath $providerPath -Algorithm SHA256).Hash
    $iniHash = (Get-FileHash -LiteralPath $iniPath -Algorithm SHA256).Hash
    $projectBefore = Get-ProjectSnapshot $projectRoot
    $dataBefore = Get-ProjectSnapshot $dataRoot
    $toolsBefore = Get-ProjectSnapshot $toolRoot
    $userStateBefore = Get-ProjectSnapshot $userStateRoot
    $settings = [ordered]@{
        ProjectRoot = $projectRoot
        GameRoot = $gameRoot
        DataRoot = $dataRoot
        FnvIniPath = $iniPath
        Mo2Path = ''
        Mo2ModsRoot = ''
        ToolPaths = [ordered]@{ geck = ''; xedit = $providerPath }
    }
    $settings | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath (Join-Path $settingsRoot 'WastelandForge\app-settings.json') -Encoding utf8NoBOM
    $trackedBefore = Get-TrackedEditorProcessIds

    $start = [Diagnostics.ProcessStartInfo]::new((Join-Path $installRoot 'WastelandForge.exe'))
    $start.UseShellExecute = $false
    $start.Environment['LOCALAPPDATA'] = $settingsRoot
    $start.Environment['WASTELANDFORGE_LOCAL_APP_DATA'] = (Join-Path $settingsRoot 'WastelandForge')
    $start.Environment['WASTELANDFORGE_FNV_USER_STATE'] = $userStateRoot
    $process = [Diagnostics.Process]::Start($start)
    Wait-Until { $process.Refresh(); $process.MainWindowHandle -ne [IntPtr]::Zero } 'Installed WastelandForge window did not open.' | Out-Null
    $window = [System.Windows.Automation.AutomationElement]::FromHandle($process.MainWindowHandle)

    [WfGameKnowledgeWindow]::MoveWindow($process.MainWindowHandle, 40, 40, 960, 640, $true) | Out-Null
    Select-ComboItem (Require-Control $window 'BuildWorkspaceComboBox') 'Game Knowledge'
    foreach ($controlId in @('GameKnowledgeTabItem','GameKnowledgeStateTextBlock','PrepareGameKnowledgeExportButton','PreparePrivateGameKnowledgeRunButton','RunGameKnowledgeExportButton','ImportGameKnowledgeExportButton','GameKnowledgeSearchTextBox')) {
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
    $knowledgeScroller = Require-Control $window 'GameKnowledgeScrollViewer'
    $scrollPattern = $knowledgeScroller.GetCurrentPattern([System.Windows.Automation.ScrollPattern]::Pattern)
    if (-not $scrollPattern.Current.VerticallyScrollable) { throw 'Game Knowledge workspace is not vertically scrollable at 960x640.' }
    foreach ($target in @(@('GameKnowledgeResultsDataGrid', 45), @('GameKnowledgeDetailsTextBox', 80))) {
        $scrollPattern.SetScrollPercent([System.Windows.Automation.ScrollPattern]::NoScroll, $target[1])
        $control = Require-Control $window $target[0]
        Wait-Until {
            -not $control.Current.IsOffscreen -and $control.Current.BoundingRectangle.Width -gt 0 -and $control.Current.BoundingRectangle.Height -gt 0
        } "Game Knowledge control cannot be brought into view at 960x640: $($target[0])" 10 | Out-Null
    }

    [WfGameKnowledgeWindow]::MoveWindow($process.MainWindowHandle, 40, 40, 1180, 760, $true) | Out-Null
    Start-Sleep -Milliseconds 300
    Invoke-Control (Require-Control $window 'PreparePrivateGameKnowledgeRunButton')
    Wait-Until { (Require-Control $window 'GameKnowledgeStateTextBlock').Current.Name -eq 'ApprovalRequired' } 'Installed private export preview did not reach ApprovalRequired.' | Out-Null
    $cacheRoot = Join-Path $settingsRoot 'WastelandForge\game-knowledge\fnv'
    $runRoot = Wait-Until { Get-ChildItem -LiteralPath (Join-Path $cacheRoot 'runs') -Directory -ErrorAction SilentlyContinue | Sort-Object LastWriteTimeUtc -Descending | Select-Object -First 1 -ExpandProperty FullName } 'Prepared private export run was not created.'
    foreach ($name in @('WastelandForgeFNVGameKnowledge.pas','run-manifest.json','execution-plan.json','state\Plugins.txt')) { if (-not (Test-Path -LiteralPath (Join-Path $runRoot $name) -PathType Leaf)) { throw "Prepared private execution bundle omitted $name." } }
    $plan = Get-Content -LiteralPath (Join-Path $runRoot 'execution-plan.json') -Raw | ConvertFrom-Json
    if ($plan.formatVersion -ne '0.2.0' -or $plan.arguments.Count -ne 13) { throw "Installed private execution plan did not contain the INI-bound 0.2.0 contract with exactly 13 arguments." }
    if ($plan.arguments[0] -ne '-FNV' -or $plan.arguments[1] -ne '-view' -or $plan.arguments[2] -ne '-autoload' -or $plan.arguments[4] -ne '-autoexit') { throw 'Installed private execution plan changed the fixed argument order.' }
    if ($plan.arguments[5] -ne ('-D:' + $dataRoot + '\') -or $plan.arguments[6] -ne ('-I:' + $iniPath) -or -not $plan.arguments[7].StartsWith('-P:', [StringComparison]::Ordinal)) { throw 'Installed private execution plan did not place the exact Fallout.ini argument after -D and before -P.' }
    if ($plan.inputs.ini.path -ne $iniPath -or $plan.inputs.ini.fileName -ne 'Fallout.ini' -or $plan.inputs.ini.sha256 -ne $iniHash.ToLowerInvariant() -or $plan.writePolicy.protectedFiles.Count -ne 1 -or $plan.writePolicy.protectedFiles[0] -ne $iniPath) { throw 'Installed private execution plan did not bind and protect the exact Fallout.ini evidence.' }
    $previewDetails = (Require-Control $window 'GameKnowledgeDetailsTextBox').GetCurrentPattern([System.Windows.Automation.ValuePattern]::Pattern).Current.Value
    if (-not $previewDetails.Contains($iniPath, [StringComparison]::OrdinalIgnoreCase) -or -not $previewDetails.Contains($iniHash.ToLowerInvariant(), [StringComparison]::Ordinal)) { throw 'Installed operator preview did not expose the exact Fallout.ini path and digest.' }
    if ((Get-Content -LiteralPath (Join-Path $runRoot 'state\Plugins.txt') -Raw) -ne "FalloutNV.esm`r`n") { throw 'Installed private execution plan did not isolate FalloutNV.esm.' }

    Invoke-Control (Require-Control $window 'RunGameKnowledgeExportButton')
    Wait-Until { (Require-Control $window 'RunGameKnowledgeExportButton').Current.Name -eq 'Confirm Run Export' } 'Installed private execution did not expose inline confirmation.' | Out-Null
    if ((Require-Control $window 'GameKnowledgeStateTextBlock').Current.Name -ne 'ApprovalRequired') { throw 'Installed private execution started before inline confirmation.' }
    if (@((Get-TrackedEditorProcessIds) | Where-Object { $_ -notin $trackedBefore }).Count -ne 0) { throw 'Installed private execution started a provider before inline confirmation.' }
    Invoke-Control (Require-Control $window 'RunGameKnowledgeExportButton')
    Wait-Until { (Require-Control $window 'GameKnowledgeStateTextBlock').Current.Name -eq 'Ready' } 'Installed synthetic private execution did not reach Ready.' | Out-Null
    foreach ($name in @('raw-export.json','logs\FNVEdit.log.txt','state\Plugins.fnvviewsettings','cache\synthetic-cache.txt','temp\synthetic-temp.txt','execution-receipt.json')) { if (-not (Test-Path -LiteralPath (Join-Path $runRoot $name) -PathType Leaf)) { throw "Synthetic private execution omitted audited output $name." } }
    if ((Get-ChildItem -LiteralPath (Join-Path $runRoot 'backups') -Force).Count -ne 0) { throw 'Synthetic private execution wrote a backup.' }
    $executionReceipt = Get-Content -LiteralPath (Join-Path $runRoot 'execution-receipt.json') -Raw | ConvertFrom-Json
    if ($executionReceipt.formatVersion -ne '0.2.0' -or -not $executionReceipt.success -or -not $executionReceipt.audit.dataUnchanged -or -not $executionReceipt.audit.providerUnchanged -or -not $executionReceipt.audit.userStateUnchanged -or -not $executionReceipt.audit.iniUnchanged -or -not $executionReceipt.ini.unchanged -or -not $executionReceipt.audit.privateWritesAllowed -or -not $executionReceipt.audit.backupsEmpty) { throw 'Installed execution receipt did not prove a successful INI-bound audit.' }
    if ($executionReceipt.ini.path -ne $iniPath -or $executionReceipt.ini.before.sha256 -ne $iniHash.ToLowerInvariant() -or $executionReceipt.ini.after.sha256 -ne $iniHash.ToLowerInvariant()) { throw 'Installed execution receipt did not preserve exact Fallout.ini before/after evidence.' }
    $index = Get-Content -LiteralPath (Join-Path $cacheRoot 'index.json') -Raw | ConvertFrom-Json
    if ($index.formatVersion -ne '0.2.0' -or -not $index.safety.forgeExecutedXEdit) { throw 'Installed automated export was not sealed as the 0.2.0 Forge-executed lineage.' }

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
    if (((Get-ProjectSnapshot $dataRoot) -join "`n") -ne ($dataBefore -join "`n")) { throw 'Installed private execution changed the synthetic Data tree.' }
    if (((Get-ProjectSnapshot $toolRoot) -join "`n") -ne ($toolsBefore -join "`n")) { throw 'Installed private execution changed the synthetic provider tree.' }
    if (((Get-ProjectSnapshot $userStateRoot) -join "`n") -ne ($userStateBefore -join "`n")) { throw 'Installed private execution changed the isolated user-state tree.' }

    Select-ComboItem (Require-Control $window 'BuildWorkspaceComboBox') 'Basic Mod Builder'
    Select-ComboItem (Require-Control $window 'BuildWorkspaceComboBox') 'Game Knowledge'
    Invoke-Control (Require-Control $window 'PreviewClearGameKnowledgeButton')
    Invoke-Control (Require-Control $window 'ConfirmClearGameKnowledgeButton')
    Wait-Until { (Require-Control $window 'GameKnowledgeStateTextBlock').Current.Name -eq 'NotIndexed' } 'Installed private-cache clear did not return to NotIndexed.' | Out-Null
    if (Test-Path -LiteralPath $cacheRoot) { throw 'Installed private-cache clear left the owned FNV cache root behind.' }
    if ((Get-FileHash -LiteralPath $masterPath -Algorithm SHA256).Hash -ne $masterHash) { throw 'Installed workflow changed the synthetic master bytes.' }
    if ((Get-FileHash -LiteralPath $providerPath -Algorithm SHA256).Hash -ne $providerHash) { throw 'Installed workflow changed the synthetic provider bytes.' }
    if ((Get-FileHash -LiteralPath $iniPath -Algorithm SHA256).Hash -ne $iniHash) { throw 'Installed workflow changed the synthetic Fallout.ini bytes.' }
    $trackedAfter = Get-TrackedEditorProcessIds
    if (@($trackedAfter | Where-Object { $_ -notin $trackedBefore }).Count -ne 0) { throw 'Installed workflow created an editor, mod-manager, or game process.' }
    Write-Host 'Gate 550 installed synthetic INI-bound private xEdit execution regression passed at 960x640 and 1180x760.'
}
catch {
    $failure = $_
    Write-Host ("Gate 550 installed synthetic INI-bound private xEdit execution regression failed: " + $_.Exception.Message)
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
            if (-not $resolved.StartsWith($tempPrefix, [StringComparison]::OrdinalIgnoreCase)) { throw "Refusing unsafe Gate 550 cleanup: $resolved" }
            Remove-Item -LiteralPath $resolved -Recurse -Force
        }
    }
    if (Test-Path -LiteralPath $installRoot) { throw "Gate 550 installed regression did not clean installation root: $installRoot" }
}

if ($null -ne $failure) { throw $failure }
