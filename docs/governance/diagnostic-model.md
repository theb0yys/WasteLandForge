# Diagnostic Model

Status: Skeleton
Research classification: Documented
Source: R004 / ADR-007 and R008 / ADR-011

WastelandForge diagnostic issue JSON is the canonical diagnostic model. Other outputs such as console text, Markdown, SARIF, and GitHub annotations are projections from the same issue data.

## Fields

- `ruleId`: stable WastelandForge rule ID, such as `WF-SEM-014`.
- `severity`: `error`, `warning`, or `note`.
- `category`: diagnostic category, such as `semantic`.
- `title`: short human-readable title.
- `message`: detailed diagnostic message.
- `projectId`: optional stable dotted lowercase project ID.
- `primaryLocation`: required source location.
- `relatedLocations`: optional supporting source locations.
- `suggestedFix`: optional remediation text.
- `docsUri`: optional documentation URI for the rule.
- `fingerprint`: optional stable identity for repeat diagnostics.

## Locations

JSON Pointer is the canonical location format for values inside source contracts. Human renderers may add friendlier displays later, but those are not the source of truth.

Locations can include:

- `file`
- `pointer`
- `line`
- `column`

## Diagnostic Reports

Gate 5 adds a deterministic report wrapper for validation runs:

- `formatVersion`: machine-readable output contract version.
- `tool`: tool metadata for the emitting command, including version once defined.
- `command`: command name that produced the payload.
- `project`: optional project metadata, including stable dotted lowercase project ID when available.
- `summary`: error, warning, and note counts.
- `issues`: sorted diagnostic issue objects.

Reports are the machine-readable output for `forge validate --format json`.

## Gate Ownership

Gate 4 creates the core C# model and deterministic JSON serialization.

Gate 10 adds SARIF 2.1.0 projection from the same canonical issue data.

Gate 11 adds Markdown summaries and GitHub workflow-command annotations from
the same canonical issue data. GitHub annotations include line and column only
when canonical `SourceLocation` includes line and column values; JSON Pointer
remains the canonical value-level location.

Gate 12 adds YAML source location mapping for YAML-backed source contracts.
Manifest schema diagnostics are emitted from runtime JSON Schema evaluation
against the normalized canonical JSON object, while locations still use the
canonical JSON Pointer and optional source line and column.

Gate 13 extends runtime schema diagnostics to dependency and capability
registry documents before semantic cross-registry validation runs.

Gate 14 extends runtime schema diagnostics to optional asset registry documents
when the manifest declares an asset registry root.

Gate 15 adds `WF-ASSET-*` semantic diagnostics for asset source and target path
rules after asset registry schema validation succeeds.

Gate 16 adds type-specific `WF-ASSET-*` semantic diagnostics for minimal source
file signatures and target-root conventions after asset path validation
succeeds.

Gate 17 adds voice and dialogue asset `WF-ASSET-*` semantic diagnostics for
voice/lip target shape and WAV/OGG/LIP pair checks using the asset registry.

Gate 69 adds MCM image asset `WF-ASSET-*` semantic diagnostics for image
filename path shape and required texture asset target resolution using the
asset registry.

Gate 70 adds no new diagnostic rule family or rule ID; it consumes the Gate 69
validation result before staging loose texture files.

Gate 71 adds no new diagnostic rule family or rule ID; it records package
metadata after validation, output validation, and loose-file staging succeed.

Gate 72 adds no new diagnostic rule family or rule ID; it writes a ZIP archive
only after validation, output validation, loose-file staging, and package
metadata generation succeed.

Gate 73 adds no new diagnostic rule family or rule ID; `forge package` reuses
the same validation, output validation, loose-file staging, package metadata,
ZIP archive, and dist-boundary diagnostics as the MCM build path.

Gate 74 adds no new diagnostic rule family. It reserves `WF-BUILD-002` for
generated package manifest schema failures and `WF-BUILD-003` for package ZIP
entry mismatches against the deterministic package payload.

Gate 75 adds no new diagnostic rule family or rule ID. It writes
`install-preview.json` only after the existing validation, package manifest
validation, and optional archive-entry validation path succeeds.

Gate 76 adds no new diagnostic rule family. It reserves `WF-BUILD-004` for
generated install-preview schema failures before local manifest/checksum
evidence is finalized.

Gate 77 adds no new diagnostic rule family or rule ID. It writes
`install-preview.md` only after the generated install-preview JSON validates
and before local manifest/checksum evidence is finalized.

Gate 78 adds no new diagnostic rule family or rule ID. It writes
`package-verification.json` only after package manifest validation,
install-preview validation, and optional archive-entry validation succeed,
and before local manifest/checksum evidence is finalized.

