# WastelandForge

WastelandForge is a research-bound developer platform for Fallout: New Vegas content workflows.

The project is currently in gated v0.1 implementation. Gate 144 adds a compact
Doctor readiness index to `forge capabilities scan`.

## Architecture Spine

- ADR-006: WastelandForge is a hybrid capability platform.
- ADR-007: Source truth is YAML/JSON contracts normalized to canonical JSON.
- ADR-008: Dependencies are capabilities satisfied by providers.
- ADR-009: Outputs are generated through a deterministic, capability-aware build graph.
- ADR-010: Developers use Forge through a stable offline-first CLI.
- ADR-011: Validation, testing, CI, release, and governance are layered and validation-first.

## Current Gate

Gate 144 builds on the closed first game-facing generator path. It keeps
the built-in Fallout: New Vegas capability/provider catalogue from Gate 57,
the path-based `forge capabilities scan` evidence from Gate 58, the
`forge capabilities explain <capability-or-provider-id>` command from Gate 59,
and `forge capabilities scan --project <path>` from Gate 60. Project scans read
declared dependency capabilities and resolve them against built-in catalogue
and scan evidence as `satisfied`, `missing`, `unknown`, or `wrong-scope`.

Gate 127 adds a derived `doctor` section to capability scan output and
target-level next actions to `forge capabilities explain`. Gate 128 adds
`forge doctor export`, which writes a redacted local handoff bundle containing
that capability scan and Doctor readiness data. Local game, data, tool,
project, and provider evidence paths are replaced with placeholders. The
export remains offline-first and AI-optional. Gate 129 projects unavailable
project capability requirements into canonical `WF-CAP-*` diagnostics, with
JSON, SARIF, and GitHub annotation output for `forge capabilities scan`.
Gate 130 threads the same provider evidence into project requirement JSON,
diagnostic JSON evidence, SARIF result properties, GitHub annotations, and
redacted Doctor export bundles.
Gate 131 adds grouped provider evidence to `forge capabilities explain`, so
capability and provider explanations now surface provider status, install
scope, detector evidence, and next actions in one grouped section.
Gate 132 adds `wrong-scope` propagation for deterministic root-vs-Data marker
checks and projects wrong-scope project requirements as `WF-CAP-004` in JSON,
SARIF, GitHub annotation, and text diagnostic output.
Gate 133 lets `forge capabilities explain --project <path>` include matching
declared project requirement context, including source pointer, optional/phase
metadata, reason, resolution status, provider statuses, and resolver message.
Gate 134 adds diagnostic handoff metadata to the same explanation output so
unavailable matching requirements show the `WF-CAP-*` rule, severity, title,
source location, fix text, and evidence that `forge capabilities scan
--project` would project.
Gate 135 adds top-level `summary` and `index` sections to redacted
`forge doctor export` bundles, so authors can scan provider/capability counts,
Doctor readiness areas, requirement counts, diagnostic counts, open questions,
and Doctor area actions before opening the nested capability scan report.
Gate 136 adds `index.diagnostics` to the same redacted bundle. It lists
already-projected `WF-CAP-*` issue IDs, severity, title, source file, source
pointer, and fix text at the top level so unavailable project requirements can
be found without opening the nested capability diagnostics report.
Gate 137 adds `index.requirements` to the same redacted bundle. It lists
unavailable project capability requirement IDs, optional flags, phases,
statuses, source files, source pointers, and resolver messages at the top
level so authors can scan declared requirement failures without opening the
nested project requirement report.
Gate 138 adds `index.actions` to the same redacted bundle. It groups non-ready
Doctor actions by area and derived source type, so authors can scan the next
local steps before opening nested Doctor area details.
Gate 139 adds `index.openQuestionDetails` while preserving the existing
`index.openQuestions` string list. It gives current catalogue policy gaps
stable IDs and a `catalogue-policy` source type without resolving those gaps.
Gate 140 adds `index.providerStatuses` to the same redacted bundle. It groups
existing provider IDs by scan status and install scope, with counts and stable
provider ordering, so authors can scan provider readiness without opening the
nested provider list.
Gate 141 adds `index.capabilityStatuses` to the same redacted bundle. It
groups existing capability IDs by scan status, with counts and stable
capability ordering, so authors can scan capability readiness without opening
the nested capability list.
Gate 142 adds `index.doctorAreaStatuses` to the same redacted bundle. It
groups existing Doctor area IDs by readiness status, with counts and stable
area ordering, so authors can scan environment-readiness totals without
opening the full Doctor area list.
Gate 143 adds `index.cataloguePolicy` to the same redacted bundle. It groups
existing structured open-question details by source type, with counts and
stable question ID ordering, so authors can scan unresolved catalogue-policy
work without opening the full question text.
Gate 144 adds `doctor.index.areaStatuses` to `forge capabilities scan`. It
groups existing Doctor area IDs by readiness status, with counts and stable
area ordering, so authors can scan readiness totals directly from the scan
command before opening the full Doctor area list or exporting a Doctor bundle.

Gate 61 adds `forge generate --target reports` and `forge build --target
reports`. `forge generate` writes deterministic metadata reports under
`generated/reports`, including validation, dependency, capability,
generation-report, and generation-manifest JSON files. `forge build` writes
the same low-risk metadata reports under `dist/build`, plus a
`build-manifest.json` and `checksums.sha256`.

Gate 62 adds immutable manifest schema `0.2.0`, dependency schema `0.2.0`,
capability schema `0.2.0`, and MCM registry schema `0.1.0`. The new
`forge generate --target mcm-json` path reads manifest-declared MCM source
intent, requires a non-optional generation dependency on
`runtime.ui.mcm_json`, and writes deterministic MCM Extender JSON files under
`generated/mcm-json/MCM/`. `forge build --target mcm-json` writes the same
runtime-shaped output under `dist/mcm-json/` with a build manifest and
checksums.

Gate 63 records upstream MCM Extender README and wiki evidence for
`Data/MCM/<menu>.json` runtime menu files, adds the
`mcm-extender-output/0.1.0` output schema, and validates generated MCM JSON
before writing files.

Gate 64 adds source-controlled MCM Extender runtime `requirements` arrays and
menu translation maps. Generated output now includes
`MCM/Translations/<modName>.ini` when translations are declared, and build
manifests/checksums include those files.

Gate 65 adds source-controlled `checkbox` and `stringToggle` setting types.
Generated MCM Extender JSON now emits checkbox options as type `5` and string
toggle options as type `6`, including optional `textOn` and `textOff` labels
when the source registry declares them.

Gate 66 adds source-controlled `keybind` settings. Generated MCM Extender JSON
now emits keybind options as type `3` with INI-backed variables for DirectX
scancode defaults.

Gate 67 adds source-controlled `header` settings. Generated MCM Extender JSON
now emits non-interactive header options as type `0` with translated titles;

Gate 68 adds source-controlled `image` settings. Generated MCM Extender JSON
now emits type `0` image options with `filename`, `width`, `height`,
`systemcolor`, and optional offset fields.

