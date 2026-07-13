# Gate 488 - Live MO2 Compatibility Smoke Preflight

Status: Deferred
Phase: v0.1 implementation
Decision base: ADR-002, ADR-008, ADR-009, ADR-011 and Gates 483-487

## Goal

Determine whether a user-controlled Fallout: New Vegas MO2 test instance is
available and explicitly authorized for one companion compatibility smoke before
performing any external installation, launch, or cleanup.

## Research grounding

| Claim | Classification | Source |
|---|---|---|
| Live compatibility requires an explicitly selected disposable/test MO2 instance and authorization. | Documented | Gate 487 next route |
| MO2 owns profiles and VFS execution context; Forge integrates rather than replacing it. | Documented | ADR-002 and FNV tooling ecosystem research |
| A process-created receipt is not editor, VFS, save, runtime, or mod correctness evidence. | Documented | Gate 483 and Gate 487 |

## Read-only preflight

- Inspected `%LOCALAPPDATA%/ModOrganizer` instance metadata and profiles.
- Found Fallout 4 London-named instance directories and two `Default` profiles.
- The readable instance metadata identifies Fallout 4 and
  `A:/games gog/Fallout 4 GOTY`, not Fallout: New Vegas.
- Inspected the detected Steam library manifests; Fallout 4 was present and no
  Fallout: New Vegas app manifest was found.
- Checked installed-program metadata, running processes, and common MO2 paths;
  no `ModOrganizer.exe` candidate was identified.
- Stopped an unbounded read-only drive search without using its incomplete
  result as evidence.

## Decision

Defer the live compatibility smoke. No suitable FNV test instance or explicit
external-mutation authorization was supplied. Repeating the generic next-gate
instruction is treated as routing past this optional environment-dependent
gate, not as permission to alter a Fallout 4 London instance.

Gate 488 may resume only when the user provides the exact FNV
`ModOrganizer.exe`/instance path and explicitly authorizes companion
installation plus one GECK or xEdit test launch.

## Actions not taken

- No companion extraction or installation.
- No MO2, Python plugin, GECK, xEdit, game, or third-party process launch.
- No profile, load order, mod priority, executable registration, configuration,
  Overwrite, game Data, or request/receipt mutation.
- No files outside the WastelandForge repository were changed.

## Next route

Gate 489: define a deterministic BSA packing-plan and validation contract over
validated package assets. The gate must classify packable, loose-only,
uncompressed-audio, MP3-refused, and capability-sensitive KF entries; emit a
tool-neutral recipe with provenance; and stop before invoking BSArchPro,
Archive.exe, or another third-party packer.