Gate 79 adds no new diagnostic rule family. It reserves `WF-BUILD-005` for
generated package-verification schema failures before local manifest/checksum
evidence is finalized.

Gate 80 adds no new diagnostic rule family or rule ID. It writes
`package-verification.md` only after the generated package-verification JSON
validates and before local manifest/checksum evidence is finalized.

Gate 81 adds no new diagnostic rule family. It reserves `WF-BUILD-006` for
package-verification evidence cross-check failures before local
manifest/checksum evidence is finalized.

Gate 82 adds no new diagnostic rule family or rule ID. It routes the same
`WF-BUILD-006` package-verification evidence mismatches through reusable
validator code.

Gate 83 adds no new diagnostic rule family or rule ID. Its file-based verifier
uses `WF-BUILD-006` for unreadable or invalid package-verification evidence
files and for mismatches reported by the reusable validator.

Gate 84 adds no new diagnostic rule family or rule ID. Its file-based verifier
uses `WF-BUILD-006` when recomputed package payload SHA-256 or length values
do not match `package-manifest.json` payload digest evidence.

Gate 85 adds no new diagnostic rule family or rule ID. Its file-based verifier
uses `WF-BUILD-006` when recomputed package archive SHA-256 or length values
do not match `package-manifest.json` archive digest evidence.

Gate 86 adds no new diagnostic rule family or rule ID. Its file-based verifier
uses `WF-BUILD-006` when generated `package.zip` entry names do not match
`package-manifest.json` entries.

Gate 87 adds no new diagnostic rule family or rule ID. It records the future
`forge package --target mcm-json --verify-existing` command shape for
surfacing existing `WF-BUILD-006` package evidence diagnostics.

Gate 88 adds no new diagnostic rule family or rule ID. It exposes existing
`WF-BUILD-006` package evidence diagnostics through
`forge package --target mcm-json --verify-existing`.

Gate 18 extends runtime schema diagnostics to optional dialogue registry
documents and adds `WF-SEM-015` for dialogue voice worklist entries whose
declared voice/lip assets are missing.

Gate 19 extends runtime schema diagnostics to optional quest registry documents
and adds `WF-SEM-016` for dialogue `questId` values that do not resolve to a
declared quest ID.

Gate 20 adds quest registry schema `0.2.0` for stage and objective skeletons
and adds `WF-SEM-017` for quest objective stage references that do not resolve
inside the declaring quest.

Gate 21 adds quest registry schema `0.3.0` for transition skeletons and adds
`WF-SEM-018` for quest transition stage references that do not resolve inside
the declaring quest.

Gate 22 adds quest registry schema `0.4.0` for condition skeletons and adds
`WF-SEM-019` for quest condition stage references that do not resolve inside
the declaring quest.

Gate 23 adds quest registry schema `0.5.0` for stage result-script skeletons
and adds `WF-SEM-020` for quest result-script condition references that do not
resolve inside the declaring quest.

Gate 24 adds quest registry schema `0.6.0` for quest variable skeletons and
adds `WF-SEM-021` for quest condition variable references that do not resolve
inside the declaring quest.

Gate 25 adds dialogue registry schema `0.2.0` for line-local dialogue
condition skeletons and adds `WF-SEM-022` and `WF-SEM-023` for dialogue
condition quest-stage and quest-variable references that do not resolve inside
the dialogue line's referenced quest.

Gate 26 adds dialogue registry schema `0.3.0` for line-local dialogue
result-script skeletons. It adds no new semantic rule because the Gate 26
result-script fields do not reference other authored state; invalid
result-script shape is reported through `WF-SCHEMA-001`.

Gate 27 adds dialogue registry schema `0.4.0` for topic declarations and
minimal `linkTo` topic link declarations. It adds `WF-SEM-024` for dialogue
line topic references that do not resolve to declared topics, and
`WF-SEM-025` for dialogue link target topic references that do not resolve to
declared topics.

Gate 28 adds dialogue registry schema `0.5.0` for quest-level dialogue gate
declarations. It adds `WF-SEM-026` for dialogue quest gates whose `questId`
does not resolve to a declared quest, `WF-SEM-027` for quest-level dialogue
gate condition stage references that do not resolve inside the gate quest, and
`WF-SEM-028` for quest-level dialogue gate condition variable references that
do not resolve inside the gate quest.

Gate 29 adds dialogue registry schema `0.6.0` for dialogue result-script
quest-variable increment mutation declarations. It adds `WF-SEM-029` for
dialogue result-script mutation variable references that do not resolve inside
the dialogue line's referenced quest.

Gate 30 adds dialogue registry schema `0.7.0` for minimal dialogue `linkFrom`
source topic declarations. It adds `WF-SEM-030` for dialogue `linkFrom` source
topic references that do not resolve to declared dialogue topics.

