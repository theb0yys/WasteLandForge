# JIP LN Text Script Generator Evidence Checkpoint

Status: Gate 202 evidence checkpoint
Research classification: Documented / Inferred / Open
Source: R005, R006 / ADR-008, ADR-009, ADR-010, ADR-011

## Purpose

Record the evidence and implementation limits for the next real
mod-building function slice before adding any JIP LN text-script source
contract, schema, generator code, CLI output, fixture, or package output.

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

## Open

- Exact JIP LN script source schema name, version, and registry placement.
- Exact allowed lifecycle filename prefixes and their schema enumeration.
- Exact safe script syntax subset for the first emitted text files.
- Exact line ending, encoding, header, comment, and deterministic formatting
  policy for generated scripts.
- Exact FormID-resolution strategy fields for non-Editor-ID scripts.
- Whether JohnnyGuitar-backed Editor ID call-through belongs in the first JIP
  contract or a later optional-capability extension.
- Exact `WF-GEN-*`, `WF-CAP-*`, or `WF-SEM-*` diagnostics for script size,
  unsafe output paths, missing capabilities, and unresolved references.

## Non-goals

- No JIP LN generator implementation.
- No source schema or schema catalog update.
- No generated script files.
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

Gate 203 should add the JIP LN text-script source contract skeleton with
synthetic validation coverage only. It should stop before text emission,
package staging, CLI target wiring, runtime probes, GECK automation, MO2 VFS
inspection, or external tool execution.
