# Source

This directory holds WastelandForge implementation projects.

Gate 2 creates empty buildable project skeletons:

- `WastelandForge.Core`
- `WastelandForge.Schema`
- `WastelandForge.Registry`
- `WastelandForge.Validation`
- `WastelandForge.Generation`
- `WastelandForge.Provenance`
- `WastelandForge.Cli`

Gate 6 adds the first package-free CLI skeleton in `WastelandForge.Cli`.
Only `forge validate` performs real validation work at this stage; the rest of
the ADR-010 command surface is reserved with stable help and status output until
later gates implement each workflow.

Gate 10 and Gate 11 add diagnostic projections in `WastelandForge.Core`:
SARIF, Markdown summaries, and GitHub workflow-command annotations.

Gate 12 adds YAML source contract ingestion and runtime manifest JSON Schema
evaluation in `WastelandForge.Validation`, backed by embedded schema text from
`WastelandForge.Schema`.

Gate 13 adds dependency and capability registry JSON Schema evaluation through
the same validation path.

Gate 14 adds optional asset registry JSON Schema evaluation through the same
validation path.

Gate 15 adds asset path semantic validation in `WastelandForge.Validation`,
including source containment, required source existence, target traversal, and
basic asset-type target extension checks.

Gate 16 adds asset type-specific semantic validation in
`WastelandForge.Validation`, including minimal source signature checks for DDS,
WAV, OGG, NIF, and KF targets plus target-root convention checks by declared
asset type.

Gate 17 adds voice and dialogue asset validation in
`WastelandForge.Validation`, including voice/lip target shape checks plus
WAV/OGG/LIP pair checks by game-relative voice target stem.

Gate 18 adds dialogue registry runtime schema validation and dialogue voice
worklist semantic validation in `WastelandForge.Validation`, backed by the
embedded dialogue schema in `WastelandForge.Schema`.

Gate 19 adds quest registry runtime schema validation and dialogue `questId`
semantic validation in `WastelandForge.Validation`, backed by the embedded
quest schema in `WastelandForge.Schema`.

Gate 20 adds quest registry schema `0.2.0` runtime validation and quest
objective stage-reference semantic validation in `WastelandForge.Validation`,
while preserving quest registry schema `0.1.0` in `WastelandForge.Schema`.

Gate 21 adds quest registry schema `0.3.0` runtime validation and quest
transition stage-reference semantic validation in `WastelandForge.Validation`,
while preserving earlier quest registry schema versions in
`WastelandForge.Schema`.

Gate 22 adds quest registry schema `0.4.0` runtime validation and quest
condition stage-reference semantic validation in `WastelandForge.Validation`,
while preserving earlier quest registry schema versions in
`WastelandForge.Schema`.

Gate 23 adds quest registry schema `0.5.0` runtime validation and quest
result-script condition-reference semantic validation in
`WastelandForge.Validation`, while preserving earlier quest registry schema
versions in `WastelandForge.Schema`.

Gate 24 adds quest registry schema `0.6.0` runtime validation and quest
condition variable-reference semantic validation in
`WastelandForge.Validation`, while preserving earlier quest registry schema
versions in `WastelandForge.Schema`.

Gate 25 adds dialogue registry schema `0.2.0` runtime validation and dialogue
condition quest-state reference semantic validation in
`WastelandForge.Validation`, while preserving dialogue registry schema
`0.1.0` in `WastelandForge.Schema`.

Gate 26 adds dialogue registry schema `0.3.0` runtime validation for
line-local dialogue result-script skeletons in `WastelandForge.Validation`,
while preserving earlier dialogue registry schema versions in
`WastelandForge.Schema`.

Gate 27 adds dialogue registry schema `0.4.0` runtime validation for dialogue
topic and `linkTo` skeletons in `WastelandForge.Validation`. It adds semantic
topic-reference checks for line `topicId` values and link target topics while
preserving earlier dialogue registry schema versions in `WastelandForge.Schema`.

Gate 28 adds dialogue registry schema `0.5.0` runtime validation for
quest-level dialogue gate skeletons in `WastelandForge.Validation`. It adds
semantic checks for gate quest references plus gate condition stage and
variable references while preserving earlier dialogue registry schema versions
in `WastelandForge.Schema`.

