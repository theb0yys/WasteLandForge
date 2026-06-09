# Gate 5 - Loader and Validation Pipeline

Status: Complete
Phase: v0.1 implementation
Decision base: Gate 0, Gate 3, Gate 4, ADR-007, ADR-008, ADR-010, ADR-011

## Gate Definition

Gate 5 creates the first source loader and validation pipeline slice.

This gate implements load/source validation, a package-free manifest schema
placeholder, a deterministic semantic check for unknown capability references,
a capability/environment stage placeholder, and deterministic JSON report output
from synthetic fixture input.

This gate does not implement YAML parsing, JsonSchema.Net runtime evaluation,
the full ADR-010 CLI skeleton, SARIF output, formal fixture tests, CI workflows,
build manifests, package validation, or release validation.

## Research Decisions Used

| Classification | Decision | Source |
|---|---|---|
| Documented | Canonical source truth is versioned YAML/JSON registry documents normalized to canonical JSON and validated by JSON Schema Draft 2020-12 plus deterministic semantic validators. | R004 / ADR-007 |
| Documented | The manifest is mandatory and dependency and capability registries are bootstrap registries. | R004 / ADR-007 |
| Documented | Registry roots may resolve to a file or directory while keeping the same logical model. | R004 / ADR-007 |
| Documented | Standalone registry documents should include `schemaVersion`, `kind`, and `id` so the loader can detect kind mismatches. | R004 / ADR-007 |
| Documented | Projects depend on capabilities, not providers; detection is local-first and deterministic. | R005 / ADR-008 |
| Documented | Validation is layered: load/source, schema, semantic, capability/environment, generation planning, output, package, release, then manifests and reports. | R008 / ADR-011 |
| Documented | Canonical diagnostics are JSON and include stable rule IDs, JSON Pointer locations, related locations, docs URIs, and fingerprints. | R004 / ADR-007 and R008 / ADR-011 |
| Documented | Public fixtures must be synthetic and redistributable, not Bethesda assets or third-party mod files. | R008 / ADR-011 |
| Documented | `forge validate` is part of the ADR-010/R006 command surface and supports JSON machine-readable diagnostics. | R006 / ADR-010 |
| Documented | JSON machine payloads include `formatVersion` and stable top-level `tool`, `command`, and `summary` shapes. | R006 / ADR-010 |
| Inferred | Gate 5 may wire a minimal `validate` CLI hook before the full Gate 6 CLI skeleton because Gate 5 exit evidence requires `forge validate` against a synthetic fixture. | Gate 0 |
| Open | Exact dependency registry schema shape remains open because Gate 3 only created the manifest schema and reserved dependency/capability schema roots. | Gate 3 / R004 |

## Deliverables

- `src/WastelandForge.Core/DiagnosticReport.cs`
- `src/WastelandForge.Core/DiagnosticReportJsonSerializer.cs`
- `src/WastelandForge.Validation/ProjectValidationPipeline.cs`
- minimal `validate [project-root] [--format json]` CLI hook in `src/WastelandForge.Cli/Program.cs`
- `fixtures/projects/ExampleMod`
- `fixtures/projects/BrokenCases/MissingCapability`
- fixture policy update in `fixtures/README.md`

## Pipeline Scope

Gate 5 implements these stages:

1. Load/source validation:
   - discovers `wastelandforge.json`, `wastelandforge.yaml`, or `wastelandforge.yml`;
   - rejects missing, duplicate, malformed, or currently unsupported YAML manifests;
   - loads JSON registry roots from either files or directories.
2. Schema validation placeholder:
   - checks the required manifest fields covered by the Gate 3 manifest schema;
   - checks kind mismatches on registry documents.
3. Semantic validation:
   - builds a capability ID set from local capability registry documents;
   - emits `WF-SEM-014` when a dependency registry references an undefined capability.
4. Capability/environment placeholder:
   - no provider probing yet; deterministic source validation must exist first.
5. Report output:
   - emits deterministic camel-case JSON with `formatVersion`, `tool`,
     `command`, `summary`, and sorted issues.

## Rule IDs Introduced

| Rule ID | Stage | Meaning |
|---|---|---|
| `WF-LOAD-001` | load/source | Project root or manifest missing. |
| `WF-LOAD-002` | load/source | Multiple root manifests found. |
| `WF-LOAD-003` | load/source | YAML manifest found before YAML ingestion is implemented. |
| `WF-LOAD-004` | load/source | JSON parse failure or non-object document root. |
| `WF-LOAD-005` | load/source | Registry path escapes the project root. |
| `WF-LOAD-006` | load/source | Registry root cannot resolve to JSON files. |
| `WF-SCHEMA-001` | schema | Missing, invalid, or mismatched contract shape. |
| `WF-SCHEMA-002` | schema | Unsupported schema version. |
| `WF-SCHEMA-003` | schema | Invalid logical ID shape. |
| `WF-SEM-014` | semantic | Dependency registry references an undefined capability. |

## Fixture Scope

The Gate 5 fixtures are synthetic JSON-only contracts:

- `fixtures/projects/ExampleMod` validates with zero issues.
- `fixtures/projects/BrokenCases/MissingCapability` emits deterministic `WF-SEM-014`.

The fixtures contain no Bethesda game assets, generated plugins, third-party mod
files, private install paths, or redistributed runtime binaries.

## Verification

Commands:

```text
dotnet build WastelandForge.sln -c Release
dotnet run --project tests/WastelandForge.UnitTests/WastelandForge.UnitTests.csproj -c Release --no-build
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build -- validate fixtures/projects/ExampleMod --format json
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build -- validate fixtures/projects/BrokenCases/MissingCapability --format json
git diff --check
```

Results:

```text
Build succeeded.
0 Warning(s)
0 Error(s)
PASS logical IDs enforce dotted lowercase identity
PASS JSON Pointer validates and escapes canonical locations
PASS semantic version constraints compare stable versions
PASS rule IDs use reserved WastelandForge families
PASS diagnostic issue JSON follows the canonical shape
ExampleMod: summary errors 0, warnings 0, notes 0, exit 0.
MissingCapability: summary errors 1, WF-SEM-014, expected exit 1.
```

## Open Checks

| Check | Status | Gate |
|---|---|---|
| Add YamlDotNet safe-subset ingestion and canonical JSON normalization for YAML contracts. | Open | Later loader gate |
| Add JsonSchema.Net Draft 2020-12 runtime schema evaluation. | Open | Later schema validation gate |
| Add dependency registry schema. | Open | Later schema gate |
| Add capability registry schema. | Open | Later schema gate |
| Add full ADR-010 CLI skeleton, help, version output, and command dispatch. | Open | Gate 6 |
| Add concrete tool version metadata to JSON payloads after the CLI version source is defined. | Open | Gate 6 |
| Convert fixture validation into formal test project coverage. | Open | Gate 7 |
| Add SARIF, Markdown, and console projections from canonical JSON diagnostics. | Open | Gate 8 |

## Next Gate

Gate 6 creates the CLI skeleton:

1. canonical ADR-010 command dispatch,
2. `forge`, `forge --help`, and `forge help`,
3. `forge --version`,
4. stable exit-code mapping,
5. JSON format plumbing without inventing non-canonical aliases.
