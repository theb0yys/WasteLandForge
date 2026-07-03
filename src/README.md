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
runtime probes, FOMOD package archives, plugin records, and external tool
execution remain future work.

Gate 71 teaches `McmJsonGenerator` to write `package-manifest.json` beside the
loose-file MCM output tree. The package manifest records menu, translation,
and asset entries plus payload digests. Later gates add ZIP archive creation
and canonical package command execution; FOMOD archive creation, plugin
records, and external tool execution remain future work.

Gate 72 teaches `McmJsonGenerator` to write `dist/mcm-json/package.zip` during
`forge build --target mcm-json`. ZIP entries come from the package payload
list, are sorted, and use normalized timestamps. FOMOD archive creation,
plugin records, and external tool execution remain future work.

Gate 73 implements the canonical `forge package` command skeleton for
`--target mcm-json`. It reuses `McmJsonGenerator` package output under
`dist/mcm-json`, writes `package.zip`, `package-manifest.json`,
`build-manifest.json`, and `checksums.sha256`, and records
`wastelandforge/package-mcm-json/v1` provenance. FOMOD installers, MO2/VFS
installation, runtime probes, in-game verification, plugin records, and
external tool execution remain future work.

Gate 74 teaches `McmJsonGenerator` to validate generated package manifests
against `package-manifest/0.1.0` and to check build/package ZIP entries against
the deterministic package payload before recording package validation evidence
in generation and build manifests. Standalone package verification, FOMOD
installers, MO2/VFS installation, runtime probes, in-game verification, plugin
records, and external tool execution remain future work.

Gate 75 teaches `McmJsonGenerator` to write `install-preview.json` beside MCM
JSON package outputs. The report lists Data-relative package entries, generated
source files, would-copy install paths, archive evidence, and preview-only
limitations, and it is included in manifests, output digests, CLI output, and
build/package checksums. Actual Data/MO2 installation, runtime probes, in-game
verification, plugin records, and external tool execution remain future work.

Gate 76 teaches `McmJsonGenerator` to validate generated install-preview
reports against `install-preview/0.1.0` before writing them and to record that
schema in generation/build manifest install-preview evidence. Actual Data/MO2
installation, runtime probes, in-game verification, plugin records, and
external tool execution remain future work.

Gate 77 teaches `McmJsonGenerator` to write `install-preview.md` beside the
validated JSON report. The Markdown summary is deterministic generated
evidence for human review and is recorded in manifests, output digests, CLI
JSON output, and build/package checksums. Actual Data/MO2 installation,
runtime probes, in-game verification, plugin records, and external tool
execution remain future work.

Gate 78 teaches `McmJsonGenerator` to write `package-verification.json` beside
the package manifest and install-preview evidence. The report summarizes
package entry counts, schema-validated evidence files, payload digest
recording, archive status, and archive entry validation, then records the
report in manifests, output digests, CLI output, and build/package checksums.
Actual Data/MO2 installation, runtime probes, in-game verification, plugin
records, and external tool execution remain future work.

Gate 79 teaches `McmJsonGenerator` to validate generated package-verification
reports against `package-verification/0.1.0` before writing them and to record
that schema in generation/build manifest package-verification evidence. Actual
Data/MO2 installation, runtime probes, in-game verification, plugin records,
and external tool execution remain future work.

Gate 80 teaches `McmJsonGenerator` to write `package-verification.md` beside
the validated JSON report. The deterministic Markdown summary is generated
evidence for human review and is recorded in manifests, output digests, CLI
JSON output, human CLI output, and build/package checksums. Actual Data/MO2
installation, runtime probes, in-game verification, plugin records, and
external tool execution remain future work.

Gate 81 teaches `McmJsonGenerator` to cross-check package-verification
evidence before final manifests and checksums are written. The generated JSON
report and Markdown summary must agree with package manifest, install-preview,
payload digest, and optional archive evidence. Successful runs record
`packageVerification.crossChecks`; mismatches emit `WF-BUILD-006`. Actual
Data/MO2 installation, runtime probes, in-game verification, plugin records,
and external tool execution remain future work.

Gate 82 extracts those package-verification evidence checks into
`McmPackageVerificationEvidenceValidator` and adds
`McmPackageVerificationEvidenceValidationRequest` so the generated-evidence
comparison can be reused outside `McmJsonGenerator`. It does not add a new CLI
command, source schema, installer behavior, MO2 inspection, runtime probe, or
plugin-record output.

Gate 83 adds `McmPackageVerificationEvidenceFileVerifier` and
`McmPackageVerificationEvidenceFileVerificationRequest`. The verifier loads
the generated package manifest, install-preview report, package-verification
JSON, and package-verification Markdown summary from disk, reconstructs
payload and archive digest evidence from the package manifest, and delegates
to the reusable validator. It still adds no CLI command, source schema,
installer behavior, MO2 inspection, runtime probe, or plugin-record output.

