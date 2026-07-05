# Tests

This directory holds WastelandForge test projects.

Gate 2 creates plain buildable SDK projects only.

Gate 4 added a package-free executable verification harness in
`WastelandForge.UnitTests` for core domain and diagnostic model behavior.

Gate 6 verifies the CLI skeleton through direct `dotnet run` commands. Formal
CLI parser/help snapshot tests remain Gate 7 work.

Gate 7 replaces the package-free harness with xUnit v3 and VSTest wiring across
the existing test projects:

- `WastelandForge.UnitTests` - core domain and diagnostic report behavior.
- `WastelandForge.SchemaTests` - manifest schema and schema catalog behavior.
- `WastelandForge.SemanticTests` - validation pipeline fixture behavior.
- `WastelandForge.GoldenTests` - CLI help and JSON golden output contracts.
- `WastelandForge.WindowsTests` - Windows path and filesystem behavior.
- `WastelandForge.BackCompatTests` - immutable schema/catalog compatibility.

Gate 9 adds release dry-run tests for Forge-owned build manifest and checksum
evidence through `WastelandForge.UnitTests` and CLI coverage through
`WastelandForge.GoldenTests`.

Gate 10 adds SARIF projection tests in `WastelandForge.UnitTests` and CLI SARIF
contract/output-file tests in `WastelandForge.GoldenTests`.

Gate 11 adds Markdown summary and GitHub annotation projection tests in
`WastelandForge.UnitTests` and CLI coverage for `--format github`, `--summary`,
and `GITHUB_STEP_SUMMARY` behavior in `WastelandForge.GoldenTests`.

Gate 12 adds runtime manifest schema validation coverage in
`WastelandForge.SchemaTests` and YAML fixture coverage in
`WastelandForge.SemanticTests`.

Gate 13 adds dependency and capability registry schema/catalog coverage in
`WastelandForge.SchemaTests` and `WastelandForge.BackCompatTests`, plus broken
registry fixture coverage in `WastelandForge.SemanticTests`.

Gate 14 adds asset registry schema/catalog coverage in
`WastelandForge.SchemaTests` and `WastelandForge.BackCompatTests`, plus broken
asset registry fixture coverage in `WastelandForge.SemanticTests`.

Gate 15 adds asset path semantic fixture coverage in
`WastelandForge.SemanticTests`.

Gate 16 adds asset type-specific semantic fixture coverage in
`WastelandForge.SemanticTests` for source signatures and target root
conventions.

Gate 17 adds voice and dialogue asset fixture coverage in
`WastelandForge.SemanticTests` for voice target shape and WAV/OGG/LIP pair
diagnostics.

Gate 18 adds dialogue registry schema/catalog coverage in
`WastelandForge.SchemaTests` and `WastelandForge.BackCompatTests`, plus
dialogue registry and voice worklist fixture coverage in
`WastelandForge.SemanticTests`.

Gate 19 adds quest registry schema/catalog coverage in
`WastelandForge.SchemaTests` and `WastelandForge.BackCompatTests`, plus quest
registry and dialogue quest reference fixture coverage in
`WastelandForge.SemanticTests`.

Gate 20 adds quest registry schema `0.2.0` schema/catalog coverage in
`WastelandForge.SchemaTests` and `WastelandForge.BackCompatTests`, plus
stage/objective schema and objective stage-reference fixture coverage in
`WastelandForge.SemanticTests`.

Gate 21 adds quest registry schema `0.3.0` schema/catalog coverage in
`WastelandForge.SchemaTests` and `WastelandForge.BackCompatTests`, plus
transition schema and transition stage-reference fixture coverage in
`WastelandForge.SemanticTests`.

Gate 22 adds quest registry schema `0.4.0` schema/catalog coverage in
`WastelandForge.SchemaTests` and `WastelandForge.BackCompatTests`, plus
condition schema and condition stage-reference fixture coverage in
`WastelandForge.SemanticTests`.

Gate 23 adds quest registry schema `0.5.0` schema/catalog coverage in
`WastelandForge.SchemaTests` and `WastelandForge.BackCompatTests`, plus
result-script schema and result-script condition-reference fixture coverage in
`WastelandForge.SemanticTests`.

Gate 24 adds quest registry schema `0.6.0` schema/catalog coverage in
`WastelandForge.SchemaTests` and `WastelandForge.BackCompatTests`, plus
variable schema and condition variable-reference fixture coverage in
`WastelandForge.SemanticTests`.

Gate 25 adds dialogue registry schema `0.2.0` schema/catalog coverage in
`WastelandForge.SchemaTests` and `WastelandForge.BackCompatTests`, plus
dialogue condition schema and quest-state reference fixture coverage in
`WastelandForge.SemanticTests`.

Gate 26 adds dialogue registry schema `0.3.0` schema/catalog coverage in
`WastelandForge.SchemaTests` and `WastelandForge.BackCompatTests`, plus
dialogue result-script schema fixture coverage in
`WastelandForge.SemanticTests`.

Gate 27 adds dialogue registry schema `0.4.0` schema/catalog coverage in
`WastelandForge.SchemaTests` and `WastelandForge.BackCompatTests`, plus
dialogue topic schema and topic/link reference fixture coverage in
`WastelandForge.SemanticTests`.

Gate 28 adds dialogue registry schema `0.5.0` schema/catalog coverage in
`WastelandForge.SchemaTests` and `WastelandForge.BackCompatTests`, plus
dialogue quest gate schema and quest-state reference fixture coverage in
`WastelandForge.SemanticTests`.

Gate 29 adds dialogue registry schema `0.6.0` schema/catalog coverage in
`WastelandForge.SchemaTests` and `WastelandForge.BackCompatTests`, plus
dialogue result-script mutation schema and mutation variable-reference fixture
coverage in `WastelandForge.SemanticTests`.

Gate 30 adds dialogue registry schema `0.7.0` schema/catalog coverage in
`WastelandForge.SchemaTests` and `WastelandForge.BackCompatTests`, plus
dialogue Link From schema and source topic-reference fixture coverage in
`WastelandForge.SemanticTests`.

Gate 218 adds xEdit audit schema/catalog and back-compat coverage, semantic
fixture coverage for `WF-SEM-044`, and unit coverage for the non-emitting
`XEditAuditAdapterPlanner`.

Gate 219 extends `WastelandForge.UnitTests` with scaffold emission coverage
for generated file placement, content, output digests, no report output, no
`Data` writes, and validation-error no-write behavior.

Gate 220 extends the same xEdit audit unit coverage with manifest and checksum
sidecar assertions, including safety flags, expected report metadata, output
digest entries, checksum paths relative to `generated/xedit-audit`, no report
output, no `Data` writes, and validation-error no-write behavior.

Gate 221 extends `WastelandForge.GoldenTests` with CLI coverage for
`forge generate --target xedit-audit`, including JSON output, generated
scaffold files, manifest/checksum evidence, no report output, no `Data`
writes, unsupported `--output`, and unsupported `forge build --target
xedit-audit`.

Gate 222 extends `WastelandForge.UnitTests` with xEdit audit report parser
coverage for synthetic JSON report fixtures, typed parsed evidence,
`WF-GEN-009` missing-report diagnostics, `WF-GEN-009` malformed JSON
diagnostics, no scaffold/sidecar emission, and no `Data` writes.

Gate 223 extends `WastelandForge.UnitTests` with xEdit audit report handoff
projection coverage for machine JSON, LF human text, parsed report summaries,
finding metadata, `WF-GEN-009` diagnostic handoff, no handoff file emission,
no scaffold/manifest/checksum writes, and no `Data` writes.

Gate 224 extends `WastelandForge.UnitTests` with xEdit audit report handoff
file emission coverage for generated JSON/text handoff files, UTF-8 without
BOM, LF line endings, output digest records, parser diagnostic no-write
behavior, no manifest/checksum sidecars, and no `Data` writes.

Gate 225 extends `WastelandForge.UnitTests` with xEdit audit report handoff
manifest/checksum coverage for manifest metadata, safety flags, generated
handoff file metadata, payload output digests, dedicated checksum rows,
checksum-file exclusion from output digests, parser diagnostic no-write
behavior, no scaffold checksum overwrite, and no `Data` writes.

Gate 226 extends `WastelandForge.UnitTests` with xEdit audit report handoff
sidecar revalidation coverage for clean generated sidecars, `WF-GEN-010`
edited digest diagnostics, missing checksum-entry diagnostics, unexpected
checksum-entry diagnostics, and no `Data` writes.

Gate 227 extends `WastelandForge.GoldenTests` with CLI coverage for
`forge generate --target xedit-audit-report-handoff`, including JSON output,
generated handoff JSON/text files, manifest/checksum evidence,
`WF-GEN-009` missing-report diagnostics, no scaffold output, no `Data` writes,
and unsupported `forge build --target xedit-audit-report-handoff`.

Gate 228 adds no new test project or assertion. It is a planning/routing
closeout gate covered by the normal build/test smoke validation and
documentation consistency scans.

Gate 229 extends `WastelandForge.GoldenTests` with CLI coverage for
`forge docs`, including JSON output, generated reference index files,
manifest/checksum evidence, dry-run no-write behavior, `WF-GEN-001` output
containment diagnostics, and no `Data` writes.

Gate 230 extends the same golden CLI coverage for `forge docs` with generated
schema reference page skeletons. Tests assert planned schema page outputs,
written `schema-reference.json` and `schema-reference.md` files, reference
index links, docs manifest entries, checksum coverage, dry-run no-write
behavior, and continued avoidance of `Data` writes.

Gate 231 extends the same golden CLI coverage for `forge docs` with generated
project registry reference page skeletons. Tests assert planned registry page
outputs, written `registry-reference.json` and `registry-reference.md` files,
reference index links, docs manifest entries, checksum coverage, dry-run
no-write behavior, JSON top-level property summaries for a synthetic registry,
and continued avoidance of `Data` writes.

Gate 232 extends the same golden CLI coverage for `forge docs` with generated
validation rule reference page skeletons. Tests assert planned rule page
outputs, written `rule-reference.json` and `rule-reference.md` files,
reference index links, docs manifest entries, checksum coverage, dry-run
no-write behavior, reserved family metadata for `WF-GEN-*`, and continued
avoidance of `Data` writes.

Gate 233 extends the same golden CLI coverage for `forge docs` with generated
built-in capability reference page skeletons. Tests assert planned capability
page outputs, written `capability-reference.json` and
`capability-reference.md` files, reference index links, docs manifest entries,
checksum coverage, dry-run no-write behavior, catalogue metadata for
`runtime.ui.mcm_json`, and continued avoidance of `Data` writes.

Gate 234 extends the same golden CLI coverage for `forge docs` with generated
built-in provider reference page skeletons. Tests assert planned provider page
outputs, written `provider-reference.json` and `provider-reference.md` files,
reference index links, docs manifest entries, checksum coverage, dry-run
no-write behavior, catalogue metadata for `provider.runtime.mcm_extender`, and
continued avoidance of `Data` writes.

