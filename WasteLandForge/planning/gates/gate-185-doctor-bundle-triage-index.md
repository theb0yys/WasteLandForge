# Gate 185 - Doctor Bundle Triage Index

Status: Complete

## Purpose

Extend `forge doctor export --bundle <path>` so Doctor handoff archives include
deterministic triage index entries. The triage index gives humans and scripts a
direct redacted start-here view for blocking items, review items, next actions,
and recommended archive paths without resolving any open provider-policy
questions.

## Research grounding

- Documented: R006/ADR-010 defines `forge doctor export` as a canonical
  offline-first CLI command and says CLI output should support humans and
  automation.
- Documented: R006 says JSON should be the primary local automation format and
  kept stable separately from human console output.
- Documented: R008/ADR-011 says JSON is canonical, Markdown is a projection,
  fixture-backed tests should cover stable generated evidence, and local
  workflows must stay offline-first and AI-optional.
- Documented: Gate 128 implements Doctor export as a redacted local handoff
  bundle over capability scan evidence.
- Documented: Gate 129 projects unavailable project capability requirements
  into existing `WF-CAP-*` diagnostics.
- Documented: Gates 158 through 163 add existing summary metadata for Doctor
  actions, requirements, diagnostics, evidence, providers, and Doctor areas.
- Documented: Gates 166 through 184 add deterministic Doctor bundle archive
  entries, README navigation, archive-local indexes, manifests, checksums, and
  bundle navigation.
- Inferred: Adding bundle-level triage index files is safe because they derive
  only from already redacted Doctor summary/index metadata and archive paths.
- Open: Provider version parsing, runtime confirmation, MO2 effective
  visibility, JIP PP LN catalogue policy, and GECK Extender mixed-scope policy
  remain unresolved.

## Implemented

- `forge doctor export --bundle <path>` still writes deterministic ZIP
  archives.
- Every Doctor bundle now includes:
  - `triage/index.json`
  - `triage/index.md`
- The triage index classifies the handoff as `ready`, `review`, or `blocked`.
- The triage index lists blocking items, review items, next actions, and
  recommended archive review paths.
- The triage index is included in the bundle manifest and checksums file.
- `README.md` links to the triage index entries.
- `bundle/index.json` and `bundle/index.md` list the triage index entries.
- Requirement-explanation bundles include requirement-explanation review paths
  in the triage index.
- Archive entry ordering and timestamps remain deterministic.
- The triage index uses archive paths and redacted metadata only and does not
  expose raw local paths.

## Not implemented

- No new command alias.
- No `--format zip` or `--format markdown`.
- No Doctor export SARIF or GitHub mode.
- No GitHub step-summary behavior for Doctor export.
- No provider detector, runtime probe, resolver behavior, Doctor planning
  behavior, diagnostic rule ID, provider-version parser, MO2 VFS inspection,
  GECK automation, network check, release publishing, catalogue-policy
  decision, or AI behavior.

## Exit evidence

| Evidence | Status |
|---|---|
| Doctor bundles include deterministic triage index JSON and Markdown | Complete |
| Triage index derives from existing redacted Doctor metadata and archive paths | Complete |
| Triage index reports `ready`, `review`, or `blocked` status | Complete |
| Triage index entries are checksummed and listed in the manifest | Complete |
| README and bundle navigation point to triage index entries | Complete |
| Requirement-explanation review paths are listed when present | Complete |
| Raw local paths are omitted from triage index payloads | Complete |
| Archive ordering and timestamps stay deterministic | Complete |
| Provider policy gaps remain open | Complete |

## Validation

- `dotnet build --no-restore` passed with 0 warnings and 0 errors.
- `dotnet test tests\WastelandForge.GoldenTests\WastelandForge.GoldenTests.csproj --no-build --no-restore --filter "FullyQualifiedName~DoctorExport"` passed.
- `dotnet test WastelandForge.sln --no-build --no-restore -m:1` passed.
- CLI smoke export inspected `triage/index.json`, `triage/index.md`,
  `bundle/index.json`, `README.md`, `checksums.sha256`, manifest entry count,
  triage index kind, triage status, blocking/review/action counts,
  requirement-explanation review path coverage, bundle-index triage coverage,
  README link, checksum entries, and absence of the fixture project root path
  in inspected triage bundle payloads.
- `git diff --check` exited 0 with only Git LF-to-CRLF working-copy warnings.
- Touched-path protected scan returned no protected-file matches.

## Next gate

Gate 186 should continue documented capability/Doctor value from existing
redacted metadata without resolving open provider policy questions.
Provider-version evidence should only start when documented file, runtime,
parser, and catalogue-policy evidence is ready.
