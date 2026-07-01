# Gate 78 - MCM Extender Package Verification Report

Status: Complete
Phase: v0.1 implementation
Decision base: Gate 0, Gate 74, Gate 75, Gate 76, Gate 77, ADR-004, ADR-009, ADR-010, ADR-011

## Definition

Gate 78 adds a deterministic package-verification report beside the existing
MCM Extender package evidence:

- `forge generate --target mcm-json` writes
  `generated/mcm-json/package-verification.json`,
- `forge build --target mcm-json` and `forge package --target mcm-json` write
  `dist/mcm-json/package-verification.json`,
- the report summarizes package root/layout, package entry counts, package
  manifest schema evidence, install-preview schema evidence, install-preview
  summary evidence, payload digest recording, archive status, and archive entry
  validation status,
- generation/build manifests include package-verification report evidence,
- CLI JSON output and human output include the report,
- build/package checksums include the report.

Gate 78 does not add a formal package-verification JSON Schema, create a
standalone verifier command, install files into a game Data folder, create an
MO2 mod, inspect MO2 VFS/profile visibility, launch the game, verify runtime
MCM visibility, generate FOMOD installers, or generate plugin records.

## Research grounding

| Claim | Classification | Source |
|---|---|---|
| v0.1 generator outputs should prioritize deterministic text and metadata artifacts. | Documented | Generator/build pipeline report / ADR-009 |
| Generated outputs are disposable and must carry provenance through local build manifests and checksums. | Documented | ADR-009 / ADR-011 |
| Stable machine-readable CLI output should remain separate from human-facing output. | Documented | R006 CLI workflow report / ADR-010 |
| Forge owns packaging manifests and workflow integration but does not replace MO2 or existing tools. | Documented | ADR-004 / FNV content pipeline report |
| A local package-verification report is the smallest safe follow-up to install-preview summary evidence because it improves package review without mutating external state. | Inferred | Gate 77 open checks plus ADR-009/ADR-010 output model |
| A formal package-verification schema and standalone verifier command remain unresolved. | Open | Later generated-evidence/package gates |

## Deliverables

- Add `package-verification.json` to `McmJsonGenerator` outputs.
- Write deterministic package-verification JSON for generate/build/package MCM
  JSON commands after package manifest, install-preview, and optional archive
  validation succeed.
- Include package-verification path in JSON and human CLI output.
- Include package-verification report evidence in generation/build manifests.
- Include package-verification in generation/build output digests.
- Include package-verification in build/package `checksums.sha256`.
- Add unit and golden CLI test coverage.
- Update current-gate documentation and `/forge` routing prompts.

## Validation mapping

Gate 78 uses this path:

```text
forge generate|build|package --target mcm-json
  -> manifest 0.2.0
  -> asset registry 0.1.0 schema validation
  -> mcm registry 0.1.0 schema validation
  -> asset semantic validation
  -> MCM image filename validation
  -> generated MCM JSON output schema validation
  -> referenced texture asset loose-file staging
  -> package-manifest schema validation
  -> optional deterministic ZIP archive creation
  -> optional ZIP entry validation against package payload
  -> install-preview schema validation
  -> install-preview JSON emission
  -> install-preview Markdown summary emission
  -> package-verification report emission
  -> local manifest/checksum evidence
```

## Verification

Planned local checks:

```text
dotnet build --no-restore
dotnet test tests/WastelandForge.UnitTests/WastelandForge.UnitTests.csproj --no-build --no-restore
dotnet test tests/WastelandForge.GoldenTests/WastelandForge.GoldenTests.csproj --no-build --no-restore
dotnet test WastelandForge.sln --no-build --no-restore -m:1
git diff --check
```

Results:

- `dotnet build --no-restore` passed with 0 warnings and 0 errors.
- `dotnet test tests/WastelandForge.UnitTests/WastelandForge.UnitTests.csproj --no-build --no-restore` passed with 19 total tests, 0 failed, and 0 skipped.
- `dotnet test tests/WastelandForge.GoldenTests/WastelandForge.GoldenTests.csproj --no-build --no-restore` passed with 31 total tests, 0 failed, and 0 skipped.
- Serial solution suite passed with 281 total tests, 0 failed, and 0 skipped.
- `git diff --check` passed with Git LF-to-CRLF working-copy warnings only.

## Open checks

| Check | Status | Notes |
|---|---|---|
| Package verification report | Complete | `package-verification.json` is emitted for generate/build/package MCM JSON commands. |
| Package-verification schema | Open | Candidate for Gate 79. |
| Standalone package verifier command | Open | Later command/report gate. |
| Actual install into Data or MO2 | Open | Not implemented by this gate. |
| MO2 profile and VFS scan | Open | Needed before effective visibility claims. |
| In-game verification | Open | Requires launching under an installed FNV/MCM Extender environment. |
| FOMOD package creation | Open | Specialized adapter remains later work. |
| Runtime/provider capability confirmation | Open | Requires runtime probes, version parsing, and `WF-CAP-*` diagnostic projection. |
| Binary plugin generation | Open | Deferred. |

## Next gate

Gate 79 should add a formal package-verification report schema and validation
step for `package-verification.json`, keeping it generated evidence rather
than canonical source truth.
