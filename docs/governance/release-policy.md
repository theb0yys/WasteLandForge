# Release Policy

Status: Skeleton
Research classification: Documented
Source: R008 / ADR-011

Release behavior is gated and validation-first.

Required before release publish exists:

- schema validation,
- semantic validation,
- capability/environment validation,
- package validation,
- release verification,
- local `build-manifest.json`,
- checksums,
- governance checks,
- explicit human approval.

Gate 9 implements release dry-run behavior through `forge release verify`.

The command validates first, stages synthetic release evidence under project
`dist/`, writes Forge-owned `build-manifest.json`, writes `checksums.sha256`,
and emits release summary and validation reports.

Gate 11 adds release diagnostic projections through `forge release verify
--format github` and `forge release verify --summary <path>`. These projections
report diagnostics only; release evidence still lives under project `dist/`.

Public release publishing remains out of scope until later verification and
governance gates exist.

Gate 269 closes the current `forge release prepare` lane after local
release-plan, release-summary, build-manifest, checksum, staging-payload,
archive-plan, deterministic archive, and archive-evidence revalidation
outputs exist. Further release-prepare work is parked unless explicitly
reopened.

Gate 270 starts the no-publish `forge release publish` governance preflight.
It reports required local evidence, governance checks, and explicit
human-approval requirements. Normal execution refuses publish with exit code
6; `--dry-run` reports the same preflight with exit code 0. The gate still
does not read release artifacts, call remote repositories, upload releases,
sign or attest artifacts, execute external tools, run runtime probes, or use
AI.

Gate 271 adds local release-prepare evidence path discovery to the same
no-publish preflight. It reports expected, present, and missing artifact
counts plus per-artifact `missing` or `present-not-validated` status. It does
not parse or validate artifact contents, revalidate checksums, reopen
archives, call remote repositories, upload releases, sign or attest artifacts,
execute external tools, run runtime probes, or use AI.

Gate 272 adds local release-prepare evidence content-shape classification.
JSON evidence is parsed only for well-formed object shape, checksum evidence
is parsed only for line shape, and the release archive remains unopened. It
does not semantically validate evidence, revalidate checksum digests, reopen
archives, call remote repositories, upload releases, sign or attest artifacts,
execute external tools, run runtime probes, or use AI.

Gate 273 adds checksum sidecar entry classification to the same no-publish
preflight. It parses `checksums.sha256` entries for expected-path coverage,
unexpected paths, duplicate paths, and malformed entry shape only. It does not
open referenced files, recompute checksum digests, semantically validate
evidence, reopen archives, call remote repositories, upload releases, sign or
attest artifacts, execute external tools, run runtime probes, or use AI.

Gate 274 adds build-manifest output cross-reference to the same no-publish
preflight. It parses `build-manifest.json` output paths and compares them to
expected release-prepare paths, local evidence paths, and checksum sidecar
paths only. It does not open referenced files, recompute checksum or
build-manifest digests, semantically validate evidence, reopen archives, call
remote repositories, upload releases, sign or attest artifacts, execute
external tools, run runtime probes, or use AI.

Gate 275 adds release-archive-evidence metadata cross-reference to the same
no-publish preflight. It parses `release-archive-evidence.json` output paths
and compares them to expected release-prepare paths, local evidence paths,
checksum sidecar paths, and build-manifest outputs only. It does not reopen
archives, recompute checksum, build-manifest, or release-archive-evidence
digests, semantically validate evidence, call remote repositories, upload
releases, sign or attest artifacts, execute external tools, run runtime
probes, or use AI.

Gate 276 adds checksum sidecar digest revalidation to the same no-publish
preflight. It recomputes SHA-256 for expected `checksums.sha256` entries whose
local release-prepare evidence files exist, reports matched, mismatched, and
missing-local-file checksum entries, and still refuses real publish. It does
not independently revalidate `build-manifest.json` or
`release-archive-evidence.json` digest metadata, semantically validate
evidence, reopen archives, call remote repositories, upload releases, sign or
attest artifacts, execute external tools, run runtime probes, or use AI.

Gate 277 adds build-manifest output digest revalidation to the same no-publish
preflight. It recomputes SHA-256 for expected `build-manifest.json` output
entries whose local release-prepare evidence files exist, reports matched and
mismatched build-manifest output digests, and still refuses real publish. It
does not independently revalidate `release-archive-evidence.json` archive
digest metadata, semantically validate evidence, reopen archives, call remote
repositories, upload releases, sign or attest artifacts, execute external
tools, run runtime probes, or use AI.

Gate 278 adds release-archive-evidence archive digest metadata revalidation to
the same no-publish preflight. It recomputes SHA-256 and length for the
expected local `dist/release-prepare/archives/release.zip` file and compares
those values to `release-archive-evidence.json` archive metadata. It still
refuses real publish and does not semantically validate evidence, reopen
archives, inspect archive entries, call remote repositories, upload releases,
sign or attest artifacts, execute external tools, run runtime probes, or use
AI.