Gate 69 validates source-authored MCM image filenames as game-relative `.dds`
paths that resolve to required texture asset targets. Existing asset
validation then checks the declared source file exists and starts with a DDS
magic header.

Gate 70 stages referenced MCM image texture assets as loose files under their
game-relative target paths. `forge generate --target mcm-json` writes those
assets under `generated/mcm-json/`; `forge build --target mcm-json` writes
them under `dist/mcm-json/`. Outputs, manifests, output digests, and build
checksums now include those staged asset paths.

Gate 71 adds `package-manifest.json` beside those MCM outputs. The package
manifest records the loose-file package root, package layout, menu,
translation, and asset entries, and payload digests. Build manifests and build
checksums include the package manifest as generated evidence.

Gate 72 adds deterministic ZIP archive creation for
`forge build --target mcm-json`. Build output now includes
`dist/mcm-json/package.zip`, the package manifest records its digest, and build
manifests/checksums include the archive. ZIP entries are sorted and use
normalized timestamps.

Gate 73 exposes the same deterministic MCM Extender package tree and ZIP
evidence through the canonical `forge package` command. `forge package` defaults
to `--target mcm-json`, writes under project `dist/mcm-json`, records
`wastelandforge/package-mcm-json/v1` in the local build manifest, and supports
human/plain/json output plus `--dry-run`.

Gate 74 adds immutable package manifest schema
`package-manifest/0.1.0/schema.json`, validates generated
`package-manifest.json`, validates build/package ZIP entry names against the
deterministic package payload, and records `packageValidation` evidence in
generation/build manifests.

Gate 75 adds `install-preview.json` beside generated/package outputs. The
report lists Data-relative package entries, generated source files,
would-copy install paths, archive evidence, and explicit preview-only
limitations. It is generated evidence only; Forge still does not install into
Data or MO2.

Gate 76 adds immutable install-preview schema
`install-preview/0.1.0/schema.json`, validates generated
`install-preview.json` before writing it, and records that schema ID in local
generation/build manifest install-preview evidence.

Gate 77 adds `install-preview.md` beside the validated JSON report. The
summary lists the package root, preview-only install flags, archive status,
would-copy Data paths, declared asset sources, and preview limitations for
human review.

Gate 78 adds `package-verification.json` beside the package and install-preview
evidence. The report summarizes local package checks, package entry counts,
package/preview evidence files, archive status, and archive entry validation.
Generation/build manifests, output digests, CLI JSON output, human CLI output,
and build/package checksums include the report. It is local package evidence
only and does not claim installation, MO2 VFS visibility, or in-game runtime
visibility.

Gate 79 adds immutable package-verification schema
`package-verification/0.1.0/schema.json`, validates generated
`package-verification.json` before writing it, and records that schema ID in
local generation/build manifest package-verification evidence.

Gate 80 adds `package-verification.md` beside the validated JSON report. The
summary records the package root, command, entry counts, archive status,
schema-backed evidence checks, payload digest count, and local verification
limitations for human review. Generation/build manifests, output digests, CLI
JSON output, human CLI output, and build/package checksums include the
summary.

Gate 81 cross-checks package-verification evidence before final manifests and
checksums are written. The generated verification report and summary must
agree with the package manifest, install-preview report, payload digest count,
and optional archive evidence. Successful generation records
`packageVerification.crossChecks` in local manifests; mismatches are blocking
`WF-BUILD-006` diagnostics.

Gate 82 extracts those package-verification evidence checks into reusable
`McmPackageVerificationEvidenceValidator` code. The validator is covered by
targeted mismatch tests for package root, payload digest count, and Markdown
summary mismatches while keeping `forge generate`, `forge build`, and
`forge package` behavior unchanged.

Gate 83 adds `McmPackageVerificationEvidenceFileVerifier` and a file-based
request type. The verifier reads generated `package-manifest.json`,
`install-preview.json`, `package-verification.json`, and
`package-verification.md`, reconstructs payload/archive digest evidence from
the package manifest, and reuses the Gate 82 validator. It is internal
reusable code only; no new `forge` command or alias is introduced.

Gate 84 makes that file-based verifier recompute package payload SHA-256 and
length values from generated files on disk. Payload files that no longer match
`package-manifest.json` payload digest evidence produce blocking
`WF-BUILD-006` diagnostics. This remains an internal verifier step and does
not add a standalone command.

Gate 85 makes the same file-based verifier recompute package archive SHA-256
and length values for generated `package.zip` files when
`package-manifest.json` records a created archive. Archive files that no
longer match manifest archive digest evidence produce blocking
`WF-BUILD-006` diagnostics. This remains an internal verifier step and does
not add a standalone command.

Gate 86 makes the same file-based verifier open generated `package.zip` files
and compare their normalized file entry names against `package-manifest.json`
entries when the manifest records a created archive. Missing or undeclared ZIP
entries produce blocking `WF-BUILD-006` diagnostics. This remains an internal
verifier step and does not add a standalone command.

Gate 87 records the command-surface decision for making that internal verifier
public later: use `forge package --target mcm-json --verify-existing` under
the canonical `forge package` command. It rejects standalone verifier aliases
such as `forge verify-package` and `/forge verify-package`, and it does not
implement the flag yet.

Gate 88 implements `forge package --target mcm-json --verify-existing`. The
mode reads existing package evidence from `dist/mcm-json` by default, or from
a `--output <path>` root under project `dist/`, then runs the file-based
package-verification verifier. It emits human/plain/json output and returns
exit code `1` when blocking `WF-BUILD-006` package evidence diagnostics are
found. It does not regenerate package outputs.

Gate 89 adds `--format sarif` and `--format github` to that verify-existing
mode. SARIF uses the existing canonical SARIF 2.1.0 diagnostic projection, and
GitHub format emits workflow-command annotations and appends a Markdown summary
when `GITHUB_STEP_SUMMARY` is present. Normal `forge package --format sarif`
and `forge package --format github` remain rejected unless `--verify-existing`
is set.

Gate 90 adds `--summary <path>` to that verify-existing mode. The summary uses
the same canonical Markdown diagnostic projection as validation and release
verification. Normal `forge package --summary <path>` remains rejected unless
`--verify-existing` is set.

Gate 91 adds checksum-file revalidation to that verify-existing mode. The
file-based verifier now reads `checksums.sha256`, recomputes SHA-256 values
for listed package files, rejects checksum paths outside the package root, and
requires checksum entries for package evidence, payload files, created package
archives, and `build-manifest.json`. The same `WF-BUILD-006` diagnostic family
is used for blocking package evidence mismatches.

Gate 92 adds build-manifest content revalidation to that verify-existing mode.
The file-based verifier now reads `build-manifest.json`, checks core package
evidence fields against the package manifest, install-preview report, and
package-verification report, and recomputes build-manifest output digests for
package payload and evidence files.

Gate 93 adds install-preview summary content revalidation to that
verify-existing mode. The file-based verifier now reads `install-preview.md`
and checks header, provenance, package root, preview-only flags, archive
status, would-copy entries, declared sources, target files, and limitations
against `install-preview.json`.

