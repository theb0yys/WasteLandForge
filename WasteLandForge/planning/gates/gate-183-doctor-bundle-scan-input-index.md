# Gate 183 - Doctor Bundle Scan-Input Index

Status: Complete

## Purpose

Extend `forge doctor export --bundle <path>` so Doctor handoff archives include
deterministic scan-input index entries. The index gives humans and scripts a
direct redacted view of what local roots, tool-path placeholders, detector
families, and disabled runtime/MO2 flags shaped the capability scan without
opening the full Doctor export JSON.

## Research grounding

- Documented: R006/ADR-010 defines `forge doctor export` as a canonical
  offline-first CLI command and says CLI output should support humans and
  automation.
- Documented: R006 says JSON should be the primary local automation format and
  kept stable separately from human console output.
- Documented: R005/ADR-008 says capability detection is local-first and
  deterministic, with runtime probes used only as later enrichment.
- Documented: R008/ADR-011 says JSON is canonical, Markdown is a projection,
  fixture-backed tests should cover stable generated evidence, and local
  workflows must stay offline-first and AI-optional.
- Documented: Gate 128 implements Doctor export as a redacted local handoff
  bundle.
- Inferred: Adding bundle-level scan-input index files is safe because they
  derive only from already redacted `DoctorExportReport.Capabilities.Inputs`
  metadata.
- Open: Provider version parsing, runtime confirmation, MO2 effective
  visibility, JIP PP LN catalogue policy, and GECK Extender mixed-scope policy
  remain unresolved.

## Implemented

- `forge doctor export --bundle <path>` still writes deterministic ZIP
  archives.
- Every Doctor bundle now includes:
  - `scan-inputs/index.json`
  - `scan-inputs/index.md`
- The scan-input index includes redacted game root, data root, tool-path
  placeholders, detector families, runtime-probe flag, and MO2 VFS flag.
- The scan-input index is included in the bundle manifest and checksums file.
- `README.md` links to the scan-input index entries.
- Archive entry ordering and timestamps remain deterministic.
- Scan-input index entries use existing redacted metadata and do not expose
  raw local paths.

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
| Doctor bundles include deterministic scan-input index JSON and Markdown | Complete |
| Scan-input index derives from existing redacted Doctor export metadata | Complete |
| README points to scan-input index entries | Complete |
| Scan-input index entries are checksummed and listed in the manifest | Complete |
| Raw local paths are omitted from scan-input index payloads | Complete |
| Archive ordering and timestamps stay deterministic | Complete |
| Provider policy gaps remain open | Complete |

## Validation

- `dotnet build --no-restore` passed with 0 warnings and 0 errors.
- `dotnet test tests\WastelandForge.GoldenTests\WastelandForge.GoldenTests.csproj --no-build --no-restore --filter "FullyQualifiedName~DoctorExport"` passed.
- `dotnet test WastelandForge.sln --no-build --no-restore -m:1` passed.
- CLI smoke export inspected `scan-inputs/index.json`,
  `scan-inputs/index.md`, `README.md`, `checksums.sha256`, manifest entry
  count, scan-input index kind, redacted game/data/tool placeholders, detector
  family count, runtime/MO2 flags, README link, checksum entries, Markdown
  heading, and absence of the fixture project root path in inspected
  scan-input bundle payloads.
- `git diff --check` exited 0 with only Git LF-to-CRLF working-copy warnings.
- Touched-path protected scan returned no protected-file matches.

## Next gate

Gate 184 should continue documented capability/Doctor value from existing
redacted metadata without resolving open provider policy questions.
Provider-version evidence should only start when documented file, runtime,
parser, and catalogue-policy evidence is ready.
