# Gate 207 - JIP LN Text-Script Duplicate Output Validation

Status: Complete

## Purpose

Add semantic validation for duplicate JIP LN text-script output filenames.
This gate prevents two manifest-declared JIP script source entries from
claiming the same future generated script filename. It stops before text
emission, package staging, CLI target wiring, runtime probes, GECK
automation, MO2 VFS inspection, live Data mutation, or external tool
execution.

## Research grounding

- Documented: R006/ADR-009 says JIP LN Script Runner scripts are selected by
  lifecycle filename prefixes and live under `Data\nvse\plugins\scripts`.
- Documented: R006/ADR-009 says JIP script generation must be explicit about
  event prefixes, size budgeting, provider requirements, and
  FormID-resolution strategy.
- Documented: ADR-007 says source contracts are validated by JSON Schema plus
  deterministic semantic validators.
- Documented: ADR-009 says generated artifacts are disposable, rebuildable,
  and must carry provenance.
- Documented: ADR-011 says public fixtures must be deterministic,
  redistributable, synthetic where needed, and must not redistribute Bethesda
  assets or third-party mod files without explicit permission.
- Inferred: Before Forge emits script files, source contracts should prevent
  duplicate future output filenames. Because WastelandForge has a mandatory
  Windows lane, the duplicate check uses a case-insensitive comparison.
- Open: Generation planning, final output path metadata, emitted file
  formatting, script syntax validation, runtime capability readiness, and
  package/install path evidence remain future gates.

## Implemented

- Added `WF-SEM-043` for duplicate JIP script `outputFile` values.
- Applied duplicate detection across all manifest-declared JIP script registry
  documents.
- Used case-insensitive comparison for Windows-first filename collision
  prevention.
- Added related-location evidence pointing to the first source occurrence.
- Added a synthetic schema-valid duplicate-output fixture.
- Added semantic fixture coverage for `WF-SEM-043`.
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
- No generation planning model.
- No capability readiness/runtime diagnostic.
- No public fixture using Bethesda assets or third-party mod files.
- No command alias.
- No new CLI output format.
- No AI behavior.

## Exit evidence

| Evidence | Status |
|---|---|
| Schema-valid duplicate `outputFile` values emit `WF-SEM-043` | Complete |
| Duplicate detection is case-insensitive for Windows-first validation | Complete |
| Diagnostic includes related location for the first source occurrence | Complete |
| Existing valid JIP source fixture remains valid | Complete |
| Broken fixture is synthetic and redistributable JSON only | Complete |
| Text emission, package staging, CLI wiring, runtime probes, GECK automation, MO2 VFS inspection, and external tools remain out of scope | Complete |

## Validation

- `dotnet build --no-restore` passed with 0 warnings and 0 errors.
- `dotnet test tests\WastelandForge.SemanticTests\WastelandForge.SemanticTests.csproj --no-build --no-restore --filter "FullyQualifiedName~JipScript"` passed.
- `dotnet test WastelandForge.sln --no-build --no-restore -m:1` passed.
- `git diff --check` passed. Git reported line-ending warnings for existing
  text files, but returned success.

## Next gate

Gate 208 should add a non-emitting JIP text-script generation planning
skeleton. It should stop before text emission, package staging, CLI target
wiring, runtime probes, GECK automation, MO2 VFS inspection, live Data
mutation, or external tool execution.
