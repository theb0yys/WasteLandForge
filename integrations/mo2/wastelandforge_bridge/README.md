# WastelandForge MO2 Bridge

Optional source-only Mod Organizer 2 Python companion for Gate 484.

Install this package through MO2's supported Python-plugin workflow. The bridge
does not bundle MO2, Python, game files, or third-party binaries. It imports an
explicit short-lived request from `%LOCALAPPDATA%/WastelandForge/Mo2LaunchRequests`,
requires profile selection and confirmation, then uses MO2's in-process
`IOrganizer.startApplication` API with an empty argument list.

Direct WastelandForge validation, generation, packaging, and tool launch do not
depend on this companion.