Gate 84 teaches `McmPackageVerificationEvidenceFileVerifier` to recompute
package payload SHA-256 and length values from generated files on disk before
delegating to the reusable validator. Payload files that differ from
`package-manifest.json` payload digest evidence emit `WF-BUILD-006`. It still
adds no CLI command, source schema, installer behavior, MO2 inspection,
runtime probe, or plugin-record output.

Gate 85 teaches `McmPackageVerificationEvidenceFileVerifier` to recompute
package archive SHA-256 and length values for generated `package.zip` files
before delegating to the reusable validator. Archive files that differ from
`package-manifest.json` archive digest evidence emit `WF-BUILD-006`. It still
adds no CLI command, source schema, installer behavior, MO2 inspection,
runtime probe, or plugin-record output.

Gate 86 teaches `McmPackageVerificationEvidenceFileVerifier` to re-open
generated `package.zip` files and compare normalized file entry names against
`package-manifest.json` entries. Missing or undeclared archive entries emit
`WF-BUILD-006`. It still adds no CLI command, source schema, installer
behavior, MO2 inspection, runtime probe, or plugin-record output.

Gate 87 records that the future public entrypoint for that verifier should be
`forge package --target mcm-json --verify-existing`. It changes no product
code, adds no CLI flag yet, and still adds no source schema, installer
behavior, MO2 inspection, runtime probe, or plugin-record output.

Gate 88 implements that entrypoint under `forge package`. The CLI now parses
`--verify-existing`, resolves existing package evidence under project `dist/`,
runs `McmPackageVerificationEvidenceFileVerifier`, and renders human/plain/json
verification output. It still adds no source schema, installer behavior, MO2
inspection, runtime probe, or plugin-record output.

Gate 89 adds SARIF and GitHub diagnostic output to that verify-existing package
evidence path. The CLI now renders the verifier `DiagnosticReport` through the
existing SARIF 2.1.0 and GitHub workflow-command annotation projections while
normal package generation remains limited to human/plain/json output.

Gate 90 adds Markdown summary file output to that verify-existing package
evidence path. The CLI now accepts `--summary <path>` with
`--verify-existing` and writes the existing Markdown diagnostic projection
without regenerating package outputs.

Gate 91 adds checksum-file revalidation to that verify-existing package
evidence path. The file-based verifier now reads `checksums.sha256`,
recomputes SHA-256 values for listed package files, requires expected package
evidence entries, and reports blocking `WF-BUILD-006` diagnostics for checksum
evidence mismatches without regenerating package outputs.

Gate 92 adds build-manifest content revalidation to that verify-existing
package evidence path. The file-based verifier now reads `build-manifest.json`,
checks package evidence fields against already loaded package files, and
recomputes build-manifest output digests without regenerating package outputs.

Gate 93 adds install-preview summary content revalidation to that
verify-existing package evidence path. The file-based verifier now reads
`install-preview.md` and checks required human-summary lines against the
schema-validated `install-preview.json` without regenerating package outputs.

Gate 94 adds install-preview/package-manifest entry content cross-checking to
that verify-existing package evidence path. The file-based verifier now
compares `package-manifest.json` entries with `install-preview.json` entries
by kind, ID, and package path, then checks mapped source files, install paths,
media types, actions, declared asset sources, and target files without
regenerating package outputs.

Gate 95 adds package-verification summary content revalidation to that
verify-existing package evidence path. The reusable package-verification
evidence validator now checks `package-verification.md` header, provenance,
project, command, target, package counts, archive state, check lines, and
limitations against `package-verification.json` and computed package evidence
without regenerating package outputs.

Gate 96 adds package-verification JSON check content revalidation to that
verify-existing package evidence path. The reusable package-verification
evidence validator now checks generated `package-verification.json` check
statuses, evidence paths, payload digest status/count, and package archive
status/validation against package evidence without regenerating package
outputs.

Gate 97 adds package-verification JSON metadata content revalidation to that
verify-existing package evidence path. The reusable package-verification
evidence validator now checks generated `package-verification.json` format,
kind, verification type, command, target, dry-run flag, project ID, package
type, layout, package counts, result, and limitations against expected
generated evidence, `package-manifest.json`, and `install-preview.json`
without regenerating package outputs.

Gate 98 adds package-verification archive detail content revalidation to that
verify-existing package evidence path. The reusable package-verification
evidence validator now checks generated `package-verification.json` archive
reason text, media type, compression, SHA-256, and length against generated
package evidence without regenerating package outputs.

Gate 99 adds install-preview archive detail content revalidation to that
verify-existing package evidence path. The reusable package-verification
evidence validator now checks generated `install-preview.json` archive reason
text, media type, compression, SHA-256, and length against generated package
evidence without regenerating package outputs.

Gate 100 adds package-manifest archive detail content revalidation to that
verify-existing package evidence path. The reusable package-verification
evidence validator now checks generated `package-manifest.json` archive reason
text, media type, and compression against generated package evidence without
regenerating package outputs. Archive SHA-256 and length remain covered by the
file-based archive digest recomputation path.

