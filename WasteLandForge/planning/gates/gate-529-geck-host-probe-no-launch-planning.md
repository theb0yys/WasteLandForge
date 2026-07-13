# Gate 529 - GECK Host-Probe No-Launch Planning

Status: Complete - repository-local planning and synthetic preflight only
Phase: post-v0.1 provider discovery
Decision base: ADR-004, ADR-008, ADR-009, ADR-013, R005, R009, and Gates
483-488 and 520-528

## Goal

Implement Gate 528's private host-probe plan and evidence services without
building an authorized DLL, staging a file, changing an MO2 profile, creating a
launch request, or starting GECK.

## Evidence classification

- **Documented:** Gate 528 requires a versioned, duplicate-key-rejecting,
  canonical digest-bound private run plan, four separate approvals, exact
  create-only staging, selected-profile effective-provider evidence, protected
  pre/post manifests, empty GECK arguments, and fail-closed recovery.
- **Documented:** R005 distinguishes physical provider presence from effective
  MO2 profile visibility and identifies `virtualFileTree()` as the supported
  read-only environment surface.
- **Documented:** Gate 527's current DLL has `launchAuthorized=false` and
  `approvedProbeRunId=build-only-unapproved`; it cannot become live evidence.
- **Inferred:** the live MO2 API adapter can remain separate from a pure
  effective-provider evaluator, allowing deterministic synthetic regression
  before a companion is installed into a real FNV instance.
- **Open:** no initialized local FNV MO2 instance/profile, installed companion,
  operator-approved probe mod, or real effective-tree snapshot exists.

## Implementation

The immutable `geck-host-probe-run-plan/0.1.0` schema fixes:

- 128-bit lowercase run identity and UTC creation time;
- exact build, game/editor/xNVSE/MO2/Python/companion file identities;
- nullable instance/profile and effective-provider evidence that fail closed;
- exact create-only and effective probe paths;
- protected-baseline digest and entry count;
- private observation filenames, empty launch arguments, and manual recovery;
- canonical plan SHA-256; and
- hard `false` build, staging, profile mutation, request, process, plugin
  mutation, physical Data write, and external-tool execution flags.

`GeckHostProbePlanning.cs` adds read-only services for:

1. strict inspection of an exact launch-authorized build manifest and DLL;
2. preview of one absent target in an existing operator-selected MO2 mod;
3. deterministic per-file protected manifests with explicit absence and
   reparse/path-escape refusal;
4. preview planning with named unresolved blockers and no executable output;
5. UTF-8-no-BOM, 4 MiB-bounded, duplicate-key-rejecting schema parsing;
6. property-sorted canonical digest verification and current file-identity
   drift checks.

The MO2 companion core now contains a pure effective-provider preflight. Given
an already enumerated synthetic snapshot, it requires exactly one approved
probe DLL and refuses incomplete inspection, unresolved context, path escape,
duplicate paths, identity errors, digest drift, extra native DLLs, and GECK
Extender/GaryHax markers. It performs no organizer calls or writes.

## Current environment result

The Gate 527 local build cannot satisfy the new build preview because its
manifest records `launchAuthorized=false` and the non-live run identity
`build-only-unapproved`. Gate 528's missing FNV MO2 instance/profile,
companion, selected probe mod, and effective tree therefore remain explicit
run-plan blockers. Gate 529 creates no executable plan for that environment.

## Validation

- Passed focused C# tests for authorized-build preview, blocked Gate 527-style
  evidence, create-only staging, protected-manifest repeatability, preview-ready
  synthetic planning, canonical parse, stale digest refusal, duplicate-key
  refusal, and no-write/no-launch flags.
- Passed schema registration and hard no-launch constant regression.
- Passed five Python companion tests, including synthetic exact-provider success
  and unresolved, incomplete, extra, duplicate, marker, and digest-mismatch
  refusal.
- Passed the complete .NET solution suite: 892 tests across unit, schema,
  semantic, golden, Windows, and backwards-compatibility assemblies.
- Initial default-parallel test compilation created excessive idle MSBuild
  nodes and was terminated; the same focused suite passed with `-m:1` and
  shared compilation disabled.
- No native build, MO2/GECK/xEdit process, profile operation, staging copy,
  request, receipt, observation, ESP/ESM access, or game-file write occurred.

## Boundary result

Gate 529 proves the no-launch planning and synthetic evaluation boundary. It
does not prove that a real MO2 virtual tree can be enumerated, that the probe is
effectively visible, that xNVSE loads it, or that GECK remains stable. It does
not authorize record APIs or plugin mutation under ADR-013.

## Next route

Gate 530: implement the companion-side MO2 `virtualFileTree()` adapter and
refresh the deterministic companion package against API-faithful synthetic
doubles. Keep live companion installation, instance/profile selection, probe
staging, request creation, and GECK launch behind separate explicit approvals.