Gate 31 adds no new schema. It adds derived dialogue link graph endpoint
validation over schema-valid dialogue `0.7.0` documents: `WF-SEM-031` for
`linkTo` target topics with no authored dialogue line, and `WF-SEM-032` for
`linkFrom` source topics with no authored dialogue line.

Gate 32 adds dialogue registry schema `0.8.0` for explicit line priority and
prompt route declarations. It adds `WF-SEM-033` for duplicate prompt routes
with the same `topicId`, `promptText`, and `priority`.

Gate 33 adds dialogue registry schema `0.9.0` for explicit line Speech
Challenge skeleton declarations. It adds no new semantic rule because the Gate
33 Speech Challenge fields do not yet reference other authored state and exact
threshold/evaluation semantics remain open; invalid Speech Challenge shape is
reported through `WF-SCHEMA-001`.

Gate 34 adds dialogue registry schema `0.10.0` for explicit line-local skill
gate skeleton declarations. It adds no new semantic rule because the Gate 34
skill gate fields do not yet reference a skill registry or other authored
state; invalid skill gate shape is reported through `WF-SCHEMA-001`.

Gate 35 adds dialogue registry schema `0.11.0` for explicit line-local perk
gate skeleton declarations. It adds no new semantic rule because the Gate 35
perk gate fields do not yet reference a perk registry or other authored state;
invalid perk gate shape is reported through `WF-SCHEMA-001`.

Gate 36 adds dialogue registry schema `0.12.0` for explicit line-local faction
relation and reputation standing gate skeleton declarations. It adds no new
semantic rule because the Gate 36 faction and reputation gate fields do not
yet reference faction or reputation registries; invalid faction/reputation gate
shape is reported through `WF-SCHEMA-001`.

Gate 37 adds dialogue registry schema `0.13.0` for explicit line-local
identity gate skeleton declarations. It adds no new semantic rule because the
Gate 37 identity gate fields do not yet reference an identity registry or other
authored state; invalid identity gate shape is reported through
`WF-SCHEMA-001`.

Gate 38 adds dialogue registry schema `0.14.0` for explicit line-local local
world flag gate skeleton declarations. It adds no new semantic rule because
the Gate 38 world flag gate fields do not yet reference a world-state registry
or other authored state; invalid local world flag gate shape is reported
through `WF-SCHEMA-001`.

Gate 39 adds dialogue registry schema `0.15.0` for explicit line-local event
history gate skeleton declarations. It adds no new semantic rule because the
Gate 39 event history gate fields do not yet reference an event-history
registry or other authored state; invalid event history gate shape is reported
through `WF-SCHEMA-001`.

Gate 40 adds dialogue registry schema `0.16.0` for explicit line-local
companion state gate skeleton declarations. It adds no new semantic rule
because the Gate 40 companion state gate fields do not yet reference a
companion registry or other authored state; invalid companion state gate shape
is reported through `WF-SCHEMA-001`.

Gate 41 adds dialogue registry schema `0.17.0` for explicit line-local
result-script side-effect gate skeleton declarations. It adds no new semantic
rule because the Gate 41 side-effect gate fields do not yet reference an
effect registry or other authored state; invalid result-script side-effect
gate shape is reported through `WF-SCHEMA-001`.

Gate 42 adds dialogue registry schema `0.18.0` for explicit line-local
condition boolean composition skeleton declarations. It adds no new semantic
rule because Gate 42 only validates the composition shape; condition ID
reference resolution remains a later semantic rule. Invalid condition boolean
composition shape is reported through `WF-SCHEMA-001`.

Gate 43 adds no new schema. It adds `WF-SEM-034` for dialogue condition logic
`conditionIds` entries that do not resolve to conditions authored on the same
dialogue line.

Gate 44 adds dialogue registry schema `0.19.0` for explicit nested dialogue
condition group skeleton declarations. It adds no new `WF-SEM-*` rule; invalid
nested group shape is reported through `WF-SCHEMA-001`, and existing
`WF-SEM-034` condition ID reference validation also applies inside nested
groups.

Gate 45 adds file-based GECK dialogue export load diagnostics for
`forge validate --geck-dialogue-export <path>`. `WF-LOAD-009` reports a
missing export file, `WF-LOAD-010` reports an unreadable or binary-looking
export file, and `WF-LOAD-011` reports an empty export file.

Gate 46 adds dialogue registry schema `0.20.0` for explicit dialogue condition
negation skeleton declarations. It adds no new `WF-SEM-*` rule; invalid
negation shape is reported through `WF-SCHEMA-001`, and existing `WF-SEM-034`
condition ID reference validation also applies to `negatedConditionIds`.