Gate 29 adds dialogue registry schema `0.6.0` runtime validation for dialogue
result-script quest-variable mutation skeletons in `WastelandForge.Validation`.
It adds semantic checks for mutation variable references while preserving
earlier dialogue registry schema versions in `WastelandForge.Schema`.

Gate 30 adds dialogue registry schema `0.7.0` runtime validation for dialogue
Link From skeletons in `WastelandForge.Validation`. It adds semantic checks for
`linkFrom` source topic references while preserving earlier dialogue registry
schema versions in `WastelandForge.Schema`.

Gate 31 adds semantic dialogue link graph endpoint validation in
`WastelandForge.Validation`. It adds checks for declared `linkTo` target topics
and declared `linkFrom` source topics that have no authored dialogue line
endpoint, without adding a new schema version.

Gate 32 adds dialogue registry schema `0.8.0` runtime validation for dialogue
priority and prompt routing skeletons in `WastelandForge.Validation`. It adds
semantic duplicate prompt route validation for schema-valid dialogue documents.

Gate 33 adds dialogue registry schema `0.9.0` runtime validation for dialogue
Speech Challenge skeletons in `WastelandForge.Validation`. It adds no semantic
rule because the new fields do not yet reference other authored state.

Gate 34 adds dialogue registry schema `0.10.0` runtime validation for
line-local dialogue skill gate skeletons in `WastelandForge.Validation`. It
adds no semantic rule because the new fields do not yet reference a skill
registry or other authored state.

Gate 35 adds dialogue registry schema `0.11.0` runtime validation for
line-local dialogue perk gate skeletons in `WastelandForge.Validation`. It adds
no semantic rule because the new fields do not yet reference a perk registry or
other authored state.

Gate 36 adds dialogue registry schema `0.12.0` runtime validation for
line-local dialogue faction relation and reputation standing gate skeletons in
`WastelandForge.Validation`. It adds no semantic rule because the new fields do
not yet reference faction or reputation registries.

Gate 37 adds dialogue registry schema `0.13.0` runtime validation for
line-local dialogue identity gate skeletons in `WastelandForge.Validation`. It
adds no semantic rule because the new fields do not yet reference an identity
registry or other authored state.

Gate 38 adds dialogue registry schema `0.14.0` runtime validation for
line-local dialogue local world flag gate skeletons in
`WastelandForge.Validation`. It adds no semantic rule because the new fields do
not yet reference a world-state registry or other authored state.

Gate 39 adds dialogue registry schema `0.15.0` runtime validation for
line-local dialogue event history gate skeletons in `WastelandForge.Validation`.
It adds no semantic rule because the new fields do not yet reference an
event-history registry or other authored state.

Gate 40 adds dialogue registry schema `0.16.0` runtime validation for
line-local dialogue companion state gate skeletons in
`WastelandForge.Validation`. It adds no semantic rule because the new fields do
not yet reference a companion registry or other authored state.

Gate 41 adds dialogue registry schema `0.17.0` runtime validation for
line-local dialogue result-script side-effect gate skeletons in
`WastelandForge.Validation`. It adds no semantic rule because the new fields do
not yet reference an effect registry or other authored state.

Gate 42 adds dialogue registry schema `0.18.0` runtime validation for
line-local dialogue condition boolean composition skeletons in
`WastelandForge.Validation`. It adds no semantic rule because the new fields do
not yet resolve condition IDs to authored conditions.

Gate 43 adds semantic dialogue condition logic reference validation in
`WastelandForge.Validation`. It emits `WF-SEM-034` when a schema-valid
`conditionLogic.conditionIds[]` entry does not resolve to a condition authored
on the same dialogue line.

Gate 44 adds dialogue registry schema `0.19.0` runtime validation for nested
dialogue condition group skeletons in `WastelandForge.Validation`. It extends
the existing `WF-SEM-034` semantic traversal to condition IDs declared inside
nested `conditionLogic.groups[]` entries.

