# Gate 16 - Asset Type-Specific Validation

Status: Complete
Phase: v0.1 implementation
Decision base: Gate 0, Gate 15, ADR-004, ADR-007, ADR-011

## Gate Definition

Gate 16 adds deterministic asset type-specific semantic validation after asset
registry schema validation and Gate 15 asset path validation.

This gate emits `WF-ASSET-005` when a source file with a known target extension
does not match the minimal expected file signature, and `WF-ASSET-006` when a
game-relative target path does not use the expected root folder for the
declared asset type.

This gate does not inspect full NIF internals, parse DDS texture metadata,
validate WAV or OGG encoding properties, validate LIP generation prerequisites,
pair voice lines with dialogue records, enforce BSA packaging rules, materialize
MO2 output mods, or run external tools.

## Research Decisions Used

| Classification | Decision | Source |
|---|---|---|
| Documented | Forge owns validation and production-layer automation, but not creative tools, raw plugin editing, or replacement of established FNV tools. | ADR-004 |
| Documented | FNV asset production is path-sensitive and record-driven. | ADR-004 |
| Documented | WastelandForge source truth is YAML/JSON contracts normalized to canonical JSON, validated by JSON Schema plus deterministic semantic validators. | R004 / ADR-007 |
| Documented | Validation runs in layered stages from load/source validation through schema and semantic validation. | R008 / ADR-011 |
| Documented | Public fixtures must be synthetic and redistributable. | R008 / ADR-011 |
| Inferred | Minimal file signature checks for DDS, WAV, OGG, NIF, and KF targets are safe in Gate 16 because they inspect tiny source-controlled bytes and do not require proprietary assets or external tools. | ADR-004 validation boundary and R008 fixture policy |
| Inferred | Target root checks are contract/path convention validation, not package materialization or BSA validation. | ADR-004 asset workflow and ADR-009 provenance boundaries |
| Open | Voice/dialogue pairing, WAV/OGG encoding constraints, LIP prerequisites, BSA packaging, NIF material references, DDS texture metadata, and RDT/KFM details remain later work. | Later asset/package gates |

## Deliverables

- `WF-ASSET-005` for source signature mismatch on known asset target types.
- `WF-ASSET-006` for target root mismatch by declared asset type.
- Minimal source signature checks for `.dds`, `.wav`, `.ogg`, `.nif`, and `.kf`.
- Target root convention checks for mesh, texture, audio, voice, lip,
  animation, interface, and config assets.
- `script`, `documentation`, and `other` assets are not root-checked in this
  gate because the research-backed Gate 14 open check did not settle those
  conventions.
- `InvalidAssetTypes` broken fixture with schema-valid type-specific failures.
- Semantic fixture tests for both Gate 16 asset rules.

## Validation Mapping

```text
load/source validation
  - discover manifest
  - parse JSON or YAML source contracts
  - normalize YAML into canonical JSON

schema validation
  - validate manifest, dependency, capability, and declared asset registries

semantic validation
  - validate asset source paths stay inside the project root
  - validate required asset source files exist
  - validate target paths are game-data-relative and traversal-free
  - validate target extensions match declared asset types
  - validate source file signatures for known target file types
  - validate target roots match declared asset types
  - preserve WF-SEM-014 for dependency references to undefined capabilities
```

## Verification

Commands:

```text
dotnet build WastelandForge.sln -c Release -m:1 --disable-build-servers
dotnet test tests/WastelandForge.SemanticTests/WastelandForge.SemanticTests.csproj -c Release --no-build --no-restore -m:1 --logger "console;verbosity=minimal"
dotnet test WastelandForge.sln -c Release --no-build --no-restore -m:1 --logger "console;verbosity=minimal"
dotnet test WastelandForge.sln -c Release --no-build --no-restore -m:1 --logger trx --results-directory TestResults/Gate16
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build -- validate fixtures/projects/ExampleMod --format json --no-input
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build -- validate fixtures/projects/YamlExample --format json --no-input
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build -- validate fixtures/projects/BrokenCases/InvalidAssetTypes --format json --no-input
git diff --check
```

Results:

```text
Build succeeded with 0 warnings and 0 errors.
Focused semantic tests passed.
Full suite passed.
TRX files emitted under TestResults/Gate16.
ExampleMod returned exit 0 with required synthetic asset sources present.
YamlExample returned exit 0 with required synthetic asset sources present.
InvalidAssetTypes returned exit 1 with WF-ASSET-005 and WF-ASSET-006.
git diff --check passed.
```

## Open Checks

| Check | Status | Gate |
|---|---|---|
| Add voice/dialogue asset pairing and audio encoding validation. | Open | Gate 17 |
| Add package-specific BSA and loose-file validation. | Open | Package/release gate |
| Add deeper NIF, DDS, WAV, OGG, LIP, KF, KFM, and RDT validators. | Open | Later asset/tool gate |
| Add provider catalogue schema validation. | Open | Later capability/provider gate |
| Replace capability placeholder in build manifest with real resolved capability set. | Open | Capability/build gate |

## Next Gate

Gate 17 should add voice and dialogue asset validation.
