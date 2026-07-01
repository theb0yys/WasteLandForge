# Gate 76 - MCM Extender Install-Preview Schema Validation

Status: Complete
Phase: v0.1 implementation
Decision base: Gate 0, Gate 74, Gate 75, ADR-007, ADR-009, ADR-010, ADR-011

## Definition

Gate 76 formalizes the Gate 75 generated install-preview report:

- adds immutable Draft 2020-12 install-preview report schema
  `install-preview/0.1.0/schema.json`,
- registers the schema in the built-in schema catalog,
- validates generated `install-preview.json` before writing it,
- records the install-preview schema ID in generation/build manifest evidence,
- keeps the report as generated evidence only.

Gate 76 does not install files into a game Data folder, create an MO2 mod,
inspect MO2 VFS/profile visibility, launch the game, verify runtime MCM
visibility, generate FOMOD installers, or generate plugin records.

## Research grounding

| Claim | Classification | Source |
|---|---|---|
| JSON Schema Draft 2020-12 is the source contract and generated contract validation mechanism selected for Forge schemas. | Documented | ADR-007 / R004 contract and schema report |
| Generated outputs are disposable and must carry provenance through local build manifests and checksums. | Documented | ADR-009 / ADR-011 |
| v0.1 generator outputs should prioritize deterministic text and metadata artifacts. | Documented | Generator/build pipeline report / ADR-009 |
| Forge owns packaging manifests and workflow integration but does not replace MO2 or existing tools. | Documented | ADR-004 / FNV content pipeline report |
| Validating install-preview JSON before manifest evidence is finalized is the smallest safe follow-up to Gate 75. | Inferred | Gate 75 open checks plus ADR-007/ADR-009 validation model |
| Actual MO2 installation, VFS conflict visibility, and in-game verification remain unresolved. | Open | Later integration/runtime gates |

## Deliverables

- Add `schemas/install-preview/0.1.0/schema.json`.
- Add install-preview schema catalog ID and embedded-resource coverage.
- Validate generated install-preview JSON inside `McmJsonGenerator`.
- Add schema, back-compat, unit, and golden CLI coverage.
- Update current-gate documentation and `/forge` routing prompts.

## Validation mapping

Gate 76 uses this path:

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
  -> install-preview report emission
  -> local manifest/checksum evidence
```

## Verification

Planned local checks:

```text
dotnet build --no-restore
dotnet test tests/WastelandForge.SchemaTests/WastelandForge.SchemaTests.csproj --no-build --no-restore
dotnet test tests/WastelandForge.BackCompatTests/WastelandForge.BackCompatTests.csproj --no-build --no-restore
dotnet test tests/WastelandForge.UnitTests/WastelandForge.UnitTests.csproj --no-build --no-restore
dotnet test tests/WastelandForge.GoldenTests/WastelandForge.GoldenTests.csproj --no-build --no-restore
dotnet test WastelandForge.sln --no-build --no-restore -m:1
git diff --check
```

Results:

- `dotnet build --no-restore` passed with 0 warnings and 0 errors.
- `dotnet test tests/WastelandForge.SchemaTests/WastelandForge.SchemaTests.csproj --no-build --no-restore` passed with 119 total tests, 0 failed, and 0 skipped.
- `dotnet test tests/WastelandForge.BackCompatTests/WastelandForge.BackCompatTests.csproj --no-build --no-restore` passed with 41 total tests, 0 failed, and 0 skipped.
- `dotnet test tests/WastelandForge.UnitTests/WastelandForge.UnitTests.csproj --no-build --no-restore` passed with 19 total tests, 0 failed, and 0 skipped.
- `dotnet test tests/WastelandForge.GoldenTests/WastelandForge.GoldenTests.csproj --no-build --no-restore` passed with 31 total tests, 0 failed, and 0 skipped.
- Serial solution suite passed with 281 total tests, 0 failed, and 0 skipped.
- `git diff --check` passed with Git LF-to-CRLF working-copy warnings only.

## Open checks

| Check | Status | Notes |
|---|---|---|
| Install-preview schema | Complete | `install-preview/0.1.0/schema.json` is embedded and cataloged. |
| Install-preview validation | Complete | Generated install-preview JSON is validated before it is written. |
| Human-readable install preview | Open | Candidate for Gate 77. |
| Standalone package verifier command | Open | Later command/report gate. |
| Actual install into Data or MO2 | Open | Not implemented by this gate. |
| MO2 profile and VFS scan | Open | Needed before effective visibility claims. |
| In-game verification | Open | Requires launching under an installed FNV/MCM Extender environment. |
| FOMOD package creation | Open | Specialized adapter remains later work. |
| Runtime/provider capability confirmation | Open | Requires runtime probes, version parsing, and `WF-CAP-*` diagnostic projection. |
| Binary plugin generation | Open | Deferred. |

## Next gate

Gate 77 adds a human-readable install-preview summary beside the
machine-readable report, keeping it generated evidence only and not installing
files.