Gate 235 extends the same golden CLI coverage for `forge docs` with generated
canonical command reference page skeletons. Tests assert planned command page
outputs, written `command-reference.json` and `command-reference.md` files,
reference index links, docs manifest entries, checksum coverage, dry-run
no-write behavior, command metadata for `forge docs`, and continued avoidance
of `Data` writes.

Gate 236 adds golden CLI coverage for `forge graph`. Tests assert planned and
written project source graph JSON/Markdown outputs, graph manifest entries,
checksum coverage, dry-run no-write behavior, source/output boundary graph
metadata, output containment with `WF-GEN-001`, and continued avoidance of
`Data` writes.

Gate 237 extends that golden CLI coverage for `forge graph` with
declaration-only capability requirement graph metadata. Tests assert
requirement, catalogue capability, and catalogue provider nodes and edges,
summary counts, manifest evidence, explicit no-scan/no-provider-resolution
execution flags, dry-run no-write behavior, and continued avoidance of `Data`
writes.

Gate 238 extends that golden CLI coverage for `forge graph` with
declaration-only generator target graph metadata. Tests assert generator target
nodes and source/output edges, generated-evidence handoff edges, summary
counts, manifest evidence, explicit no-execution flags, dry-run no-write
behavior, and continued avoidance of `Data` writes.

Gate 239 extends that golden CLI coverage for `forge graph` with
declaration-only generated artifact expectation graph metadata. Tests assert
artifact expectation nodes, target-to-expectation edges, expectation-to-boundary
edges, summary counts, manifest evidence, explicit no-existence-check flags,
dry-run no-write behavior, and continued avoidance of `Data` writes.

Gate 240 extends that golden CLI coverage for `forge graph` with
declaration-only manifest provenance reference graph metadata. Tests assert
manifest reference nodes, target-to-manifest edges, manifest-to-artifact edges,
manifest-to-boundary edges, summary counts, manifest evidence, explicit
no-manifest-read flags, dry-run no-write behavior, and continued avoidance of
`Data` writes.

Gate 241 is a planning/routing closeout gate for `forge graph`. It adds no new
fixture, golden output, test project, or runtime behavior; validation stays on
build/test smoke plus documentation and routing consistency checks.

Gate 242 extends `WastelandForge.GoldenTests` coverage for top-level
`forge explain` planning. Tests assert `forge help explain` lists diagnostic,
target, output, capability, and provenance subjects, and that Gate 242
placeholder JSON includes those planned subjects plus false execution flags
while still exiting with usage code 2.

Gate 243 extends `WastelandForge.GoldenTests` coverage for
`forge explain diagnostic <rule-id>`. Tests assert plain output uses reserved
rule-family metadata, JSON output includes false execution flags, invalid rule
IDs return usage JSON, and non-diagnostic explain subjects still use the Gate
242 placeholder status.

Gate 244 extends `WastelandForge.GoldenTests` coverage for
`forge explain diagnostic <rule-id>` rule metadata. Tests assert documented
concrete rule metadata appears in plain and JSON output, execution flags remain
false, and valid reserved IDs without embedded concrete metadata fall back to
family-level output.

Gate 245 extends `WastelandForge.GoldenTests` coverage for
`forge explain target <target-id>`. Tests assert documented target metadata
appears in plain and JSON output, execution flags remain false, unknown target
IDs return usage JSON, and non-target explain subjects still use the Gate 242
placeholder status.

Gate 246 extends `WastelandForge.GoldenTests` coverage for
`forge explain output <generated-or-dist-path>`. Tests assert documented
generated/dist output path metadata appears in plain and JSON output,
execution flags remain false, unknown output paths return usage JSON, and
the capability and provenance explain subjects still use placeholder status at
Gate 246.

Gate 247 extends `WastelandForge.GoldenTests` coverage for
`forge explain capability <capability-id>`. Tests assert built-in capability
catalogue metadata appears in plain and JSON output, execution flags remain
false, and unknown capability IDs return usage JSON.

Gate 248 extends `WastelandForge.GoldenTests` coverage for
`forge explain provenance <manifest-or-output-path>`. Tests assert provenance
boundary planning appears in plain and JSON output, execution flags remain
false, unknown provenance paths return usage JSON, and unknown explain
subjects still return reserved-command JSON.

Gate 249 adds no new test files or runtime test cases. It closes the
documented top-level `forge explain` command slice and routes the next
implementation lane to `forge clean` planning, relying on the Gate 243 through
Gate 248 golden coverage plus the full local suite for regression evidence.

Gate 250 extends `WastelandForge.GoldenTests` coverage for the `forge clean`
planning skeleton. Tests assert `forge help clean` lists documented scopes,
reserved JSON includes planned scopes and false execution flags, unsupported
scopes return usage JSON, and a synthetic generated file remains untouched
after `forge clean --generated --format json`.

Gate 251 extends `WastelandForge.GoldenTests` coverage for `forge clean`
dry-run path planning. Tests assert generated-scope path plans, default
generated-scope behavior, all-scope generated/dist/cache root planning,
unsupported-scope usage JSON, false execution flags, and preservation of
synthetic generated, dist, and cache files.

Gate 252 extends `WastelandForge.GoldenTests` coverage for `forge clean --all`
confirmation/refusal planning. Tests assert exit code 6 and `status: refused`
without `--yes` and `--confirm <project-id>`, exit code 0 when both are
provided, false execution flags, and preservation of synthetic generated, dist,
and cache files in both paths.

Gate 253 extends `WastelandForge.GoldenTests` coverage for explicit
`forge clean --generated` execution. Tests assert deleted generated-root
reporting, missing generated-root reporting, generated dry-run preservation,
default generated-scope non-mutation, unchanged all-scope refusal behavior,
and synthetic temp-directory fixtures only.

Gate 254 extends `WastelandForge.GoldenTests` coverage for explicit
`forge clean --dist` execution. Tests assert deleted dist-root reporting,
missing dist-root reporting, dist dry-run preservation, generated-scope
regression behavior, unchanged all-scope refusal behavior, and synthetic
temp-directory fixtures only.

Gate 255 extends `WastelandForge.GoldenTests` coverage for explicit
`forge clean --cache` execution. Tests assert deleted cache-root reporting,
missing cache-root reporting, cache dry-run preservation, generated/dist-scope
regression behavior, unchanged all-scope refusal behavior, and synthetic
temp-directory fixtures only.

Gate 256 extends `WastelandForge.GoldenTests` coverage for confirmed
`forge clean --all` execution. Tests assert all-root deletion reporting,
all-root missing reporting, all-scope dry-run preservation, unchanged
all-scope refusal behavior, generated/dist/cache regression behavior, and
synthetic temp-directory fixtures only.

Gate 257 extends `WastelandForge.GoldenTests` coverage for all-scope
project-ID confirmation validation. Tests assert matched JSON and YAML
manifest identity paths, mismatched project-ID refusal, missing-manifest
refusal, all-scope dry-run preservation, generated/dist/cache regression
behavior, and synthetic temp-directory fixtures only.

Gate 258 extends `WastelandForge.GoldenTests` coverage for active build/cache
lock safety. Tests assert explicit cache clean refusal, manifest-confirmed
all-scope refusal, `cacheLock` report metadata, filesystem preservation when
`.wastelandforge/cache/build.lock` exists, and synthetic temp-directory
fixtures only.

Gate 259 adds no new test fixture or runtime test. It records clean command
closeout and keeps Gate 250 through Gate 258 clean tests as regression
coverage for the parked clean lane.

Gate 260 extends `WastelandForge.GoldenTests` coverage for planning-only
`forge release prepare`. Tests assert JSON planning metadata, help boundary
text, unsafe output-root refusal, false execution flags, and no filesystem
writes using temporary synthetic project directories only.

Gate 261 extends `WastelandForge.GoldenTests` coverage for local
`release-plan.json` emission from `forge release prepare`. Tests assert the
written release plan, CLI `writtenOutputs` metadata, `--dry-run` no-write
behavior, unsafe output-root refusal, and no release summary, build manifest,
checksum, staging, archive, publish, external tool, runtime probe, or AI
behavior using temporary synthetic project directories only.

Gate 262 extends `WastelandForge.GoldenTests` coverage for local
`release-summary.json` emission from `forge release prepare`. Tests assert the
written release summary beside the release plan, CLI `output.releaseSummary`
and `writtenOutputs` metadata, `--dry-run` no-write behavior, unsafe
output-root refusal, and no build manifest, checksum, staging, archive,
publish, external tool, runtime probe, or AI behavior using temporary
synthetic project directories only.

Gate 263 extends `WastelandForge.GoldenTests` coverage for local
`build-manifest.json` emission from `forge release prepare`. Tests assert the
written build manifest beside the release plan and summary, CLI
`output.buildManifest` and `writtenOutputs` metadata, output digests for the
plan and summary, `--dry-run` no-write behavior, unsafe output-root refusal,
and no checksum, staging, archive, publish, external tool, runtime probe, or
AI behavior using temporary synthetic project directories only.

Gate 264 extends `WastelandForge.GoldenTests` coverage for local
`checksums.sha256` emission from `forge release prepare`. Tests assert the
written checksum sidecar beside the release plan, summary, and build manifest,
CLI `output.checksums` and `writtenOutputs` metadata, checksum entries for
the three evidence files, `--dry-run` no-write behavior, unsafe output-root
refusal, and no staging, archive, publish, external tool, runtime probe, or
AI behavior using temporary synthetic project directories only.

Gate 31 adds semantic fixture coverage in `WastelandForge.SemanticTests` for
dialogue link graph endpoints: `linkTo` targets and `linkFrom` sources that are
declared topics but have no authored dialogue line.

Gate 32 adds dialogue registry schema `0.8.0` schema/catalog coverage in
`WastelandForge.SchemaTests` and `WastelandForge.BackCompatTests`, plus prompt
route schema and duplicate prompt route fixture coverage in
`WastelandForge.SemanticTests`.

Gate 33 adds dialogue registry schema `0.9.0` schema/catalog coverage in
`WastelandForge.SchemaTests` and `WastelandForge.BackCompatTests`, plus Speech
Challenge schema fixture coverage in `WastelandForge.SemanticTests`.

Gate 34 adds dialogue registry schema `0.10.0` schema/catalog coverage in
`WastelandForge.SchemaTests` and `WastelandForge.BackCompatTests`, plus skill
gate schema fixture coverage in `WastelandForge.SemanticTests`.

Gate 35 adds dialogue registry schema `0.11.0` schema/catalog coverage in
`WastelandForge.SchemaTests` and `WastelandForge.BackCompatTests`, plus perk
gate schema fixture coverage in `WastelandForge.SemanticTests`.

