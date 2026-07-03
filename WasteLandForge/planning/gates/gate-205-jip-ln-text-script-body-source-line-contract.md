# Gate 205 - JIP LN Text-Script Body Source-Line Contract

Status: Complete

## Purpose

Add the first JIP LN text-script body/source-line source contract skeleton.
This gate records opaque source lines for future validation and generation
gates only. It stops before JIP syntax validation, text emission, package
staging, CLI target wiring, runtime probes, GECK automation, MO2 VFS
inspection, live Data mutation, or external tool execution.

## Research grounding

- Documented: R006/ADR-009 says JIP LN Script Runner scripts are selected by
  lifecycle filename prefixes, run in the console environment, cannot exceed
  16,384 bytes, and do not resolve Editor IDs by default.
- Documented: R006/ADR-009 says JIP script generation must be explicit about
  event prefixes, size budgeting, provider requirements, and
  FormID-resolution strategy.
- Documented: ADR-007 says canonical source truth is versioned YAML/JSON
  contracts validated by JSON Schema plus deterministic semantic validators.
- Documented: ADR-009 says generated artifacts are disposable and rebuildable
  and must carry provenance.
- Documented: ADR-011 says public fixtures must be deterministic,
  redistributable, synthetic where needed, and must not redistribute Bethesda
  assets or third-party mod files without explicit permission.
- Inferred: The next safe step after source identity and semantic gating is an
  opaque source-line container. This lets Forge preserve source-line intent
  without claiming a safe JIP syntax subset or writing files.
- Open: JIP syntax validation, deterministic line endings, encoding, header
  policy, comments, emitted byte accounting, unresolved reference diagnostics,
  capability readiness diagnostics, and JohnnyGuitar extension fields remain
  future gates.

## Implemented

- Extended the unpublished `jip-scripts/0.1.0` source schema with required
  `body`.
- Added `body.lineMode` with const value `opaqueText`.
- Added `body.lines[].text` as required non-empty single-line opaque text.
- Updated synthetic JIP source fixtures to include opaque source lines.
- Added an invalid body/source-line fixture for runtime schema diagnostics.
- Extended `JipScriptDefinition` with `SourceLines`.
- Added `JipScriptSourceLine` with text and source location.
- Updated `ProjectValidationPipeline.ReadJipScripts` to read body/source-line
  records from schema-valid JIP source registries.
- Added schema and semantic/read-model tests.
- Updated planning, generation notes, ADRs, governance docs, README files,
  and project-local prompt routing.

## Not implemented

- No JIP syntax validation.
- No generated JIP script files.
- No generator implementation.
- No build graph target.
- No `forge generate` or `forge build` target wiring.
- No package staging.
- No runtime probe.
- No GECK automation.
- No MO2 VFS inspection.
- No live game Data mutation.
- No binary plugin generation.
- No external tool execution.
- No generated output schema.
- No source-line byte-budget semantic validation.
- No capability readiness/runtime diagnostic.
- No public fixture using Bethesda assets or third-party mod files.
- No command alias.
- No new CLI output format.
- No AI behavior.

## Exit evidence

| Evidence | Status |
|---|---|
| JIP source schema requires opaque `body` records | Complete |
| Body lines are non-empty single-line opaque text records | Complete |
| Runtime schema diagnostics catch invalid body/source-line shape | Complete |
| Typed read model exposes source lines and JSON source locations | Complete |
| Existing JIP semantic fixtures remain focused on `WF-SEM-040` and `WF-SEM-041` | Complete |
| Text emission, package staging, CLI wiring, runtime probes, GECK automation, MO2 VFS inspection, and external tools remain out of scope | Complete |

## Validation

- `dotnet build --no-restore` passed with 0 warnings and 0 errors.
- `dotnet test tests\WastelandForge.SchemaTests\WastelandForge.SchemaTests.csproj --no-build --no-restore --filter "FullyQualifiedName~JipScript"` passed.
- `dotnet test tests\WastelandForge.SemanticTests\WastelandForge.SemanticTests.csproj --no-build --no-restore --filter "FullyQualifiedName~JipScript"` passed.
- `dotnet test WastelandForge.sln --no-build --no-restore -m:1` passed.
- `git diff --check` passed. Git reported line-ending warnings for existing
  text files, but returned success.

## Next gate

Gate 206 should add JIP LN text-script source-line byte-budget semantic
validation. It should stop before text emission, package staging, CLI target
wiring, runtime probes, GECK automation, MO2 VFS inspection, live Data
mutation, or external tool execution.
