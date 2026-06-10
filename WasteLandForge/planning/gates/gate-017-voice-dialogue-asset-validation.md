# Gate 17 - Voice and Dialogue Asset Validation

Status: Complete
Phase: v0.1 implementation
Decision base: Gate 0, Gate 16, ADR-004, ADR-007, ADR-011

## Gate Definition

Gate 17 adds deterministic voice and dialogue asset validation that can be
performed from the current asset registry model.

This gate emits `WF-ASSET-007` when a voice or lip target under `sound/voice/`
does not include plugin and voice type folders, `WF-ASSET-008` when a required
voice asset target stem does not declare both `.wav` and `.ogg` assets, and
`WF-ASSET-009` when a required voice asset target stem does not declare a
matching `.lip` asset.

This gate does not validate audio sample rate, bitrate, channel count, codec
metadata, dialogue-record membership, GECK lip processing files, BSA packaging,
MO2 materialization, or external encoder/tool execution.

## Research Decisions Used

| Classification | Decision | Source |
|---|---|---|
| Documented | Voice and dialogue production are brittle, high-value areas for Forge generation, validation, and build automation. | ADR-004 / FNV asset pipeline research |
| Documented | Voice assets must sit under `Data\Sound\Voice\[PluginName]\[VoiceType]`. | FNV asset pipeline research |
| Documented | Lip generation needs WAV and OGG assets, and New Vegas OGG files have specific 24 kHz, average 64 kbps VBR, mono expectations. | FNV asset pipeline research |
| Documented | Forge should verify WAV/OGG pairs, check sample-rate/channel constraints, detect lip prerequisites, and generate dialogue/voice manifests. | FNV asset pipeline research |
| Documented | WastelandForge source truth is YAML/JSON contracts normalized to canonical JSON, validated by JSON Schema plus deterministic semantic validators. | R004 / ADR-007 |
| Documented | Public fixtures must be synthetic and redistributable. | R008 / ADR-011 |
| Inferred | The existing asset registry can deterministically validate voice target shape and declared WAV/OGG/LIP pair presence by target stem before dialogue registry contracts exist. | ADR-004 voice workflow and ADR-007 registry model |
| Inferred | Dialogue-record membership and audio encoding checks are outside this gate because the current v0.1 asset registry does not contain dialogue records or parsed audio metadata. | R004 registry order and FNV asset pipeline requirements |
| Open | Dialogue manifest/worklist contracts, filename-to-dialogue validation, WAV/OGG metadata validation, GECK lip processing prerequisite detection, and tool-backed voice build steps remain later work. | Later dialogue, audio, and capability gates |

## Deliverables

- `WF-ASSET-007` for voice/lip target paths missing plugin or voice type folders.
- `WF-ASSET-008` for required voice asset stems missing a `.wav` or `.ogg` pair.
- `WF-ASSET-009` for required voice asset stems missing a matching `.lip` pair.
- Cross-document asset aggregation before pair validation.
- `InvalidVoiceAssets` broken fixture with synthetic WAV/OGG/LIP stand-ins.
- Semantic fixture tests for all Gate 17 asset rules.

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
  - validate voice/lip target shape under sound/voice
  - validate required voice WAV/OGG pairs by target stem
  - validate required voice LIP pairs by target stem
  - preserve WF-SEM-014 for dependency references to undefined capabilities
```

## Verification

Commands:

```text
dotnet build WastelandForge.sln -c Release -m:1 --disable-build-servers
dotnet test tests/WastelandForge.SemanticTests/WastelandForge.SemanticTests.csproj -c Release --no-build --no-restore -m:1 --logger "console;verbosity=minimal"
dotnet test WastelandForge.sln -c Release --no-build --no-restore -m:1 --logger "console;verbosity=minimal"
dotnet test WastelandForge.sln -c Release --no-build --no-restore -m:1 --logger trx --results-directory TestResults/Gate17
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build -- validate fixtures/projects/ExampleMod --format json --no-input
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build -- validate fixtures/projects/YamlExample --format json --no-input
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build -- validate fixtures/projects/BrokenCases/InvalidVoiceAssets --format json --no-input
git diff --check
```

Results:

```text
Build succeeded with 0 warnings and 0 errors.
Focused semantic tests passed.
Full suite passed.
TRX files emitted under TestResults/Gate17.
ExampleMod returned exit 0 with required synthetic asset sources present.
YamlExample returned exit 0 with required synthetic asset sources present.
InvalidVoiceAssets returned exit 1 with WF-ASSET-007 through WF-ASSET-009.
git diff --check passed.
```

## Open Checks

| Check | Status | Gate |
|---|---|---|
| Add dialogue registry and voice worklist contracts so filenames can be validated against declared dialogue lines. | Open | Gate 18 |
| Add WAV/OGG sample-rate, bitrate, channel, and codec metadata validation. | Open | Later audio gate |
| Detect GECK lip processing prerequisites in local environment or capability scans. | Open | Later capability/environment gate |
| Add package-specific BSA and loose-file validation. | Open | Package/release gate |
| Add deeper NIF, DDS, WAV, OGG, LIP, KF, KFM, and RDT validators. | Open | Later asset/tool gate |

## Next Gate

Gate 18 should add the dialogue registry and voice worklist skeleton.
