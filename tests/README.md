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

Run the full local suite serially:

```text
dotnet test WastelandForge.sln -c Release --no-build --no-restore -m:1
```