Gate 94 adds install-preview/package-manifest entry content cross-checking to
that verify-existing mode. The file-based verifier now compares
`package-manifest.json` entries with `install-preview.json` entries by package
kind, ID, and path, then checks source files, install paths, media types,
actions, declared asset sources, and target files without regenerating package
outputs.

Gate 95 adds package-verification summary content revalidation to that
verify-existing mode. The reusable package-verification evidence validator now
checks `package-verification.md` header, provenance, project, command, target,
package counts, archive state, check lines, and limitations against
`package-verification.json` and computed package evidence without regenerating
package outputs.

Gate 96 adds package-verification JSON check content revalidation to that
verify-existing mode. The reusable package-verification evidence validator now
checks generated `package-verification.json` check objects for expected
schema-check statuses, evidence paths, install-preview summary evidence,
payload digest status, payload digest count, and package archive
status/validation against package evidence without regenerating package
outputs.

Gate 97 adds package-verification JSON metadata content revalidation to that
verify-existing mode. The reusable package-verification evidence validator now
checks `package-verification.json` format, kind, verification type, command,
target, dry-run flag, project ID, package type, layout, package counts,
result, and local-only limitations against package manifest, install-preview,
and expected generated package evidence without regenerating package outputs.

Gate 98 adds package-verification archive detail content revalidation to that
verify-existing mode. The reusable package-verification evidence validator now
checks no-archive reason text and created archive media type, compression,
SHA-256, and length recorded in `package-verification.json` against generated
package evidence without regenerating package outputs.

Gate 99 adds install-preview archive detail content revalidation to that
verify-existing mode. The reusable package-verification evidence validator now
checks no-archive reason text and created archive media type, compression,
SHA-256, and length recorded in `install-preview.json` against generated
package evidence without regenerating package outputs.

Gate 100 adds package-manifest archive detail content revalidation to that
verify-existing mode. The reusable package-verification evidence validator now
checks no-archive reason text and created archive media type and compression
recorded in `package-manifest.json` against generated package evidence without
regenerating package outputs. Package-manifest archive SHA-256 and length
continue to be covered by the existing archive digest recomputation check.

Gate 101 adds archive detail cross-report consistency revalidation to that
verify-existing mode. When `package-manifest.json` archive digest evidence is
already stale against the actual archive, the reusable validator now also
checks SHA-256 and length agreement across `package-manifest.json`,
`install-preview.json`, and `package-verification.json` without regenerating
package outputs.

Gate 102 adds package archive presence revalidation to that verify-existing
mode. If `package.zip` exists beside `package-manifest.json` while the package
evidence records archive status `not-created`, the file-based verifier emits a
blocking `WF-BUILD-006` diagnostic without regenerating package outputs.

Gate 103 adds checksum unexpected-entry revalidation to that verify-existing
mode. If `checksums.sha256` records a valid file entry that is not declared by
package evidence, the file-based verifier emits a blocking `WF-BUILD-006`
diagnostic without regenerating package outputs.

Gate 104 adds checksum duplicate-entry revalidation to that verify-existing
mode. If `checksums.sha256` records the same normalized package-root-relative
file entry more than once, the file-based verifier emits a blocking
`WF-BUILD-006` diagnostic without regenerating package outputs.

Gate 105 adds checksum canonical-order revalidation to that verify-existing
mode. If expected package evidence entries in `checksums.sha256` are not sorted
by normalized package-root-relative path, the file-based verifier emits a
blocking `WF-BUILD-006` diagnostic without regenerating package outputs.

Gate 106 adds checksum digest canonical-casing revalidation to that
verify-existing mode. If expected package evidence entries in
`checksums.sha256` use uppercase SHA-256 hex, the file-based verifier emits a
blocking `WF-BUILD-006` diagnostic without regenerating package outputs.

Gate 107 adds checksum line-ending and trailing-newline revalidation to that
verify-existing mode. If `checksums.sha256` is missing its final newline or
uses non-canonical line endings for the current Forge-generated checksum
format, the file-based verifier emits blocking `WF-BUILD-006` diagnostics
without regenerating package outputs.

Gate 108 adds checksum path separator canonicalization revalidation to that
verify-existing mode. If an expected package evidence entry in
`checksums.sha256` uses backslash separators instead of Forge-generated `/`
package-root-relative paths, the file-based verifier emits a blocking
`WF-BUILD-006` diagnostic without regenerating package outputs.

Gate 109 adds checksum blank-line revalidation to that verify-existing mode.
If `checksums.sha256` contains blank or whitespace-only rows, the file-based
verifier emits a blocking `WF-BUILD-006` diagnostic without regenerating
package outputs.

Gate 110 adds checksum entry spacing canonicalization revalidation to that
verify-existing mode. If an expected `checksums.sha256` entry does not use
exactly two spaces between the digest and path, or has leading/trailing path
whitespace, the file-based verifier emits a blocking `WF-BUILD-006` diagnostic
without regenerating package outputs.

Gate 111 adds checksum path casing canonicalization revalidation to that
verify-existing mode. If an expected `checksums.sha256` entry matches package
evidence only case-insensitively, the file-based verifier emits a blocking
`WF-BUILD-006` diagnostic without also reporting missing or unexpected checksum
entries.

Gate 112 adds checksum case-insensitive duplicate revalidation to that
verify-existing mode. If two `checksums.sha256` entries record the same
normalized package-root-relative path ignoring case, the file-based verifier
emits one blocking `WF-BUILD-006` duplicate-entry diagnostic without also
reporting path-casing, missing, or unexpected checksum entries for the later
duplicate row.

Gate 113 adds checksum malformed-entry format revalidation to that
verify-existing mode. If an expected `checksums.sha256` row has a malformed
digest or entry shape but still names a recognizable package-root-relative path,
the file-based verifier emits one blocking `WF-BUILD-006` malformed-entry
diagnostic without also reporting the same path as missing.

Gate 114 adds checksum path containment revalidation to that verify-existing
mode. If a `checksums.sha256` row uses parent-directory traversal or another
path form that escapes the package root, the file-based verifier emits one
blocking `WF-BUILD-006` path-containment diagnostic without also reporting the
same recognizable expected path as missing.

Gate 115 adds checksum comment-line rejection revalidation to that
verify-existing mode. If `checksums.sha256` contains a `#` comment line, the
file-based verifier emits one blocking `WF-BUILD-006` comment-line diagnostic
instead of treating the row as a generic malformed checksum entry.

Gate 116 adds `install-plan.json` and `install-plan.md` to the MCM Extender
generate/build/package output set. `install-plan.json` is validated against
`install-plan/0.1.0`, recorded in generation/build manifests, included in
output digests and distribution checksums, and surfaced in CLI JSON output. It
records Data-relative copy intent with `requiresManualApproval: true`,
`writesToGameData: false`, `writesToMo2Profile: false`, and
`launchesGame: false`.