Gate 279 adds release archive reopening/revalidation to the same no-publish
preflight. After lower-layer path, checksum, build-manifest, and archive
digest metadata checks are clean, it reopens the expected local
`dist/release-prepare/archives/release.zip` file and compares entry names,
entry order, deterministic timestamps, and stored compression metadata against
`release-archive-evidence.json`. It still refuses real publish and does not
semantically validate release evidence, validate archive payload contents,
call remote repositories, upload releases, sign or attest artifacts, execute
external tools, run runtime probes, or use AI.

Gate 280 adds semantic release-evidence validation to the same no-publish
preflight. After lower-layer evidence checks are clean, it validates local
release-prepare evidence kind/status contracts, project and output path maps,
release-summary counters, staging payload boundaries, archive-plan metadata
and inputs, archive-evidence checks, build-manifest output sets, and no-publish
execution flags. It still refuses real publish and does not validate archive
payload contents, call remote repositories, upload releases, sign or attest
artifacts, execute external tools, run runtime probes, or use AI.

Gate 281 adds local governance-check evaluation to the same no-publish
preflight. It evaluates immutable schema policy evidence, the Forge tool
SemVer version string, local workflow least-privilege permission evidence,
CODEOWNERS coverage for sensitive paths, redistributable fixture policy, and
AI-optional release correctness evidence. Missing policy files report
`missing-local-evidence`; matching files report `passed`; incomplete local
evidence reports `failed`. It still refuses real publish and does not call
remote repositories, upload releases, sign or attest artifacts, execute
external tools, run runtime probes, or use AI.

Gate 282 adds local schema-validation evidence evaluation to the same
no-publish preflight. It reads `dist/release-dry-run/validation.json` as prior
`forge validate --format json` evidence, checks the diagnostic report identity
and summary/issue shape, and reports whether any `WF-SCHEMA-*` diagnostics are
present. Missing validation reports stay `missing`, clean reports are
`complete-schema-validated`, and schema diagnostics report
`schema-diagnostics-present`. It still refuses real publish and does not call
remote repositories, upload releases, sign or attest artifacts, execute
external tools, run runtime probes, or use AI.

Gate 283 adds local capability/environment evidence evaluation to the same
no-publish preflight. It reads
`dist/release-dry-run/capabilities-scan.json` as prior
`forge capabilities scan --project --format json` evidence, checks the report
identity, project-scoped requirement summary, local-only scan flags, Doctor
summary shape, and `WF-CAP-*` diagnostic count. Missing scan reports stay
`missing`, clean project-scoped reports are
`complete-capability-environment-validated`, and required-unavailable
requirements or `WF-CAP-*` diagnostics report
`capability-diagnostics-present`. It still refuses real publish and does not
call remote repositories, upload releases, sign or attest artifacts, execute
external tools, run runtime probes, automate MO2 or GECK, mutate plugins, or
use AI.

Gate 284 adds local package-validation evidence evaluation to the same
no-publish preflight. It reads `dist/release-dry-run/package-verify.json` as
prior `forge package --target mcm-json --verify-existing --format json`
evidence, checks the report identity, `dist/` output scope, summary/output
shape, and `WF-BUILD-*` diagnostic count. Missing package reports stay
`missing`, clean verifier reports are `complete-package-validated`, and
package verifier diagnostics report `package-diagnostics-present`. It still
refuses real publish and does not call remote repositories, upload releases,
sign or attest artifacts, execute external tools, run runtime probes, automate
MO2 or GECK, mutate plugins, or use AI.

Gate 285 adds local release-verification evidence evaluation to the same
no-publish preflight. It reads `dist/release-dry-run/release-verify.json` as
prior `forge release verify --format json` evidence, checks the report
identity, dry-run flag, optional `dist/` output scope, summary/output shape,
and `WF-REL-*` diagnostic count. Missing release-verify reports stay
`missing`, clean verifier reports are `complete-release-verified`, and
release verifier diagnostics report `release-diagnostics-present`. It still
refuses real publish and does not call remote repositories, upload releases,
sign or attest artifacts, execute external tools, run runtime probes, automate
MO2 or GECK, mutate plugins, or use AI.

Gate 286 adds explicit human-approval evaluation to the same no-publish
preflight. It accepts `--yes --confirm <project-id>`, reads the root
`wastelandforge.json`, `wastelandforge.yaml`, or `wastelandforge.yml`
manifest ID when approval is attempted, and reports whether the confirmation
matches. Missing, incomplete, missing-manifest, multiple-manifest,
unreadable-manifest, missing-ID, invalid-ID, and mismatched-ID approval states
are reported in the command output. Matching approval reports `provided`, but
normal execution still refuses real publish with exit code 6. It still does
not call remote repositories, upload releases, sign or attest artifacts,
execute external tools, run runtime probes, automate MO2 or GECK, mutate
plugins, or use AI.

Gate 287 adds publish-readiness aggregation to the same no-publish preflight.
It derives `publishReadiness` from required local evidence, governance checks,
and explicit approval, reports satisfied and blocking checks, and separately
reports that publish execution remains disabled. Local preconditions can be
satisfied, but `readyForRealPublish` remains false until a later gate enables
actual publication. It still does not call remote repositories, upload
releases, sign or attest artifacts, execute external tools, run runtime probes,
automate MO2 or GECK, mutate plugins, or use AI.

