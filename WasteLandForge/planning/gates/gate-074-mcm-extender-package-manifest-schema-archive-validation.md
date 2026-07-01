# Gate 74 - MCM Extender Package Manifest Schema and Archive Validation

Status: Complete
Phase: v0.1 implementation
Decision base: Gate 0, Gate 63, Gate 71, Gate 72, Gate 73, ADR-007, ADR-009, ADR-011

## Definition

Gate 74 formalizes the MCM Extender package evidence emitted by `forge
generate --target mcm-json`, `forge build --target mcm-json`, and `forge
package --target mcm-json`:

- adds immutable Draft 2020-12 package manifest schema
  `package-manifest/0.1.0/schema.json`,
- registers the package manifest schema in the built-in schema catalog,
- validates generated `package-manifest.json` before writing it,
- validates build/package ZIP entries against the deterministic package payload,
- records package validation evidence in `generation-manifest.json` and
  `build-manifest.json`.

Gate 74 does not add a standalone package verifier command, FOMOD installers,
MO2 profile/VFS inspection, runtime provider probes, in-game verification,
plugin generation, or external tool execution.

## Research grounding

| Claim | Classification | Source |
|---|---|---|
| Package outputs and manifests are part of the deterministic v0.1 generator/build surface. | Documented | Generator/build pipeline report / ADR-009 |
| Package validation sits before release validation in the layered validation stack. | Documented | R008 / ADR-011 |
| Generated outputs must carry provenance and local build manifests. | Documented | ADR-009 / ADR-011 |
| Public fixtures must remain synthetic and redistributable. | Documented | R008 / ADR-011 |
| Validating the generated package manifest with an embedded schema is the smallest safe package-validation step after Gate 73. | Inferred | Gate 73 open checks plus ADR-007 schema model |
| Standalone package verification, FOMOD shape, MO2 visibility, and in-game verification remain unresolved. | Open | Later package/runtime gates |

## Deliverables

- Add `schemas/package-manifest/0.1.0/schema.json`.
- Add `WastelandForgeSchemaIds.PackageManifest010`.
- Add the package manifest schema to `WastelandForgeSchemaCatalog`.
- Validate `McmJsonGenerator` package manifests with JSON Schema before
  writing `package-manifest.json`.
- Validate ZIP archive entry names against the sorted package payload for
  build/package distribution commands.
- Add `WF-BUILD-002` and `WF-BUILD-003` diagnostics for internal package
  evidence failures.
- Extend schema, back-compat, unit, and golden test coverage.
- Update current-gate documentation and `/forge` routing prompts.

## Validation mapping

Gate 74 uses this path:

```text
forge build|package --target mcm-json
  -> manifest 0.2.0
  -> asset registry 0.1.0 schema validation
  -> mcm registry 0.1.0 schema validation
  -> asset semantic validation
  -> MCM image filename validation
  -> generated MCM JSON output schema validation
  -> referenced texture asset loose-file staging
  -> package-manifest schema validation
  -> deterministic ZIP archive creation
  -> ZIP entry validation against package payload
  -> package validation evidence in local build manifest
  -> checksum evidence
```

## Verification

Ran:

```text
dotnet build --no-restore
dotnet test tests/WastelandForge.SchemaTests/WastelandForge.SchemaTests.csproj --no-build --no-restore
dotnet test tests/WastelandForge.BackCompatTests/WastelandForge.BackCompatTests.csproj --no-build --no-restore
dotnet test tests/WastelandForge.UnitTests/WastelandForge.UnitTests.csproj --no-build --no-restore
dotnet test tests/WastelandForge.GoldenTests/WastelandForge.GoldenTests.csproj --no-build --no-restore
dotnet test WastelandForge.sln --no-build --no-restore -m:1
```

Results:

- `dotnet build --no-restore` passed with 0 warnings and 0 errors.
- `WastelandForge.SchemaTests`: 116 passed.
- `WastelandForge.BackCompatTests`: 40 passed.
- `WastelandForge.UnitTests`: 19 passed.
- `WastelandForge.GoldenTests`: 31 passed.
- Serial solution suite passed with 277 total tests, 0 failed, and 0 skipped.

The first broad `dotnet test --no-restore` attempt was stopped after it stayed
silent for several minutes and left many responsive `dotnet` worker processes.
The impacted test projects were then run individually, and the full solution
suite was rerun serially with `-m:1`.

## Open checks

| Check | Status | Notes |
|---|---|---|
| Package manifest schema | Complete | `package-manifest/0.1.0/schema.json` is embedded and cataloged. |
| Package manifest validation | Complete | `McmJsonGenerator` validates generated package metadata before writing it. |
| Archive entry validation | Partial | Build/package ZIP entry names are checked against the package payload. |
| Standalone package verifier command | Open | Candidate for a later command/report gate. |
| Package install-preview report | Open | Candidate for Gate 75. |
| FOMOD package creation | Open | Specialized adapter remains later work. |
| MO2 profile and VFS scan | Open | Needed before effective visibility claims. |
| In-game verification | Open | Requires launching under an installed FNV/MCM Extender environment. |
| Full DDS metadata validation | Open | Gate 74 still only uses existing DDS magic-header validation. |
| Runtime/provider capability confirmation | Open | Requires runtime probes, version parsing, and `WF-CAP-*` diagnostic projection. |
| Binary plugin generation | Open | Deferred. |

## Next gate

Gate 75 should add a deterministic package install-preview report for the MCM
Extender loose-file package. It should explain the Data-relative package
entries and archive evidence without installing files, invoking MO2, launching
the game, or claiming runtime verification.
