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

Build the package with `eng/Build-Mo2CompanionPackage.ps1`. Extract the archive
contents directly into the MO2 `plugins` folder so the resulting entrypoint is
`plugins/wastelandforge_bridge/plugin.py`. Close MO2 before installing or
removing the bridge. Removal deletes only the `wastelandforge_bridge` folder.

The package does not discover or modify MO2. See its `INSTALL.md`, manifest,
internal `checksums.sha256`, and external ZIP checksum before installation.