Gate 101 adds archive detail cross-report consistency revalidation to that
verify-existing package evidence path. When `package-manifest.json` archive
SHA-256 or length is already stale against the actual archive, the reusable
validator now compares the archive SHA-256 and length fields across
`package-manifest.json`, `install-preview.json`, and
`package-verification.json` without regenerating package outputs.

Gate 102 adds package archive presence revalidation to that verify-existing
package evidence path. When `package.zip` exists beside `package-manifest.json`
but package evidence records archive status `not-created`, the file-based
verifier now emits a blocking `WF-BUILD-006` diagnostic without regenerating
package outputs.

Gate 103 adds checksum unexpected-entry revalidation to that verify-existing
package evidence path. The file-based verifier now reports a blocking
`WF-BUILD-006` diagnostic when `checksums.sha256` records a valid
package-root-relative entry that package evidence does not expect, without
regenerating package outputs.

Gate 104 adds checksum duplicate-entry revalidation to that verify-existing
package evidence path. The file-based verifier now reports a blocking
`WF-BUILD-006` diagnostic when `checksums.sha256` records the same normalized
package-root-relative entry more than once, without regenerating package
outputs.

Gate 105 adds checksum canonical-order revalidation to that verify-existing
package evidence path. The file-based verifier now reports a blocking
`WF-BUILD-006` diagnostic when expected package evidence entries in
`checksums.sha256` are not sorted by normalized package-root-relative path,
without regenerating package outputs.

Gate 106 adds checksum digest canonical-casing revalidation to that
verify-existing package evidence path. The file-based verifier now reports a
blocking `WF-BUILD-006` diagnostic when expected package evidence entries in
`checksums.sha256` use uppercase SHA-256 hex, without regenerating package
outputs.

Gate 107 adds checksum line-ending and trailing-newline revalidation to that
verify-existing package evidence path. The file-based verifier now reports
blocking `WF-BUILD-006` diagnostics when `checksums.sha256` is missing its
final newline or uses non-canonical line endings for the current
Forge-generated checksum format, without regenerating package outputs.

Gate 108 adds checksum path separator canonicalization revalidation to that
verify-existing package evidence path. The file-based verifier now reports a
blocking `WF-BUILD-006` diagnostic when an expected checksum entry uses
backslash separators instead of Forge-generated `/` package-root-relative
paths, without regenerating package outputs.

Gate 109 adds checksum blank-line revalidation to that verify-existing package
evidence path. The file-based verifier now reports a blocking `WF-BUILD-006`
diagnostic when `checksums.sha256` contains blank or whitespace-only rows,
without regenerating package outputs.

Gate 110 adds checksum entry spacing canonicalization revalidation to that
verify-existing package evidence path. The file-based verifier now reports a
blocking `WF-BUILD-006` diagnostic when an expected checksum entry does not use
exactly two spaces between digest and path, or has leading/trailing path
whitespace, without regenerating package outputs.

Gate 111 adds checksum path casing canonicalization revalidation to that
verify-existing package evidence path. The file-based verifier now reports a
blocking `WF-BUILD-006` diagnostic when an expected checksum entry matches
package evidence only case-insensitively, while suppressing missing/unexpected
checksum entry cascades for that same path.

Gate 112 adds checksum case-insensitive duplicate revalidation to that
verify-existing package evidence path. The file-based verifier now reports a
single blocking `WF-BUILD-006` duplicate-entry diagnostic when two checksum
entries record the same normalized package-root-relative path ignoring case,
while suppressing path-casing, missing, and unexpected checksum entry cascades
for the later duplicate row.

Gate 113 adds checksum malformed-entry format revalidation to that
verify-existing package evidence path. The file-based verifier now reports a
single blocking `WF-BUILD-006` malformed-entry diagnostic when an expected
checksum row has a malformed SHA-256 digest or entry shape but still names a
recognizable package-root-relative path, while suppressing the missing-entry
cascade for that same path.

Gate 114 adds checksum path containment revalidation to that verify-existing
package evidence path. The file-based verifier now reports a single blocking
`WF-BUILD-006` path-containment diagnostic when a checksum row escapes the
package root, while suppressing the missing-entry cascade for the same
recognizable expected path.

Gate 115 adds checksum comment-line rejection revalidation to that
verify-existing package evidence path. The file-based verifier now reports a
single blocking `WF-BUILD-006` comment-line diagnostic when `checksums.sha256`
contains a `#` comment line, without also reporting the same row as a generic
malformed checksum entry.

Gate 116 adds schema-validated `install-plan.json` and generated
`install-plan.md` evidence to the MCM Extender generate/build/package path.
`McmJsonGenerator` records the install plan in local manifests, output
digests, distribution checksums, and CLI output while keeping Data/MO2 writes
outside this gate.