Gate 36 adds dialogue registry schema `0.12.0` schema/catalog coverage in
`WastelandForge.SchemaTests` and `WastelandForge.BackCompatTests`, plus faction
and reputation gate schema fixture coverage in `WastelandForge.SemanticTests`.

Gate 37 adds dialogue registry schema `0.13.0` schema/catalog coverage in
`WastelandForge.SchemaTests` and `WastelandForge.BackCompatTests`, plus
identity gate schema fixture coverage in `WastelandForge.SemanticTests`.

Gate 38 adds dialogue registry schema `0.14.0` schema/catalog coverage in
`WastelandForge.SchemaTests` and `WastelandForge.BackCompatTests`, plus local
world flag gate schema fixture coverage in `WastelandForge.SemanticTests`.

Gate 39 adds dialogue registry schema `0.15.0` schema/catalog coverage in
`WastelandForge.SchemaTests` and `WastelandForge.BackCompatTests`, plus event
history gate schema fixture coverage in `WastelandForge.SemanticTests`.

Gate 40 adds dialogue registry schema `0.16.0` schema/catalog coverage in
`WastelandForge.SchemaTests` and `WastelandForge.BackCompatTests`, plus
companion state gate schema fixture coverage in `WastelandForge.SemanticTests`.

Gate 41 adds dialogue registry schema `0.17.0` schema/catalog coverage in
`WastelandForge.SchemaTests` and `WastelandForge.BackCompatTests`, plus
result-script side-effect gate schema fixture coverage in
`WastelandForge.SemanticTests`.

Gate 42 adds dialogue registry schema `0.18.0` schema/catalog coverage in
`WastelandForge.SchemaTests` and `WastelandForge.BackCompatTests`, plus
condition boolean composition schema fixture coverage in
`WastelandForge.SemanticTests`.

Gate 43 adds semantic fixture coverage in `WastelandForge.SemanticTests` for
dialogue condition logic references that do not resolve to authored
line-local conditions.

Gate 44 adds dialogue registry schema `0.19.0` schema/catalog coverage in
`WastelandForge.SchemaTests` and `WastelandForge.BackCompatTests`, plus nested
condition group schema and nested condition logic reference fixture coverage
in `WastelandForge.SemanticTests`.

Gate 45 adds `WastelandForge.GoldenTests` CLI coverage for
`forge validate --geck-dialogue-export <path>` success, missing export, and
empty export diagnostics.

Gate 46 adds dialogue registry schema `0.20.0` schema/catalog coverage in
`WastelandForge.SchemaTests` and `WastelandForge.BackCompatTests`, plus
condition negation schema and negated condition logic reference fixture
coverage in `WastelandForge.SemanticTests`.

Gate 47 adds dialogue registry schema `0.21.0` schema/catalog coverage in
`WastelandForge.SchemaTests` and `WastelandForge.BackCompatTests`, plus
condition precedence schema fixture coverage in `WastelandForge.SemanticTests`.

Gate 48 adds dialogue registry schema `0.22.0` schema/catalog coverage in
`WastelandForge.SchemaTests` and `WastelandForge.BackCompatTests`, plus
condition short-circuit schema fixture coverage in
`WastelandForge.SemanticTests`.

Gate 49 adds semantic fixture coverage in `WastelandForge.SemanticTests` for
duplicate root or nested dialogue condition logic IDs inside one line-local
condition logic tree.

Gate 50 adds dialogue registry schema `0.23.0` schema/catalog coverage in
`WastelandForge.SchemaTests` and `WastelandForge.BackCompatTests`, plus
response route schema fixture coverage in `WastelandForge.SemanticTests`.

Gate 51 adds semantic fixture coverage in `WastelandForge.SemanticTests` for
dialogue response route `targetTopicId` values that do not resolve to declared
dialogue topics.

Gate 52 adds semantic fixture coverage in `WastelandForge.SemanticTests` for
declared dialogue response route target topics that have no authored dialogue
line endpoint.

Gate 53 adds semantic fixture coverage in `WastelandForge.SemanticTests` for
duplicate response route IDs authored on the same dialogue line.

Gate 54 adds semantic fixture coverage in `WastelandForge.SemanticTests` for
duplicate response route keys authored on the same dialogue line.

Gate 55 adds no test project, fixture, or assertion. It is verified by the
existing build, semantic fixture suite, full test suite, CLI fixture checks,
and whitespace check while preserving Gate 54 behavior.

Gate 56 adds no test project, fixture, or assertion. It is a docs-only
evidence pack skeleton and is verified by the existing build, semantic fixture
suite, full test suite, CLI fixture checks, and whitespace check.

Gate 57 adds `WastelandForge.GoldenTests` coverage for
`forge capabilities list --format json` and
`forge capabilities list --kind providers --format json`. It preserves the
reserved JSON status contract for `forge capabilities scan`.

Gate 58 updates `WastelandForge.GoldenTests` so
`forge capabilities scan --format json` is implemented. It adds temp-only
synthetic path layout coverage for probable root-file, data-file, and
executable-tool evidence, no-input unknown evidence, scan output-file writing,
and the reserved JSON status contract for `forge capabilities explain`.

Gate 59 updates `WastelandForge.GoldenTests` so
`forge capabilities explain --format json` is implemented. It adds temp-only
synthetic path layout coverage for capability explanations, provider
explanations, unknown capability/provider usage JSON, and explanation
output-file writing.

Gate 60 updates `WastelandForge.GoldenTests` so
`forge capabilities scan --project --format json` reports project requirement
resolution. It covers a satisfied requirement against temp-only xNVSE scan
evidence and a required-but-unknown requirement that returns exit code `4`.

Gate 61 updates `WastelandForge.UnitTests` for the metadata report generator
and `WastelandForge.GoldenTests` for CLI JSON output. It covers
`forge generate --target reports`, `forge build --target reports`, generation
manifest output, build manifest output, checksums, and refusal of generate
outputs outside project `generated/`.

Gate 62 updates schema and back-compat tests for manifest `0.2.0`,
dependency `0.2.0`, capability `0.2.0`, and MCM `0.1.0` schemas. It updates
unit and golden CLI tests for `forge generate --target mcm-json`,
`forge build --target mcm-json`, MCM runtime output, manifests, checksums,
and refusal when `runtime.ui.mcm_json` is not declared as a generation
dependency.

Gate 63 updates schema and back-compat tests for
`mcm-extender-output/0.1.0`, and updates unit and golden CLI tests to assert
runtime-shaped MCM Extender JSON output, output validation manifest evidence,
and `MCM/<menu>.json` output paths.

Gate 64 updates unit and golden CLI tests to assert MCM Extender runtime
requirements pass-through, translation INI output, manifest/checksum evidence
for translation files, and `WF-GEN-006` duplicate translation-output refusal.

Gate 65 updates unit and golden CLI tests to assert generated MCM Extender
checkbox option type `5`, string-toggle option type `6`, string-toggle
`textOn`/`textOff` labels, and synthetic translation coverage for the new
fixture settings.

Gate 66 updates unit and golden CLI tests to assert generated MCM Extender
keybind option type `3`, INI-backed keybind default output, and synthetic
translation coverage for the new fixture setting.

Gate 67 updates unit and golden CLI tests to assert generated MCM Extender
header option type `0`, no `vars` on header output, and synthetic translation
coverage for the new fixture setting.

Gate 68 updates unit and golden CLI tests to assert generated MCM Extender
image option type `0`, image map field pass-through, no `vars` on image
output, and synthetic translation coverage for the new fixture setting.

Gate 69 updates semantic fixture tests to assert `WF-ASSET-010` and
`WF-ASSET-011` for invalid MCM image filenames and unresolved required
texture asset targets.

Gate 70 updates unit and golden CLI tests to assert staged MCM texture asset
outputs, manifest asset evidence, output digests, and build checksums.

Gate 71 updates unit and golden CLI tests to assert `package-manifest.json`,
loose-file package entries, payload digests, and build checksum coverage.

Gate 72 updates unit and golden CLI tests to assert build-time
`package.zip`, sorted ZIP entries, normalized ZIP timestamps, package-manifest
archive digest evidence, output digests, and build checksum coverage.

Gate 73 updates unit and golden CLI tests to assert canonical `forge package`
execution for `--target mcm-json`, package-specific provenance, package archive
output, package manifest archive evidence, and checksum coverage.

Gate 74 updates schema and back-compat tests for
`package-manifest/0.1.0`, and updates unit and golden CLI tests to assert
package validation evidence for generated MCM Extender package manifests and
ZIP entry checks.

Gate 75 updates unit and golden CLI tests to assert `install-preview.json`,
Data-relative install paths, preview-only flags, archive evidence, manifest
evidence, output digests, and build/package checksum coverage.

Gate 76 updates schema and back-compat tests for
`install-preview/0.1.0`, and updates unit and golden CLI tests to assert
install-preview schema evidence in generated MCM JSON manifests.

Gate 77 updates unit and golden CLI tests to assert `install-preview.md`,
human-readable would-copy paths, manifest summary evidence, output digests,
CLI JSON output, and build/package checksum coverage.

Gate 78 updates unit and golden CLI tests to assert
`package-verification.json`, package counts, evidence check status, archive
verification status, manifest report evidence, output digests, CLI output, and
build/package checksum coverage.

Gate 79 updates schema and back-compat tests for
`package-verification/0.1.0`, and updates unit and golden CLI tests to assert
package-verification schema evidence in generated MCM JSON manifests.

Gate 80 updates unit and golden CLI tests to assert `package-verification.md`,
human-readable package evidence checks, manifest summary evidence, output
digests, CLI JSON output, human CLI output, and build/package checksum
coverage.

Gate 81 updates unit and golden CLI tests to assert
`packageVerification.crossChecks` manifest evidence and package-verification
Markdown cross-check summary lines for generate/build/package MCM JSON output.

Gate 82 adds focused `WastelandForge.UnitTests` coverage for the reusable
package-verification evidence validator, including consistent evidence,
package-root mismatch, payload-digest count mismatch, and Markdown summary
mismatch cases.

Gate 83 adds `WastelandForge.UnitTests` coverage for the file-based
package-verification verifier using generated MCM JSON evidence files. It
covers generate output without an archive, build output with an archive, and
an edited package-verification JSON mismatch.

Gate 84 extends that coverage with an edited generated payload file case. The
file-based verifier recomputes the payload SHA-256 and length and reports a
blocking `WF-BUILD-006` mismatch against `package-manifest.json` evidence.

Gate 85 extends that coverage with an edited generated `package.zip` archive
case. The file-based verifier recomputes the archive SHA-256 and length and
reports a blocking `WF-BUILD-006` mismatch against `package-manifest.json`
evidence.

Gate 86 extends that coverage with an edited generated `package.zip` archive
entry case. The file-based verifier compares ZIP entry names against
`package-manifest.json` entries and reports a blocking `WF-BUILD-006`
mismatch for undeclared archive entries.