Gate 47 adds dialogue registry schema `0.21.0` for explicit dialogue condition
precedence skeleton declarations. It adds no new `WF-SEM-*` rule; invalid
precedence shape is reported through `WF-SCHEMA-001`.

Gate 48 adds dialogue registry schema `0.22.0` for explicit dialogue condition
short-circuit skeleton declarations. It adds no new `WF-SEM-*` rule; invalid
short-circuit shape is reported through `WF-SCHEMA-001`.

Gate 49 adds no new schema. It adds `WF-SEM-035` for duplicate root or nested
dialogue condition logic IDs inside one line-local condition logic tree.

Gate 50 adds dialogue registry schema `0.23.0` for explicit dialogue response
route skeleton declarations. It adds no new `WF-SEM-*` rule; invalid response
route shape is reported through `WF-SCHEMA-001`.

Gate 51 adds no new schema. It adds `WF-SEM-036` for dialogue response route
`targetTopicId` values that do not resolve to declared dialogue topics.

Gate 52 adds no new schema. It adds `WF-SEM-037` for dialogue response route
target topics that are declared but have no authored dialogue line endpoint.

Gate 53 adds no new schema. It adds `WF-SEM-038` for duplicate dialogue
response route IDs authored on the same dialogue line.

Gate 54 adds no new schema. It adds `WF-SEM-039` for duplicate dialogue
response route keys authored on the same dialogue line.

Gate 55 adds no schema, diagnostic rule, or report field. It records that
response route taxonomy and selection behavior remain evidence-blocked before
Forge adds new diagnostics for route meaning.

Gate 56 adds no schema, diagnostic rule, or report field. It creates a
dialogue response route taxonomy evidence pack skeleton before Forge adds new
diagnostics for route meaning.

Gate 57 adds no schema, diagnostic rule, or diagnostic report field. It adds
catalogue listing output for `forge capabilities list`; local provider
detection and future `WF-CAP-*` diagnostics remain later work.

Gate 58 adds `forge capabilities scan` output as a separate capability scan
report. It adds no `WF-CAP-*` diagnostics and does not change the canonical
diagnostic report shape used by `forge validate` or `forge release verify`.

Gate 59 adds `forge capabilities explain` output as a separate capability
explanation report over catalogue and scan evidence. It adds no `WF-CAP-*`
diagnostics and does not change the canonical diagnostic report shape used by
`forge validate` or `forge release verify`.

Gate 60 extends the separate `forge capabilities scan` report with an optional
project requirement resolution section when `--project` is supplied. It adds no
`WF-CAP-*` diagnostics and does not change the canonical diagnostic report
shape used by `forge validate` or `forge release verify`.

Gate 61 adds separate metadata report output for `forge generate --target
reports` and `forge build --target reports`. The generated validation report
uses the existing canonical diagnostic report aggregate. The command report
adds no canonical diagnostic report fields, but it may include blocking
diagnostics such as `WF-GEN-001` when a generate output path escapes
`generated/` or `WF-BUILD-001` when a build output path escapes `dist/`.

Gate 62 adds separate MCM JSON generator command output for `forge generate
--target mcm-json` and `forge build --target mcm-json`. It does not change
the canonical diagnostic report shape. Blocking generator diagnostics may
include `WF-GEN-002` for missing declared generation capability,
`WF-GEN-003` for no MCM source menus, and `WF-GEN-004` for duplicate MCM
output files.

Gate 63 keeps the same command output shape and adds output-schema validation
before MCM JSON files are written. Blocking generator diagnostics may also
include `WF-GEN-005` for unsupported MCM source settings, missing runtime
metadata needed by the Gate 63 output subset, or output schema validation
failures.

Gate 64 keeps the same command output shape and adds translation-file outputs
when MCM source declares translations. Blocking generator diagnostics may also
include `WF-GEN-006` when translated menus collide on the same
`MCM/Translations/<modName>.ini` output path.

Gate 65 keeps the same command and diagnostic output shape while expanding
`WF-GEN-005` coverage to the larger supported MCM option subset. It adds no
new diagnostic report fields and no new rule family member.

Gate 66 keeps the same command and diagnostic output shape while adding
keybind to the supported MCM option subset. It adds no new diagnostic report
fields and no new rule family member.

Gate 67 keeps the same command and diagnostic output shape while adding header
to the supported MCM option subset. It adds no new diagnostic report fields
and no new rule family member.

Gate 68 keeps the same command and diagnostic output shape while adding image
to the supported MCM option subset. It adds no new diagnostic report fields
and no new rule family member.

Gate 5 creates the diagnostic report aggregate and uses it for loader and
validation pipeline output.