Gate 117 extends `McmPackageVerificationEvidenceFileVerifier` so
`--verify-existing` revalidates install-plan JSON and Markdown content against
package-manifest evidence. It checks metadata, archive details, entries,
required copy actions, manual-approval/non-mutation flags, and summary lines
without regenerating outputs.

Gate 118 extends `McmPackageVerificationEvidenceFileVerifier` so
`--verify-existing` first revalidates existing `install-plan.json` against the
embedded `install-plan/0.1.0` schema. Invalid install-plan documents produce
one `WF-BUILD-006` schema diagnostic and skip deeper install-plan content
checks to avoid cascading diagnostics.

Gate 119 extends `McmPackageVerificationEvidenceFileVerifier` so
`--verify-existing` first revalidates existing `package-manifest.json` against
the embedded `package-manifest/0.1.0` schema. Invalid package manifests
produce one `WF-BUILD-006` schema diagnostic and skip dependent package
evidence checks to avoid cascading diagnostics from an invalid manifest shape.

Gate 120 extends `McmPackageVerificationEvidenceFileVerifier` so
`--verify-existing` first revalidates existing `install-preview.json` against
the embedded `install-preview/0.1.0` schema. Invalid install previews produce
one `WF-BUILD-006` schema diagnostic and skip dependent package evidence
checks to avoid cascading diagnostics from an invalid install-preview shape.

Gate 121 extends `McmPackageVerificationEvidenceFileVerifier` so
`--verify-existing` first revalidates existing `package-verification.json`
against the embedded `package-verification/0.1.0` schema. Invalid package
verification reports produce one `WF-BUILD-006` schema diagnostic and skip
dependent package evidence checks to avoid cascading diagnostics from an
invalid package-verification shape.

Gate 122 keeps the verifier behavior unchanged and adds golden CLI coverage
for schema-gated verify-existing diagnostics through SARIF, GitHub annotation,
and Markdown summary projections.

Gate 123 extends `McmPackageVerificationEvidenceFileVerifier` so missing
required JSON or Markdown evidence files are reported as explicit
`WF-BUILD-006` missing-evidence diagnostics instead of generic read failures.

Gate 124 extends `McmPackageVerificationEvidenceFileVerifier` so malformed
required JSON evidence files are reported as explicit `WF-BUILD-006`
malformed-JSON diagnostics, while valid non-object JSON evidence keeps a
separate top-level-shape diagnostic.

Gate 125 keeps verifier behavior unchanged and adds golden CLI coverage for
malformed package-verification JSON diagnostics through SARIF, GitHub
annotation, and Markdown summary projections.

Gate 126 closes the current MCM Extender implementation lane without changing
verifier behavior. Next source work should return to broader Forge value in
capability scanner and Doctor-style environment reporting code.

Gate 127 adds `CapabilityDoctorPlanner` in `WastelandForge.Registry`, adds a
derived Doctor readiness report to `CapabilityScanReport`, and surfaces
target-level actions in `CapabilityExplanationTarget`. The CLI renders the
Doctor report in JSON and text without adding runtime probes or new commands.

Gate 128 adds CLI-side `DoctorExportReport`, redaction, JSON serialization,
and text rendering for the canonical `forge doctor export` command. It reuses
the capability scan report and project requirement resolver, then redacts
local paths before output.

Gate 129 adds `CapabilityDiagnosticProjector` in `WastelandForge.Registry` and
wires `forge capabilities scan` JSON, SARIF, GitHub, and text output through
the canonical diagnostic model for unavailable project capability
requirements.

Gate 130 adds optional evidence to `DiagnosticIssue`, carries scan-derived
provider evidence through `CapabilityRequirementResolution`, serializes that
evidence in capability scan JSON, SARIF, GitHub annotations, and text output,
and redacts nested requirement evidence paths in `forge doctor export`.

Gate 131 adds `CapabilityExplanationEvidenceGroup` and wires grouped provider
evidence into `forge capabilities explain` JSON and text output, reusing
existing scan evidence and Doctor planner actions.

Gate 132 adds `wrong-scope` scan and requirement statuses for deterministic
root-vs-Data marker evidence, projects wrong-scope project requirements as
`WF-CAP-004`, and exposes wrong-scope counts in capability scan JSON and text
output.

Gate 133 adds optional `CapabilityExplanationProjectRequirements` to
capability explanation reports and wires `forge capabilities explain
--project` through existing project requirement loading and resolution.

Gate 134 extends `CapabilityExplanationProjectRequirements` with diagnostic
handoff issues projected by `CapabilityDiagnosticProjector`, so
`forge capabilities explain --project` can show the scan `WF-CAP-*` rule that
would apply to each unavailable matching requirement.

Gate 135 extends `DoctorExportReport` with top-level summary and index records
derived from the redacted capability scan report. `forge doctor export` JSON
now includes `summary` and `index`, and text output prints the same scanable
sections before the nested capability scan report.

