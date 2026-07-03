# Gate 206 - JIP LN Text-Script Source-Line Byte-Budget Validation

Status: Complete

## Purpose

Add semantic validation for JIP LN text-script source-line byte budgets. This
gate compares opaque source-line text against `sizePolicy.maxBytes` before
any generator work. It stops before JIP syntax validation, final emitted-file
byte accounting, text emission, package staging, CLI target wiring, runtime
probes, GECK automation, MO2 VFS inspection, live Data mutation, or external
tool execution.

## Research grounding

- Documented: R006/ADR-009 says JIP LN Script Runner scripts cannot exceed
  16,384 bytes and JIP script generation must be explicit about size
  budgeting.
- Documented: R006/ADR-009 says JIP LN Script Runner scripts are selected by
  lifecycle filename prefixes and run in the console environment.
- Documented: ADR-007 says source contracts are validated by JSON Schema plus
  deterministic semantic validators.
- Documented: ADR-009 says generated artifacts are disposable and rebuildable
  and must carry provenance.
- Documented: ADR-011 says public fixtures must be deterministic,
  redistributable, synthetic where needed, and must not redistribute Bethesda
  assets or third-party mod files without explicit permission.
- Inferred: Before Forge emits script files, source-line records should have
  a deterministic byte-budget guard. Gate 206 uses UTF-8 bytes for
  `body.lines[].text` and one LF byte between stored source lines.
- Open: Final emitted line endings, encoding, headers, comments, formatting,
  emitted byte accounting, unresolved reference diagnostics, capability
  readiness diagnostics, and JohnnyGuitar extension fields remain future
  gates.

## Implemented

- Added `WF-SEM-042` for source bodies that exceed declared
  `sizePolicy.maxBytes`.
- Defined the Gate 206 source-budget calculation as UTF-8 bytes for opaque
  source-line text with one LF byte between stored lines.
- Added a synthetic schema-valid fixture for the exceeded budget case.
- Added semantic fixture coverage for `WF-SEM-042`.
- Updated planning, generation notes, ADRs, governance docs, README files,
  and project-local prompt routing.

## Not implemented

- No JIP syntax validation.
- No final emitted-file byte accounting.
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
- No duplicate output filename validation.
- No capability readiness/runtime diagnostic.
- No public fixture using Bethesda assets or third-party mod files.
- No command alias.
- No new CLI output format.
- No AI behavior.

## Exit evidence

| Evidence | Status |
|---|---|
| Schema-valid source body exceeding `sizePolicy.maxBytes` emits `WF-SEM-042` | Complete |
| Budget calculation is deterministic and source-level only | Complete |
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

Gate 207 should add duplicate JIP output filename semantic validation. It
should stop before text emission, package staging, CLI target wiring, runtime
probes, GECK automation, MO2 VFS inspection, live Data mutation, or external
tool execution.