Gate 117 extends `forge package --target mcm-json --verify-existing` so the
file-based verifier reads `install-plan.json` and `install-plan.md`, checks
install-plan metadata, archive evidence, package-manifest entry consistency,
required copy actions, manual-approval/non-mutation flags, and Markdown
summary content, and reports stale evidence as blocking `WF-BUILD-006`
diagnostics without regenerating outputs.

Gate 118 extends the same verify-existing path so existing `install-plan.json`
is revalidated against the embedded `install-plan/0.1.0` schema before deeper
install-plan content checks. Schema failures are reported as blocking
`WF-BUILD-006` diagnostics, coalesced to one install-plan schema diagnostic
per invalid install-plan document, and do not regenerate outputs or install
files.

Gate 119 extends the same verify-existing path so existing
`package-manifest.json` is revalidated against the embedded
`package-manifest/0.1.0` schema before package-manifest-derived checks.
Schema failures are reported as blocking `WF-BUILD-006` diagnostics,
coalesced to one package-manifest schema diagnostic per invalid manifest, and
dependent package evidence checks are skipped to avoid cascades.

Gate 120 extends the same verify-existing path so existing
`install-preview.json` is revalidated against the embedded
`install-preview/0.1.0` schema before install-preview-derived checks. Schema
failures are reported as blocking `WF-BUILD-006` diagnostics, coalesced to one
install-preview schema diagnostic per invalid install preview, and dependent
package evidence checks are skipped to avoid cascades.

Gate 121 extends the same verify-existing path so existing
`package-verification.json` is revalidated against the embedded
`package-verification/0.1.0` schema before package-verification-derived
checks. Schema failures are reported as blocking `WF-BUILD-006` diagnostics,
coalesced to one package-verification schema diagnostic per invalid package
verification report, and dependent package evidence checks are skipped to
avoid cascades.

Gate 122 adds focused SARIF, GitHub annotation, and Markdown diagnostic summary
coverage for schema-gated verify-existing failures. It proves those existing
diagnostic projections carry the same `WF-BUILD-006` package-verification
schema diagnostic without adding new command behavior.

Gate 123 adds explicit missing required evidence diagnostics to the same
verify-existing path. If a required package evidence JSON or Markdown file is
absent, the file-based verifier now reports a blocking `WF-BUILD-006`
`MCM package ... evidence is missing` diagnostic at the expected evidence path
before deeper package evidence checks run.

Gate 124 adds explicit malformed evidence diagnostics to the same
verify-existing path. If a required JSON evidence file cannot be parsed, Forge
reports `MCM package ... evidence is malformed JSON`; if it parses but is not
a JSON object, Forge reports `MCM package ... evidence is not a JSON object`.
Both remain blocking `WF-BUILD-006` diagnostics at the evidence path.

Gate 125 adds focused SARIF, GitHub annotation, and Markdown diagnostic summary
coverage for malformed package-verification JSON evidence. It proves the
existing diagnostic projections carry the malformed JSON `WF-BUILD-006`
diagnostic without adding new command behavior.

Gate 126 records the MCM Extender lane as parked after Gates 62-125. The
implemented slice remains available through `forge generate --target mcm-json`,
`forge build --target mcm-json`, `forge package --target mcm-json`, and
`forge package --target mcm-json --verify-existing`, but the next development
lane moves to capability scanner and Doctor-style environment value instead of
continuing MCM verifier micro-gates.

Gate 127 implements that capability/Doctor value through existing
`forge capabilities scan` and `forge capabilities explain` commands. Gate 128
implements the canonical `forge doctor export` command for a redacted local
handoff bundle without adding new aliases.

The scanner and explainer currently cover path evidence for the game root,
xNVSE, JIP LN, JohnnyGuitar, ShowOff, UIO, MCM, MCM Extender JSON, kNVSE,
GECK, Hot Reload, xEdit, and MO2. JIP PP LN and GECK Extender remain
`unknown` in this gate because their safe file-marker policy remains open.

Gate 126 does not inspect an MO2 profile, launch through MO2 VFS, probe a
runtime, parse provider versions, emit new `WF-CAP-*` diagnostics, generate
in-game-verified MCM Extender files, generate JIP text scripts, package
archives as FOMOD installers, or compile plugin records. Remaining advanced
MCM Extender option types, FOMOD package creation,
capability-to-runtime requirement inference, callbacks, non-object diagnostic
projection coverage, and in-game runtime verification remain deferred unless a
later gate explicitly reopens the MCM lane.

Gate 127 still does not inspect MO2 profiles, launch through MO2 VFS, probe a
runtime session, parse provider versions, emit `WF-CAP-*` diagnostics, resolve
GECK Extender safe markers, or resolve JIP PP LN aliases. It summarizes those
limits as open capability questions where relevant.

Gate 128 still does not inspect MO2 profiles, launch through MO2 VFS, probe a
runtime session, parse provider versions, emit `WF-CAP-*` diagnostics, resolve
GECK Extender safe markers, resolve JIP PP LN aliases, call AI, or create a
Doctor bundle archive. It exports the current local evidence only.

Gate 129 still does not inspect MO2 profiles, launch through MO2 VFS, probe a
runtime session, parse provider versions, resolve wrong-scope installs,
resolve GECK Extender safe markers, resolve JIP PP LN aliases, call AI, or
create Doctor SARIF/GitHub bundle mode. It projects the existing project
requirement statuses into `WF-CAP-001`, `WF-CAP-002`, and `WF-CAP-003`.

Gate 130 still does not inspect MO2 profiles, launch through MO2 VFS, probe a
runtime session, parse provider versions, resolve wrong-scope installs,
resolve GECK Extender safe markers, resolve JIP PP LN aliases, call AI, or
create Doctor SARIF/GitHub bundle mode. It only enriches the existing
project requirement diagnostics with scan-derived provider evidence.

Gate 131 still does not inspect MO2 profiles, launch through MO2 VFS, probe a
runtime session, parse provider versions, resolve wrong-scope installs,
resolve GECK Extender safe markers, resolve JIP PP LN aliases, call AI, or
create Doctor SARIF/GitHub bundle mode. It only groups existing scan-derived
provider evidence inside `forge capabilities explain`.

Gate 132 still does not inspect MO2 profiles, launch through MO2 VFS, probe a
runtime session, parse provider versions, resolve mixed-scope GECK Extender
markers, resolve JIP PP LN aliases, call AI, or create Doctor SARIF/GitHub
bundle mode. It only detects root-vs-Data misplaced markers for existing
deterministic path detectors.

Gate 133 still does not inspect MO2 profiles, launch through MO2 VFS, probe a
runtime session, parse provider versions, resolve mixed-scope GECK Extender
markers, resolve JIP PP LN aliases, call AI, or create Doctor SARIF/GitHub
bundle mode. It only adds project requirement context to `capabilities
explain` using existing deterministic project loading and resolution.

Gate 134 still does not inspect MO2 profiles, launch through MO2 VFS, probe a
runtime session, parse provider versions, resolve mixed-scope GECK Extender
markers, resolve JIP PP LN aliases, call AI, add new `WF-CAP-*` rule IDs, or
create SARIF/GitHub output for `capabilities explain`. It only reuses the
existing scan diagnostic projector to display handoff context inside explain
output.