Gate 136 extends the Doctor export index with compact diagnostics entries
derived from the redacted capability diagnostic report. `forge doctor export`
JSON now includes `index.diagnostics`, and text output prints matching
diagnostic index lines before the nested capability scan report.

Gate 137 extends the Doctor export index with compact unavailable project
requirement entries derived from the redacted project requirement resolution
report. `forge doctor export` JSON now includes `index.requirements`, and text
output prints matching requirement index lines before the diagnostics index and
nested capability scan report.

Gate 138 extends the Doctor export index with compact action groups derived
from redacted Doctor area actions. `forge doctor export` JSON now includes
`index.actions`, and text output prints matching action groups before
requirements, diagnostics, open questions, and the nested capability scan
report.

Gate 139 extends the Doctor export index with structured open-question detail
entries derived from existing Doctor open-question text. `forge doctor export`
JSON now includes `index.openQuestionDetails`, while preserving the existing
`index.openQuestions` string list.

Gate 140 extends the Doctor export index with compact provider-status entries
derived from existing redacted provider scan results. `forge doctor export`
JSON now includes `index.providerStatuses`, and text output prints matching
provider-status groups under `Doctor index`.

Gate 141 extends the Doctor export index with compact capability-status
entries derived from existing redacted capability scan results. `forge doctor
export` JSON now includes `index.capabilityStatuses`, and text output prints
matching capability-status groups under `Doctor index`.

Gate 142 extends the Doctor export index with compact Doctor area-status
entries derived from existing redacted Doctor area results. `forge doctor
export` JSON now includes `index.doctorAreaStatuses`, and text output prints
matching Doctor area-status groups under `Doctor index`.

Gate 143 extends the Doctor export index with compact catalogue-policy
entries derived from existing structured open-question details. `forge doctor
export` JSON now includes `index.cataloguePolicy`, and text output prints
matching catalogue-policy groups under `Doctor index`.

Gate 144 extends capability scan Doctor output with compact readiness index
entries derived from existing Doctor area results. `forge capabilities scan`
JSON now includes `doctor.index.areaStatuses`, and text output prints a
matching `Doctor readiness index`.

Gate 145 extends capability scan output with compact provider/capability
status index entries derived from existing scan results. `forge capabilities
scan` JSON now includes top-level `index.providerStatuses` and
`index.capabilityStatuses`, and text output prints a matching `Scan status
index`.

Gate 146 extends capability scan output with compact action index entries
derived from existing non-ready Doctor area actions. `forge capabilities scan`
JSON now includes top-level `index.actions`, and text output prints matching
action groups before the full Doctor area list.

Gate 147 extends capability scan output with compact requirement index entries
derived from existing project requirement resolution output. `forge
capabilities scan` JSON now includes top-level `index.requirements`, and text
output prints matching unavailable requirement entries before the full project
requirement report.

Gate 148 extends capability scan output with compact diagnostic index entries
derived from existing diagnostic projection output. `forge capabilities scan`
JSON now includes top-level `index.diagnostics`, and text output prints
matching diagnostic entries before the full diagnostics report.

Gate 149 extends capability scan output with compact catalogue-policy index
entries derived from existing Doctor open-question output. `forge capabilities
scan` JSON now includes top-level `index.cataloguePolicy`, and text output
prints matching catalogue-policy groups before the full Doctor open-question
text.

Gate 150 extends capability scan output with compact open-question detail
entries derived from existing Doctor open-question output. `forge capabilities
scan` JSON now includes top-level `index.openQuestionDetails`, and text output
prints matching detail entries before the full Doctor open-question text.

Gate 151 extends capability explain output with catalogue-policy
open-question detail entries derived from existing Doctor open-question
output. `forge capabilities explain` JSON now includes
`cataloguePolicy.openQuestionDetails`, and text output prints matching detail
entries before provider evidence groups.

Gate 152 extends capability explain output with catalogue-policy diagnostic
handoff entries derived from existing Doctor open-question output.
`forge capabilities explain` JSON now includes
`cataloguePolicy.diagnosticHandoff`, and text output prints matching handoff
entries before provider evidence groups.

Gate 153 extends Doctor export output with catalogue-policy diagnostic
handoff entries derived from existing Doctor open-question output.
`forge doctor export` JSON now includes
`index.cataloguePolicyDiagnosticHandoff`, and text output prints matching
handoff entries under `Doctor index`.

Gate 154 extends capability scan output with catalogue-policy diagnostic
handoff entries derived from existing Doctor open-question output.
`forge capabilities scan` JSON now includes
`index.cataloguePolicyDiagnosticHandoff`, and text output prints matching
handoff entries under `Scan status index`.

Gate 155 extracts shared catalogue-policy diagnostic handoff rendering helpers
for `forge capabilities explain`, `forge capabilities scan`, and
`forge doctor export`. The JSON shapes and text sections stay unchanged.

Gate 156 extracts shared catalogue-policy open-question detail and source-type
index rendering helpers for `forge capabilities explain`,
`forge capabilities scan`, and `forge doctor export`. The JSON shapes and text
sections stay unchanged.

