# WastelandForge MO2 Bridge

Optional source-only Mod Organizer 2 Python companion for Gate 484 and the
deterministic separately installable Gate 485 package.

Install this package through MO2's supported Python-plugin workflow. The bridge
does not bundle MO2, Python, game files, or third-party binaries. It imports an
explicit short-lived request from `%LOCALAPPDATA%/WastelandForge/Mo2LaunchRequests`,
requires profile selection and confirmation, then uses MO2's in-process
`IOrganizer.startApplication` API with an empty argument list.

Direct WastelandForge validation, generation, packaging, and tool launch do not
depend on this companion.

Gate 530 connects the Gate 529 effective-provider preflight to MO2's read-only
`IOrganizer.virtualFileTree()` and `resolvePath()` APIs. The adapter inspects
only the already-current profile, resolves and hashes the effective files under
`Data/NVSE/Plugins`, requires one exact
`WastelandForge.GeckProbe.dll`, and refuses profile drift, unresolved winners,
additional native DLLs, digest drift, or GECK Extender/GaryHax markers. The
adapter does not select or mutate a profile, stage files, create a launch
request, or start GECK. Live compatibility remains unproven until the companion
is installed into an explicitly approved FNV MO2 test instance.

Build the package with `eng/Build-Mo2CompanionPackage.ps1`. Extract the archive
contents directly into the MO2 `plugins` folder so the resulting entrypoint is
`plugins/wastelandforge_bridge/plugin.py`. Close MO2 before installing or
removing the bridge. Removal deletes only the `wastelandforge_bridge` folder.

The package does not discover or modify MO2. See its `INSTALL.md`, manifest,
internal `checksums.sha256`, and external ZIP checksum before installation.