Gate 87 is documentation-only. It records the future command skeleton target
for existing package evidence verification and does not add test cases.

Gate 88 adds `WastelandForge.GoldenTests` coverage for
`forge package --target mcm-json --verify-existing`. The tests cover a clean
existing package evidence verification and an edited generated payload file
that returns blocking `WF-BUILD-006`.

Gate 89 adds `WastelandForge.GoldenTests` coverage for
`forge package --target mcm-json --verify-existing --format sarif`,
`--format github`, GitHub step-summary append behavior, and rejection of
SARIF for normal package generation without `--verify-existing`.

Gate 90 adds `WastelandForge.GoldenTests` coverage for
`forge package --target mcm-json --verify-existing --summary <path>`. The
tests cover clean summary output, blocking `WF-BUILD-006` summary output, and
rejection of `--summary` for normal package generation without
`--verify-existing`.

Gate 91 adds `WastelandForge.UnitTests` and `WastelandForge.GoldenTests`
coverage for checksum-file revalidation in
`forge package --target mcm-json --verify-existing`. The tests cover edited
checksum digests, missing checksum entries, verify-existing JSON evidence, and
the extra checksum diagnostic produced when a payload file no longer matches
`checksums.sha256`.

Gate 92 adds `WastelandForge.UnitTests` and `WastelandForge.GoldenTests`
coverage for build-manifest content revalidation in
`forge package --target mcm-json --verify-existing`. The tests cover edited
build-manifest cross-check evidence, edited build-manifest output digests,
verify-existing JSON evidence, and the extra build-manifest diagnostic
produced when a payload file no longer matches recorded output digests.

Gate 93 adds `WastelandForge.UnitTests` and `WastelandForge.GoldenTests`
coverage for install-preview summary content revalidation in
`forge package --target mcm-json --verify-existing`. The tests edit
`install-preview.md`, refresh checksum/build-manifest digest evidence, and
assert a single blocking `WF-BUILD-006` summary-content diagnostic.

Gate 94 adds `WastelandForge.UnitTests` and `WastelandForge.GoldenTests`
coverage for install-preview/package-manifest entry content cross-checking in
`forge package --target mcm-json --verify-existing`. The tests edit an
`install-preview.json` entry, refresh install-preview summary,
checksum/build-manifest digest evidence, and assert a single blocking
`WF-BUILD-006` entry-content diagnostic.

Gate 95 adds `WastelandForge.UnitTests` and `WastelandForge.GoldenTests`
coverage for package-verification summary content revalidation in
`forge package --target mcm-json --verify-existing`. The tests edit
`package-verification.md`, refresh checksum/build-manifest digest evidence,
and assert a single blocking `WF-BUILD-006` summary-content diagnostic.

Gate 96 adds `WastelandForge.UnitTests` and `WastelandForge.GoldenTests`
coverage for package-verification JSON check content revalidation in
`forge package --target mcm-json --verify-existing`. The tests edit
`package-verification.json` check evidence, refresh checksum/build-manifest
digest evidence, and assert a single blocking `WF-BUILD-006` check-content
diagnostic.

Gate 97 adds `WastelandForge.UnitTests` and `WastelandForge.GoldenTests`
coverage for package-verification JSON metadata content revalidation in
`forge package --target mcm-json --verify-existing`. The tests edit
`package-verification.json` verification metadata, refresh
checksum/build-manifest digest evidence, and assert a single blocking
`WF-BUILD-006` metadata-content diagnostic.

Gate 98 adds `WastelandForge.UnitTests` and `WastelandForge.GoldenTests`
coverage for package-verification archive detail content revalidation in
`forge package --target mcm-json --verify-existing`. The tests edit
`package-verification.json` archive digest metadata, refresh
checksum/build-manifest digest evidence, and assert a single blocking
`WF-BUILD-006` archive-detail diagnostic.

Gate 99 adds `WastelandForge.UnitTests` and `WastelandForge.GoldenTests`
coverage for install-preview archive detail content revalidation in
`forge package --target mcm-json --verify-existing`. The tests edit
`install-preview.json` archive digest metadata, refresh
checksum/build-manifest digest evidence, and assert a single blocking
`WF-BUILD-006` archive-detail diagnostic.

Gate 100 adds `WastelandForge.UnitTests` and `WastelandForge.GoldenTests`
coverage for package-manifest archive detail content revalidation in
`forge package --target mcm-json --verify-existing`. The tests edit
`package-manifest.json` archive metadata, refresh checksum/build-manifest
digest evidence, and assert a single blocking `WF-BUILD-006` archive-detail
diagnostic.

Gate 101 adds `WastelandForge.UnitTests` and `WastelandForge.GoldenTests`
coverage for archive detail cross-report consistency revalidation in
`forge package --target mcm-json --verify-existing`. The tests make
`package-manifest.json` archive digest evidence stale, make
`install-preview.json` and `package-verification.json` disagree with it,
refresh checksum/build-manifest digest evidence, and assert blocking
`WF-BUILD-006` cross-report diagnostics.

Gate 102 adds `WastelandForge.UnitTests` and `WastelandForge.GoldenTests`
coverage for package archive presence revalidation in
`forge package --target mcm-json --verify-existing`. The tests rewrite package
evidence to record archive status `not-created` while leaving the generated
`package.zip` in place, refresh checksum/build-manifest digest evidence where
the CLI needs it, and assert a single blocking `WF-BUILD-006` archive-presence
diagnostic.

Gate 103 adds `WastelandForge.UnitTests` and `WastelandForge.GoldenTests`
coverage for checksum unexpected-entry revalidation in
`forge package --target mcm-json --verify-existing`. The tests add a temp-only
extra generated file, append a matching `checksums.sha256` line for that file,
and assert a single blocking `WF-BUILD-006` checksum unexpected-entry
diagnostic.

Gate 104 adds `WastelandForge.UnitTests` and `WastelandForge.GoldenTests`
coverage for checksum duplicate-entry revalidation in
`forge package --target mcm-json --verify-existing`. The tests append a second
matching `checksums.sha256` line for an expected package file and assert a
single blocking `WF-BUILD-006` checksum duplicate-entry diagnostic.

Gate 105 adds `WastelandForge.UnitTests` and `WastelandForge.GoldenTests`
coverage for checksum canonical-order revalidation in
`forge package --target mcm-json --verify-existing`. The tests move an expected
checksum entry before an earlier-sorting package file and assert a single
blocking `WF-BUILD-006` checksum canonical-order diagnostic.

Gate 106 adds `WastelandForge.UnitTests` and `WastelandForge.GoldenTests`
coverage for checksum digest canonical-casing revalidation in
`forge package --target mcm-json --verify-existing`. The tests rewrite an
expected checksum digest to uppercase while preserving the correct digest value
and assert a single blocking `WF-BUILD-006` checksum casing diagnostic.

Gate 107 adds `WastelandForge.UnitTests` and `WastelandForge.GoldenTests`
coverage for checksum line-ending and trailing-newline revalidation in
`forge package --target mcm-json --verify-existing`. The tests remove the final
newline and rewrite checksum line endings to a non-canonical separator, then
assert single blocking `WF-BUILD-006` checksum text-format diagnostics.

Gate 108 adds `WastelandForge.UnitTests` and `WastelandForge.GoldenTests`
coverage for checksum path separator canonicalization revalidation in
`forge package --target mcm-json --verify-existing`. The tests rewrite an
expected checksum path to use backslash separators while preserving the correct
digest value, then assert a single blocking `WF-BUILD-006` checksum path
separator diagnostic.

Gate 109 adds `WastelandForge.UnitTests` and `WastelandForge.GoldenTests`
coverage for checksum blank-line revalidation in
`forge package --target mcm-json --verify-existing`. The tests insert a blank
row into generated `checksums.sha256` while preserving all valid entries, then
assert a single blocking `WF-BUILD-006` checksum blank-line diagnostic.

Gate 110 adds `WastelandForge.UnitTests` and `WastelandForge.GoldenTests`
coverage for checksum entry spacing canonicalization revalidation in
`forge package --target mcm-json --verify-existing`. The tests rewrite an
expected checksum entry to use a single space between digest and path while
preserving the correct digest value, then assert a single blocking
`WF-BUILD-006` checksum entry spacing diagnostic.

Gate 111 adds `WastelandForge.UnitTests` and `WastelandForge.GoldenTests`
coverage for checksum path casing canonicalization revalidation in
`forge package --target mcm-json --verify-existing`. The tests rewrite an
expected checksum path to differ only by directory casing while preserving the
correct digest value, then assert a single blocking `WF-BUILD-006` checksum
path casing diagnostic.

Gate 112 adds `WastelandForge.UnitTests` and `WastelandForge.GoldenTests`
coverage for checksum case-insensitive duplicate revalidation in
`forge package --target mcm-json --verify-existing`. The tests append a second
checksum entry whose normalized package-root-relative path differs only by
case, then assert a single blocking `WF-BUILD-006` checksum duplicate-entry
diagnostic.

Gate 113 adds `WastelandForge.UnitTests` and `WastelandForge.GoldenTests`
coverage for checksum malformed-entry format revalidation in
`forge package --target mcm-json --verify-existing`. The tests rewrite an
expected checksum digest to malformed hex while preserving the expected path,
then assert a single blocking `WF-BUILD-006` checksum malformed-entry
diagnostic.

Gate 114 adds `WastelandForge.UnitTests` and `WastelandForge.GoldenTests`
coverage for checksum path containment revalidation in
`forge package --target mcm-json --verify-existing`. The tests rewrite an
expected checksum path to escape the package root, then assert a single
blocking `WF-BUILD-006` checksum path-containment diagnostic.

Gate 115 adds `WastelandForge.UnitTests` and `WastelandForge.GoldenTests`
coverage for checksum comment-line rejection revalidation in
`forge package --target mcm-json --verify-existing`. The tests insert a `#`
comment line into `checksums.sha256`, then assert a single blocking
`WF-BUILD-006` checksum comment-line diagnostic.

Gate 116 adds schema, back-compat, unit, and golden CLI coverage for
`install-plan/0.1.0`, generated `install-plan.json`, generated
`install-plan.md`, manifest/checksum coverage, and `--verify-existing` output
paths for install-plan evidence.

Gate 117 adds `WastelandForge.UnitTests` and `WastelandForge.GoldenTests`
coverage for install-plan JSON and Markdown content revalidation in
`forge package --target mcm-json --verify-existing`. The tests edit
install-plan metadata, entry content, and summary content, then assert
blocking `WF-BUILD-006` diagnostics.

Gate 118 adds `WastelandForge.UnitTests` and `WastelandForge.GoldenTests`
coverage for install-plan schema revalidation in
`forge package --target mcm-json --verify-existing`. The tests edit existing
`install-plan.json` schema shape and assert blocking `WF-BUILD-006` schema
diagnostics while preserving schema-valid install-plan content drift coverage.

