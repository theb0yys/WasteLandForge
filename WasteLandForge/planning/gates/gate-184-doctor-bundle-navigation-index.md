# Gate 184 - Doctor Bundle Navigation Index

Status: Complete

## Purpose

Extend `forge doctor export --bundle <path>` so Doctor handoff archives include
deterministic bundle navigation index entries. The index gives humans and
scripts a direct list of the archive's redacted reports, supplemental indexes,
optional requirement explanations, manifest, and checksums without scraping
`README.md`.

## Research grounding

- Documented: R006/ADR-010 defines `forge doctor export` as a canonical
  offline-first CLI command and says CLI output should support humans and
  automation.
- Documented: R006 says JSON should be the primary local automation format and
  kept stable separately from human console output.
- Documented: R008/ADR-011 says JSON is canonical, Markdown is a projection,
  fixture-backed tests should cover stable generated evidence, and local
  workflows must stay offline-first and AI-optional.
- Documented: Gate 166 implements Doctor export bundles as deterministic
  redacted ZIP handoff archives with manifests and checksums.
- Documented: Gate 171 adds bundle README navigation.
- Documented: Gates 172 through 183 add deterministic archive-local index
  supplements derived from existing redacted Doctor metadata.
- Inferred: Adding a bundle-level navigation index is safe because it derives
  only from already redacted bundle metadata and archive supplement paths.
- Open: Provider version parsing, runtime confirmation, MO2 effective
  visibility, JIP PP LN catalogue policy, and GECK Extender mixed-scope policy
  remain unresolved.

## Implemented

- `forge doctor export --bundle <path>` still writes deterministic ZIP
  archives.
- Every Doctor bundle now includes:
  - `bundle/index.json`
  - `bundle/index.md`
- The bundle navigation index lists core Doctor reports, archive-local index
  supplements, optional per-requirement explanation entries, the bundle
  manifest, and the checksums file.
- The bundle navigation index is included in the bundle manifest and checksums
  file.
- `README.md` links to the bundle navigation index entries.
- Requirement-explanation bundles include those dynamic explanation paths in
  the bundle navigation index.
- Archive entry ordering and timestamps remain deterministic.
- The bundle navigation index uses archive paths and redacted metadata only and
  does not expose raw local paths.

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
| Doctor bundles include deterministic bundle navigation index JSON and Markdown | Complete |
| Bundle navigation index derives from existing redacted bundle metadata and archive paths | Complete |
| README points to bundle navigation index entries | Complete |
| Bundle navigation index entries are checksummed and listed in the manifest | Complete |
| Requirement-explanation archive entries are listed when present | Complete |
| Raw local paths are omitted from bundle navigation index payloads | Complete |
| Archive ordering and timestamps stay deterministic | Complete |
| Provider policy gaps remain open | Complete |

## Validation

- `dotnet build --no-restore` passed with 0 warnings and 0 errors.
- `dotnet test tests\WastelandForge.GoldenTests\WastelandForge.GoldenTests.csproj --no-build --no-restore --filter "FullyQualifiedName~DoctorExport"` passed.
- `dotnet test WastelandForge.sln --no-build --no-restore -m:1` passed.
- CLI smoke export inspected `bundle/index.json`, `bundle/index.md`,
  `README.md`, `checksums.sha256`, manifest entry count, bundle index kind,
  bundle index entry count, requirement-explanation path coverage, manifest
  path coverage, checksum path coverage, README link, checksum entries, and
  absence of the fixture project root path in inspected bundle navigation
  payloads.
- `git diff --check` exited 0 with only Git LF-to-CRLF working-copy warnings.
- Touched-path protected scan returned no protected-file matches.

## Next gate

Gate 185 should continue documented capability/Doctor value from existing
redacted metadata without resolving open provider policy questions.
Provider-version evidence should only start when documented file, runtime,
parser, and catalogue-policy evidence is ready.
