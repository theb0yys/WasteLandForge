# JIP LN Text Script Generator Evidence Checkpoint

Status: Gate 217 command slice closed
Research classification: Documented / Inferred / Open
Source: R005, R006 / ADR-008, ADR-009, ADR-010, ADR-011

## Purpose

Record the evidence and implementation limits for the JIP LN text-script
mod-building function slice while source contracts, validation, generated-file
emission, canonical generate-command access, and canonical build-command
access are introduced ahead of package and runtime output.

Gate 203 adds the first source contract skeleton and synthetic validation
coverage. Gate 204 adds semantic checks for lifecycle/output prefix
consistency and required JIP Script Runner capability declaration. Gate 205
adds opaque body/source-line records for future validation and generation
gates. Gate 206 adds source-level byte-budget validation over those opaque
lines. Gate 207 adds duplicate output filename validation. Gate 208 adds a
non-emitting generation planning model. Gate 209 adds an in-memory renderer
for opaque source lines. Gate 210 writes rendered files under
`generated/jip-scripts` only. Gate 211 adds local emission manifest,
checksum, and digest evidence under that same generated root. Gate 212 adds
an embedded generated-evidence schema and manifest validation before checksum
sidecar emission. Gate 213 adds checksum sidecar revalidation for the emitted
manifest and generated script files. Gate 214 wires that emitter into
`forge generate --target jip-scripts`. Gate 215 wires rendered JIP script
payloads into `forge build --target jip-scripts` under `dist/jip-scripts`,
with local build-manifest and checksum evidence. Gate 216 wires rendered JIP
script payloads into `forge package --target jip-scripts` under
`dist/jip-scripts/package/Data/nvse/plugins/scripts`, with package-manifest,
install-plan, build-manifest, and checksum evidence. Runtime probes, GECK
automation, MO2 VFS inspection, live Data mutation, external tool execution,
FOMOD generation, archive generation, and JIP package verify-existing remain
out of scope. Gate 217 closes this command slice and moves the next
implementation lane to xEdit audit and inspection support.

## Documented

- R006/ADR-009 identifies carefully bounded JIP LN text scripts as a
  second-wave game-support generator after deterministic text artefacts and
  MCM Extender JSON.
- R006/ADR-009 says JIP LN Script Runner scripts live in
  `Data\nvse\plugins\scripts`, are selected by lifecycle filename prefixes,
  run in the console environment, cannot exceed 16,384 bytes, and do not
  resolve Editor IDs by default.
- R006/ADR-009 says JIP script generation is viable only when event prefixes,
  size budgeting, provider requirements, and FormID-resolution strategy are
  explicit.
- R005/ADR-008 says generated JIP tooling should depend on composable
  capabilities such as `runtime.scripting.jip_script_runner` and, when
  Editor ID call-through is required, a separate JohnnyGuitar capability.
- R005/ADR-008 says missing JIP LN must fail validation for generated runtime
  text scripts; missing JohnnyGuitar may still allow a narrower path using
  explicit references.
- ADR-009 keeps generated outputs disposable and rebuildable under
  `generated/` or `dist/`, with provenance and build-manifest evidence.
- ADR-010 keeps the user-facing command surface under canonical
  `forge generate` and `forge build` targets only.
- ADR-011 requires deterministic redistributable fixtures and forbids public
  fixtures that redistribute Bethesda assets or third-party mod files without
  explicit permission.

## Inferred

- The first implementation slice should add a source contract skeleton before
  adding text emission. The source contract needs enough structure to express
  script identity, lifecycle/event prefix intent, output filename intent,
  required capabilities, size policy, and FormID-resolution policy.
- The first generated output target should stay under Forge-owned output
  roots such as `generated/` for generate and `dist/` for build/package
  staging. Any game-relative `Data\nvse\plugins\scripts` path should be
  recorded as intended package/install path metadata, not written into a live
  game or MO2-managed Data tree by this slice.
- The first generator should accept only synthetic, deliberately small script
  fixtures until the script subset and validation rules are explicit.
- Gate 204 uses `WF-SEM-040` for lifecycle/output prefix mismatches and
  `WF-SEM-041` when a JIP source contract does not declare
  `runtime.scripting.jip_script_runner`.
- Gate 205 records script bodies as `lineMode: "opaqueText"` plus
  `body.lines[].text`. Those lines are canonical source records only; they
  are not validated as JIP syntax and are not emitted as script files.