Gate 157 extracts a shared catalogue-policy view model for the same command
family. The view carries raw open questions, structured details, source-type
indexes, and diagnostic handoff entries while preserving the JSON shapes and
text sections.

Gate 158 extracts a shared Doctor action summary helper for
`forge capabilities scan` and `forge doctor export`. JSON output now includes
`index.actionSummary`, and text output prints an `Action summary:` section
that summarizes existing non-ready Doctor area actions by source type and
area status.

Gate 159 extracts a shared provider evidence summary helper for
`forge capabilities scan` and `forge doctor export`. JSON output now includes
`index.evidenceSummary`, and text output prints an `Evidence summary:` section
that summarizes existing provider detector evidence by detector kind, status,
and scope.

Gate 160 extracts a shared requirement summary helper for
`forge capabilities scan` and `forge doctor export`. JSON output now includes
`index.requirementSummary`, and text output prints a `Requirement summary:`
section that summarizes existing project requirement resolution data by
status, phase, and optionality.

Gate 161 extracts a shared diagnostic summary helper for
`forge capabilities scan` and `forge doctor export`. JSON output now includes
`index.diagnosticSummary`, and text output prints a `Diagnostic summary:`
section that summarizes already-projected diagnostics by severity, rule ID,
and category.

Gate 162 extracts a shared provider inventory summary helper for
`forge capabilities scan` and `forge doctor export`. JSON output now includes
`index.providerInventorySummary`, and text output prints a
`Provider inventory summary:` section that summarizes existing providers by
provider type, install scope, and provider status.

Gate 163 extracts a shared Doctor area capability summary helper for
`forge capabilities scan` and `forge doctor export`. JSON output now includes
`index.doctorAreaCapabilitySummary`, and text output prints a
`Doctor area capability summary:` section that summarizes existing Doctor
areas by capability status, provider status/install scope, and actionable
action count.

Gate 164 adds a Doctor export Markdown summary renderer. `forge doctor export
--summary <path>` now writes a redacted Markdown sidecar derived from the
existing Doctor export report while preserving the primary human/plain/json
output behavior.

Gate 165 adds a capability scan Markdown summary renderer. `forge capabilities
scan --summary <path>` now writes a path-minimized Markdown sidecar derived
from the existing capability scan report and projected diagnostics while
preserving the selected primary scan output behavior.

Gate 166 adds a Doctor export archive writer. `forge doctor export --bundle
<path>` now writes a deterministic redacted ZIP sidecar containing
`doctor-export.json`, `doctor-export.md`, `doctor-bundle-manifest.json`, and
`checksums.sha256` while preserving the selected primary Doctor export output
behavior.

Gate 167 adds a capability explain Markdown summary renderer. `forge
capabilities explain --summary <path>` now writes a path-minimized Markdown
sidecar derived from the existing capability/provider explanation report while
preserving the selected primary explanation output behavior.

Gate 168 extends the Doctor export archive writer. `forge doctor export
--bundle <path>` now adds deterministic
`requirement-explanations/<capability-id>.md` entries when project
requirements are included and unavailable, reusing the existing capability
explanation Markdown renderer while preserving the base Doctor archive output
behavior.

Gate 169 extends the same Doctor export archive supplements with deterministic
`requirement-explanations/<capability-id>.json` entries. Those entries reuse
the existing capability explanation JSON serializer after path redaction, and
are listed in the bundle manifest and checksums.

Gate 170 adds a Doctor export requirement explanation index renderer. Doctor
bundle archives with unavailable project requirements now include
`requirement-explanations/index.json` and
`requirement-explanations/index.md`, which point to the per-requirement JSON
and Markdown explanations while preserving deterministic archive output.

Gate 171 adds a Doctor export archive README renderer. `forge doctor export
--bundle <path>` archives now include `README.md`, which points to the
redacted Doctor reports, optional requirement explanation index, manifest, and
checksums while preserving deterministic archive output and local path
redaction.

Gate 172 adds a Doctor export diagnostic index renderer. `forge doctor export
--bundle <path>` archives now include `diagnostics/index.json` and
`diagnostics/index.md`, derived from the already redacted Doctor diagnostic
summary and compact diagnostic entries while preserving deterministic archive
output and local path redaction.

Gate 173 adds a Doctor export action index renderer. `forge doctor export
--bundle <path>` archives now include `actions/index.json` and
`actions/index.md`, derived from the already redacted Doctor action summary
and compact action entries while preserving deterministic archive output and
local path redaction.

Gate 174 adds a Doctor export requirement index renderer. `forge doctor
export --bundle <path>` archives now include `requirements/index.json` and
`requirements/index.md`, derived from the already redacted Doctor requirement
summary and compact unavailable requirement entries while preserving
deterministic archive output and local path redaction.

