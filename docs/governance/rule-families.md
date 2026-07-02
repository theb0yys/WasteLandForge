# Rule Families

Status: Skeleton
Research classification: Documented
Source: R008 / ADR-011

WastelandForge diagnostic rule IDs use reserved families:

| Family | Scope |
|---|---|
| `WF-LOAD-*` | File discovery, parsing, encoding, duplicate file IDs |
| `WF-SCHEMA-*` | Schema and contract shape |
| `WF-SEM-*` | Semantic and cross-registry rules |
| `WF-CAP-*` | Capability/provider rules |
| `WF-ASSET-*` | Asset and path rules |
| `WF-GEN-*` | Generator rules |
| `WF-BUILD-*` | Build graph and cache rules |
| `WF-REL-*` | Release rules |
| `WF-GOV-*` | Governance rules |
| `WF-SEC-*` | Security and policy rules |

Gate 4 defines the issue model. Gate 5 assigns the first concrete load,
schema, and semantic rule IDs for the loader and validation pipeline.
Gate 12 adds YAML-specific load diagnostics, including unsupported YAML
features and duplicate YAML mapping keys, while preserving the existing
`WF-LOAD-*` family.
Gate 13 applies `WF-SCHEMA-*` diagnostics to dependency and capability registry
contract shape through runtime JSON Schema validation.
Gate 14 applies `WF-SCHEMA-*` diagnostics to asset registry contract shape.
Path existence, file-type, and packaging checks remain future `WF-ASSET-*`
semantic diagnostics.
Gate 15 adds the first concrete `WF-ASSET-*` diagnostics:
`WF-ASSET-001` for asset source escape, `WF-ASSET-002` for missing required
source files, `WF-ASSET-003` for non-game-relative target paths, and
`WF-ASSET-004` for target extension mismatch.
Gate 16 adds `WF-ASSET-005` for source signature mismatch and `WF-ASSET-006`
for target root mismatch.
Gate 17 adds `WF-ASSET-007` for invalid voice target shape, `WF-ASSET-008`
for incomplete WAV/OGG voice pairs, and `WF-ASSET-009` for missing LIP pairs.
Gate 69 adds `WF-ASSET-010` for invalid MCM image filename paths and
`WF-ASSET-011` for MCM image filenames that do not resolve to required
texture asset targets.
Gate 18 adds `WF-SEM-015` for dialogue voice worklist entries missing declared
voice/lip assets.
Gate 19 adds `WF-SEM-016` for dialogue `questId` references that are not
declared in the quest registry.
Gate 20 adds `WF-SEM-017` for quest objective stage references that are not
declared in the same quest.
Gate 21 adds `WF-SEM-018` for quest transition stage references that are not
declared in the same quest.
Gate 22 adds `WF-SEM-019` for quest condition stage references that are not
declared in the same quest.
Gate 23 adds `WF-SEM-020` for quest result-script condition references that
are not declared in the same quest.
Gate 24 adds `WF-SEM-021` for quest condition variable references that are not
declared in the same quest.
Gate 25 adds `WF-SEM-022` for dialogue condition quest-stage references and
`WF-SEM-023` for dialogue condition quest-variable references that are not
declared inside the dialogue line's referenced quest.
Gate 26 adds no new `WF-SEM-*` rule; invalid dialogue result-script shape is
covered by `WF-SCHEMA-001`.
Gate 27 adds `WF-SEM-024` for dialogue line topic references that are not
declared in dialogue topics and `WF-SEM-025` for dialogue `linkTo` target
topic references that are not declared in dialogue topics.
Gate 28 adds `WF-SEM-026` for dialogue quest gates that reference undeclared
quests, `WF-SEM-027` for dialogue quest gate stage references that are not
declared inside the gate quest, and `WF-SEM-028` for dialogue quest gate
variable references that are not declared inside the gate quest.
Gate 29 adds `WF-SEM-029` for dialogue result-script mutation variable
references that are not declared inside the dialogue line's referenced quest.
Gate 30 adds `WF-SEM-030` for dialogue `linkFrom` source topic references
that are not declared in dialogue topics.
Gate 31 adds `WF-SEM-031` for dialogue `linkTo` target topics with no authored
dialogue line endpoint and `WF-SEM-032` for dialogue `linkFrom` source topics
with no authored dialogue line endpoint.
Gate 32 adds `WF-SEM-033` for duplicate dialogue prompt routes with the same
`topicId`, `promptText`, and `priority`.
Gate 33 adds no new `WF-SEM-*` rule; invalid dialogue Speech Challenge shape
is covered by `WF-SCHEMA-001`.
Gate 34 adds no new `WF-SEM-*` rule; invalid dialogue skill gate shape is
covered by `WF-SCHEMA-001`.
Gate 35 adds no new `WF-SEM-*` rule; invalid dialogue perk gate shape is
covered by `WF-SCHEMA-001`.
Gate 36 adds no new `WF-SEM-*` rule; invalid dialogue faction and reputation
gate shape is covered by `WF-SCHEMA-001`.
Gate 37 adds no new `WF-SEM-*` rule; invalid dialogue identity gate shape is
covered by `WF-SCHEMA-001`.
Gate 38 adds no new `WF-SEM-*` rule; invalid dialogue local world flag gate
shape is covered by `WF-SCHEMA-001`.
Gate 39 adds no new `WF-SEM-*` rule; invalid dialogue event history gate shape
is covered by `WF-SCHEMA-001`.
Gate 40 adds no new `WF-SEM-*` rule; invalid dialogue companion state gate
shape is covered by `WF-SCHEMA-001`.
Gate 41 adds no new `WF-SEM-*` rule; invalid dialogue result-script
side-effect gate shape is covered by `WF-SCHEMA-001`.
Gate 42 adds no new `WF-SEM-*` rule; invalid dialogue condition boolean
composition shape is covered by `WF-SCHEMA-001`.
Gate 43 adds `WF-SEM-034` for dialogue condition logic references that are not
authored on the same dialogue line.
Gate 44 adds no new `WF-SEM-*` rule; invalid dialogue nested condition group
shape is covered by `WF-SCHEMA-001`, and `WF-SEM-034` also applies inside
nested groups.
Gate 45 adds `WF-LOAD-009` for missing GECK dialogue export files,
`WF-LOAD-010` for unreadable or binary-looking GECK dialogue export files, and
`WF-LOAD-011` for empty GECK dialogue export files.
Gate 46 adds no new `WF-SEM-*` rule; invalid dialogue condition negation
shape is covered by `WF-SCHEMA-001`, and `WF-SEM-034` also applies to
`negatedConditionIds`.
Gate 47 adds no new `WF-SEM-*` rule; invalid dialogue condition precedence
shape is covered by `WF-SCHEMA-001`.
Gate 48 adds no new `WF-SEM-*` rule; invalid dialogue condition short-circuit
shape is covered by `WF-SCHEMA-001`.
Gate 49 adds `WF-SEM-035` for duplicate dialogue condition logic IDs inside
one line-local condition logic tree.
Gate 50 adds no new `WF-SEM-*` rule; invalid dialogue response route shape is
covered by `WF-SCHEMA-001`.
Gate 51 adds `WF-SEM-036` for dialogue response route target topics that are
not declared in dialogue topics.
Gate 52 adds `WF-SEM-037` for dialogue response route target topics that are
declared but have no authored dialogue line endpoint.
Gate 53 adds `WF-SEM-038` for duplicate dialogue response route IDs authored
on the same dialogue line.
Gate 54 adds `WF-SEM-039` for duplicate dialogue response route keys authored
on the same dialogue line.
Gate 55 adds no new rule family member. It records that response route
taxonomy and selection behavior require more evidence before new `WF-SEM-*`
rules are added.
Gate 56 adds no new rule family member. It creates an evidence pack skeleton
for response route taxonomy before any new route-meaning diagnostics are
defined.
Gate 57 adds no new rule family member. It introduces catalogue listing for
`forge capabilities list`; `WF-CAP-*` diagnostics remain reserved for local
capability scan and provider-resolution gates.
Gate 58 adds no new rule family member. It implements scan evidence output
for `forge capabilities scan`, but `WF-CAP-*` diagnostics remain reserved
until provider-resolution and project requirement gates.
Gate 59 adds no new rule family member. It implements explanation output for
`forge capabilities explain`, but `WF-CAP-*` diagnostics remain reserved until
diagnostic projection over provider-resolution and project requirement gates.
Gate 60 adds project requirement resolution output for
`forge capabilities scan --project`, but still adds no `WF-CAP-*` diagnostic
rule family member.
Gate 129 adds the first concrete `WF-CAP-*` diagnostics:
`WF-CAP-001` for missing required capabilities, `WF-CAP-002` for required
capabilities unverifiable from local evidence, and `WF-CAP-003` for optional
capabilities unavailable from local evidence. Version, wrong-scope, and
runtime-only capability diagnostics remain future `WF-CAP-*` rules.
Gate 130 adds provider evidence detail to those existing capability
diagnostics without assigning new rule IDs. Version, wrong-scope, and
runtime-only capability diagnostics remain future `WF-CAP-*` rules.
Gate 132 adds `WF-CAP-004` for capability providers detected in the wrong
root/Data install scope. Version, MO2 effective-scope, mixed-scope, and
runtime-only capability diagnostics remain future `WF-CAP-*` rules.
Gate 133 adds no new rule ID. `forge capabilities explain --project` displays
matching project requirement context but does not project new diagnostics.
Gate 134 adds no new rule ID. `forge capabilities explain --project` displays
diagnostic handoff metadata using existing `WF-CAP-001` through `WF-CAP-004`
projection rules, but it does not add new rules or SARIF/GitHub explain
output.
Gate 135 adds no new rule ID. `forge doctor export` displays top-level
summary and index metadata derived from existing capability scan and Doctor
data, but it does not add new rules or SARIF/GitHub Doctor export output.
Gate 136 adds no new rule ID. `forge doctor export` displays compact
top-level diagnostics index metadata derived from existing `WF-CAP-*`
capability scan diagnostics, but it does not add new rules or SARIF/GitHub
Doctor export output.
Gate 137 adds no new rule ID. `forge doctor export` displays compact
top-level requirements index metadata derived from existing project capability
requirement resolution data, but it does not add new rules or SARIF/GitHub
Doctor export output.
Gate 138 adds no new rule ID. `forge doctor export` displays compact
top-level action index metadata derived from existing Doctor area actions, but
it does not add new rules or SARIF/GitHub Doctor export output.
Gate 139 adds no new rule ID. `forge doctor export` displays structured
open-question metadata derived from existing Doctor open-question text, but it
does not add new rules or SARIF/GitHub Doctor export output.
Gate 140 adds no new rule ID. `forge doctor export` displays compact
provider-status metadata derived from existing redacted provider scan results,
but it does not add new rules or SARIF/GitHub Doctor export output.
Gate 141 adds no new rule ID. `forge doctor export` displays compact
capability-status metadata derived from existing redacted capability scan
results, but it does not add new rules or SARIF/GitHub Doctor export output.
Gate 142 adds no new rule ID. `forge doctor export` displays compact Doctor
area-status metadata derived from existing redacted Doctor area results, but
it does not add new rules or SARIF/GitHub Doctor export output.
Gate 143 adds no new rule ID. `forge doctor export` displays compact
catalogue-policy metadata derived from existing structured open-question
details, but it does not add new rules or SARIF/GitHub Doctor export output.
Gate 144 adds no new rule ID. `forge capabilities scan` displays compact
Doctor readiness metadata derived from existing Doctor area results, but it
does not add new rules or SARIF/GitHub scan output changes.
Gate 145 adds no new rule ID. `forge capabilities scan` displays compact
provider/capability status metadata derived from existing scan results, but
it does not add new rules or SARIF/GitHub scan output changes.
Gate 146 adds no new rule ID. `forge capabilities scan` displays compact
action metadata derived from existing non-ready Doctor area actions, but it
does not add new rules or SARIF/GitHub scan output changes.
Gate 147 adds no new rule ID. `forge capabilities scan` displays compact
requirement metadata derived from existing project requirement resolution
output, but it does not add new rules or SARIF/GitHub scan output changes.
Gate 148 adds no new rule ID. `forge capabilities scan` displays compact
diagnostic metadata derived from existing projected `WF-CAP-*` issues, but it
does not add new rules or SARIF/GitHub scan output changes.
Gate 149 adds no new rule ID. `forge capabilities scan` displays compact
catalogue-policy metadata derived from existing Doctor open questions, but it
does not add new rules or SARIF/GitHub scan output changes.
Gate 150 adds no new rule ID. `forge capabilities scan` displays compact
open-question detail metadata derived from existing Doctor open questions,
but it does not add new rules or SARIF/GitHub scan output changes.
Gate 151 adds no new rule ID. `forge capabilities explain` displays compact
catalogue-policy open-question detail metadata derived from existing Doctor
open questions, but it does not add new rules or SARIF/GitHub explain output
changes.
Gate 152 adds no new rule ID. `forge capabilities explain` displays compact
catalogue-policy diagnostic handoff metadata derived from existing Doctor open
questions, but it does not add new rules or SARIF/GitHub explain output
changes.
Gate 153 adds no new rule ID. `forge doctor export` displays compact
catalogue-policy diagnostic handoff metadata derived from existing Doctor open
questions, but it does not add new rules or SARIF/GitHub Doctor export output
changes.
Gate 154 adds no new rule ID. `forge capabilities scan` displays compact
catalogue-policy diagnostic handoff metadata derived from existing Doctor open
questions, but it does not add new rules or SARIF/GitHub scan output changes.

