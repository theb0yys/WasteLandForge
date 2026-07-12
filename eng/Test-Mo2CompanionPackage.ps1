[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
$repositoryRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$first = 'artifacts/test-temp/mo2-package-a'
$second = 'artifacts/test-temp/mo2-package-b'
$firstRoot = Join-Path $repositoryRoot $first
$secondRoot = Join-Path $repositoryRoot $second
try {
    & (Join-Path $PSScriptRoot 'Build-Mo2CompanionPackage.ps1') -OutputRoot $first
    & (Join-Path $PSScriptRoot 'Build-Mo2CompanionPackage.ps1') -OutputRoot $second
    $name = 'WastelandForge-MO2-Bridge-0.1.0.zip'
    $one = Join-Path $firstRoot $name
    $two = Join-Path $secondRoot $name
    $archiveHash = (Get-FileHash $one -Algorithm SHA256).Hash.ToLowerInvariant()
    if ($archiveHash -ne (Get-FileHash $two -Algorithm SHA256).Hash.ToLowerInvariant()) { throw 'MO2 companion archive is not repeatable.' }
    if ((Get-Content -LiteralPath "$one.sha256" -Raw) -ne "$archiveHash  $name`n") { throw 'External archive checksum is invalid.' }
    $build = Get-Content -LiteralPath (Join-Path $firstRoot 'package-build-manifest.json') -Raw | ConvertFrom-Json
    $guide = Join-Path $firstRoot 'INSTALL.md'
    $guideHash = (Get-FileHash $guide -Algorithm SHA256).Hash.ToLowerInvariant()
    if ($build.sha256 -ne $archiveHash -or $build.length -ne (Get-Item $one).Length -or $build.installGuide -ne 'INSTALL.md' -or $build.installGuideSha256 -ne $guideHash -or $build.deterministic -ne $true -or $build.liveMo2Executed -ne $false -or $build.mo2StateChanged -ne $false) { throw 'Package build manifest is invalid.' }
    $stream = [IO.File]::OpenRead($one)
    try {
        $archive = [IO.Compression.ZipArchive]::new($stream, [IO.Compression.ZipArchiveMode]::Read)
        try {
            $expected = @('INSTALL.md','checksums.sha256','wastelandforge-bridge-manifest.json','wastelandforge_bridge/README.md','wastelandforge_bridge/__init__.py','wastelandforge_bridge/core.py','wastelandforge_bridge/plugin.py')
            $actual = @($archive.Entries | ForEach-Object FullName)
            if ([string]::Join("`n", $actual) -ne [string]::Join("`n", $expected)) { throw "Unexpected archive entries: $($actual -join ', ')" }
            foreach ($entry in $archive.Entries) {
                if ($entry.LastWriteTime -ne [DateTimeOffset]::new(2000,1,1,0,0,0,[TimeSpan]::Zero)) { throw "Non-deterministic timestamp: $($entry.FullName)" }
                if ($entry.CompressedLength -ne $entry.Length) { throw "Archive entry is not stored without compression: $($entry.FullName)" }
            }
            $checksumEntry = $archive.GetEntry('checksums.sha256')
            $reader = [IO.StreamReader]::new($checksumEntry.Open(), [Text.Encoding]::UTF8)
            try { $checksums = $reader.ReadToEnd() } finally { $reader.Dispose() }
            foreach ($entry in $archive.Entries | Where-Object FullName -ne 'checksums.sha256') {
                $entryStream = $entry.Open()
                $memory = [IO.MemoryStream]::new()
                try { $entryStream.CopyTo($memory); $sha=([Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($memory.ToArray()))).ToLowerInvariant() } finally { $entryStream.Dispose(); $memory.Dispose() }
                if (-not $checksums.Contains("$sha  $($entry.FullName)`n", [StringComparison]::Ordinal)) { throw "Missing or invalid checksum: $($entry.FullName)" }
            }
            $manifestEntry = $archive.GetEntry('wastelandforge-bridge-manifest.json')
            $reader = [IO.StreamReader]::new($manifestEntry.Open(), [Text.Encoding]::UTF8)
            try { $manifest = $reader.ReadToEnd() | ConvertFrom-Json } finally { $reader.Dispose() }
            if ($manifest.entrypoint -ne 'wastelandforge_bridge/plugin.py' -or $manifest.bundledThirdPartyBinaries -ne $false -or $manifest.installRoot -ne 'MO2/plugins') { throw 'Package manifest contract is invalid.' }
        } finally { $archive.Dispose() }
    } finally { $stream.Dispose() }
    Write-Host 'Gate 485 MO2 companion package regression passed.'
} finally {
    foreach ($path in @($firstRoot,$secondRoot)) {
        $full=[IO.Path]::GetFullPath($path); $allowed=(Join-Path $repositoryRoot 'artifacts\test-temp\mo2-package-')
        if ($full.StartsWith($allowed,[StringComparison]::OrdinalIgnoreCase) -and (Test-Path -LiteralPath $full)) { Remove-Item -LiteralPath $full -Recurse -Force }
    }
}