Gate 119 adds `WastelandForge.UnitTests` and `WastelandForge.GoldenTests`
coverage for package-manifest schema revalidation in
`forge package --target mcm-json --verify-existing`. The tests edit existing
`package-manifest.json` schema shape and assert blocking `WF-BUILD-006`
schema diagnostics before dependent package evidence checks run.

Gate 120 adds `WastelandForge.UnitTests` and `WastelandForge.GoldenTests`
coverage for install-preview schema revalidation in
`forge package --target mcm-json --verify-existing`. The tests edit existing
`install-preview.json` schema shape and assert blocking `WF-BUILD-006` schema
diagnostics before dependent package evidence checks run.

Gate 121 adds `WastelandForge.UnitTests` and `WastelandForge.GoldenTests`
coverage for package-verification schema revalidation in
`forge package --target mcm-json --verify-existing`. The tests edit existing
`package-verification.json` schema shape and assert blocking `WF-BUILD-006`
schema diagnostics before dependent package evidence checks run. Older
metadata drift tests now mutate schema-valid command evidence so they continue
to exercise deeper content validation after the schema gate.

Gate 122 adds `WastelandForge.GoldenTests` coverage for schema-gated
verify-existing diagnostic projections. The tests edit existing
`package-verification.json` schema shape, refresh local package evidence, and
assert the same `WF-BUILD-006` package-verification schema diagnostic through
SARIF, GitHub workflow-command annotation, and Markdown diagnostic summary
output.

Gate 123 adds `WastelandForge.UnitTests` coverage for missing JSON and
Markdown package-verification evidence files, plus `WastelandForge.GoldenTests`
coverage for the CLI JSON diagnostic when `package-verification.json` is
absent from existing package evidence.

Gate 124 adds `WastelandForge.UnitTests` coverage for malformed JSON and
non-object package-verification evidence, plus `WastelandForge.GoldenTests`
coverage for the CLI JSON diagnostic when existing `package-verification.json`
contains malformed JSON.

Gate 125 adds `WastelandForge.GoldenTests` coverage for malformed
package-verification JSON diagnostic projections. The tests corrupt existing
`package-verification.json` in temp-only package evidence and assert the same
`WF-BUILD-006` malformed JSON diagnostic through SARIF, GitHub
workflow-command annotation, and Markdown diagnostic summary output.

Gate 126 adds no new test category. It closes the current MCM Extender test
lane and points the next testing work at capability scanner and Doctor-style
environment reporting coverage.

Gate 127 extends `WastelandForge.GoldenTests` coverage for the capability
Doctor report in `forge capabilities scan --format json` and target actions in
`forge capabilities explain --format json`. The tests use temp-only synthetic
provider marker files.

Gate 128 extends `WastelandForge.GoldenTests` coverage for
`forge doctor export --format json`, redacted path placeholders, embedded
capability scan data, project requirement export, and output-file writing. The
tests keep provider evidence temp-only and synthetic.

Gate 129 extends `WastelandForge.GoldenTests` coverage for `WF-CAP-*`
projection from `forge capabilities scan --project`, including nested JSON
diagnostics, SARIF 2.1.0 output, and GitHub workflow-command annotations.
Gate 130 extends `WastelandForge.UnitTests` and `WastelandForge.GoldenTests`
coverage for diagnostic evidence JSON, SARIF result evidence properties,
GitHub annotation evidence text, project requirement provider evidence, and
Doctor export redaction of nested requirement evidence paths.
Gate 131 extends `WastelandForge.GoldenTests` coverage for grouped provider
evidence in `forge capabilities explain` JSON and text output.
Gate 132 extends `WastelandForge.GoldenTests` coverage for wrong-scope
capability scan JSON, `WF-CAP-004` project diagnostics, SARIF projection, and
GitHub workflow-command annotations.
Gate 133 extends `WastelandForge.GoldenTests` coverage for capability explain
project requirement context in JSON and plain output, provider-target
requirement filtering, and project read failure diagnostics.
Gate 134 extends `WastelandForge.GoldenTests` coverage for capability explain
diagnostic handoff JSON and plain output for matching project requirements.
Gate 135 extends `WastelandForge.GoldenTests` coverage for Doctor export
summary and index JSON, plain output, output-file JSON, and redaction safety.
Gate 136 extends `WastelandForge.GoldenTests` coverage for Doctor export
compact diagnostics index JSON, plain output, output-file JSON, and redaction
safety.
Gate 137 extends `WastelandForge.GoldenTests` coverage for Doctor export
compact requirements index JSON, plain output, output-file JSON, and redaction
safety.
Gate 138 extends `WastelandForge.GoldenTests` coverage for Doctor export
compact action index JSON, plain output, output-file JSON, and redaction
safety.
Gate 139 extends `WastelandForge.GoldenTests` coverage for Doctor export
structured open-question details in JSON and plain output while preserving the
existing `index.openQuestions` list.
Gate 140 extends `WastelandForge.GoldenTests` coverage for Doctor export
provider-status groups in JSON, plain output, and output-file JSON.
Gate 141 extends `WastelandForge.GoldenTests` coverage for Doctor export
capability-status groups in JSON, plain output, and output-file JSON.
Gate 142 extends `WastelandForge.GoldenTests` coverage for Doctor export
Doctor area-status groups in JSON, plain output, and output-file JSON.
Gate 143 extends `WastelandForge.GoldenTests` coverage for Doctor export
catalogue-policy groups in JSON, plain output, redacted bundle JSON, and
output-file JSON.
Gate 144 extends `WastelandForge.GoldenTests` coverage for capability scan
Doctor readiness groups in JSON, plain output, and output-file JSON.
Gate 145 extends `WastelandForge.GoldenTests` coverage for capability scan
provider/capability status groups in JSON, plain output, and output-file JSON.
Gate 146 extends `WastelandForge.GoldenTests` coverage for capability scan
action groups in JSON, plain output, and output-file JSON.
Gate 147 extends `WastelandForge.GoldenTests` coverage for capability scan
requirement entries in JSON, plain output, and output-file JSON.
Gate 148 extends `WastelandForge.GoldenTests` coverage for capability scan
diagnostic entries in JSON, plain output, and output-file JSON.
Gate 149 extends `WastelandForge.GoldenTests` coverage for capability scan
catalogue-policy groups in JSON, plain output, and output-file JSON.
Gate 150 extends `WastelandForge.GoldenTests` coverage for capability scan
open-question detail entries in JSON, plain output, and output-file JSON.
Gate 151 extends `WastelandForge.GoldenTests` coverage for capability explain
catalogue-policy open-question detail entries in JSON, plain output, and
output-file JSON.
Gate 152 extends `WastelandForge.GoldenTests` coverage for capability explain
catalogue-policy diagnostic handoff entries in JSON, plain output, and
output-file JSON.
Gate 153 extends `WastelandForge.GoldenTests` coverage for Doctor export
catalogue-policy diagnostic handoff entries in JSON, plain output, and
output-file JSON.
Gate 154 extends `WastelandForge.GoldenTests` coverage for capability scan
catalogue-policy diagnostic handoff entries in JSON, plain output, and
output-file JSON.
Gate 155 adds focused `WastelandForge.GoldenTests` coverage for the shared
catalogue-policy diagnostic handoff renderer JSON shape and text indentation,
and keeps existing scan, explain, and Doctor export handoff coverage as the
output-contract guard.
Gate 156 adds focused `WastelandForge.GoldenTests` coverage for the shared
catalogue-policy open-question renderer JSON shape, source-type grouping, and
text indentation, and keeps existing scan, explain, and Doctor export
coverage as the output-contract guard.
Gate 157 adds focused `WastelandForge.GoldenTests` coverage for shared
catalogue-policy view derivation, copied open-question inputs, and empty
input behavior, and keeps existing scan, explain, and Doctor export coverage
as the output-contract guard.
Gate 158 adds focused `WastelandForge.GoldenTests` coverage for Doctor action
summary grouping, JSON shape, text indentation, and empty output behavior,
plus scan and Doctor export golden coverage for JSON/plain action summaries.
Gate 159 adds focused `WastelandForge.GoldenTests` coverage for provider
evidence summary grouping, JSON shape, text indentation, and empty output
behavior, plus scan and Doctor export golden coverage for JSON/plain evidence
summaries.
Gate 160 adds focused `WastelandForge.GoldenTests` coverage for requirement
summary grouping, JSON shape, text indentation, and empty output behavior,
plus scan and Doctor export golden coverage for JSON/plain requirement
summaries.
Gate 161 adds focused `WastelandForge.GoldenTests` coverage for diagnostic
summary grouping, JSON shape, text indentation, and empty output behavior,
plus scan and Doctor export golden coverage for JSON/plain diagnostic
summaries.
Gate 162 adds focused `WastelandForge.GoldenTests` coverage for provider
inventory summary grouping, JSON shape, text indentation, and empty output
behavior, plus scan and Doctor export golden coverage for JSON/plain provider
inventory summaries.
Gate 163 adds focused `WastelandForge.GoldenTests` coverage for Doctor area
capability summary grouping, JSON shape, text indentation, and empty output
behavior, plus scan and Doctor export golden coverage for JSON/plain Doctor
area capability summaries.
Gate 164 adds `WastelandForge.GoldenTests` coverage for
`forge doctor export --summary <path>` Markdown sidecar output, redaction of
local paths in that summary, and missing summary-path usage errors.

Gate 165 adds `WastelandForge.GoldenTests` coverage for
`forge capabilities scan --summary <path>` Markdown sidecar output, omission
of local paths in that summary, and missing summary-path usage errors.

Gate 166 adds `WastelandForge.GoldenTests` coverage for
`forge doctor export --bundle <path>` deterministic ZIP sidecar output,
redaction of local paths inside archive payloads, manifest/checksum entries,
stable ZIP timestamps, and missing bundle-path usage errors.

Gate 167 adds `WastelandForge.GoldenTests` coverage for
`forge capabilities explain --summary <path>` Markdown sidecar output,
omission of local paths in that summary, and missing summary-path usage
errors.

Gate 168 adds `WastelandForge.GoldenTests` coverage for
`forge doctor export --bundle <path>` archives that include
`requirement-explanations/<capability-id>.md` entries for unavailable project
requirements, manifest/checksum coverage for those entries, stable ZIP
timestamps, and omission of local paths inside the supplemental Markdown.

Gate 169 extends that `WastelandForge.GoldenTests` coverage to matching
`requirement-explanations/<capability-id>.json` entries, including
manifest/checksum coverage, `capabilities explain` JSON command metadata,
redacted project roots, stable ZIP timestamps, and omission of local paths.

