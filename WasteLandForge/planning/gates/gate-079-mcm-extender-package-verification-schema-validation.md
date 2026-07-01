# Gate 79 - MCM Extender Package Verification Schema Validation

Status: Complete
Phase: v0.1 implementation
Decision base: Gate 0, Gate 74, Gate 76, Gate 78, ADR-007, ADR-009, ADR-010, ADR-011

## Definition

Gate 79 formalizes the Gate 78 generated package-verification report:

- adds immutable Draft 2020-12 package-verification report schema
  `package-verification/0.1.0/schema.json`,
- registers the schema in the built-in schema catalog,
- validates generated `package-verification.json` before writing it,
- reserves `WF-BUILD-005` for generated package-verification schema failures,
- records the package-verification schema ID in generation/build manifest
  evidence,
- keeps the generated report local, deterministic, and non-installing.

Gate 79 does not create a standalone verifier command, add a human Markdown
summary, install files into a game Data folder, create an MO2 mod, inspect MO2
VFS/profile visibility, launch the game, verify runtime MCM visibility,
generate FOMOD installers, or generate plugin records.

## Research grounding

| Claim | Classification | Source |
|---|---|---|
| JSON Schema Draft 2020-12 is the canonical schema dialect for WastelandForge contracts. | Documented | R004 contract/schema report / ADR-007 |
| Released schema IDs should be absolute, versioned, immutable, and locally resolvable offline. | Documented | R004 contract/schema report / R008 validation-governance report |
| Generated artifacts are outputs, not source truth, and must carry provenance through local manifests and checksums. | Documented | ADR-009 / ADR-011 |
| Generated evidence schemas should validate report shape before local manifest/checksum evidence is finalized. | Inferred | Gate 74 and Gate 76 precedent plus ADR-007 validation model |
| Standalone package verifier command and actual MO2/in-game validation remain unresolved. | Open | Later package/runtime gates |

## Deliverables

- Add `schemas/package-verification/0.1.0/schema.json`.
- Add package-verification schema README entry.
- Add `WastelandForgeSchemaIds.PackageVerification010`.
- Add package-verification schema to `WastelandForgeSchemaCatalog`.
- Validate generated package-verification JSON inside `McmJsonGenerator`.
- Add `WF-BUILD-005` diagnostic documentation.
- Add schema/back-compat/unit/golden test coverage.
- Update current-gate documentation and `/forge` routing prompts.

## Validation mapping

Gate 79 uses this path:

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
  -> package-verification schema validation
  -> package-verification report emission
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
- `dotnet test tests/WastelandForge.SchemaTests/WastelandForge.SchemaTests.csproj --no-build --no-restore` passed with 122 total tests, 0 failed, and 0 skipped.
- `dotnet test tests/WastelandForge.BackCompatTests/WastelandForge.BackCompatTests.csproj --no-build --no-restore` passed with 42 total tests, 0 failed, and 0 skipped.
- `dotnet test tests/WastelandForge.UnitTests/WastelandForge.UnitTests.csproj --no-build --no-restore` passed with 19 total tests, 0 failed, and 0 skipped.
- `dotnet test tests/WastelandForge.GoldenTests/WastelandForge.GoldenTests.csproj --no-build --no-restore` passed with 31 total tests, 0 failed, and 0 skipped.
- Serial solution suite passed with 285 total tests, 0 failed, and 0 skipped.
- `git diff --check` passed with Git LF-to-CRLF working-copy warnings only.

## Open checks

| Check | Status | Notes |
|---|---|---|
| Package-verification schema | Complete | `package-verification/0.1.0/schema.json` is embedded and cataloged. |
| Package-verification validation | Complete | Generated package-verification JSON is validated before it is written. |
| Package-verification human summary | Open | Candidate for Gate 80. |
| Standalone package verifier command | Open | Later command/report gate. |
| Actual install into Data or MO2 | Open | Not implemented by this gate. |
| MO2 profile and VFS scan | Open | Needed before effective visibility claims. |
| In-game verification | Open | Requires launching under an installed FNV/MCM Extender environment. |
| FOMOD package creation | Open | Specialized adapter remains later work. |
| Runtime/provider capability confirmation | Open | Requires runtime probes, version parsing, and `WF-CAP-*` diagnostic projection. |
| Binary plugin generation | Open | Deferred. |

## Next gate

Gate 80 should add a human-readable package-verification summary beside
`package-verification.json`, keeping it generated evidence only.
