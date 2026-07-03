# Gate 182 - Doctor Bundle Open-Question Index

Status: Complete

## Purpose

Extend `forge doctor export --bundle <path>` so Doctor handoff archives include
deterministic open-question index entries. The index gives humans and scripts
a direct way to inspect unresolved Doctor/catalogue-policy questions and their
diagnostic handoff metadata without opening the full Doctor export JSON or the
catalogue-policy index.

## Research grounding

- Documented: R006/ADR-010 defines `forge doctor export` as a canonical
  offline-first CLI command and says CLI output should support humans and
  automation.
- Documented: R006 says JSON should be the primary local automation format and
  kept stable separately from human console output.
- Documented: R008/ADR-011 says JSON is canonical, Markdown is a projection,
  fixture-backed tests should cover stable generated evidence, and local
  workflows must stay offline-first and AI-optional.
- Documented: Gate 139 adds structured open-question details to Doctor export
  while preserving the raw open-question text list.
- Documented: Gate 153 adds catalogue-policy diagnostic handoff metadata to
  Doctor export without turning those open questions into `WF-CAP-*`
  diagnostics.
- Inferred: Adding bundle-level open-question index files is safe because they
  derive only from already redacted `DoctorExportReport.Index.CataloguePolicy`
  metadata.
- Open: Provider version parsing, runtime confirmation, MO2 effective
  visibility, JIP PP LN catalogue policy, and GECK Extender mixed-scope policy
  remain unresolved.

## Implemented

- `forge doctor export --bundle <path>` still writes deterministic ZIP
  archives.
- Every Doctor bundle now includes:
  - `open-questions/index.json`
  - `open-questions/index.md`
- The open-question index includes source-type groups, structured
  open-question details, diagnostic handoff metadata, and raw open-question
  text.
- The open-question index is included in the bundle manifest and checksums
  file.
- `README.md` links to the open-question index entries.
- Archive entry ordering and timestamps remain deterministic.
- Open-question index entries use existing redacted metadata and do not expose
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
| Doctor bundles include deterministic open-question index JSON and Markdown | Complete |
| Open-question index derives from existing redacted Doctor export metadata | Complete |
| README points to open-question index entries | Complete |
| Open-question index entries are checksummed and listed in the manifest | Complete |
| Raw local paths are omitted from open-question index payloads | Complete |
| Archive ordering and timestamps stay deterministic | Complete |
| Provider policy gaps remain open | Complete |

## Validation

- `dotnet build --no-restore` passed with 0 warnings and 0 errors.
- `dotnet test tests\WastelandForge.GoldenTests\WastelandForge.GoldenTests.csproj --no-build --no-restore --filter "FullyQualifiedName~DoctorExport"` passed.
- `dotnet test WastelandForge.sln --no-build --no-restore -m:1` passed.
- CLI smoke export inspected `open-questions/index.json`,
  `open-questions/index.md`, `README.md`, `checksums.sha256`, manifest entry
  count, open-question index kind, open-question counts, diagnostic handoff
  counts, README link, checksum entries, Markdown heading, known
  catalogue-policy question IDs, and absence of the fixture project root path
  in inspected open-question bundle payloads.
- `git diff --check` exited 0 with only Git LF-to-CRLF working-copy warnings.
- Touched-path protected scan returned no protected-file matches.

## Next gate

Gate 183 should continue documented capability/Doctor value from existing
redacted metadata without resolving open provider policy questions.
Provider-version evidence should only start when documented file, runtime,
parser, and catalogue-policy evidence is ready.