Gate 170 extends that `WastelandForge.GoldenTests` coverage to
`requirement-explanations/index.json` and
`requirement-explanations/index.md`, including manifest/checksum coverage,
entry-path references, summary counts, stable ZIP timestamps, and omission of
local paths.

Gate 171 extends that `WastelandForge.GoldenTests` coverage to the Doctor
bundle `README.md`, including manifest/checksum coverage, base report links,
requirement explanation index links when present, stable ZIP timestamps, and
omission of local paths.

Gate 172 extends that `WastelandForge.GoldenTests` coverage to
`diagnostics/index.json` and `diagnostics/index.md` entries in Doctor bundle
archives, including manifest/checksum coverage, README links, summary counts,
stable ZIP timestamps, and omission of local paths.

Gate 173 extends that `WastelandForge.GoldenTests` coverage to
`actions/index.json` and `actions/index.md` entries in Doctor bundle archives,
including manifest/checksum coverage, README links, summary counts, stable ZIP
timestamps, and omission of local paths.

Gate 174 extends that `WastelandForge.GoldenTests` coverage to
`requirements/index.json` and `requirements/index.md` entries in Doctor bundle
archives, including manifest/checksum coverage, README links, summary counts,
stable ZIP timestamps, and omission of local paths.

Gate 175 extends that `WastelandForge.GoldenTests` coverage to
`providers/index.json` and `providers/index.md` entries in Doctor bundle
archives, including manifest/checksum coverage, README links, summary counts,
provider evidence summaries, stable ZIP timestamps, and omission of local
paths.

Gate 176 extends that `WastelandForge.GoldenTests` coverage to
`capabilities/index.json` and `capabilities/index.md` entries in Doctor bundle
archives, including manifest/checksum coverage, README links, summary counts,
capability status groups, stable ZIP timestamps, and omission of local paths.

Gate 177 extends that `WastelandForge.GoldenTests` coverage to
`doctor-areas/index.json` and `doctor-areas/index.md` entries in Doctor bundle
archives, including manifest/checksum coverage, README links, summary counts,
area status groups, stable ZIP timestamps, and omission of local paths.

Gate 178 extends that `WastelandForge.GoldenTests` coverage to
`catalogue-policy/index.json` and `catalogue-policy/index.md` entries in
Doctor bundle archives, including manifest/checksum coverage, README links,
open-question counts, diagnostic handoff counts, stable ZIP timestamps, and
omission of local paths.

Gate 179 extends that `WastelandForge.GoldenTests` coverage to
`summary/index.json` and `summary/index.md` entries in Doctor bundle archives,
including manifest/checksum coverage, README links, report summary counts,
derived summary counts, stable ZIP timestamps, and omission of local paths.

Gate 180 extends that `WastelandForge.GoldenTests` coverage to
`evidence/index.json` and `evidence/index.md` entries in Doctor bundle
archives, including manifest/checksum coverage, README links, evidence
summary counts, compact provider evidence entries, stable ZIP timestamps, and
omission of local paths.
Gate 181 extends that `WastelandForge.GoldenTests` coverage to
`redaction/index.json` and `redaction/index.md` entries in Doctor bundle
archives, including manifest/checksum coverage, README links, redaction token
and note counts, stable ZIP timestamps, and omission of local paths.
Gate 182 extends that `WastelandForge.GoldenTests` coverage to
`open-questions/index.json` and `open-questions/index.md` entries in Doctor
bundle archives, including manifest/checksum coverage, README links,
open-question counts, diagnostic handoff counts, stable ZIP timestamps, and
omission of local paths.
Gate 183 extends that `WastelandForge.GoldenTests` coverage to
`scan-inputs/index.json` and `scan-inputs/index.md` entries in Doctor bundle
archives, including manifest/checksum coverage, README links, redacted input
placeholders, detector-family counts, runtime/MO2 flags, stable ZIP
timestamps, and omission of local paths.
Gate 184 extends that `WastelandForge.GoldenTests` coverage to
`bundle/index.json` and `bundle/index.md` entries in Doctor bundle archives,
including manifest/checksum coverage, README links, archive entry path
coverage, requirement-explanation path coverage, stable ZIP timestamps, and
omission of local paths.
Gate 185 extends that `WastelandForge.GoldenTests` coverage to
`triage/index.json` and `triage/index.md` entries in Doctor bundle archives,
including manifest/checksum coverage, README links, bundle-index coverage,
blocking/review/action counts, requirement-explanation review paths, stable
ZIP timestamps, and omission of local paths.
Gate 186 extends that `WastelandForge.GoldenTests` coverage to primary Doctor
export triage JSON, plain text, and Markdown summary output, including
blocking/review/action counts, report-section references, bundle-path
separation, and omission of local paths.
Gate 187 extends that `WastelandForge.GoldenTests` coverage to Doctor triage
command hints in primary JSON, plain text, Markdown summaries, and bundle
triage entries, including canonical command strings, placeholder usage,
section/path separation, and omission of local paths.
Gate 188 extends that `WastelandForge.GoldenTests` coverage to Doctor triage
worklist entries in primary JSON, plain text, Markdown summaries, and bundle
triage entries, including order, priority, command-hint links, section/path
separation, and omission of local paths.
Gate 189 extends that `WastelandForge.GoldenTests` coverage to Doctor worklist
summary metadata in primary JSON, plain text, Markdown summaries, and bundle
triage entries, including priority groups, section/path source groups, and
omission of local paths.
Gate 190 extends that `WastelandForge.GoldenTests` coverage to Doctor
remediation status headers in primary JSON, plain text, Markdown summaries,
and bundle triage entries, including status, headline, first command,
section/path separation, and omission of local paths.
Gate 191 extends that `WastelandForge.GoldenTests` coverage to human operator
handoff sections in plain text, Markdown summaries, and bundle triage
Markdown, including checklist commands, priority/source summaries,
section/path separation, and omission of local paths.

Gate 192 extends that `WastelandForge.GoldenTests` coverage to
`handoff-summary.md` in Doctor bundle archives, including archive entry,
README, bundle-index, manifest, checksum, command-hint, key-path, and
redaction coverage.

Gate 193 extends `WastelandForge.GoldenTests` coverage to scan-side operator
handoff sections in `forge capabilities scan` plain output and Markdown
summary sidecars, including status, priority/source summaries, command hints,
work items, and continued local-path omission in Markdown summaries.

Gate 194 extends `WastelandForge.GoldenTests` coverage to explain-side
operator handoff sections in `forge capabilities explain` plain output and
Markdown summary sidecars, including blocked status, priority/source
summaries, command hints, work items, and continued local-path omission in
Markdown summaries.

Gate 195 extends `WastelandForge.GoldenTests` coverage to Doctor bundle
requirement explanation Markdown. Archive tests assert that
`requirement-explanations/<capability>.md` includes the explain-side operator
handoff checklist, the bundle README and requirement explanation index
describe that Markdown handoff behavior, paired explanation JSON entries do
not add `operatorHandoff`, and Markdown entries continue to omit redacted
project-root tokens.

Gate 196 extends `WastelandForge.GoldenTests` coverage to provider-version
declaration metadata in `forge capabilities list` JSON/plain output and
`forge capabilities explain` JSON/plain/Markdown output. The tests assert
that metadata remains declaration-only with `not-parsed` local-version status
and `not-evaluated` resolution status.

Gate 197 extends `WastelandForge.GoldenTests` coverage to the same
declaration-only provider-version metadata in `forge capabilities scan`
JSON/plain/Markdown output, embedded Doctor export scan JSON, and Doctor
bundle provider-index JSON/Markdown output. The tests still assert
`not-parsed` local-version status and `not-evaluated` resolution status.

Gate 198 adds no test fixtures or code tests. It records that Gate 199 should
use synthetic unit-test inputs for pure provider-version parsing and must not
commit real provider DLLs, Bethesda assets, or third-party mod files.

Gate 199 adds `WastelandForge.SemanticTests` coverage for the pure
provider-version parser contract. The tests use synthetic raw values for
`semver`, `integer`, and `scaled-integer`, assert raw-value preservation on
failure, and do not add real provider DLLs, Bethesda assets, or third-party
mod files.

Gate 200 adds documentation only. It adds no test fixtures, no parser tests,
and no real provider DLLs, Bethesda assets, third-party mod files, local
install snapshots, MO2 profiles, GECK outputs, or external tool fixtures.

Gate 201 adds `WastelandForge.SemanticTests` coverage for
`ProviderVersionParsedEvidence`. The tests use synthetic parser results only
and assert that successful parsed evidence and failed raw evidence preserve
provider ID, source kind, raw value, parsed state, normalized value, numeric
components, failure reason, and provenance. It adds no real provider DLLs,
Bethesda assets, third-party mod files, local install snapshots, MO2 profiles,
GECK outputs, or external tool fixtures.

Gate 202 adds documentation only. It adds no JIP LN script fixtures, generator
tests, schema tests, local install snapshots, GECK outputs, MO2 profiles,
Bethesda assets, third-party mod files, or external tool fixtures. Gate 203
should use synthetic validation coverage for the source contract skeleton.

Gate 203 adds schema, back-compat, and semantic fixture coverage for the JIP
LN text-script source contract skeleton. The fixtures are synthetic JSON
registry documents only and add no generated script files, local install
snapshots, GECK outputs, MO2 profiles, Bethesda assets, third-party mod files,
or external tool fixtures.

Gate 204 adds semantic fixture coverage for `WF-SEM-040` lifecycle/output
prefix mismatch and `WF-SEM-041` missing JIP Script Runner capability
requirement. The fixtures are synthetic JSON registry documents only and add
no generated script files, local install snapshots, GECK outputs, MO2
profiles, Bethesda assets, third-party mod files, or external tool fixtures.

Gate 205 adds schema and semantic fixture coverage for opaque JIP
body/source-line records. The fixtures are synthetic JSON registry documents
only and add no generated script files, local install snapshots, GECK outputs,
MO2 profiles, Bethesda assets, third-party mod files, or external tool
fixtures.

Gate 206 adds semantic fixture coverage for `WF-SEM-042` source-line
byte-budget validation. The fixture is synthetic JSON only and adds no
generated script files, local install snapshots, GECK outputs, MO2 profiles,
Bethesda assets, third-party mod files, or external tool fixtures.

Gate 207 adds semantic fixture coverage for `WF-SEM-043` duplicate JIP script
`outputFile` values. The fixture is synthetic JSON only and adds no generated
script files, local install snapshots, GECK outputs, MO2 profiles, Bethesda
assets, third-party mod files, or external tool fixtures.

Gate 208 adds focused `WastelandForge.UnitTests` coverage for
`JipScriptGenerationPlanner`. The tests assert non-emitting plan metadata for
the synthetic valid JIP fixture and validation-error short-circuit behavior
for the duplicate-output fixture. It adds no generated script files, local
install snapshots, GECK outputs, MO2 profiles, Bethesda assets, third-party
mod files, or external tool fixtures.