Gate 175 adds a Doctor export provider index renderer. `forge doctor export
--bundle <path>` archives now include `providers/index.json` and
`providers/index.md`, derived from the already redacted Doctor provider
summary, provider-status groups, provider inventory summary, evidence summary,
and compact provider scan entries while preserving deterministic archive
output and local path redaction.

Gate 176 adds a Doctor export capability index renderer. `forge doctor export
--bundle <path>` archives now include `capabilities/index.json` and
`capabilities/index.md`, derived from the already redacted Doctor capability
summary, capability-status groups, Doctor area capability summary, and compact
capability scan entries while preserving deterministic archive output and
local path redaction.

Gate 177 adds a Doctor export Doctor area index renderer. `forge doctor export
--bundle <path>` archives now include `doctor-areas/index.json` and
`doctor-areas/index.md`, derived from the already redacted Doctor readiness
summary, area-status groups, Doctor area capability summary, and compact
Doctor area entries while preserving deterministic archive output and local
path redaction.

Gate 178 adds a Doctor export catalogue-policy index renderer. `forge doctor
export --bundle <path>` archives now include
`catalogue-policy/index.json` and `catalogue-policy/index.md`, derived from
the already redacted source-type groups, open-question details, diagnostic
handoff entries, and open-question text while preserving deterministic archive
output and local path redaction.

Gate 179 adds a Doctor export summary index renderer. `forge doctor export
--bundle <path>` archives now include `summary/index.json` and
`summary/index.md`, derived from the already redacted report summary and
already-derived action, requirement, diagnostic, provider inventory, evidence,
Doctor area capability, and catalogue-policy summary metadata while preserving
deterministic archive output and local path redaction.

Gate 180 adds a Doctor export evidence index renderer. `forge doctor export
--bundle <path>` archives now include `evidence/index.json` and
`evidence/index.md`, derived from the already redacted evidence summary and
compact provider detector evidence entries while preserving deterministic
archive output and local path redaction.
Gate 181 adds a Doctor export redaction index renderer. `forge doctor export
--bundle <path>` archives now include `redaction/index.json` and
`redaction/index.md`, derived from the existing Doctor export redaction mode,
path policy, placeholder tokens, and redaction notes while preserving
deterministic archive output and local path redaction.
Gate 182 adds a Doctor export open-question index renderer. `forge doctor
export --bundle <path>` archives now include `open-questions/index.json` and
`open-questions/index.md`, derived from the existing Doctor export
open-question metadata while preserving deterministic archive output and local
path redaction.
Gate 183 adds a Doctor export scan-input index renderer. `forge doctor export
--bundle <path>` archives now include `scan-inputs/index.json` and
`scan-inputs/index.md`, derived from the existing redacted capability scan
input metadata while preserving deterministic archive output and local path
redaction.
Gate 184 adds a Doctor export bundle navigation index renderer. `forge doctor
export --bundle <path>` archives now include `bundle/index.json` and
`bundle/index.md`, derived from existing archive supplement paths and
redacted bundle metadata while preserving deterministic archive output and
local path redaction.
Gate 185 adds a Doctor export triage index renderer. `forge doctor export
--bundle <path>` archives now include `triage/index.json` and
`triage/index.md`, derived from existing redacted summary, diagnostic,
requirement, action, wrong-scope, and open-question metadata while preserving
deterministic archive output and local path redaction.
Gate 186 adds primary Doctor export triage projection. `forge doctor export`
JSON now includes top-level `triage`, plain output includes `Triage:`, and
Markdown summaries include `## Triage`, all derived from the same redacted
metadata while keeping primary references section-based and archive references
path-based.
Gate 187 adds Doctor triage command hints. `forge doctor export` primary JSON,
plain output, Markdown summaries, and bundle triage entries now list canonical
next-command hints derived from existing redacted metadata, using placeholders
instead of local paths and without adding aliases or provider detection logic.
Gate 188 adds a Doctor triage remediation worklist. `forge doctor export`
primary JSON, plain output, Markdown summaries, and bundle triage entries now
list ordered work items derived from existing redacted metadata, linked to
command-hint IDs and report sections or archive paths.
Gate 189 adds Doctor worklist summary metadata. `forge doctor export` primary
JSON, plain output, Markdown summaries, and bundle triage entries now include
priority and source group summaries derived from the existing worklist.
Gate 190 adds a compact Doctor remediation status header. `forge doctor
export` primary JSON, plain output, Markdown summaries, and bundle triage
entries now include status, headline, work item counts, first work item, first
command hint, and first canonical command derived from existing worklist data.
Gate 191 adds a human Doctor operator handoff checklist. `forge doctor export`
plain output, Markdown summaries, and bundle triage Markdown now include
copyable checklist items derived from remediation, worklist summary, and
command-hint data without changing the JSON contract.

Gate 192 adds a Doctor bundle handoff summary sidecar. `forge doctor export
--bundle <path>` now includes `handoff-summary.md`, a redacted Markdown
summary derived from existing triage metadata that points at remediation,
immediate worklist items, command hints, and key archive paths. The sidecar is
linked from `README.md`, listed in `bundle/index.*`, and included in the
archive manifest and checksums.