Gate 135 still does not inspect MO2 profiles, launch through MO2 VFS, probe a
runtime session, parse provider versions, resolve mixed-scope GECK Extender
markers, resolve JIP PP LN aliases, call AI, add new `WF-CAP-*` rule IDs, or
create SARIF/GitHub output for Doctor export. It only derives summary and
index data from the already-redacted capability scan report.

Gate 136 still does not inspect MO2 profiles, launch through MO2 VFS, probe a
runtime session, parse provider versions, resolve mixed-scope GECK Extender
markers, resolve JIP PP LN aliases, call AI, add new `WF-CAP-*` rule IDs, or
create SARIF/GitHub output for Doctor export. It only adds a compact top-level
diagnostics index derived from the already-redacted capability scan diagnostic
report.

Gate 137 still does not inspect MO2 profiles, launch through MO2 VFS, probe a
runtime session, parse provider versions, resolve mixed-scope GECK Extender
markers, resolve JIP PP LN aliases, call AI, add new `WF-CAP-*` rule IDs, or
create SARIF/GitHub output for Doctor export. It only adds a compact top-level
requirements index derived from the already-redacted project requirement
resolution report.

Gate 138 still does not inspect MO2 profiles, launch through MO2 VFS, probe a
runtime session, parse provider versions, resolve mixed-scope GECK Extender
markers, resolve JIP PP LN aliases, call AI, add new `WF-CAP-*` rule IDs, or
create SARIF/GitHub output for Doctor export. It only adds a compact top-level
action index derived from the already-redacted Doctor area actions.

Gate 139 still does not inspect MO2 profiles, launch through MO2 VFS, probe a
runtime session, parse provider versions, resolve mixed-scope GECK Extender
markers, resolve JIP PP LN aliases, call AI, add new `WF-CAP-*` rule IDs, or
create SARIF/GitHub output for Doctor export. It only adds structured
open-question details derived from existing Doctor open-question text.

Gate 140 still does not inspect MO2 profiles, launch through MO2 VFS, probe a
runtime session, parse provider versions, resolve mixed/effective scope,
resolve JIP PP LN aliases, call AI, add new `WF-CAP-*` rule IDs, or create
SARIF/GitHub output for Doctor export. It only groups existing scan-derived
provider statuses by install scope in the redacted handoff index.

Gate 141 still does not inspect MO2 profiles, launch through MO2 VFS, probe a
runtime session, parse provider versions, change capability resolution, call
AI, add new `WF-CAP-*` rule IDs, or create SARIF/GitHub output for Doctor
export. It only groups existing scan-derived capability statuses in the
redacted handoff index.

Gate 142 still does not inspect MO2 profiles, launch through MO2 VFS, probe a
runtime session, parse provider versions, change Doctor readiness planning,
call AI, add new `WF-CAP-*` rule IDs, or create SARIF/GitHub output for
Doctor export. It only groups existing Doctor area readiness statuses in the
redacted handoff index.

Gate 143 still does not inspect MO2 profiles, launch through MO2 VFS, probe a
runtime session, parse provider versions, resolve JIP PP LN aliases, resolve
mixed-scope GECK Extender markers, call AI, add new `WF-CAP-*` rule IDs, or
create SARIF/GitHub output for Doctor export. It only groups existing
open-question detail IDs by catalogue-policy source type in the redacted
handoff index.

Gate 144 still does not inspect MO2 profiles, launch through MO2 VFS, probe a
runtime session, parse provider versions, change capability resolution,
change Doctor readiness planning, call AI, add new `WF-CAP-*` rule IDs, or
change SARIF/GitHub output for capability scan. It only groups existing
Doctor area readiness statuses under the scan-side `doctor.index`.

The current response route baseline still includes:

- `WF-SEM-036` for response route `targetTopicId` values that do not resolve
  to declared dialogue topics when topics are declared,
- `WF-SEM-037` for response route target topics that are declared but have no
  authored dialogue line endpoint,
- `WF-SEM-038` for duplicate response route IDs authored on the same dialogue
  line,
- `WF-SEM-039` for duplicate response route keys authored on the same dialogue
  line,
- deterministic fixtures for a missing response route target topic and a
  missing response route target line endpoint, plus a duplicate response route
  identity fixture and duplicate response route key fixture,
- no new schema version; dialogue registry schema `0.23.0` remains current,
- no Gate 56 runtime behavior change; allowed response route taxonomy,
  selection behavior, Speech Challenge branching integration, and GECK/plugin
  output mapping remain open until an evidence pack is populated.

The existing gated baseline includes:

- optional `registries.quests` manifest wiring,
- immutable Draft 2020-12 manifest schemas for `0.1.0` and `0.2.0`,
- immutable Draft 2020-12 dependency registry schemas for `0.1.0` and
  `0.2.0`,
- immutable Draft 2020-12 capability registry schemas for `0.1.0` and
  `0.2.0`,
- immutable Draft 2020-12 MCM registry schema `0.1.0`,
- immutable Draft 2020-12 dialogue registry schemas for `0.1.0`, `0.2.0`,
  `0.3.0`, `0.4.0`, `0.5.0`, `0.6.0`, `0.7.0`, `0.8.0`, `0.9.0`,
  `0.10.0`, `0.11.0`, `0.12.0`, `0.13.0`, `0.14.0`, `0.15.0`, `0.16.0`,
  `0.17.0`, `0.18.0`, `0.19.0`, `0.20.0`, `0.21.0`, `0.22.0`, and
  `0.23.0`,
- immutable Draft 2020-12 quest registry schemas for `0.1.0`, `0.2.0`,
  `0.3.0`, `0.4.0`, `0.5.0`, and `0.6.0`,
- runtime dialogue registry schema validation,
- runtime quest registry schema validation,
- a valid synthetic `ExampleMod` dialogue registry with line-local
  quest-stage and quest-variable condition declarations plus dialogue
  result-script declarations, topic declarations, `linkTo` and `linkFrom`
  topic links, explicit priority, prompt route, Speech Challenge, skill gate,
  perk gate, faction gate, reputation gate, identity gate, local world flag
  gate, event history gate, companion state gate, and result-script
  side-effect gate declarations plus condition boolean composition
  declarations with nested child condition groups and explicit negated
  condition references plus authored precedence ranks and short-circuit
  intent plus a response route skeleton, and a quest-level dialogue gate
  declaration plus a dialogue result-script quest-variable increment
  declaration,
- a valid synthetic `ExampleMod` quest registry with stage and objective
  declarations plus transition, condition, and stage result-script
  declarations plus quest variable declarations,
- `WF-SEM-016` cross-registry validation from dialogue `questId` values to
  declared quest IDs,
- `WF-SEM-017` semantic validation for quest objectives whose stage references
  do not resolve to stages declared in the same quest,
- `WF-SEM-018` semantic validation for quest transitions whose stage references
  do not resolve to stages declared in the same quest,
- `WF-SEM-019` semantic validation for quest conditions whose stage references
  do not resolve to stages declared in the same quest,
