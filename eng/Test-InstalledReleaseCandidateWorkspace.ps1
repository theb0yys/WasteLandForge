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
    Wait-Until { $Control.Current.IsEnabled } "Installed UI control did not become enabled: $($Control.Current.AutomationId)" | Out-Null
    $Control.GetCurrentPattern([System.Windows.Automation.InvokePattern]::Pattern).Invoke()
}

function Select-Tab([System.Windows.Automation.AutomationElement] $Window, [string] $Name) {
    $condition = [System.Windows.Automation.PropertyCondition]::new([System.Windows.Automation.AutomationElement]::NameProperty, $Name)
    $tab = $Window.FindFirst([System.Windows.Automation.TreeScope]::Descendants, $condition)
    if ($null -eq $tab) { throw "Installed tab was not found: $Name" }
    $tab.GetCurrentPattern([System.Windows.Automation.SelectionItemPattern]::Pattern).Select()
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
$mo2Root = Join-Path $env:TEMP "WastelandForge-Gate466-MO2-$runId"
$releaseHandoffRoot = Join-Path $env:TEMP "WastelandForge-Gate477-Release-$runId"
$geckProjectRoot = Join-Path $env:TEMP "WastelandForge-Gate480-Project-$runId"
$geckStubRoot = Join-Path $env:TEMP "WastelandForge-Gate480-Stub-$runId"
$xeditProjectRoot = Join-Path $env:TEMP "WastelandForge-Gate482-Project-$runId"
$xeditStubRoot = Join-Path $env:TEMP "WastelandForge-Gate482-Stub-$runId"
$process = $null
$installed = $false
$failure = $null
$createdMo2Requests = @()
$createdMo2Receipts = @()

try {
    $install = Start-Process -FilePath $installer -ArgumentList @('/VERYSILENT', '/SUPPRESSMSGBOXES', '/NORESTART', "/DIR=$installRoot") -Wait -PassThru -WindowStyle Hidden
    if ($install.ExitCode -ne 0) { throw "Installer exited with $($install.ExitCode)." }
    $installed = $true
    Write-Host 'Release Candidate UI regression: installed.'
    Copy-Item -LiteralPath (Join-Path $installRoot 'DemoProjects\CombinedModExample') -Destination $readyRoot -Recurse
    Copy-Item -LiteralPath (Join-Path $installRoot 'DemoProjects\CombinedModExample') -Destination $blockedRoot -Recurse
    Copy-Item -LiteralPath (Join-Path $installRoot 'DemoProjects\ExampleMod') -Destination $geckProjectRoot -Recurse
    Copy-Item -LiteralPath (Join-Path $installRoot 'DemoProjects\ExampleMod') -Destination $xeditProjectRoot -Recurse
    Copy-Item -LiteralPath (Join-Path $installRoot 'ForgeBackend') -Destination $geckStubRoot -Recurse
    Move-Item -LiteralPath (Join-Path $geckStubRoot 'forge.exe') -Destination (Join-Path $geckStubRoot 'GECK.exe')
    Copy-Item -LiteralPath (Join-Path $installRoot 'ForgeBackend') -Destination $xeditStubRoot -Recurse
    Move-Item -LiteralPath (Join-Path $xeditStubRoot 'forge.exe') -Destination (Join-Path $xeditStubRoot 'xEdit.exe')
    $xeditPlugin = Join-Path $xeditProjectRoot 'src\plugins\ReviewTarget.esp'
    $xeditRegistry = Join-Path $xeditProjectRoot 'src\registries\plugin-artifacts\main.json'
    New-Item -ItemType Directory -Path (Split-Path $xeditPlugin) -Force | Out-Null
    New-Item -ItemType Directory -Path (Split-Path $xeditRegistry) -Force | Out-Null
    [IO.File]::WriteAllBytes($xeditPlugin, [byte[]](0x53,0x59,0x4E,0x54,0x48))
    $xeditPluginHash = (Get-FileHash -LiteralPath $xeditPlugin -Algorithm SHA256).Hash.ToLowerInvariant()
    $xeditManifestPath = Join-Path $xeditProjectRoot 'wastelandforge.json'
    $xeditManifest = Get-Content -LiteralPath $xeditManifestPath -Raw | ConvertFrom-Json
    $xeditManifest.schemaVersion = '0.3.0'
    $xeditManifest.registries | Add-Member -NotePropertyName pluginArtifacts -NotePropertyValue 'src/registries/plugin-artifacts/' -Force
    $xeditManifest | ConvertTo-Json -Depth 30 | Set-Content -LiteralPath $xeditManifestPath -Encoding utf8NoBOM
    [ordered]@{ schemaVersion='0.1.0'; kind='plugin-artifact'; id='io.wastelandforge.example.pluginartifacts'; plugins=@([ordered]@{ id='io.wastelandforge.example.reviewtarget'; file='src/plugins/ReviewTarget.esp'; pluginType='esp'; dataPath='ReviewTarget.esp'; sha256=$xeditPluginHash; length=5; authoringTool='xedit'; reviewStatus='pending' }) } | ConvertTo-Json -Depth 20 | Set-Content -LiteralPath $xeditRegistry -Encoding utf8NoBOM
    $geckPackage = Start-Process -FilePath (Join-Path $installRoot 'ForgeBackend\forge.exe') -ArgumentList @('package', $geckProjectRoot, '--target', 'geck-handoff', '--format', 'json') -WorkingDirectory $geckProjectRoot -Wait -PassThru -WindowStyle Hidden
    if ($geckPackage.ExitCode -ne 0) { throw "Synthetic GECK handoff package exited with $($geckPackage.ExitCode)." }
    New-Item -ItemType Directory -Path $mo2Root | Out-Null
    New-Item -ItemType Directory -Path $releaseHandoffRoot | Out-Null
    $blockedManifest = Join-Path $blockedRoot 'wastelandforge.json'
    $blocked = Get-Content -LiteralPath $blockedManifest -Raw | ConvertFrom-Json
    $blocked.id = 'INVALID ID'
    $blocked | ConvertTo-Json -Depth 20 | Set-Content -LiteralPath $blockedManifest -Encoding utf8NoBOM

    $start = [Diagnostics.ProcessStartInfo]::new((Join-Path $installRoot 'WastelandForge.exe'))
    $start.UseShellExecute = $false
    $start.Environment['LOCALAPPDATA'] = $settingsRoot
    $process = [Diagnostics.Process]::Start($start)
    Wait-Until { $process.Refresh(); $process.MainWindowHandle -ne [IntPtr]::Zero } 'Installed WastelandForge window did not open.' | Out-Null
    Write-Host 'Release Candidate UI regression: window opened.'
    $window = [System.Windows.Automation.AutomationElement]::FromHandle($process.MainWindowHandle)
    Set-Value (Require-Control $window 'ProjectPathTextBox') $readyRoot
    Select-Tab $window 'Project Outputs'
    $mo2PackageRoot = Join-Path $installRoot 'Integrations\MO2\Package'
    $mo2PackageArchive = Join-Path $mo2PackageRoot 'WastelandForge-MO2-Bridge-0.1.0.zip'
    foreach ($relative in @('WastelandForge-MO2-Bridge-0.1.0.zip','WastelandForge-MO2-Bridge-0.1.0.zip.sha256','package-build-manifest.json','INSTALL.md')) { if (-not (Test-Path -LiteralPath (Join-Path $mo2PackageRoot $relative) -PathType Leaf)) { throw "Installed MO2 companion package evidence is missing: $relative" } }
    $mo2PackageBuild = Get-Content -LiteralPath (Join-Path $mo2PackageRoot 'package-build-manifest.json') -Raw | ConvertFrom-Json
    if ((Get-FileHash -LiteralPath $mo2PackageArchive -Algorithm SHA256).Hash.ToLowerInvariant() -ne $mo2PackageBuild.sha256) { throw 'Installed MO2 companion package digest is invalid.' }
    foreach ($controlId in @('OpenMo2CompanionFolderButton','OpenMo2CompanionArchiveButton','OpenMo2CompanionGuideButton')) { if (-not (Require-Control $window $controlId).Current.IsEnabled) { throw "Installed verified MO2 companion handoff action was disabled: $controlId" } }
    $mo2CompanionStatus = Require-Control $window 'Mo2CompanionPackageStatusTextBlock'
    if ($mo2CompanionStatus.Current.Name -ne 'Verified optional MO2 companion package ready.') { throw "Installed MO2 companion handoff was not verified: $($mo2CompanionStatus.Current.Name)" }
    Write-Host 'Gate 486 UI regression: verified contained MO2 companion package handoff ready without MO2 execution.'
    $workflow = Require-Control $window 'ProjectOutputWorkflowComboBox'
    Select-ComboItem $workflow 'Build FOMOD installer package'
    Invoke-Control (Require-Control $window 'RunProjectOutputWorkflowButton')
    $projectOutputStatus = Require-Control $window 'ProjectOutputsStatusTextBlock'
    Wait-Until { $projectOutputStatus.Current.Name -eq 'FOMOD installer package completed.' } "Installed FOMOD workflow did not complete; status: $($projectOutputStatus.Current.Name)" | Out-Null
    foreach ($relative in @('dist\fomod\package.zip', 'dist\fomod\staging\fomod\info.xml', 'dist\fomod\staging\fomod\ModuleConfig.xml', 'dist\fomod\fomod-manifest.json', 'dist\fomod\build-manifest.json', 'dist\fomod\checksums.sha256')) {
        if (-not (Test-Path -LiteralPath (Join-Path $readyRoot $relative) -PathType Leaf)) { throw "Installed FOMOD workflow did not write $relative" }
    }
    Write-Host 'Gate 473 UI regression: FOMOD package created.'
    Remove-Item -LiteralPath (Join-Path $readyRoot 'dist\fomod') -Recurse -Force

    Select-Tab $window 'Release Candidate'

    $projectPath = Require-Control $window 'ProjectPathTextBox'
    $run = Require-Control $window 'RunReleaseCandidateButton'
    $state = Require-Control $window 'ReleaseCandidateStateTextBlock'
    Set-Value $projectPath $readyRoot
    Invoke-Control $run
    Wait-Until { $state.Current.Name -eq 'Candidate ready' } "Ready project did not reach Candidate ready; current state: $($state.Current.Name)" | Out-Null
    if (-not (Test-Path -LiteralPath (Join-Path $readyRoot 'dist\fomod\package.zip') -PathType Leaf)) { throw 'Release Candidate did not recreate the FOMOD distributable.' }
    foreach ($relative in @('dist\release-prepare\archives\release.zip', 'dist\release-prepare\staging\release-payload.json', 'dist\release-prepare\build-manifest.json', 'dist\release-prepare\checksums.sha256')) {
        if (-not (Test-Path -LiteralPath (Join-Path $readyRoot $relative) -PathType Leaf)) { throw "Release Candidate did not prepare $relative" }
    }
    $preparedPayload = Get-Content -Raw -LiteralPath (Join-Path $readyRoot 'dist\release-prepare\staging\release-payload.json') | ConvertFrom-Json
    if ($preparedPayload.payload.status -ne 'staged-fomod') { throw "Release Candidate prepared payload status was $($preparedPayload.payload.status)." }
    Write-Host 'Gate 474 UI regression: candidate recreated FOMOD distributable.'
    Write-Host 'Gate 476 UI regression: candidate prepared the FOMOD release archive.'
    Write-Host 'Release Candidate UI regression: ready candidate verified.'

    $process.Refresh()
    $window = [System.Windows.Automation.AutomationElement]::FromHandle($process.MainWindowHandle)
    $state = Require-Control $window 'ReleaseCandidateStateTextBlock'

    foreach ($controlId in @('OpenCandidateFomodFolderButton', 'OpenCandidateFomodArchiveButton', 'OpenCandidatePackageFolderButton', 'OpenCandidatePackageArchiveButton', 'OpenCandidateReleaseEvidenceButton', 'OpenCandidateReleaseHandoffButton', 'OpenCandidatePreparedFolderButton', 'OpenCandidatePreparedArchiveButton')) {
        Write-Host "Release Candidate UI regression: checking $controlId."
        if (-not (Require-Control $window $controlId).Current.IsEnabled) { throw "Ready evidence action was disabled: $controlId" }
    }
    Write-Host 'Release Candidate UI regression: evidence actions verified.'
    $localReleaseDestination = Require-Control $window 'LocalReleaseDestinationTextBox'
    $previewLocalRelease = Require-Control $window 'PreviewLocalReleaseHandoffButton'
    $createLocalRelease = Require-Control $window 'CreateLocalReleaseHandoffButton'
    $localReleaseStatus = Require-Control $window 'LocalReleaseHandoffStatusTextBlock'
    Set-Value $localReleaseDestination $releaseHandoffRoot
    Invoke-Control $previewLocalRelease
    Wait-Until { $createLocalRelease.Current.IsEnabled } "Installed local release preview did not enable creation; status: $($localReleaseStatus.Current.Name)" | Out-Null
    if ((Get-ChildItem -LiteralPath $releaseHandoffRoot -Force).Count -ne 0) { throw 'Local release preview wrote files.' }
    Invoke-Control $createLocalRelease
    $releaseArchive = Join-Path $releaseHandoffRoot 'Combined-Mod-Example-0.1.0.zip'
    Wait-Until { Test-Path -LiteralPath $releaseArchive -PathType Leaf } "Installed local release handoff was not created; status: $($localReleaseStatus.Current.Name)" | Out-Null
    foreach ($path in @($releaseArchive, "$releaseArchive.sha256", (Join-Path $releaseHandoffRoot 'Combined-Mod-Example-0.1.0.handoff.json'))) { if (-not (Test-Path -LiteralPath $path -PathType Leaf)) { throw "Installed local release evidence is missing: $path" } }
    if (-not (Require-Control $window 'OpenLocalReleaseHandoffButton').Current.IsEnabled) { throw 'Open Destination did not enable after local release creation.' }
    Write-Host 'Gate 477 UI regression: versioned local release handoff created.'
    $verifyLocalRelease = Require-Control $window 'VerifyLocalReleaseHandoffButton'
    if (-not $verifyLocalRelease.Current.IsEnabled) { throw 'Verify Existing did not enable for the handoff folder.' }
    Invoke-Control $verifyLocalRelease
    Wait-Until { $localReleaseStatus.Current.Name -like 'Local release handoff verified:*' } "Installed local release verification failed; status: $($localReleaseStatus.Current.Name)" | Out-Null
    Write-Host 'Gate 478 UI regression: existing local release handoff verified read-only.'
    $candidateModsRoot = Require-Control $window 'CandidateMo2ModsRootTextBox'
    $candidateModName = Require-Control $window 'CandidateMo2ModNameTextBox'
    $previewTestCopy = Require-Control $window 'PreviewCandidateMo2TestCopyButton'
    $createTestCopy = Require-Control $window 'CreateCandidateMo2TestCopyButton'
    $testCopyStatus = Require-Control $window 'CandidateMo2TestCopyStatusTextBlock'
    Set-Value $candidateModsRoot $mo2Root
    Set-Value $candidateModName 'Gate 466 Synthetic Test'
    Invoke-Control $previewTestCopy
    Wait-Until { $createTestCopy.Current.IsEnabled } "Installed test-copy preview did not enable creation; status: $($testCopyStatus.Current.Name)" | Out-Null
    if (Test-Path -LiteralPath (Join-Path $mo2Root 'Gate 466 Synthetic Test')) { throw 'Dry-run preview created the external destination.' }
    Invoke-Control $createTestCopy
    $testCopyDestination = Join-Path $mo2Root 'Gate 466 Synthetic Test'
    Wait-Until { Test-Path -LiteralPath $testCopyDestination -PathType Container } "Installed app did not create the MO2 test copy; status: $($testCopyStatus.Current.Name)" | Out-Null
    if (Test-Path -LiteralPath (Join-Path $testCopyDestination 'Data')) { throw 'Installed test copy contains an incorrect nested Data directory.' }
    if ((Get-ChildItem -LiteralPath $testCopyDestination -File -Recurse).Count -lt 1) { throw 'Installed test copy contains no payload files.' }
    if (-not (Require-Control $window 'OpenCandidateMo2TestCopyButton').Current.IsEnabled) { throw "Open Test Copy did not enable after verified creation. Status: $($testCopyStatus.Current.Name)" }
    if ($state.Current.Name -ne 'Candidate ready') { throw "Existing-package export did not preserve Candidate ready; state: $($state.Current.Name)" }
    $testCopyDetails = (Require-Control $window 'CandidateMo2TestCopyDetailsTextBox').GetCurrentPattern([System.Windows.Automation.ValuePattern]::Pattern).Current.Value
    foreach ($expected in @('Manifest:', 'Checksums:')) { if (-not $testCopyDetails.Contains($expected, [StringComparison]::Ordinal)) { throw "Test-copy handoff did not expose $expected evidence." } }
    Write-Host 'Gate 466 UI regression: verified MO2 test copy created.'
    $geckManifest = Join-Path $geckProjectRoot 'dist\geck-handoff\handoff-manifest.json'
    $geckWorklist = Join-Path $geckProjectRoot 'dist\geck-handoff\worklists\unresolved-actions.tsv'
    $geckManifestBefore = (Get-FileHash -LiteralPath $geckManifest -Algorithm SHA256).Hash
    $geckWorklistBefore = (Get-FileHash -LiteralPath $geckWorklist -Algorithm SHA256).Hash
    Set-Value $projectPath $geckProjectRoot
    Select-Tab $window 'Settings'
    Set-Value (Require-Control $window 'GeckPathTextBox') (Join-Path $geckStubRoot 'GECK.exe')
    Select-Tab $window 'GECK Handoff'
    Invoke-Control (Require-Control $window 'LoadGeckHandoffButton')
    $geckStatus = Require-Control $window 'GeckHandoffWorkspaceStatusTextBlock'
    $previewGeck = Require-Control $window 'PreviewGeckLaunchButton'
    $launchGeck = Require-Control $window 'LaunchGeckButton'
    Wait-Until { $previewGeck.Current.IsEnabled } "Installed GECK handoff did not become launch-preview ready; status: $($geckStatus.Current.Name)" | Out-Null
    Invoke-Control $previewGeck
    Wait-Until { $launchGeck.Current.IsEnabled } "Installed GECK launch preview did not enable launch; status: $($geckStatus.Current.Name)" | Out-Null
    $geckDetails = (Require-Control $window 'GeckLaunchDetailsTextBox').GetCurrentPattern([System.Windows.Automation.ValuePattern]::Pattern).Current.Value
    foreach ($expected in @('GECK.exe', 'Arguments: none', 'Direct physical launch only', 'MO2 VFS is not used')) { if (-not $geckDetails.Contains($expected, [StringComparison]::Ordinal)) { throw "GECK launch preview did not expose: $expected" } }
    $previewGeckMo2 = Require-Control $window 'PreviewGeckMo2RequestButton'
    $createGeckMo2 = Require-Control $window 'CreateGeckMo2RequestButton'
    Invoke-Control $previewGeckMo2
    Wait-Until { $createGeckMo2.Current.IsEnabled } "Installed GECK MO2-request preview did not enable creation; status: $($geckStatus.Current.Name)" | Out-Null
    $geckMo2Details = (Require-Control $window 'GeckLaunchDetailsTextBox').GetCurrentPattern([System.Windows.Automation.ValuePattern]::Pattern).Current.Value
    $geckMo2RequestPath = ($geckMo2Details -split "`r?`n" | Where-Object { $_.StartsWith('MO2 request: ', [StringComparison]::Ordinal) } | Select-Object -First 1).Substring(13)
    $expectedMo2RequestRoot = [IO.Path]::GetFullPath((Join-Path ([Environment]::GetFolderPath([Environment+SpecialFolder]::LocalApplicationData)) 'WastelandForge\Mo2LaunchRequests'))
    if ([IO.Path]::GetFullPath((Split-Path -Parent $geckMo2RequestPath)) -ne $expectedMo2RequestRoot) { throw "Installed GECK MO2 request preview escaped the private request root: $geckMo2RequestPath" }
    Invoke-Control $createGeckMo2
    Wait-Until { Test-Path -LiteralPath $geckMo2RequestPath -PathType Leaf } 'Installed GECK MO2 launch request was not created.' | Out-Null
    $createdMo2Requests += $geckMo2RequestPath
    $geckMo2Request = Get-Content -LiteralPath $geckMo2RequestPath -Raw | ConvertFrom-Json
    if ($geckMo2Request.tool.kind -ne 'geck' -or $geckMo2Request.tool.arguments.Count -ne 0 -or $geckMo2Request.safety.automaticLaunch -ne $false) { throw 'Installed GECK MO2 launch request contract is invalid.' }
    Invoke-Control $launchGeck
    Wait-Until { $geckStatus.Current.Name -like 'GECK process created (PID*' } "Installed controlled GECK stub was not launched; status: $($geckStatus.Current.Name)" | Out-Null
    if ((Get-FileHash -LiteralPath $geckManifest -Algorithm SHA256).Hash -ne $geckManifestBefore -or (Get-FileHash -LiteralPath $geckWorklist -Algorithm SHA256).Hash -ne $geckWorklistBefore) { throw 'GECK launch changed deterministic handoff evidence.' }
    Write-Host 'Gate 480 UI regression: preview-gated controlled GECK stub process created with handoff evidence unchanged.'
    $xeditPluginBefore = (Get-FileHash -LiteralPath $xeditPlugin -Algorithm SHA256).Hash
    $xeditRegistryBefore = (Get-FileHash -LiteralPath $xeditRegistry -Algorithm SHA256).Hash
    Set-Value $projectPath $xeditProjectRoot
    Select-Tab $window 'Settings'
    Set-Value (Require-Control $window 'XEditPathTextBox') (Join-Path $xeditStubRoot 'xEdit.exe')
    Select-Tab $window 'Plugin Intake'
    Invoke-Control (Require-Control $window 'LoadPendingPluginsButton')
    $previewXEdit = Require-Control $window 'PreviewXEditLaunchButton'
    $launchXEdit = Require-Control $window 'LaunchXEditButton'
    $pluginStatus = Require-Control $window 'PluginIntakeStatusTextBlock'
    Wait-Until { $previewXEdit.Current.IsEnabled } "Installed pending plugin did not become xEdit-preview ready; status: $($pluginStatus.Current.Name)" | Out-Null
    Invoke-Control $previewXEdit
    Wait-Until { $launchXEdit.Current.IsEnabled } "Installed xEdit launch preview did not enable launch; status: $($pluginStatus.Current.Name)" | Out-Null
    $xeditDetails = (Require-Control $window 'XEditLaunchDetailsTextBox').GetCurrentPattern([System.Windows.Automation.ValuePattern]::Pattern).Current.Value
    foreach ($expected in @('xEdit.exe', 'Arguments: none', 'ReviewTarget.esp', 'not passed as an argument', 'MO2 VFS is not used')) { if (-not $xeditDetails.Contains($expected, [StringComparison]::Ordinal)) { throw "xEdit launch preview did not expose: $expected" } }
    $previewXEditMo2 = Require-Control $window 'PreviewXEditMo2RequestButton'
    $createXEditMo2 = Require-Control $window 'CreateXEditMo2RequestButton'
    Invoke-Control $previewXEditMo2
    Wait-Until { $createXEditMo2.Current.IsEnabled } "Installed xEdit MO2-request preview did not enable creation; status: $($pluginStatus.Current.Name)" | Out-Null
    $xeditMo2Details = (Require-Control $window 'XEditLaunchDetailsTextBox').GetCurrentPattern([System.Windows.Automation.ValuePattern]::Pattern).Current.Value
    $xeditMo2RequestPath = ($xeditMo2Details -split "`r?`n" | Where-Object { $_.StartsWith('MO2 request: ', [StringComparison]::Ordinal) } | Select-Object -First 1).Substring(13)
    if ([IO.Path]::GetFullPath((Split-Path -Parent $xeditMo2RequestPath)) -ne $expectedMo2RequestRoot) { throw "Installed xEdit MO2 request preview escaped the private request root: $xeditMo2RequestPath" }
    Invoke-Control $createXEditMo2
    Wait-Until { Test-Path -LiteralPath $xeditMo2RequestPath -PathType Leaf } 'Installed xEdit MO2 launch request was not created.' | Out-Null
    $createdMo2Requests += $xeditMo2RequestPath
    $xeditMo2Request = Get-Content -LiteralPath $xeditMo2RequestPath -Raw | ConvertFrom-Json
    if ($null -eq $xeditMo2Request -or $xeditMo2Request.project.contextKind -ne 'pending-plugin-review' -or $xeditMo2Request.tool.arguments.Count -ne 0 -or $xeditMo2Request.safety.automaticLaunch -ne $false) { throw 'Installed xEdit MO2 launch request contract is invalid.' }
    Invoke-Control $launchXEdit
    Wait-Until { $pluginStatus.Current.Name -like 'xEdit process created (PID*' } "Installed controlled xEdit stub was not launched; status: $($pluginStatus.Current.Name)" | Out-Null
    if ((Get-FileHash -LiteralPath $xeditPlugin -Algorithm SHA256).Hash -ne $xeditPluginBefore -or (Get-FileHash -LiteralPath $xeditRegistry -Algorithm SHA256).Hash -ne $xeditRegistryBefore) { throw 'xEdit launch changed plugin or registry bytes.' }
    Write-Host 'Gate 482 UI regression: preview-gated controlled xEdit stub process created with plugin and registry unchanged.'
    foreach ($requestPath in $createdMo2Requests) { if (Test-Path -LiteralPath ($requestPath -replace '\.json$', '.receipt.json')) { throw 'Desktop request generation unexpectedly created an MO2 receipt.' } }
    if (-not (Test-Path -LiteralPath (Join-Path $installRoot 'Integrations\MO2\wastelandforge_bridge\core.py') -PathType Leaf)) { throw 'Installed optional MO2 companion source is missing.' }
    Write-Host 'Gate 484 UI regression: GECK/xEdit MO2 requests created without MO2 execution; companion source installed.'
    $syntheticReceiptPath = $xeditMo2RequestPath -replace '\.json$', '.receipt.json'
    $syntheticReceipt = [ordered]@{
        formatVersion='0.1'; kind='wastelandforge.mo2-launch-receipt'; requestId=$xeditMo2Request.requestId
        requestSha256=(Get-FileHash -LiteralPath $xeditMo2RequestPath -Algorithm SHA256).Hash.ToLowerInvariant()
        instance='Synthetic FNV'; profile='Testing'; toolKind='xedit'; executableSha256=$xeditMo2Request.tool.sha256
        processId=9001; processCreated=$true; createdUtc=[DateTimeOffset]::UtcNow.ToString('O'); handleCloseError=$null
    }
    $syntheticReceipt | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath $syntheticReceiptPath -Encoding utf8NoBOM
    $createdMo2Receipts += $syntheticReceiptPath
    Select-Tab $window 'Project Outputs'
    Invoke-Control (Require-Control $window 'RefreshMo2LaunchReceiptsButton')
    $receiptStatus = Require-Control $window 'Mo2LaunchReceiptStatusTextBlock'
    Wait-Until { $receiptStatus.Current.Name -like 'Verified * MO2 process-created receipt(s); refused *.' } "Installed receipt verification failed: $($receiptStatus.Current.Name)" | Out-Null
    $receiptDetails = (Require-Control $window 'Mo2LaunchReceiptDetailsTextBox').GetCurrentPattern([System.Windows.Automation.ValuePattern]::Pattern).Current.Value
    foreach ($expected in @('MO2 instance: Synthetic FNV','Profile: Testing','PID: 9001','does not prove VFS contents')) { if (-not $receiptDetails.Contains($expected,[StringComparison]::Ordinal)) { throw "Installed receipt details did not expose: $expected" } }
    Write-Host 'Gate 487 UI regression: synthetic process-created receipt verified read-only with limitations visible.'
    Write-Host 'Installed Release Candidate and external-tool launch regression passed.'
    Write-Host 'Candidate gating, local handoff, MO2 test copy, controlled GECK/xEdit process creation, unchanged evidence, and cleanup verified.'
}
catch {
    $failure = $_
    Write-Host ("Installed Release Candidate and external-tool launch regression failed: " + $_.Exception.Message)
}
finally {
    foreach ($receiptPath in $createdMo2Receipts) {
        $fullReceiptPath = [IO.Path]::GetFullPath($receiptPath)
        $privateRootPrefix = [IO.Path]::GetFullPath((Join-Path ([Environment]::GetFolderPath([Environment+SpecialFolder]::LocalApplicationData)) 'WastelandForge\Mo2LaunchRequests')).TrimEnd('\') + '\'
        if ($fullReceiptPath.StartsWith($privateRootPrefix, [StringComparison]::OrdinalIgnoreCase) -and (Test-Path -LiteralPath $fullReceiptPath -PathType Leaf)) { Remove-Item -LiteralPath $fullReceiptPath -Force }
    }
    foreach ($requestPath in $createdMo2Requests) {
        $fullRequestPath = [IO.Path]::GetFullPath($requestPath)
        $privateRootPrefix = [IO.Path]::GetFullPath((Join-Path ([Environment]::GetFolderPath([Environment+SpecialFolder]::LocalApplicationData)) 'WastelandForge\Mo2LaunchRequests')).TrimEnd('\') + '\'
        if ($fullRequestPath.StartsWith($privateRootPrefix, [StringComparison]::OrdinalIgnoreCase) -and (Test-Path -LiteralPath $fullRequestPath -PathType Leaf)) { Remove-Item -LiteralPath $fullRequestPath -Force }
    }
    if ($null -ne $process -and -not $process.HasExited) {
        $process.Kill($true)
        $process.WaitForExit(5000) | Out-Null
    }
    if ($installed -and (Test-Path -LiteralPath (Join-Path $installRoot 'unins000.exe'))) {
        Start-Process -FilePath (Join-Path $installRoot 'unins000.exe') -ArgumentList @('/VERYSILENT', '/SUPPRESSMSGBOXES', '/NORESTART') -Wait -WindowStyle Hidden
    }
    foreach ($path in @($readyRoot, $blockedRoot, $settingsRoot, $mo2Root, $releaseHandoffRoot, $geckProjectRoot, $geckStubRoot, $xeditProjectRoot, $xeditStubRoot)) {
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

if ($null -ne $failure) { throw $failure }