Gate 209 extends that focused `WastelandForge.UnitTests` coverage for
`JipScriptTextRenderer`. The tests assert in-memory rendered text, LF
separator metadata, UTF-8 byte counts, path metadata preservation, and
validation-error short-circuit behavior. It adds no generated script files,
local install snapshots, GECK outputs, MO2 profiles, Bethesda assets,
third-party mod files, or external tool fixtures.

Gate 210 extends that focused `WastelandForge.UnitTests` coverage for
`JipScriptFileEmitter`. The tests use temp-copied synthetic fixtures to assert
generated-root-only writes under `generated/jip-scripts`, exact rendered
content, no writes to `Data`, and validation-error no-write behavior. It adds
no local install snapshots, GECK outputs, MO2 profiles, Bethesda assets,
third-party mod files, or external tool fixtures.

Gate 211 extends the same focused `JipScriptFileEmitter` unit coverage to
assert `jip-script-emission-manifest.json`, `checksums.sha256`, script payload
digest entries, manifest digest entries, checksum-file exclusion from output
digests, package non-mutation flags, and validation-error no-manifest behavior.
It adds no local install snapshots, GECK outputs, MO2 profiles, Bethesda
assets, third-party mod files, or external tool fixtures.

Gate 212 extends schema catalog tests for
`jip-script-emission-manifest/0.1.0` and focused `JipScriptFileEmitter` unit
coverage for generated manifest schema validation and `WF-GEN-007` malformed
manifest diagnostics. It adds no local install snapshots, GECK outputs, MO2
profiles, Bethesda assets, third-party mod files, or external tool fixtures.

Gate 213 extends focused `JipScriptFileEmitter` unit coverage for generated
checksum sidecar revalidation and `WF-GEN-008` diagnostics covering clean
evidence, edited digest drift, missing expected entries, and unexpected
entries. It adds no local install snapshots, GECK outputs, MO2 profiles,
Bethesda assets, third-party mod files, or external tool fixtures.

Gate 214 extends `CliGoldenTests` for
`forge generate --target jip-scripts` and the unsupported
`forge build --target jip-scripts` boundary. The tests use temp-copied
synthetic fixtures to assert generated script output, manifest/checksum
evidence, CLI JSON shape, output digest evidence, and no live Data writes. It
adds no local install snapshots, GECK outputs, MO2 profiles, Bethesda assets,
third-party mod files, or external tool fixtures.

Gate 215 extends focused JIP unit tests and `CliGoldenTests` for
`forge build --target jip-scripts`. The tests use temp-copied synthetic
fixtures to assert dist script output, build-manifest evidence, checksum
evidence, dry-run no-write behavior, output containment diagnostics,
validation-error no-write behavior, and no live Data writes. It adds no local
install snapshots, GECK outputs, MO2 profiles, Bethesda assets, third-party
mod files, or external tool fixtures.

Gate 216 extends focused JIP unit tests and `CliGoldenTests` for
`forge package --target jip-scripts`. The tests use temp-copied synthetic
fixtures to assert package staging under `dist/jip-scripts/package/Data`,
package-manifest evidence, install-plan evidence, build-manifest evidence,
checksum evidence, dry-run no-write behavior, output containment diagnostics,
validation-error no-write behavior, and no live Data writes. It adds no local
install snapshots, GECK outputs, MO2 profiles, Bethesda assets, third-party
mod files, or external tool fixtures.

Gate 265 extends `CliGoldenTests` for `forge release prepare` with local
staging payload skeleton coverage. The tests assert
`dist/release-prepare/staging/release-payload.json`, staging root metadata,
release-plan/summary/build-manifest staging references, checksum coverage,
dry-run no-write behavior, output containment diagnostics, and no archive,
installer, publish, remote, external-tool, runtime-probe, AI, plugin, MO2, or
GECK behavior. It adds no local install snapshots, GECK outputs, MO2 profiles,
Bethesda assets, third-party mod files, or external tool fixtures.

Gate 266 extends `CliGoldenTests` for `forge release prepare` with local
release archive planning metadata coverage. The tests assert
`dist/release-prepare/release-archive-plan.json`, planned archive path
metadata, release evidence archive-plan references, build-manifest and
checksum coverage, dry-run no-write behavior, output containment diagnostics,
and no archive file, archive directory, installer, publish, remote,
external-tool, runtime-probe, AI, plugin, MO2, or GECK behavior. It adds no
local install snapshots, GECK outputs, MO2 profiles, Bethesda assets,
third-party mod files, or external tool fixtures.

Gate 267 extends `CliGoldenTests` for `forge release prepare` with
deterministic local release archive coverage. The tests assert
`dist/release-prepare/archives/release.zip`, sorted stored ZIP entries,
deterministic ZIP-compatible timestamps, release evidence archive references,
build-manifest and checksum coverage, repeated-run archive digest stability,
dry-run no-write behavior, output containment diagnostics, and no FOMOD,
installer, publish, remote, external-tool, runtime-probe, AI, plugin, MO2, or
GECK behavior. It adds no local install snapshots, GECK outputs, MO2 profiles,
Bethesda assets, third-party mod files, or external tool fixtures.

Gate 268 extends `CliGoldenTests` for `forge release prepare` with release
archive evidence revalidation coverage. The tests assert
`dist/release-prepare/release-archive-evidence.json`, archive SHA-256 and
length evidence, expected and actual ZIP entry names, stored-compression
status, deterministic timestamp metadata, build-manifest and checksum
coverage, dry-run no-write behavior, output containment diagnostics, and no
FOMOD, installer, publish, remote, external-tool, runtime-probe, AI, plugin,
MO2, or GECK behavior. It adds no local install snapshots, GECK outputs, MO2
profiles, Bethesda assets, third-party mod files, or external tool fixtures.

Gate 269 adds no new test fixture or runtime test surface. It records
`forge release prepare` lane closeout and routes the next release work to a
no-publish `forge release publish` governance preflight skeleton. Existing
release-prepare coverage from Gates 260 through 268 remains the active
behavioral test surface.

Gate 270 adds golden CLI coverage for `forge release publish` governance
preflight. Tests verify normal no-publish refusal with exit code 6, dry-run
preflight with exit code 0, required local evidence reporting, governance
check reporting, missing human approval reporting, false publish/remote/upload
/signing/tool/runtime/AI execution flags, help text, unsupported-option usage
JSON, and no `dist/` output creation.

Gate 271 extends golden CLI coverage for `forge release publish` local
evidence discovery. Tests verify empty project roots report eight missing
release-prepare artifacts, prepared project roots report eight
`present-not-validated` artifacts, artifact path checks are enabled, artifact
content reads remain disabled, and publish/remote/upload/signing/tool/runtime
/AI execution remains disabled.

Gate 272 extends golden CLI coverage for `forge release publish` content-shape
classification. Tests verify well-formed JSON evidence, well-formed
`checksums.sha256` line shape, binary archive deferral, malformed JSON
reporting, enabled shape classification, and disabled checksum revalidation,
archive revalidation, publish, remote, upload, signing, external-tool,
runtime-probe, and AI execution.

Gate 273 extends golden CLI coverage for `forge release publish` checksum
sidecar entry classification. Tests verify parsed checksum entries,
expected-path coverage, unchanged digest text reported as
`expected-not-revalidated`, missing expected path coverage, unexpected path
classification, enabled checksum entry classification, and disabled checksum
digest revalidation, archive revalidation, publish, remote, upload, signing,
external-tool, runtime-probe, and AI execution.

Gate 274 extends golden CLI coverage for `forge release publish`
build-manifest output cross-reference. Tests verify parsed manifest outputs,
expected output coverage, local artifact and checksum sidecar path
cross-reference, rewritten output path mismatch reporting, enabled
build-manifest output cross-reference, and disabled checksum digest
revalidation, build-manifest digest revalidation, semantic evidence
validation, archive revalidation, publish, remote, upload, signing,
external-tool, runtime-probe, and AI execution.

Gate 275 extends golden CLI coverage for `forge release publish`
release-archive-evidence metadata cross-reference. Tests verify parsed
archive-evidence output metadata, expected path coverage, local artifact,
checksum sidecar, and build-manifest output cross-reference, rewritten
archive-evidence metadata path mismatch reporting, enabled
release-archive-evidence metadata cross-reference, and disabled checksum
digest revalidation, build-manifest digest revalidation,
release-archive-evidence digest revalidation, semantic evidence validation,
archive revalidation, publish, remote, upload, signing, external-tool,
runtime-probe, and AI execution.

Gate 276 extends golden CLI coverage for `forge release publish` checksum
sidecar digest revalidation. Tests verify expected local checksum entries are
recomputed, matched entries report `matched-revalidated`, edited digest values
report `mismatched-revalidated`, missing checksum coverage remains classified
separately, enabled checksum digest revalidation, and disabled build-manifest
digest revalidation, release-archive-evidence digest revalidation, semantic
evidence validation, archive revalidation, publish, remote, upload, signing,
external-tool, runtime-probe, and AI execution.

Gate 277 extends golden CLI coverage for `forge release publish`
build-manifest output digest revalidation. Tests verify expected local
build-manifest outputs are recomputed, matched outputs report
`matched-revalidated`, edited build-manifest output digest values report
`mismatched-revalidated`, cross-reference mismatches remain classified
separately, enabled build-manifest digest revalidation, and disabled semantic
evidence validation, archive revalidation, publish, remote, upload, signing,
external-tool, runtime-probe, and AI execution.

Gate 278 extends golden CLI coverage for `forge release publish`
release-archive-evidence archive digest metadata revalidation. Tests verify
the expected local archive file SHA-256 and length metadata are recomputed,
matched archive metadata reports `complete-digest-revalidated`, edited
archive SHA metadata reports `mismatch-digest-revalidated`, cross-reference
mismatches remain classified separately, enabled archive-evidence digest
revalidation, and disabled semantic evidence validation, archive payload
validation, publish, remote, upload, signing, external-tool, runtime-probe,
and AI execution.

Gate 279 extends golden CLI coverage for `forge release publish` archive
reopening/revalidation. Tests verify the expected local release archive is
opened only after lower-layer cross-reference and digest metadata checks pass,
entry names, entry order, deterministic timestamps, and stored compression
metadata are reported, unexpected archive entries report
`mismatch-archive-revalidated`, and semantic evidence validation, archive
payload validation, publish, remote, upload, signing, external-tool,
runtime-probe, and AI execution remain disabled.

Gate 280 extends golden CLI coverage for `forge release publish` semantic
release-evidence validation. Tests verify clean local release-prepare evidence
reports `complete-semantic-validated`, all 15 semantic checks pass, tampered
release-summary counters report `semantic-evidence-mismatch-validated` while
lower checksum, build-manifest, and archive checks remain complete, and
archive payload validation, publish, remote, upload, signing, external-tool,
runtime-probe, and AI execution remain disabled.