Gate 193 adds a scan-side operator handoff projection. `forge capabilities
scan` plain output and `--summary <path>` Markdown sidecars now include a
checklist derived from existing requirement resolution, projected diagnostics,
Doctor actions, wrong-scope counts, and catalogue-policy open questions. Scan
JSON output is unchanged.

Gate 194 adds an explain-side operator handoff projection. `forge
capabilities explain` plain output and `--summary <path>` Markdown sidecars
now include a checklist derived from existing target actions, provider
evidence groups, matching project requirements, diagnostic handoff, and
catalogue-policy handoff entries. Explain JSON output and capability
resolution are unchanged.

Gate 195 extends generated Doctor bundle documentation around existing
requirement explanation supplements. `requirement-explanations/index.md` and
bundle `README.md` now state that per-requirement Markdown explanations
include operator handoff checklists with placeholder commands, while paired
JSON entries keep the existing `capabilities explain` contract.

Gate 196 adds a declaration-only `ProviderVersionDeclaration` metadata object
to provider definitions. The built-in catalogue populates provider-version
scheme, source, status, local-version status, resolution status, and notes;
CLI list/explain serializers render that metadata without parsing local
provider versions or changing requirement resolution.

Gate 197 moves the same declaration-only provider-version projection into
`forge capabilities scan` provider JSON/plain/Markdown output and Doctor
provider index JSON/Markdown output. The scan Markdown summary now includes a
path-minimized provider table. Requirement resolution, provider detection,
runtime probing, unsupported-version diagnostics, and Doctor planning remain
unchanged.

Gate 198 records the provider-version parser research checkpoint. It keeps
parser code out of the implementation and decides that the next safe slice is
a pure parser contract for synthetic raw `semver`, `integer`, and
`scaled-integer` values. Runtime probes, DLL/EXE file metadata inspection,
resolver behavior, unsupported-version diagnostics, and Doctor planning remain
unchanged.

Gate 199 adds `ProviderVersionParser` and `ProviderVersionParseResult` to
`WastelandForge.Registry`. The parser handles synthetic `semver`, `integer`,
and `scaled-integer` raw values, normalizes successful parses, and preserves
raw values on failed parses. No scan detector, Doctor export, CLI renderer,
capability resolver, runtime probe, DLL/EXE metadata reader, or diagnostic
rule consumes parser results yet.

Gate 200 adds provider-version parser contract documentation and future
scan-evidence projection notes under `docs/capabilities/`. It does not change
source code, parser behavior, detector behavior, CLI renderers, Doctor export,
capability resolution, runtime probes, DLL/EXE metadata readers, or
diagnostic rules.

Gate 201 adds `ProviderVersionParsedEvidence` and
`ProviderVersionEvidenceSourceKinds` to `WastelandForge.Registry`. The model
maps a `ProviderVersionParseResult` into standalone future scan evidence while
preserving provider ID, source kind, raw value, parsed status, normalized
value, numeric components, failure reason, and provenance. No scanner,
resolver, Doctor planner, CLI renderer, runtime probe, DLL/EXE metadata
reader, or diagnostic rule consumes it yet.

Gate 202 adds no source code. It records the JIP LN text-script generator
evidence checkpoint under `docs/generation/` and leaves source contracts,
schemas, generator code, CLI target wiring, build/package staging, runtime
probes, GECK automation, MO2 VFS inspection, and external tool execution for
later gates.

Gate 203 adds schema catalog wiring and validation pipeline loading for the
`jip-script` source registry kind. It adds `ProjectJipScriptReadResult` and
`JipScriptDefinition` as typed read models for schema-valid source contracts.
No generator, CLI target, package staging, runtime probe, GECK automation,
MO2 VFS inspection, live Data mutation, or external tool execution consumes
the model yet.

Gate 204 adds semantic validation over schema-valid JIP source contracts:
`WF-SEM-040` for lifecycle/output prefix mismatch and `WF-SEM-041` for a
missing `runtime.scripting.jip_script_runner` script requirement. No
generator, CLI target, package staging, runtime probe, GECK automation, MO2
VFS inspection, live Data mutation, or external tool execution consumes these
contracts yet.

Gate 205 extends `JipScriptDefinition` with opaque source lines and source
locations. `ProjectValidationPipeline.ReadJipScripts` reads
`body.lines[].text` from schema-valid JIP source registries. No generator, CLI
target, package staging, runtime probe, GECK automation, MO2 VFS inspection,
live Data mutation, or external tool execution consumes these source lines
yet.

Gate 206 adds `WF-SEM-042` in `ProjectValidationPipeline` for JIP source
bodies whose opaque line text exceeds `sizePolicy.maxBytes`. The budget uses
UTF-8 bytes for stored source-line text with one LF byte between lines, and is
not final emitted-file byte accounting.
