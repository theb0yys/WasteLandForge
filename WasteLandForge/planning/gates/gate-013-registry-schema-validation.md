# Gate 13 - Registry Schema Validation

Status: Complete
Phase: v0.1 implementation
Decision base: Gate 0, Gate 3, Gate 5, Gate 12, ADR-007, ADR-008, ADR-011

## Gate Definition

Gate 13 adds runtime schema validation for dependency and capability registry
documents.

This gate creates immutable Draft 2020-12 public schemas for
`dependencies/0.1.0` and `capabilities/0.1.0`, registers both schemas in the
built-in schema catalog, validates dependency and capability registry documents
after load/source validation and before semantic validation, and updates
synthetic fixtures to use schema-backed capability provider data.

This gate does not add asset registry schema validation, provider catalogue
schema validation, environment detection, provider scans, generation, package
creation, or release publishing.

## Research Decisions Used

| Classification | Decision | Source |
|---|---|---|
| Documented | Source truth is versioned YAML/JSON registry documents normalized to canonical JSON and validated by JSON Schema Draft 2020-12 plus deterministic semantic validators. | R004 / ADR-007 |
| Documented | Dependency and capability registries are mandatory bootstrap registries. | R004 / ADR-007 |
| Documented | Standalone registry documents should include `schemaVersion`, `kind`, and `id`. | R004 / ADR-007 |
| Documented | Projects depend on capabilities, not provider names. Providers are versioned registry data that satisfy capabilities. | R005 / ADR-008 |
| Documented | Validation runs in layered stages from load/source validation through schema and semantic validation. | R008 / ADR-011 |
| Inferred | Gate 13 validates project dependency and capability registry documents but defers a separate provider catalogue schema until provider detection begins. | ADR-008 and Gate 13 scope |
| Inferred | The v0.1 capability registry document uses `satisfiedBy` provider references so capability data can evolve toward provider catalogues without adding detector execution in this gate. | R005 / ADR-008 |
| Open | Asset registry schema validation remains unresolved. | Later registry/assets gate |
| Open | Provider catalogue schema and detector result schema remain unresolved. | Later capability/provider gate |

## Deliverables

- `schemas/dependencies/0.1.0/schema.json`
- `schemas/capabilities/0.1.0/schema.json`
- `WastelandForgeSchemaIds.Dependency010`
- `WastelandForgeSchemaIds.Capability010`
- built-in schema catalog entries for dependency and capability schemas
- runtime dependency registry schema validation
- runtime capability registry schema validation
- updated synthetic valid fixtures using capability `satisfiedBy`
- invalid dependency and capability registry broken-case fixtures
- schema, semantic, and back-compat tests for registry schemas

## Validation Mapping

```text
load/source validation
  - discover manifest
  - parse JSON or YAML source contracts
  - normalize YAML into canonical JSON

schema validation
  - validate manifest against manifest/0.1.0
  - validate dependency registries against dependencies/0.1.0
  - validate capability registries against capabilities/0.1.0

semantic validation
  - run only after blocking schema diagnostics are absent
  - preserve WF-SEM-014 for dependency references to undefined capabilities
```

## Verification

Commands:

```text
dotnet build WastelandForge.sln -c Release -m:1 --disable-build-servers
dotnet test tests/WastelandForge.SchemaTests/WastelandForge.SchemaTests.csproj -c Release --no-build --no-restore --logger "console;verbosity=minimal"
dotnet test tests/WastelandForge.SemanticTests/WastelandForge.SemanticTests.csproj -c Release --no-build --no-restore --logger "console;verbosity=minimal"
dotnet test tests/WastelandForge.BackCompatTests/WastelandForge.BackCompatTests.csproj -c Release --no-build --no-restore --logger "console;verbosity=minimal"
dotnet test WastelandForge.sln -c Release --no-build --no-restore -m:1 --logger "console;verbosity=minimal"
dotnet test WastelandForge.sln -c Release --no-build --no-restore -m:1 --logger trx --results-directory TestResults/Gate13
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build -- validate fixtures/projects/BrokenCases/InvalidDependencyRegistry --format json --no-input
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build -- validate fixtures/projects/BrokenCases/InvalidCapabilityRegistry --format json --no-input
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build -- validate fixtures/projects/BrokenCases/MissingCapability --format sarif --no-input
git diff --check
```

Results:

```text
Build succeeded with 0 warnings and 0 errors.
Focused schema tests passed.
Focused semantic tests passed.
Focused back-compat tests passed.
Full suite passed.
TRX files emitted under TestResults/Gate13.
InvalidDependencyRegistry returned exit 1 with WF-SCHEMA-001.
InvalidCapabilityRegistry returned exit 1 with WF-SCHEMA-001.
MissingCapability still returned WF-SEM-014 after registry schema validation.
git diff --check passed.
```

## Open Checks

| Check | Status | Gate |
|---|---|---|
| Add asset registry JSON Schema validation. | Open | Gate 14 |
| Add provider catalogue schema validation. | Open | Later capability/provider gate |
| Add detector result schema validation. | Open | Later capability/provider gate |
| Replace capability placeholder in build manifest with real resolved capability set. | Open | Capability/build gate |
| Add editor problem matcher or language server output. | Open | Later editor gate |

## Next Gate

Gate 14 should add asset registry schema validation.
