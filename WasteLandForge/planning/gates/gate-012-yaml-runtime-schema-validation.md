# Gate 12 - YAML Runtime Schema Validation

Status: Complete
Phase: v0.1 implementation
Decision base: Gate 0, Gate 5, Gate 8, Gate 11, ADR-007, ADR-010, ADR-011

## Gate Definition

Gate 12 implements YAML source contract ingestion and runtime manifest schema
validation without changing the ADR-010 command surface.

This gate adds `.yaml` and `.yml` support for root manifests and registry
documents, normalizes YAML into the canonical JSON source model, evaluates the
embedded manifest JSON Schema Draft 2020-12 schema at runtime, maps YAML source
nodes to JSON Pointer locations with line and column data, and adds focused
fixtures for valid YAML, invalid manifest schema shape, and unsupported YAML.

This gate does not add dependency, capability, or asset registry schemas; does
not implement editor problem matchers; and does not implement capability scan,
generation, packaging, or release publishing behavior.

## Research Decisions Used

| Classification | Decision | Source |
|---|---|---|
| Documented | Canonical source truth is versioned YAML/JSON registry documents normalized to canonical JSON. | R004 / ADR-007 |
| Documented | JSON Schema Draft 2020-12 plus deterministic semantic validators form the source contract validation path. | R004 / ADR-007 |
| Documented | Validation runs in layered stages from load/source validation through schema and semantic validation. | R008 / ADR-011 |
| Documented | The core correctness path must remain offline-first and AI-optional. | R007 / ADR-010 and R008 / ADR-011 |
| Inferred | The first runtime schema evaluator should be scoped to the existing manifest schema because dependency and capability schemas are reserved skeletons. | Gate 3 schema state and Gate 12 scope |
| Inferred | YAML anchors, aliases, merge keys, duplicate mapping keys, and custom tags are rejected for deterministic canonical JSON normalization. | ADR-007 canonical JSON requirement |
| Open | Dependency, capability, and asset registry schemas still need their own public schema contracts. | Later registry schema gate |
| Open | Editor problem matchers and language server diagnostics remain separate editor integration work. | Later editor gate |

## Deliverables

- Manifest discovery for `wastelandforge.json`, `wastelandforge.yaml`, and
  `wastelandforge.yml`.
- YAML registry document loading for `.json`, `.yaml`, and `.yml` source
  contracts.
- YAML normalization into the canonical `JsonObject` validation model.
- Rejection of unsupported YAML features with `WF-LOAD-007`.
- Rejection of duplicate YAML mapping keys with `WF-LOAD-008`.
- Runtime manifest schema evaluation through the embedded manifest schema.
- YAML line and column mapping for diagnostics where source nodes are known.
- `YamlDotNet` and `JsonSchema.Net` validation dependencies.
- Synthetic YAML and broken-case fixtures.

## Validation Mapping

```text
load/source validation
  - discover one root manifest
  - parse JSON or YAML
  - normalize YAML into canonical JSON
  - reject unsupported YAML features
  - record YAML line and column positions

schema validation
  - evaluate embedded manifest/0.1.0/schema.json at runtime
  - emit WF-SCHEMA-002 for unsupported schemaVersion
  - emit WF-SCHEMA-003 for invalid project id
  - emit WF-SCHEMA-001 for other manifest shape failures

semantic validation
  - run only after blocking load and schema diagnostics are absent
  - preserve Gate 5 capability-reference validation behavior
```

## Verification

Commands:

```text
dotnet restore WastelandForge.sln
dotnet build WastelandForge.sln -c Release -m:1 --disable-build-servers
dotnet test tests/WastelandForge.SchemaTests/WastelandForge.SchemaTests.csproj -c Release --no-build --no-restore --logger "console;verbosity=minimal"
dotnet test tests/WastelandForge.SemanticTests/WastelandForge.SemanticTests.csproj -c Release --no-build --no-restore --logger "console;verbosity=minimal"
dotnet test WastelandForge.sln -c Release --no-build --no-restore -m:1 --logger "console;verbosity=minimal"
dotnet test WastelandForge.sln -c Release --no-build --no-restore -m:1 --logger trx --results-directory TestResults/Gate12
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build -- validate fixtures/projects/YamlExample --format json --no-input
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build -- validate fixtures/projects/BrokenCases/InvalidManifestSchema --format json --no-input
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build -- validate fixtures/projects/BrokenCases/UnsupportedYamlFeature --format github --no-input
git diff --check
```

Results:

```text
dotnet restore succeeded after network approval for NuGet package restore.
Build succeeded with 0 warnings and 0 errors.
Focused schema tests passed.
Focused semantic tests passed.
Full suite passed.
TRX files emitted under TestResults/Gate12.
YamlExample validation returned exit 0 with no issues.
InvalidManifestSchema returned exit 1 with WF-SCHEMA-003.
UnsupportedYamlFeature returned exit 1 with WF-LOAD-007.
git diff --check passed.
```

## Open Checks

| Check | Status | Gate |
|---|---|---|
| Add dependency registry JSON Schema validation. | Open | Gate 13 |
| Add capability registry JSON Schema validation. | Open | Gate 13 |
| Add asset registry JSON Schema validation. | Open | Later registry/assets gate |
| Add editor problem matcher or language server output. | Open | Later editor gate |
| Replace capability placeholder in build manifest with real resolved capability set. | Open | Capability/build gate |

## Next Gate

Gate 13 should add dependency and capability registry schema validation.
