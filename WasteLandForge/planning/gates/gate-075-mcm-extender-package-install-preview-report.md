# Gate 75 - MCM Extender Package Install-Preview Report

Status: Complete
Phase: v0.1 implementation
Decision base: Gate 0, Gate 71, Gate 72, Gate 73, Gate 74, ADR-004, ADR-009, ADR-010, ADR-011

## Definition

Gate 75 adds a deterministic install-preview report for the MCM Extender
loose-file package path:

- `forge generate --target mcm-json` writes
  `generated/mcm-json/install-preview.json`,
- `forge build --target mcm-json` and `forge package --target mcm-json` write
  `dist/mcm-json/install-preview.json`,
- the report lists Data-relative package entries, generated source files,
  would-copy install paths, package archive evidence, and explicit
  preview-only limitations,
- generation/build manifests include install-preview evidence,
- build/package checksums include the install-preview report.

Gate 75 does not install files into a game Data folder, create an MO2 mod,
inspect MO2 VFS/profile visibility, launch the game, verify runtime MCM
visibility, generate FOMOD installers, or generate plugin records.

## Research grounding

| Claim | Classification | Source |
|---|---|---|
| v0.1 generator outputs should prioritize deterministic text and metadata artifacts. | Documented | Generator/build pipeline report / ADR-009 |
| Forge owns packaging manifests, release metadata, and workflow integration but does not replace MO2 or existing tools. | Documented | ADR-004 / FNV content pipeline report |
| `forge package` assembles a distributable staging tree; release/install publication is later. | Documented | R006 CLI workflow report / ADR-010 |
| Generated outputs are disposable and must carry provenance through local build manifests and checksums. | Documented | ADR-009 / ADR-011 |
| A preview-only install report is the smallest safe step after package manifest/archive validation because it explains install intent without mutating external state. | Inferred | Gate 74 open checks plus ADR-004/ADR-009 boundaries |
| Actual MO2 installation, VFS conflict visibility, and in-game verification remain unresolved. | Open | Later integration/runtime gates |

## Deliverables

- Add `install-preview.json` to `McmJsonGenerator` outputs.
- Include install-preview path in JSON and human CLI output.
- Write install-preview JSON for generate/build/package MCM JSON commands.
- Include install-preview in generation/build manifest output digests.
- Include install-preview in build/package `checksums.sha256`.
- Add unit and golden CLI test coverage.
- Update current-gate documentation and `/forge` routing prompts.

## Validation mapping

Gate 75 uses this path:

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
  -> install-preview report emission
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
- Serial solution suite passed with 277 total tests, 0 failed, and 0 skipped.

## Open checks

| Check | Status | Notes |
|---|---|---|
| Install-preview report | Complete | JSON report is emitted for generate/build/package MCM JSON commands. |
| Install-preview schema | Open | Candidate for Gate 76. |
| Standalone package verifier command | Open | Later command/report gate. |
| Actual install into Data or MO2 | Open | Not implemented by this gate. |
| MO2 profile and VFS scan | Open | Needed before effective visibility claims. |
| In-game verification | Open | Requires launching under an installed FNV/MCM Extender environment. |
| FOMOD package creation | Open | Specialized adapter remains later work. |
| Runtime/provider capability confirmation | Open | Requires runtime probes, version parsing, and `WF-CAP-*` diagnostic projection. |
| Binary plugin generation | Open | Deferred. |

## Next gate

Gate 76 adds a formal schema and validation step for `install-preview.json`,
keeping it as generated evidence and not source truth.
