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
