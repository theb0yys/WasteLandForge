# JIP LN Text Script Generator Evidence Checkpoint

Status: Gate 211 emission manifest and digest skeleton added
Research classification: Documented / Inferred / Open
Source: R005, R006 / ADR-008, ADR-009, ADR-010, ADR-011

## Purpose

Record the evidence and implementation limits for the JIP LN text-script
mod-building function slice while source contracts, validation, and
non-emitting planning are introduced ahead of script text emission, CLI
output, and package output.

Gate 203 adds the first source contract skeleton and synthetic validation
coverage. Gate 204 adds semantic checks for lifecycle/output prefix
consistency and required JIP Script Runner capability declaration. Gate 205
adds opaque body/source-line records for future validation and generation
gates. Gate 206 adds source-level byte-budget validation over those opaque
lines. Gate 207 adds duplicate output filename validation. Gate 208 adds a
non-emitting generation planning model. Gate 209 adds an in-memory renderer
for opaque source lines. Gate 210 writes rendered files under
`generated/jip-scripts` only. Gate 211 adds local emission manifest,
checksum, and digest evidence under that same generated root. CLI targets,
package staging, runtime probes, GECK automation, MO2 VFS inspection, live
Data mutation, and external tool execution remain out of scope.

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

## Open

- Exact safe script syntax subset for the first emitted text files.
- Exact line ending, encoding, header, comment, and deterministic formatting
  policy for generated scripts.
- Exact generated output report/schema shape for JIP generation planning and
  emitted script evidence.
- Exact generated JIP emission manifest schema and validation rules.
- Exact future FormID-resolution fields beyond the initial explicit-reference
  strategy.
- Exact JohnnyGuitar-backed Editor ID call-through extension point.
- Exact `WF-GEN-*`, `WF-CAP-*`, or later `WF-SEM-*` diagnostics for emitted
  script size, unsafe output paths, unresolved references, and capability
  readiness remain future work.

## Non-goals

- No generated JIP emission manifest schema.
- No generated JIP emission manifest validation.
- No build graph target.
- No CLI behavior or output contract change.
- No runtime probe.
- No GECK automation.
- No MO2 VFS inspection or installation.
- No live game Data mutation.
- No binary ESP/ESM generation.
- No external tool execution.
- No Bethesda assets, third-party mod files, or real local install snapshots.

## Next implementation slice

Gate 212 should add a generated JIP emission manifest schema and validation
skeleton. It should stop before package staging, CLI target wiring, runtime
probes, GECK automation, MO2 VFS inspection, live Data mutation, or external
tool execution.