- `WF-SEM-020` semantic validation for quest stage result scripts whose
  optional condition references do not resolve to conditions declared in the
  same quest,
- `WF-SEM-021` semantic validation for quest variable conditions whose
  variable references do not resolve to variables declared in the same quest,
- `WF-SEM-022` semantic validation for dialogue conditions whose quest stage
  references do not resolve inside the dialogue line's referenced quest,
- `WF-SEM-023` semantic validation for dialogue conditions whose quest
  variable references do not resolve inside the dialogue line's referenced
  quest,
- `WF-SEM-024` semantic validation for dialogue lines whose `topicId`
  references do not resolve to declared dialogue topics when topics are
  declared,
- `WF-SEM-025` semantic validation for dialogue `linkTo` target topic
  references that do not resolve to declared dialogue topics when topics are
  declared,
- `WF-SEM-026` semantic validation for dialogue quest-level gates whose
  `questId` references do not resolve to declared quest IDs,
- `WF-SEM-027` semantic validation for dialogue quest-level gate conditions
  whose quest stage references do not resolve inside the gate's referenced
  quest,
- `WF-SEM-028` semantic validation for dialogue quest-level gate conditions
  whose quest variable references do not resolve inside the gate's referenced
  quest,
- `WF-SEM-029` semantic validation for dialogue result-script variable
  mutations whose variable references do not resolve inside the dialogue
  line's referenced quest,
- `WF-SEM-030` semantic validation for dialogue `linkFrom` source topic
  references that do not resolve to declared dialogue topics when topics are
  declared,
- `WF-SEM-031` semantic validation for dialogue `linkTo` target topics that
  are declared but have no authored dialogue line endpoint,
- `WF-SEM-032` semantic validation for dialogue `linkFrom` source topics that
  are declared but have no authored dialogue line endpoint,
- `WF-SEM-033` semantic validation for duplicate authored prompt routes with
  the same `topicId`, `promptText`, and `priority`,
- `WF-SEM-034` semantic validation for dialogue condition logic entries whose
  `conditionIds` or `negatedConditionIds` references, including nested group
  references, do not resolve to conditions authored on the same dialogue line,
- `WF-SEM-035` semantic validation for duplicate root or nested dialogue
  condition logic IDs authored inside one dialogue line's condition logic tree,
- `WF-SEM-036` semantic validation for dialogue response route target topics
  that do not resolve to declared dialogue topics when topics are declared,
- `WF-SEM-037` semantic validation for dialogue response route target topics
  that are declared but have no authored dialogue line endpoint,
- `WF-SEM-038` semantic validation for duplicate response route IDs authored
  on the same dialogue line,
- `WF-SEM-039` semantic validation for duplicate response route keys authored
  on the same dialogue line,
- deterministic fixtures for invalid quest registry shape, invalid
  stage/objective shape, missing dialogue quest references, and missing quest
  objective stage references, plus invalid transition shape and missing
  transition stage references, invalid condition shape, and missing condition
  stage references, invalid result-script shape, and missing result-script
  condition references, invalid variable shape, and missing condition variable
  references, invalid dialogue condition shape, missing dialogue condition
  stage references, missing dialogue condition variable references, and invalid
  dialogue result-script shape, invalid dialogue topic shape, missing dialogue
  topic references, missing dialogue topic link references, invalid dialogue
  quest gate shape, missing dialogue quest gate references, missing dialogue
  quest gate stage references, missing dialogue quest gate variable references,
  invalid dialogue result-script mutation shape, and missing dialogue
  result-script mutation variable references, invalid dialogue Link From
  shape, missing dialogue Link From source topic references, missing dialogue
  Link To target line endpoints, and missing dialogue Link From source line
  endpoints, invalid dialogue prompt route shape, invalid dialogue Speech
  Challenge shape, invalid dialogue skill gate shape, invalid dialogue perk
  gate shape, invalid dialogue faction gate shape, invalid dialogue reputation
  gate shape, invalid dialogue identity gate shape, invalid dialogue local
  world flag gate shape, invalid dialogue event history gate shape, and
  invalid dialogue companion state gate shape, invalid dialogue result-script
  side-effect gate shape, invalid dialogue condition boolean composition
  shape, invalid dialogue nested condition group shape, missing dialogue
  condition logic references, missing nested dialogue condition logic
  references, invalid dialogue condition negation shape, missing negated
  dialogue condition logic references, invalid dialogue condition precedence
  shape, invalid dialogue condition short-circuit shape, duplicate dialogue
  condition logic identity, invalid dialogue response route shape, and
  missing dialogue response route target references, missing dialogue response
  route target line endpoints, duplicate dialogue response route identity,
  duplicate dialogue response route key, and duplicate dialogue prompt routes.

Gate 56 preserves the earlier asset, voice, dialogue registry, dialogue
voice worklist, and dialogue quest reference validation from Gates 14 through
55.