Gate 45 adds `GeckDialogueExportValidator` in `WastelandForge.Validation` and
wires it into `forge validate --geck-dialogue-export <path>` from
`WastelandForge.Cli`. The bridge is file-based: it validates that a GECK
dialogue export text file exists, is readable text, and is non-empty.

Gate 46 adds dialogue registry schema `0.20.0` runtime validation for explicit
line-local dialogue condition negation skeletons in `WastelandForge.Validation`.
It extends existing `WF-SEM-034` semantic traversal to condition IDs declared
inside `conditionLogic.negatedConditionIds[]`.

Gate 47 adds dialogue registry schema `0.21.0` runtime validation for explicit
line-local dialogue condition precedence skeletons in
`WastelandForge.Validation`. Precedence is schema-only in this gate and does
not decide evaluation ordering or GECK condition-list mapping.

Gate 48 adds dialogue registry schema `0.22.0` runtime validation for explicit
line-local dialogue condition short-circuit skeletons in
`WastelandForge.Validation`. Short-circuit intent is schema-only in this gate
and does not decide evaluation behavior or GECK condition-list mapping.

Gate 49 adds `WF-SEM-035` semantic validation in `WastelandForge.Validation`
for duplicate root or nested dialogue condition logic IDs inside one
line-local condition logic tree. It does not add a new schema version.

Gate 50 adds dialogue registry schema `0.23.0` runtime validation for explicit
line-local dialogue response route skeletons in `WastelandForge.Validation`.
Response route intent is schema-only in this gate and does not decide target
reference validation, route selection behavior, or GECK mapping.

Gate 51 adds `WF-SEM-036` semantic validation in
`WastelandForge.Validation` for schema-valid dialogue response route
`targetTopicId` values that do not resolve to declared dialogue topics. It
does not add a new schema version.

Gate 52 adds `WF-SEM-037` semantic validation in
`WastelandForge.Validation` for schema-valid dialogue response route target
topics that are declared but have no authored dialogue line endpoint. It does
not add a new schema version.

Gate 53 adds `WF-SEM-038` semantic validation in
`WastelandForge.Validation` for duplicate response route IDs authored on the
same dialogue line. It does not add a new schema version.

Gate 54 adds `WF-SEM-039` semantic validation in
`WastelandForge.Validation` for duplicate response route keys authored on the
same dialogue line. It does not add a new schema version.

Gate 55 adds no runtime code. It preserves dialogue registry schema `0.23.0`
and the existing `WF-SEM-036` through `WF-SEM-039` response route validators
while recording response route taxonomy as evidence-blocked.

Gate 56 adds no runtime code. It creates a dialogue response route taxonomy
evidence pack skeleton under `docs/dialogue/` and preserves the existing
response route validators unchanged.

Gate 57 adds `CapabilityCatalog`, `CapabilityDefinition`, and
`ProviderDefinition` records plus `BuiltInFnvCapabilityCatalog` in
`WastelandForge.Registry`. It wires `forge capabilities list` through
`WastelandForge.Cli` with text and JSON renderers. `capabilities scan` and
`capabilities explain` remain reserved.

Gate 58 adds capability scan records and `BuiltInFnvCapabilityScanner` in
`WastelandForge.Registry`. It wires `forge capabilities scan` through
`WastelandForge.Cli` with text and JSON renderers for root-file, data-file,
and executable-tool evidence. Runtime probes, MO2 VFS launch, provider
version checks, `WF-CAP-*` diagnostics, and `capabilities explain` remain
future work.

Gate 59 adds capability explanation records and `BuiltInFnvCapabilityExplainer`
in `WastelandForge.Registry`. It wires `forge capabilities explain` through
`WastelandForge.Cli` with text and JSON renderers for built-in capability and
provider targets over Gate 58 scan evidence. Runtime probes, MO2 VFS launch,
provider version checks, `WF-CAP-*` diagnostics, and project requirement
resolution remain future work.

Gate 60 adds capability requirement resolution records and
`BuiltInFnvCapabilityRequirementResolver` in `WastelandForge.Registry`. It
exposes a dependency requirement reader through `WastelandForge.Validation`
and wires `forge capabilities scan --project` through `WastelandForge.Cli`
with text and JSON requirement summaries. Runtime probes, MO2 VFS launch,
provider version checks, and `WF-CAP-*` diagnostics remain future work.