- Gate 206 uses `WF-SEM-042` when the source body exceeds `sizePolicy.maxBytes`.
  The source budget uses UTF-8 bytes for stored line text with one LF byte
  between lines. This is not final emitted-file byte accounting.
- Gate 207 uses `WF-SEM-043` for duplicate JIP `outputFile` values across
  manifest-declared JIP script registries. The comparison is
  case-insensitive to match the Windows-first validation baseline.
- Gate 208 uses `JipScriptGenerationPlanner` to derive future generated path,
  game-relative Data path, install path, source-size, capability, and source
  location metadata from validated JIP source contracts without writing files.
- Gate 209 uses `JipScriptTextRenderer` to join validated opaque source lines
  with LF separators in memory, record UTF-8 byte counts, and preserve Gate
  208 path metadata without writing files.
- Gate 210 uses `JipScriptFileEmitter` to write rendered documents under
  `generated/jip-scripts/nvse/plugins/scripts/...` only. Game-relative
  `Data/nvse/plugins/scripts/...` remains install metadata and is not written.
- Gate 211 writes `jip-script-emission-manifest.json` and `checksums.sha256`
  under `generated/jip-scripts`. The manifest records package non-mutation
  flags, script output metadata, generated payload digests, and known
  limitations. The result output digest list covers generated script files and
  the manifest, but not the checksum file itself.
- Gate 212 adds `jip-script-emission-manifest/0.1.0/schema.json` as a
  generated-evidence schema, embeds it through `WastelandForgeSchemaCatalog`,
  validates emitted manifest JSON before writing checksum evidence, and uses
  `WF-GEN-007` for generated JIP emission manifest schema failures.
- Gate 213 adds `JipScriptEmissionChecksumVerifier` and `WF-GEN-008` for
  generated checksum sidecar drift. It verifies expected manifest/script
  entries, rejects missing or unexpected entries, rejects malformed or
  escaping checksum paths, and recomputes SHA-256 for local generated files.
- Gate 214 adds canonical `forge generate --target jip-scripts` CLI target
  wiring for the existing generated-root emitter. CLI output reports generated
  script metadata, manifest/checksum evidence, diagnostics, and output
  digests. The target rejects `--output` and `--dry-run` in this gate instead
  of silently ignoring unsupported options.
- Gate 215 adds canonical `forge build --target jip-scripts` CLI target
  wiring. It writes rendered scripts under
  `dist/jip-scripts/nvse/plugins/scripts/...`, records
  `build-manifest.json` and `checksums.sha256`, supports dry-run planning, and
  validates custom build output containment under `dist/`.
- Gate 216 adds canonical `forge package --target jip-scripts` CLI target
  wiring. It writes rendered scripts under
  `dist/jip-scripts/package/Data/nvse/plugins/scripts/...`, records
  `package-manifest.json`, `install-plan.json`, `build-manifest.json`, and
  `checksums.sha256`, supports dry-run planning, and validates custom package
  output containment under `dist/`.
- Gate 217 parks the current JIP command lane after generate, build, and
  package target wiring. Further JIP verify-existing, runtime probe, syntax
  parser, installer, archive, or live install work must be explicitly
  requested before it reopens.

## Open

- Exact safe script syntax subset for the first emitted text files.
- Exact line ending, encoding, header, comment, and deterministic formatting
  policy for generated scripts.
- Exact generated output report/schema shape for future JIP package evidence
  beyond dist build emission.
- Exact future FormID-resolution fields beyond the initial explicit-reference
  strategy.
- Exact JohnnyGuitar-backed Editor ID call-through extension point.
- Exact `WF-GEN-*`, `WF-CAP-*`, or later `WF-SEM-*` diagnostics for emitted
  script size, unsafe output paths, unresolved references, and capability
  readiness remain future work.

## Non-goals

- No install preview for JIP script payloads.
- No JIP package verify-existing mode.
- No installer or archive generation.
- No runtime probe.
- No GECK automation.
- No MO2 VFS inspection or installation.
- No live game Data mutation.
- No binary ESP/ESM generation.
- No external tool execution.
- No Bethesda assets, third-party mod files, or real local install snapshots.

## Next implementation slice

Gate 218 should start the xEdit audit and inspection lane with an evidence
checkpoint and adapter boundary. It should define the first synthetic fixture
and output contract for xEdit audit script/report support while stopping
before xEdit process execution, plugin patch generation, plugin mutation,
MO2 automation, GECK automation, runtime probes, or real third-party plugin
fixtures.
