# Gate 213 - JIP LN Text-Script Emission Checksum Revalidation

Status: Complete

## Purpose

Add local checksum sidecar revalidation for generated JIP LN text-script
emission evidence under `generated/jip-scripts`. This gate verifies
`checksums.sha256` against the generated manifest and emitted script files,
without adding package staging, CLI target wiring, runtime probes, GECK
automation, MO2 VFS inspection, live Data mutation, or external tool
execution.

## Research grounding

- Documented: ADR-009 says generated outputs are disposable, rebuildable, and
  must carry provenance.
- Documented: ADR-011 reserves `WF-GEN-*` for generator output/provenance gaps
  and requires deterministic fixture-backed tests.
- Documented: ADR-011 requires local build/report evidence and checksums in
  the validation-first operating model.
- Inferred: The JIP emission checksum sidecar is generated evidence, not
  source truth, so revalidation should compare it to the generated manifest
  and files under the emission root only.
- Inferred: A checksum verifier should reject missing, unexpected, malformed,
  duplicate, escaping, unreadable, and digest-mismatched entries before any
  future command target or package step relies on the evidence.
- Open: Wiring this JIP emission path into canonical `forge generate` remains
  future work.

## Implemented

- Added `JipScriptEmissionChecksumVerifier`.
- Added `WF-GEN-008` for generated JIP emission checksum sidecar failures.
- Revalidated generated `checksums.sha256` from `JipScriptFileEmitter.Emit`
  after manifest and checksum files are written.
- Verified checksum entries for the generated manifest and generated script
  payloads declared by the manifest.
- Recomputed SHA-256 for generated manifest/script entries and reported
  mismatches.
- Reported malformed checksum entries, paths escaping the emission root,
  duplicate entries, missing expected entries, unexpected entries, and
  unreadable files.
- Added focused unit coverage for clean checksum evidence, digest drift,
  missing manifest checksum entry, and unexpected checksum entry.
- Updated planning, generation notes, governance docs, ADRs, README files, and
  project-local prompt routing.

## Not implemented

- No `forge generate` or `forge build` target wiring.
- No package staging.
- No build graph target.
- No runtime probe.
- No GECK automation.
- No MO2 VFS inspection.
- No live game Data mutation.
- No binary plugin generation.
- No external tool execution.
- No public fixture using Bethesda assets or third-party mod files.
- No command alias.
- No new CLI output format.
- No AI behavior.

## Exit evidence

| Evidence | Status |
|---|---|
| JIP emission checksum sidecar verifier exists | Complete |
| Generated emission checksums are revalidated after emission | Complete |
| Valid generated checksum evidence returns no `WF-GEN-008` diagnostics | Complete |
| Edited checksum digest produces `WF-GEN-008` | Complete |
| Missing expected checksum entry produces `WF-GEN-008` | Complete |
| Unexpected checksum entry produces `WF-GEN-008` | Complete |
| Package staging, CLI wiring, runtime probes, GECK automation, MO2 VFS inspection, live Data mutation, and external tools remain out of scope | Complete |

## Validation

- `dotnet build --no-restore` passed with 0 warnings and 0 errors.
- `dotnet test tests\WastelandForge.UnitTests\WastelandForge.UnitTests.csproj --no-build --no-restore --filter "FullyQualifiedName~JipScriptGenerationPlanner"` passed with 10 tests.
- `dotnet test WastelandForge.sln --no-build --no-restore -m:1` passed with
  552 tests.
- `git diff --check` passed. Git reported line-ending warnings for working
  tree text files, but returned success.
- `rg -n "[ \t]$" . --glob '!**/bin/**' --glob '!**/obj/**' --glob '!generated/**' --glob '!dist/**'` found no trailing whitespace outside build output trees.
- Protected path scan found no matching protected filenames. Protected content
  scan found only older gate audit lines that mention the protected-scan
  phrase, not protected project content.

## Next gate

Gate 214 should add canonical `forge generate --target jip-scripts` CLI target
wiring for the existing JIP file emitter. It should stop before `forge build`
target wiring, package staging, runtime probes, GECK automation, MO2 VFS
inspection, live Data mutation, or external tool execution.