Gate 61 adds `WastelandForge.Generation` metadata report records and
`MetadataReportGenerator`. It wires `forge generate --target reports` and
`forge build --target reports` through `WastelandForge.Cli` with text and JSON
output, deterministic report files, generation/build manifests, and build
checksums. MCM Extender JSON, JIP scripts, package archives, plugin records,
runtime probes, and external tool execution remain future work.

Gate 62 adds typed MCM menu reads in `WastelandForge.Validation` and
`McmJsonGenerator` in `WastelandForge.Generation`. It wires
`forge generate --target mcm-json` and `forge build --target mcm-json`
through `WastelandForge.Cli` with text and JSON output, deterministic MCM JSON
skeleton files, manifests, and build checksums. Runtime MCM Extender schema
confirmation, JIP scripts, package archives, plugin records, runtime probes,
and external tool execution remain future work.

Gate 63 replaces the Gate 62 MCM skeleton with a minimal upstream-shaped MCM
Extender JSON output model. `WastelandForge.Validation` reads
`minMCMVersion` and slider scale metadata, `WastelandForge.Generation`
validates generated output against `mcm-extender-output/0.1.0`, and the CLI
writes `MCM/<menu>.json` under generated or dist output roots. Advanced MCM
Extender options, translations, callbacks, in-game runtime probes, JIP scripts,
package archives, plugin records, and external tool execution remain future
work.

Gate 64 extends the MCM source read model with runtime `requirements` arrays
and translation maps. `WastelandForge.Generation` passes requirements through
to root and submenu JSON, writes deterministic
`MCM/Translations/<modName>.ini` files when translations are declared, records
translation files in manifests/checksums, and emits `WF-GEN-006` for duplicate
translation output paths. Additional MCM Extender options, callbacks,
capability-to-runtime requirement inference, in-game runtime probes, package
archives, plugin records, and external tool execution remain future work.

Gate 65 extends the MCM source read model with optional string-toggle
`textOn`/`textOff` labels and teaches `McmJsonGenerator` to emit MCM Extender
checkbox option type `5` and string-toggle option type `6`. Additional MCM
Extender options, callbacks, capability-to-runtime requirement
inference, in-game runtime probes, package archives, plugin records, and
external tool execution remain future work.

Gate 66 teaches `McmJsonGenerator` to emit MCM Extender keybind option type
`3` through the existing INI-backed variable path. Multi-slider, color picker,
callbacks, capability-to-runtime requirement inference, in-game runtime probes,
package archives, plugin records, and external tool execution remain future
work.

Gate 67 teaches `McmJsonGenerator` to emit MCM Extender header option type
`0` without variables. Image maps, multi-slider, color picker, callbacks,
capability-to-runtime requirement inference, in-game runtime probes, package
archives, plugin records, and external tool execution remain future work.

Gate 68 teaches `McmJsonGenerator` to emit MCM Extender image option maps on
type `0` options.

Gate 69 teaches `ProjectValidationPipeline` to validate MCM image filenames
against required texture asset targets. Existing asset source existence and
DDS header checks then validate the declared source file. Multi-slider, color
picker, callbacks, capability-to-runtime requirement inference, in-game
runtime probes, package archives, plugin records, and external tool execution
remain future work.

Gate 70 teaches `McmJsonGenerator` to stage referenced MCM texture assets as
loose files under their game-relative target paths and to include those staged
paths in outputs, manifests, output digests, and build checksums. Multi-slider,
color picker, callbacks, capability-to-runtime requirement inference, in-game
runtime probes, ZIP/FOMOD package archives, plugin records, and external tool
execution remain future work.

Gate 71 teaches `McmJsonGenerator` to write `package-manifest.json` beside the
loose-file MCM output tree. The package manifest records menu, translation,
and asset entries plus payload digests, while ZIP/FOMOD archive creation,
actual `forge package` execution, plugin records, and external tool execution
remain future work.