Gate 57 adds the first built-in FNV capability catalogue and the
`forge capabilities list` command. Gate 58 adds path-based
`forge capabilities scan` evidence. Gate 59 adds
`forge capabilities explain` over the built-in catalogue and scan evidence.
Gate 60 adds project requirement resolution to
`forge capabilities scan --project`. It preserves Gate 56 dialogue evidence
status unchanged.
Gate 61 adds deterministic metadata report generation and build manifest
output for `forge generate --target reports` and `forge build --target
reports`.
Gate 62 adds the first `mcm-json` generate/build target and source registry
contract while preserving Gate 61 report outputs.
Gate 63 replaces the Gate 62 placeholder MCM output with a minimal
runtime-shaped MCM Extender JSON subset, output schema validation, and the
upstream-documented `MCM/<menu>.json` staging path.
Gate 64 adds `MCM/Translations/<modName>.ini` output and pass-through runtime
requirements while preserving the same `forge generate` and `forge build`
command surface.
Gate 65 adds `checkbox` and `stringToggle` MCM source settings and emits
documented MCM Extender option types `5` and `6`.
Gate 66 adds `keybind` MCM source settings and emits documented MCM Extender
option type `3`.
Gate 67 adds `header` MCM source settings and emits documented MCM Extender
option type `0`.
Gate 68 adds `image` MCM source settings and emits documented MCM Extender
type `0` image maps.
Gate 69 validates MCM image filenames against required texture asset targets
and existing DDS source-file checks.
Gate 70 stages validated referenced MCM texture assets as loose files under
their game-relative target paths and records them in manifests, output
digests, and build checksums.
Gate 71 adds `package-manifest.json` for the `mcm-json` loose-file output
tree and records it in generation/build manifests, output digests, and build
checksums.
Gate 72 adds `dist/mcm-json/package.zip` for `forge build --target mcm-json`
and records it in the package manifest, build manifest, output digests, and
build checksums.
Gate 73 adds canonical `forge package --target mcm-json` execution for that
same deterministic MCM Extender package tree and records package-specific
provenance in `build-manifest.json`.
Gate 74 adds `package-manifest/0.1.0` package evidence validation, checks
build/package ZIP entries against the package payload, and records
`packageValidation` in generation/build manifests.
Gate 75 adds `install-preview.json` for the `mcm-json` output tree and records
that report in manifests, output digests, CLI JSON output, and build/package
checksums.
Gate 76 adds `install-preview/0.1.0` generated evidence validation and records
that schema in manifest install-preview evidence.
Gate 77 adds `install-preview.md` human summary evidence and records that
summary in manifest install-preview evidence, output digests, CLI JSON output,
and build/package checksums.
Gate 78 adds `package-verification.json` package evidence and records that
report in generation/build manifests, output digests, CLI JSON output, human
CLI output, and build/package checksums.
Gate 79 adds `package-verification/0.1.0` generated evidence validation and
records that schema in manifest package-verification evidence.
Gate 80 adds `package-verification.md` human summary evidence and records that
summary in manifest package-verification evidence, output digests, CLI JSON
output, human CLI output, and build/package checksums.
Gate 81 adds deterministic package-verification evidence cross-checks and
records passed cross-check status in manifest package-verification evidence.
Gate 82 extracts package-verification evidence checks into a reusable
validator and adds focused mismatch tests while preserving the same
generate/build/package command surface.
Gate 83 adds an internal file-based package-verification verifier and
generated-file tests while preserving the same generate/build/package command
surface.
Gate 84 adds package payload digest recomputation to that file-based verifier
and keeps the same generate/build/package command surface.
Gate 85 adds package archive digest recomputation to that file-based verifier
and keeps the same generate/build/package command surface.
Gate 86 adds package archive entry-name revalidation to that file-based
verifier and keeps the same generate/build/package command surface.
Gate 87 records `forge package --target mcm-json --verify-existing` as the
future command shape for existing package evidence verification, without
implementing the flag yet.
Gate 88 implements that verify-existing package command skeleton with
human/plain/json output and keeps the same generate/build/package command
surface.
Gate 89 adds SARIF and GitHub diagnostic projections to that verify-existing
mode and keeps normal package generation on human/plain/json output only.
Gate 90 adds Markdown diagnostic summary output to that verify-existing mode
and keeps normal package generation from accepting diagnostic summary files.
Gate 91 adds checksum-file revalidation to that verify-existing mode without
adding package-verifier behavior to normal package generation.
Gate 92 adds build-manifest content revalidation to that verify-existing mode
without adding package-verifier behavior to normal package generation.
Gate 93 adds install-preview summary content revalidation to that
verify-existing mode without adding package-verifier behavior to normal
package generation.
Gate 94 adds install-preview/package-manifest entry content cross-check
revalidation to that verify-existing mode without adding package-verifier
behavior to normal package generation.
Gate 95 adds package-verification summary content revalidation to that
verify-existing mode without adding package-verifier behavior to normal
package generation.
Gate 96 adds package-verification JSON check content revalidation to that
verify-existing mode without adding package-verifier behavior to normal
package generation.
Gate 97 adds package-verification JSON metadata content revalidation to that
verify-existing mode without adding package-verifier behavior to normal
package generation.
Gate 98 adds package-verification archive detail content revalidation to that
verify-existing mode without adding package-verifier behavior to normal
package generation.
Gate 99 adds install-preview archive detail content revalidation to that
verify-existing mode without adding package-verifier behavior to normal
package generation.
Gate 100 adds package-manifest archive detail content revalidation to that
verify-existing mode without adding package-verifier behavior to normal
package generation.
Gate 101 adds archive detail cross-report consistency revalidation to that
verify-existing mode without adding package-verifier behavior to normal
package generation.
Gate 102 adds package archive presence revalidation to that verify-existing
mode without adding package-verifier behavior to normal package generation.
Gate 103 adds checksum unexpected-entry revalidation to that verify-existing
mode without adding package-verifier behavior to normal package generation.
Gate 104 adds checksum duplicate-entry revalidation to that verify-existing
mode without adding package-verifier behavior to normal package generation.
Gate 105 adds checksum canonical-order revalidation to that verify-existing
mode without adding package-verifier behavior to normal package generation.
Gate 106 adds checksum digest canonical-casing revalidation to that
verify-existing mode without adding package-verifier behavior to normal package
generation.
Gate 107 adds checksum line-ending and trailing-newline revalidation to that
verify-existing mode without adding package-verifier behavior to normal package
generation.
Gate 108 adds checksum path separator canonicalization revalidation to that
verify-existing mode without adding package-verifier behavior to normal package
generation.
Gate 109 adds checksum blank-line revalidation to that verify-existing mode
without adding package-verifier behavior to normal package generation.
Gate 110 adds checksum entry spacing canonicalization revalidation to that
verify-existing mode without adding package-verifier behavior to normal package
generation.
Gate 111 adds checksum path casing canonicalization revalidation to that
verify-existing mode without adding package-verifier behavior to normal package
generation.
Gate 112 adds checksum case-insensitive duplicate revalidation to that
verify-existing mode without adding package-verifier behavior to normal package
generation.
Gate 113 adds checksum malformed-entry format revalidation to that
verify-existing mode without adding package-verifier behavior to normal package
generation.
Gate 114 adds checksum path containment revalidation to that verify-existing
mode without adding package-verifier behavior to normal package generation.
Gate 115 adds checksum comment-line rejection revalidation to that
verify-existing mode without adding package-verifier behavior to normal package
generation.

