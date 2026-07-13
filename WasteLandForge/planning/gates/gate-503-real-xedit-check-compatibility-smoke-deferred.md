# Gate 503 - Real xEdit Check Compatibility Smoke Deferred

Status: Deferred
Phase: v0.1 implementation
Decision base: ADR-002, ADR-004, ADR-008, ADR-011 and Gates 501-502

## Goal

Perform a separately authorized compatibility smoke of Gate 502's generated
Pascal against a user-supplied FNV-capable xEdit installation, or record the
smoke deferred without overstating real xEdit compatibility.

## Read-only preflight

- Checked `PATH` resolution for `FNVEdit.exe` and `xEdit.exe`.
- Checked WastelandForge's LocalAppData settings location for a configured xEdit
  executable.
- Performed bounded read-only discovery in standard Program Files, tool,
  modding, game, and known Steam-library roots for `FNVEdit.exe`,
  `FNVEdit64.exe`, `xEdit.exe`, and `xEdit64.exe`.
- Checked repository configuration and documentation references for a supplied
  real executable path.

No user-supplied FNV-capable xEdit executable was found. Repository synthetic
executables were deliberately excluded because they cannot establish Pascal
script compatibility with xEdit.

## Claim classification

- **Documented:** Gate 501 requires real xEdit execution to be a manual extended
  test using the user's own installation and explicit authorization.
- **Documented:** Gate 502 proves deterministic generation, strict report
  provenance, typed diagnostics, and no plugin mutation using synthetic inputs.
- **Open:** compilation and execution of the generated Pascal, exact xEdit API
  behavior in FNV mode, and the manual findings-to-envelope handoff remain
  unverified against a real xEdit process.

## Actions deliberately not taken

- Did not download, install, copy, bundle, or execute xEdit.
- Did not launch MO2, GECK, the game, or any external provider.
- Did not open, parse, mutate, save, patch, or copy a real plugin.
- Did not alter game Data, load order, profiles, settings, or tool directories.
- Did not claim that Gate 502's Pascal has passed a real xEdit compiler/runtime.

## Validation

- Read-only executable and configuration discovery completed; no real provider
  was found.
- Production code, schemas, tests, and generated distributables were unchanged
  by this gate, so the Gate 502 build and 864-test suite were not rerun.

## Unblock condition

Resume only when the user supplies the exact absolute path to a real
`FNVEdit.exe` or `xEdit.exe`, identifies an isolated synthetic FNV test plugin,
and explicitly authorizes launching that executable for the manual script
smoke. The executable identity and plugin digest must be previewed before
launch; no production plugin or normal load order may be used implicitly.

## Next route

Gate 504: close the xEdit Check ingestion lane at its deterministic synthetic
boundary, keep real-provider compatibility explicitly deferred, and select the
next repository-local mod-building value slice rather than adding more xEdit
report edge cases.