Gate 61 adds `WF-GEN-001` for `forge generate` output paths that resolve
outside project `generated/` and `WF-BUILD-001` for `forge build` output paths
that resolve outside project `dist/`. It adds no `WF-CAP-*` diagnostics.

Gate 62 adds `WF-GEN-002` when `mcm-json` generation lacks a declared
non-optional `runtime.ui.mcm_json` generation dependency, `WF-GEN-003` when
no MCM menus are declared for that target, and `WF-GEN-004` when multiple MCM
menus resolve to the same output file. It still adds no `WF-CAP-*`
diagnostics.

Gate 63 adds `WF-GEN-005` when a source MCM registry cannot be translated into
the Gate 63 MCM Extender output subset or generated output does not validate
against `mcm-extender-output/0.1.0/schema.json`. It still adds no
`WF-CAP-*` diagnostics.

Gate 64 adds `WF-GEN-006` when multiple translated MCM menus resolve to the
same generated `MCM/Translations/<modName>.ini` file. It still adds no
`WF-CAP-*` diagnostics.

Gate 65 adds no new rule ID. Unsupported or under-specified MCM option
generation continues to use `WF-GEN-005`, now with checkbox and string-toggle
settings included in the supported subset.

Gate 66 adds no new rule ID. Unsupported or under-specified keybind generation
continues to use `WF-GEN-005`.

Gate 67 adds no new rule ID. Unsupported header generation continues to use
`WF-GEN-005`.

Gate 68 adds no new rule ID. Unsupported or under-specified image generation
continues to use `WF-GEN-005`.