Gate 288 closes the `forge release publish` no-publish lane without enabling
publication. It reports `laneCloseout` metadata, keeps release publication,
remote repository calls, release uploads, signing, attestation, external tool
execution, plugin mutation, MO2/GECK automation, runtime probes, and AI
disabled, and routes the next local value slice to `forge doctor export`
release-readiness handoff work.

Gate 289 adds that local release-readiness handoff to `forge doctor export`.
It reuses the existing `forge release publish <project-root> --dry-run --format json --no-input`
preflight planner when a project root is supplied and exposes the aggregate
status in JSON, plain, Markdown, and bundle archive indexes. It does not
publish releases, call remote repositories, upload assets, sign or attest
artifacts, execute external tools, mutate plugins, automate MO2 or GECK, run
runtime probes, or use AI.

Gate 290 integrates local release-readiness blockers into Doctor export
triage and worklists. Blocking release-readiness checks are reported as
Doctor triage blockers and one local work item per blocking check, with
command hints limited to `forge release publish --dry-run` preflight forms.
It does not publish releases, call remote repositories, upload assets, sign
or attest artifacts, execute external tools, mutate plugins, automate MO2 or
GECK, run runtime probes, or use AI.

Gate 291 closes the current local Doctor export release-readiness lane and
parks further Doctor release-readiness edge cases unless explicitly reopened.
The next implementation slice routes back to local `forge build --target
reports` build-plan/report-index evidence. It does not publish releases, call
remote repositories, upload assets, sign or attest artifacts, execute external
tools, mutate plugins, automate MO2 or GECK, run runtime probes, or use AI.

Gate 292 adds build-plan and build-report-index evidence to
`forge build --target reports`. This is local build evidence only. It does not
publish releases, call remote repositories, upload assets, sign or attest
artifacts, execute external tools, mutate plugins, automate MO2 or GECK, run
runtime probes, or use AI.

Gate 293 adds build-plan and build-report-index Markdown summaries to
`forge build --target reports`. This is local human-readable build evidence
only. It does not publish releases, call remote repositories, upload assets,
sign or attest artifacts, execute external tools, mutate plugins, automate MO2
or GECK, run runtime probes, or use AI.

Gate 294 closes the local `forge build --target reports` build-evidence lane
and routes the next implementation slice to local `forge package --target
reports` package planning/staging evidence. It does not publish releases, call
remote repositories, upload assets, sign or attest artifacts, execute external
tools, mutate plugins, automate MO2 or GECK, run runtime probes, execute
package behavior, or use AI.

Gate 295 implements local `forge package --target reports` package-plan and
staging-layout evidence under `dist/reports-package`. This is local package
planning evidence only. It does not copy package inputs, create archives,
publish releases, call remote repositories, upload assets, sign or attest
artifacts, execute external tools, mutate plugins, automate MO2 or GECK, run
runtime probes, or use AI.

Gate 296 adds local present/missing input discovery to
`forge package --target reports`. This is package planning evidence only. It
does not read package input contents, copy package inputs, create archives,
publish releases, call remote repositories, upload assets, sign or attest
artifacts, execute external tools, mutate plugins, automate MO2 or GECK, run
runtime probes, or use AI.

Gate 297 adds local staging-copy behavior to `forge package --target
reports`. This is still local package evidence only: expected present
`dist/build` report files are copied into
`dist/reports-package/staging/reports`, missing expected inputs are skipped,
and staged payloads are included in local package manifest/checksum evidence.
It does not create archives, publish releases, call remote repositories,
upload assets, sign or attest artifacts, execute external tools, mutate
plugins, automate MO2 or GECK, run runtime probes, or use AI.

Gate 298 adds deterministic local archive creation to `forge package --target
reports`. This is still local package evidence only: package runs create
`dist/reports-package/package.zip` from package plan/layout evidence and
staged `reports/*` payload entries, then cover the archive in local
build-manifest/checksum evidence. It does not publish releases, call remote
repositories, upload assets, sign or attest artifacts, execute external
tools, mutate plugins, automate MO2 or GECK, run runtime probes, or use AI.

Gate 299 adds local archive evidence revalidation to `forge package --target
reports`. This is still local package evidence only: package runs reopen
`dist/reports-package/package.zip`, write
`dist/reports-package/package-archive-evidence.json`, and cover that sidecar
in local build-manifest/checksum evidence. It does not publish releases, call
remote repositories, upload assets, sign or attest artifacts, execute external
tools, mutate plugins, automate MO2 or GECK, run runtime probes, or use AI.

Gate 300 closes the current reports package lane and parks further reports
package edge cases unless explicitly reopened. The next local value route is
`forge release verify` self-report evidence under
`dist/release-dry-run/release-verify.json`, so existing release-publish
preflight release-verification evidence checks have a first-class local
producer. It does not add `reports` package verify-existing behavior, publish
releases, call remote repositories, upload assets, sign or attest artifacts,
execute external tools, mutate plugins, automate MO2 or GECK, run runtime
probes, or use AI.