Gate 281 extends golden CLI coverage for `forge release publish`
governance-check evaluation. Tests verify missing local governance policy
evidence reports `incomplete-governance-evaluated`, synthetic temp policy
files can satisfy all six governance checks, each check reports evidence
path/detail/status, governance execution is true, and publish, remote, upload,
signing, external-tool, runtime-probe, and AI behavior remain disabled.

Gate 282 extends golden CLI coverage for `forge release publish`
schema-validation evidence evaluation. Tests verify missing validation reports
remain `missing`, clean temp-only `dist/release-dry-run/validation.json`
reports `complete-schema-validated`, `WF-SCHEMA-*` issues report
`schema-diagnostics-present`, schema evidence execution is true, and publish,
remote, upload, signing, external-tool, runtime-probe, and AI behavior remain
disabled.

Gate 283 extends golden CLI coverage for `forge release publish`
capability/environment evidence evaluation. Tests verify missing capability
scan reports remain `missing`, clean synthetic project-scoped
`dist/release-dry-run/capabilities-scan.json` reports
`complete-capability-environment-validated`, `WF-CAP-*` issues and
required-unavailable requirements report `capability-diagnostics-present`,
capability/environment evidence execution is true, and publish, remote,
upload, signing, external-tool, runtime-probe, MO2 automation, and AI behavior
remain disabled.

Gate 284 extends golden CLI coverage for `forge release publish`
package-validation evidence evaluation. Tests verify missing package verifier
reports remain `missing`, clean temp-only
`dist/release-dry-run/package-verify.json` reports
`complete-package-validated`, saved `forge package --target mcm-json
--verify-existing --format json` reports with `WF-BUILD-*` issues report
`package-diagnostics-present`, package-validation evidence execution is true,
and publish, remote, upload, signing, external-tool, runtime-probe, MO2
automation, and AI behavior remain disabled.

Gate 301 extends unit and golden CLI coverage for `forge release verify`
self-report evidence. Tests verify `dist/release-dry-run/release-verify.json`
is written from temp-copied synthetic fixtures, uses the existing release
verify JSON report identity, is surfaced as `outputs.releaseVerification`,
and is covered by release dry-run build-manifest and checksum evidence.

Gate 302 extends unit and golden CLI coverage for `forge release verify`
evidence-index output. Tests verify
`dist/release-dry-run/release-evidence-index.json` is written from temp-copied
synthetic fixtures, lists release-publish preflight evidence paths and command
hints, records disabled execution boundaries, is surfaced as
`outputs.releaseEvidenceIndex`, and is covered by release dry-run
build-manifest and checksum evidence.

Gate 303 extends unit and golden CLI coverage for `forge release verify`
evidence-handoff output. Tests verify
`dist/release-dry-run/release-evidence-handoff.md` is written from
temp-copied synthetic fixtures, renders evidence rows and command hints,
records disabled execution boundaries, is surfaced as
`outputs.releaseEvidenceHandoff`, and is covered by release dry-run
build-manifest and checksum evidence.

Gate 304 extends unit and golden CLI coverage for `forge release verify`
evidence-status output. Tests verify
`dist/release-dry-run/release-evidence-status.json` is written from
temp-copied synthetic fixtures, marks indexed release-publish preflight
evidence paths as present or missing, projects present/missing counts into
the evidence index and Markdown handoff, is surfaced as
`outputs.releaseEvidenceStatus`, and is covered by release dry-run
build-manifest and checksum evidence.

Gate 305 extends unit and golden CLI coverage for `forge release verify`
missing-evidence action output. Tests verify
`dist/release-dry-run/release-evidence-actions.json` is written from
temp-copied synthetic fixtures, lists manual actions for missing
capability/environment and package-validation evidence, links from the
evidence index, status projection, and Markdown handoff, is surfaced as
`outputs.releaseEvidenceActions`, and is covered by release dry-run
build-manifest and checksum evidence.

Gate 306 extends unit and golden CLI coverage for `forge release verify`
evidence collection-plan output. Tests verify
`dist/release-dry-run/release-evidence-collection-plan.json` is written from
temp-copied synthetic fixtures, orders release-publish preflight evidence
steps, links missing manual steps to action IDs, links from the evidence
index, status projection, action checklist, and Markdown handoff, is surfaced
as `outputs.releaseEvidenceCollectionPlan`, and is covered by release dry-run
build-manifest and checksum evidence.

Gate 307 extends golden CLI coverage for `forge release publish`
collection-plan evidence. Tests verify publish dry-run reads
`dist/release-dry-run/release-evidence-collection-plan.json` from
temp-copied synthetic fixtures, validates identity, links, ordered steps,
summary counters, and no-execution flags, surfaces `collectionPlanEvidence`,
adds `release-dry-run-collection-plan` to required evidence/readiness, and
keeps release publishing, uploads, external tools, runtime probes, and AI
disabled.

Gate 308 extends golden CLI coverage for `forge release publish` release
dry-run evidence cross-links. Tests verify publish dry-run reads the local
`release-evidence-index.json`, `release-evidence-status.json`,
`release-evidence-actions.json`, `release-evidence-collection-plan.json`, and
`release-evidence-handoff.md` files from temp-copied synthetic fixtures,
validates identity, links, required evidence rows, actions, collection steps,
summary counters, handoff references, and no-execution flags, surfaces
`dryRunCrossLinkEvidence`, adds `release-dry-run-cross-links` to required
evidence/readiness, updates Doctor release-readiness counts, and keeps release
publishing, uploads, external tools, runtime probes, and AI disabled.

Gate 309 extends golden CLI coverage for `forge release publish` release
dry-run evidence remediation. Tests verify missing, malformed,
cross-link-mismatched, and complete dry-run evidence states, surface
`dryRunEvidenceRemediation`, assert manual action items and command hints,
assert summary and execution fields, and keep release publishing, uploads,
external tools, runtime probes, and AI disabled.

Gate 310 extends golden CLI coverage for `forge doctor export` release
dry-run evidence remediation handoff. Tests verify primary JSON, plain text,
Markdown summaries, release-readiness bundle indexes, triage indexes, and
handoff summaries expose `dryRunEvidenceRemediation`, the manual
`restore-release-dry-run-evidence-files` work item, and the local
`forge release verify <project-root> --format json --no-input` command hint,
while keeping release publishing, uploads, external tools, runtime probes, and
AI disabled.

Gate 311 is a planning/routing closeout for the release dry-run remediation
handoff lane. It adds no new fixture corpus, golden output, runtime command
behavior, schema, diagnostic, release execution, Doctor execution, init
execution, external tool execution, runtime probe, or AI requirement. Normal
build/test smoke checks remain the validation expectation.

Gate 312 extends golden CLI coverage for `forge init` scaffold planning.
Tests verify init help, JSON planning output, derived project identity,
planned scaffold paths, false execution flags, existing planned-path refusal
with exit code `6`, diagnostic-only format rejection, and no scaffold writes
against temporary synthetic directories. It adds no public fixture corpus,
real provider sample, Bethesda asset, third-party mod file, external tool
fixture, runtime probe, or AI requirement.

Gate 316 extends golden CLI coverage for `forge init` minimal scaffold
emission. Tests verify normal init writes only the manifest and dependency/
capability registry files, dry-run remains no-write, generated/dist/editor/
workflow/README paths remain absent, written-path execution metadata is
reported, existing planned-path refusal remains exit code `6`, and the
created temp scaffold passes `forge validate` with zero errors. It adds no
public fixture corpus, real provider sample, Bethesda asset, third-party mod
file, external tool fixture, runtime probe, or AI requirement.

Gate 317 extends golden CLI coverage for `forge init` config and README
scaffold emission. Tests verify normal init writes `.wastelandforge/config.jsonc`
and `README.md`, dry-run remains no-write, generated/dist/cache/editor/
workflow paths remain absent, config and README write metadata is reported,
existing planned-path refusal remains exit code `6`, and the created temp
scaffold passes `forge validate` with zero errors. It adds no public fixture
corpus, real provider sample, Bethesda asset, third-party mod file, external
tool fixture, runtime probe, or AI requirement.

Gate 318 extends golden CLI coverage for `forge init` VS Code task scaffold
emission. Tests verify normal init writes `.vscode/tasks.json`, parses the task
file, checks the validate/capability-scan/build task labels and validate
problem matcher, dry-run remains no-write, generated/dist/cache/workflow paths
remain absent, VS Code write metadata is reported, existing planned-path
refusal remains exit code `6`, and the created temp scaffold passes
`forge validate` with zero errors. It adds no public fixture corpus, real
provider sample, Bethesda asset, third-party mod file, external tool fixture,
runtime probe, or AI requirement.

Gate 319 extends golden CLI coverage for `forge init` GitHub Actions workflow
scaffold emission. Tests verify normal init writes
`.github/workflows/wastelandforge.yml`, checks Windows and Ubuntu lane markers,
least-privilege permissions, pinned action SHAs, SARIF/artifact/release dry-run
markers, dry-run remains no-write, generated/dist/cache paths remain absent,
workflow write metadata is reported, existing planned-path refusal remains
exit code `6`, and the created temp scaffold passes `forge validate` with zero
errors. It adds no public fixture corpus, real provider sample, Bethesda asset,
third-party mod file, external tool fixture, runtime probe, or AI requirement.

Gate 320 extends golden CLI coverage for `forge init` editor schema
association scaffold emission. Tests verify normal init writes
`.vscode/settings.json`, parses the settings file, checks JSON and YAML schema
association mappings, dry-run remains no-write, generated/dist/cache paths
remain absent, editor schema write metadata is reported, existing planned-path
refusal remains exit code `6`, and the created temp scaffold passes
`forge validate` with zero errors. It adds no public fixture corpus, real
provider sample, Bethesda asset, third-party mod file, external tool fixture,
runtime probe, VS Code extension process, language-server process, or AI
requirement.

Gate 321 adds no new runtime test coverage. It is a docs and prompt routing
closeout for the `forge init` onboarding lane and routes the next value slice
to Forge CLI runner/bootstrap planning. No public fixtures, generated payload
fixtures, real provider samples, Bethesda assets, third-party mod files,
external tool fixtures, runtime probes, VS Code extension processes,
language-server processes, or AI requirements are added.

Gate 322 adds no new runtime test coverage. It is a docs and prompt routing
planning gate for the Forge CLI runner/bootstrap model and routes Gate 323 to
a source-built runner shim scaffold. No public fixtures, generated payload
fixtures, real provider samples, Bethesda assets, third-party mod files,
external tool fixtures, runtime probes, VS Code extension processes,
language-server processes, local tool package publication, or AI requirements
are added.

Run the full local suite serially:

```text
dotnet test WastelandForge.sln -c Release --no-build --no-restore -m:1
```
