[CmdletBinding()]
param(
    [string] $OutputRoot = 'artifacts/integrations/mo2',
    [string] $Version = '0.1.0'
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
$repositoryRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$sourceRoot = Join-Path $repositoryRoot 'integrations\mo2\wastelandforge_bridge'
$outputRootPath = if ([IO.Path]::IsPathRooted($OutputRoot)) { [IO.Path]::GetFullPath($OutputRoot) } else { [IO.Path]::GetFullPath((Join-Path $repositoryRoot $OutputRoot)) }
$relativeOutput = [IO.Path]::GetRelativePath($repositoryRoot, $outputRootPath).Replace('\', '/')
if ($relativeOutput -ne 'artifacts/integrations/mo2' -and -not $relativeOutput.StartsWith('artifacts/test-temp/mo2-package-', [StringComparison]::Ordinal)) {
    throw "Refusing MO2 companion package output outside artifacts/integrations/mo2 or artifacts/test-temp/mo2-package-*: $outputRootPath"
}
if ($Version -notmatch '^\d+\.\d+\.\d+$') { throw "Version must be SemVer core form: $Version" }

$required = @('__init__.py', 'core.py', 'plugin.py', 'README.md')
foreach ($name in $required) {
    if (-not (Test-Path -LiteralPath (Join-Path $sourceRoot $name) -PathType Leaf)) { throw "Required companion source is missing: $name" }
}

$utf8 = [Text.UTF8Encoding]::new($false)
$newline = "`n"
function Get-Sha256([byte[]] $Bytes) { ([Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($Bytes))).ToLowerInvariant() }
function New-Entry([string] $Path, [byte[]] $Bytes) {
    [pscustomobject]@{ Path = $Path.Replace('\', '/'); Bytes = $Bytes; Length = $Bytes.LongLength; Sha256 = Get-Sha256 $Bytes }
}
function Get-OrdinalEntries($Items) { @($Items | Sort-Object { [Convert]::ToHexString($utf8.GetBytes($_.Path)) }) }

$entries = [Collections.Generic.List[object]]::new()
foreach ($name in $required) {
    $entries.Add((New-Entry "wastelandforge_bridge/$name" ([IO.File]::ReadAllBytes((Join-Path $sourceRoot $name)))))
}
$install = @"
# Install WastelandForge MO2 Bridge $Version

Requirements:

- Mod Organizer 2 with its supported Python plugin available and enabled.
- WastelandForge remains fully usable without this optional bridge.

Install:

1. Close Mod Organizer 2.
2. Open the Mod Organizer 2 installation folder, then open `plugins`.
3. Extract the archive contents directly into `plugins`.
4. Confirm `plugins\wastelandforge_bridge\plugin.py` exists.
5. Start Mod Organizer 2 and open `WastelandForge Launch Request` from its tools UI.

Remove:

1. Close Mod Organizer 2.
2. Delete only `plugins\wastelandforge_bridge`.

The bridge does not install or update MO2, register executables, select a
profile automatically, alter mod/load order, or launch anything until a user
imports a valid short-lived request and explicitly approves the selected
profile. A process-created receipt is not evidence of editor or mod correctness.
"@
$entries.Add((New-Entry 'INSTALL.md' $utf8.GetBytes($install.TrimEnd() + $newline)))

$manifestEntries = @(Get-OrdinalEntries $entries | ForEach-Object { [ordered]@{ path=$_.Path; length=$_.Length; sha256=$_.Sha256 } })
$manifest = [ordered]@{
    formatVersion = '0.1'
    kind = 'wastelandforge.mo2-companion-package'
    name = 'WastelandForge MO2 Bridge'
    version = $Version
    installRoot = 'MO2/plugins'
    entrypoint = 'wastelandforge_bridge/plugin.py'
    offline = $true
    aiOptional = $true
    bundledThirdPartyBinaries = $false
    entries = $manifestEntries
}
$manifestBytes = $utf8.GetBytes(($manifest | ConvertTo-Json -Depth 10) + $newline)
$entries.Add((New-Entry 'wastelandforge-bridge-manifest.json' $manifestBytes))
$checksumText = ((Get-OrdinalEntries $entries | ForEach-Object { "$($_.Sha256)  $($_.Path)" }) -join $newline) + $newline
$entries.Add((New-Entry 'checksums.sha256' $utf8.GetBytes($checksumText)))

New-Item -ItemType Directory -Force -Path $outputRootPath | Out-Null
$archiveName = "WastelandForge-MO2-Bridge-$Version.zip"
$archivePath = Join-Path $outputRootPath $archiveName
$temporary = "$archivePath.tmp-$([Guid]::NewGuid().ToString('N'))"
try {
    $stream = [IO.File]::Open($temporary, [IO.FileMode]::CreateNew, [IO.FileAccess]::ReadWrite, [IO.FileShare]::None)
    try {
        $archive = [IO.Compression.ZipArchive]::new($stream, [IO.Compression.ZipArchiveMode]::Create, $true)
        try {
            foreach ($item in (Get-OrdinalEntries $entries)) {
                $entry = $archive.CreateEntry($item.Path, [IO.Compression.CompressionLevel]::NoCompression)
                $entry.LastWriteTime = [DateTimeOffset]::new(2000, 1, 1, 0, 0, 0, [TimeSpan]::Zero)
                $output = $entry.Open()
                try { $output.Write($item.Bytes, 0, $item.Bytes.Length) } finally { $output.Dispose() }
            }
        } finally { $archive.Dispose() }
    } finally { $stream.Dispose() }
    Move-Item -LiteralPath $temporary -Destination $archivePath -Force
} finally { if (Test-Path -LiteralPath $temporary) { Remove-Item -LiteralPath $temporary -Force } }

$archiveBytes = [IO.File]::ReadAllBytes($archivePath)
$archiveSha = Get-Sha256 $archiveBytes
[IO.File]::WriteAllText("$archivePath.sha256", "$archiveSha  $archiveName$newline", $utf8)
[IO.File]::WriteAllText((Join-Path $outputRootPath 'INSTALL.md'), $install.TrimEnd() + $newline, $utf8)
$buildManifest = [ordered]@{
    formatVersion='0.1'; kind='wastelandforge.mo2-companion-package-build'; version=$Version
    archive=$archiveName; length=$archiveBytes.LongLength; sha256=$archiveSha
    installGuide='INSTALL.md'; installGuideSha256=(Get-Sha256 $utf8.GetBytes($install.TrimEnd() + $newline))
    deterministic=$true; archiveTimestamp='2000-01-01T00:00:00+00:00'; compression='stored'
    liveMo2Executed=$false; mo2StateChanged=$false
}
[IO.File]::WriteAllText((Join-Path $outputRootPath 'package-build-manifest.json'), ($buildManifest | ConvertTo-Json -Depth 5) + $newline, $utf8)
Write-Host "WastelandForge MO2 companion package complete: $archivePath"
Write-Host "SHA-256: $archiveSha"