Gate 116 adds install-plan JSON and Markdown evidence to the MCM package
generate/build/package output set without installing files.
Gate 117 adds install-plan JSON and Markdown content revalidation to
`forge package --target mcm-json --verify-existing` without adding
package-verifier behavior to normal package generation.
Gate 118 adds install-plan schema revalidation to
`forge package --target mcm-json --verify-existing` without adding
package-verifier behavior to normal package generation.
Gate 119 adds package-manifest schema revalidation to
`forge package --target mcm-json --verify-existing` without adding
package-verifier behavior to normal package generation.
Gate 120 adds install-preview schema revalidation to
`forge package --target mcm-json --verify-existing` without adding
package-verifier behavior to normal package generation.
Gate 121 adds package-verification schema revalidation to
`forge package --target mcm-json --verify-existing` without adding
package-verifier behavior to normal package generation.
Gate 122 adds SARIF, GitHub annotation, and Markdown diagnostic summary
coverage for schema-gated verify-existing failures without adding
package-verifier behavior to normal package generation.
Gate 123 adds missing evidence-file diagnostics to
`forge package --target mcm-json --verify-existing` without adding
package-verifier behavior to normal package generation.
Gate 124 adds malformed evidence-file diagnostics to
`forge package --target mcm-json --verify-existing` without adding
package-verifier behavior to normal package generation.
Gate 125 adds SARIF, GitHub annotation, and Markdown diagnostic summary
coverage for malformed JSON evidence failures without adding package-verifier
behavior to normal package generation.
Gate 126 closes the current MCM Extender lane and moves next implementation
work to capability scanner and Doctor-style environment value instead of more
MCM verify-existing edge-case gates.
Gate 127 adds the Doctor-style capability readiness report and explain actions
without changing the canonical CLI command surface.
Gate 128 implements the canonical `forge doctor export` redacted local handoff
bundle without adding aliases, runtime probes, network calls, or AI.
Gate 129 adds the first canonical `WF-CAP-*` diagnostics for capability
requirement resolution and exposes them through JSON, SARIF, and GitHub
annotation projections for `forge capabilities scan`.
Gate 130 adds provider evidence detail to those diagnostics and preserves
redaction inside `forge doctor export`.
Gate 131 adds grouped provider evidence to `forge capabilities explain`
without adding new detector behavior.
Gate 132 adds wrong-scope capability diagnostics for deterministic root-vs-Data
marker evidence without adding runtime probes, MO2 VFS checks, provider
version checks, or new command names.
Gate 133 adds project requirement context to `forge capabilities explain`
without adding new detectors, diagnostics, aliases, runtime probes, MO2 VFS
checks, provider version checks, or external tool execution.
Gate 134 adds diagnostic handoff context to `forge capabilities explain`
without adding new detectors, rule IDs, aliases, runtime probes, MO2 VFS
checks, provider version checks, or external tool execution.
Gate 135 adds Doctor export summary/index sections without adding new
detectors, rule IDs, aliases, runtime probes, MO2 VFS checks, provider version
checks, or external tool execution.
Gate 136 adds a compact Doctor export diagnostics index without adding new
detectors, rule IDs, aliases, runtime probes, MO2 VFS checks, provider version
checks, SARIF/GitHub Doctor export modes, or external tool execution.
Gate 137 adds a compact Doctor export requirements index without adding new
detectors, rule IDs, aliases, runtime probes, MO2 VFS checks, provider version
checks, SARIF/GitHub Doctor export modes, or external tool execution.
Gate 138 adds a compact Doctor export action index without adding new
detectors, rule IDs, aliases, runtime probes, MO2 VFS checks, provider version
checks, SARIF/GitHub Doctor export modes, or external tool execution.
Gate 139 adds structured Doctor export open-question details without adding
new detectors, rule IDs, aliases, runtime probes, MO2 VFS checks, provider
version checks, SARIF/GitHub Doctor export modes, or external tool execution.
Gate 140 adds a compact Doctor export provider-status index without adding
new detectors, rule IDs, aliases, runtime probes, MO2 VFS checks, provider
version checks, SARIF/GitHub Doctor export modes, or external tool execution.
Gate 141 adds a compact Doctor export capability-status index without adding
new detectors, rule IDs, aliases, runtime probes, MO2 VFS checks, provider
version checks, SARIF/GitHub Doctor export modes, or external tool execution.
Gate 142 adds a compact Doctor export area-status index without adding new
detectors, rule IDs, aliases, runtime probes, MO2 VFS checks, provider version
checks, SARIF/GitHub Doctor export modes, or external tool execution.
Gate 143 adds a compact Doctor export catalogue-policy index without adding
new detectors, rule IDs, aliases, runtime probes, MO2 VFS checks, provider
version checks, SARIF/GitHub Doctor export modes, or external tool execution.
Gate 144 adds a compact capability scan Doctor readiness index without adding
new detectors, rule IDs, aliases, runtime probes, MO2 VFS checks, provider
version checks, SARIF/GitHub scan changes, or external tool execution.

It intentionally does not create:

- VS Code problem matchers,
- release publishing,
- FOMOD package creation,
- deep NIF, DDS, WAV, OGG, LIP, KF, RDT, or BSA validation,
- WAV/OGG sample-rate, bitrate, channel, or codec validation,
- validation that voice filenames correspond to dialogue records in a master file,
- GECK lip processing asset detection,
- full GECK dialogue condition language beyond quest stage and quest variable
  equality skeletons,
- full GECK condition language,
- dialogue quest gate execution, ordering, or compilation semantics,
- raw result-script bodies, full result-script mutation semantics, quest stage
  mutations, lockouts, or branch semantics,
- condition evaluation or result-script execution semantics,
- dialogue condition compilation into plugin records,
- dialogue result-script compilation into plugin records,
- external plugin record validation for quest or topic references,
- reciprocal Link To/Link From requirements,
- full dialogue graph traversal and cycle checks,
- exact GECK priority range, priority ordering, prompt routing execution,
  response route selection, response route route-key taxonomy, and
  condition-aware prompt selection,
- Speech Challenge success/failure routing, display behavior, exact threshold
  ranges, or execution semantics,
- exact skill taxonomy, skill threshold ranges, skill gate condition mapping,
  or skill gate execution semantics,
- exact perk taxonomy, perk registry resolution, perk condition mapping, or
  perk gate execution semantics,
- exact faction registry resolution, faction relation taxonomy, reputation
  standing taxonomy, or faction/reputation gate execution semantics,
- exact identity taxonomy, actor/player identity mapping, or identity gate
  execution semantics,
- exact local world flag taxonomy, world-state registry resolution,
  boolean/value modeling, scope semantics, GECK condition mapping, or local
  world flag gate execution semantics,
- exact event-history registry resolution, event signal taxonomy, lifecycle
  modeling, consumer routing, time horizon semantics, GECK condition mapping,
  or event history gate execution semantics,
- exact companion registry resolution, companion-specific observer model,
  companion state taxonomy, trust/history/trigger value semantics, GECK
  condition mapping, or companion state gate execution semantics,
- exact result-script side-effect registry resolution, effect taxonomy, raw
  script semantics, execution ordering, GECK condition mapping, or
  side-effect gate execution semantics,
- exact nested condition group evaluation semantics, negation execution
  semantics, precedence execution ordering, short-circuit execution behavior,
  GECK condition-list mapping, or condition composition execution semantics,
- global logical ID uniqueness across all registry object types,
- live GECK session control, GECK process automation, dialogue export import,
  GECK record parsing, or GECK record mutation,
- plugin record compilation,
- external tool-backed asset inspection,
- automatic provider discovery without explicit paths,
- runtime provider confirmation, MO2 VFS/profile inspection, provider version
  parsing, mixed/effective-scope diagnostics beyond root-vs-Data marker
  checks, and future `WF-CAP-*` diagnostics beyond `WF-CAP-001` through
  `WF-CAP-004`,
- in-game-verified MCM Extender output, FOMOD installers, release
  prepare/publish, binary plugin generation, JIP text scripts, or external
  tool execution.

Those belong to later gates recorded in `WasteLandForge/planning/`.

## Source and Output Boundaries

Canonical source belongs in version control. Generated and distribution outputs are disposable:

- `schemas/`, `src/`, `tests/`, `fixtures/`, and `docs/` are source or test inputs.
- `generated/` and `dist/` are output locations.
- `.wastelandforge/cache/`, `.wastelandforge/logs/`, and `.wastelandforge/tmp/` are local state.

## Correctness Rules

The core path must work offline and without AI:

- parse,
- normalize,
- validate,
- resolve capabilities,
- generate outputs,
- write build manifests,
- package release candidates.

AI may draft or explain, but AI output is not canonical truth.
