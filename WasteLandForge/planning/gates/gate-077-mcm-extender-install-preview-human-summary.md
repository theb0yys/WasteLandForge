# Gate 77 - MCM Extender Install-Preview Human Summary

Status: Complete
Phase: v0.1 implementation
Decision base: Gate 0, Gate 75, Gate 76, ADR-004, ADR-009, ADR-010, ADR-011

## Definition

Gate 77 adds a deterministic human-readable summary beside the validated
install-preview JSON report:

- `forge generate --target mcm-json` writes
  `generated/mcm-json/install-preview.md`,
- `forge build --target mcm-json` and `forge package --target mcm-json` write
  `dist/mcm-json/install-preview.md`,
- the summary lists package root, preview-only install flags, archive status,
  would-copy Data paths, declared asset sources, target files, and preview
  limitations,
- generation/build manifests include install-preview summary evidence,
- build/package checksums include the install-preview summary.

Gate 77 does not install files into a game Data folder, create an MO2 mod,
inspect MO2 VFS/profile visibility, launch the game, verify runtime MCM
visibility, generate FOMOD installers, or generate plugin records.

## Research grounding

| Claim | Classification | Source |
|---|---|---|
| v0.1 generator outputs should prioritize deterministic text and metadata artifacts. | Documented | Generator/build pipeline report / ADR-009 |
| Human-facing CLI/output should be concise, scanable, and separate from stable machine-readable JSON. | Documented | R006 CLI workflow report / ADR-010 |
| Generated outputs are disposable and must carry provenance through local build manifests and checksums. | Documented | ADR-009 / ADR-011 |
| Forge owns packaging manifests and workflow integration but does not replace MO2 or existing tools. | Documented | ADR-004 / FNV content pipeline report |
| A Markdown summary is the smallest safe follow-up to the schema-validated JSON report because it improves human review without mutating external state. | Inferred | Gate 76 open checks plus ADR-009/ADR-010 output model |
| Actual MO2 installation, VFS conflict visibility, and in-game verification remain unresolved. | Open | Later integration/runtime gates |

## Deliverables

- Add `install-preview.md` to `McmJsonGenerator` outputs.
- Write deterministic Markdown summaries for generate/build/package MCM JSON
  commands after install-preview JSON validation succeeds.
- Include install-preview summary path in JSON and human CLI output.
- Include install-preview summary in generation/build manifest output digests.
- Include install-preview summary in build/package `checksums.sha256`.
- Add unit and golden CLI test coverage.
- Update current-gate documentation and `/forge` routing prompts.

## Validation mapping

Gate 77 uses this path:

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
| Human-readable install preview | Complete | `install-preview.md` is emitted for generate/build/package MCM JSON commands. |
| Install-preview schema | Complete | `install-preview/0.1.0/schema.json` remains the machine-readable report schema. |
| Package verification report | Open | Candidate for Gate 78. |
| Standalone package verifier command | Open | Later command/report gate. |
| Actual install into Data or MO2 | Open | Not implemented by this gate. |
| MO2 profile and VFS scan | Open | Needed before effective visibility claims. |
| In-game verification | Open | Requires launching under an installed FNV/MCM Extender environment. |
| FOMOD package creation | Open | Specialized adapter remains later work. |
| Runtime/provider capability confirmation | Open | Requires runtime probes, version parsing, and `WF-CAP-*` diagnostic projection. |
| Binary plugin generation | Open | Deferred. |

## Next gate

Gate 78 should add a package verification report skeleton for the existing MCM
package tree, keeping it local, deterministic, and non-installing.
