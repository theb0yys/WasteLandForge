# Gate 178 - Doctor Bundle Catalogue-Policy Index

Status: Complete

## Purpose

Extend `forge doctor export --bundle <path>` so Doctor handoff archives include
deterministic catalogue-policy index entries. The index gives humans and
scripts a direct redacted bundle path for existing open catalogue-policy
questions without opening the full Doctor export JSON or nested scan report.

## Research grounding

- Documented: R006/ADR-010 defines `forge doctor export` as a canonical
  offline-first CLI command and says CLI output should support humans and
  automation.
- Documented: R005/ADR-008 says provider-version policy, runtime-probe policy,
  JIP PP LN alias policy, mixed-scope GECK Extender handling, and MO2
  effective-visibility scope remain open provider/catalogue questions.
- Documented: Gate 139 adds structured Doctor export open-question details.
- Documented: Gate 143 adds compact catalogue-policy grouping to
  `forge doctor export`.
- Documented: Gate 153 adds catalogue-policy diagnostic handoff metadata to
  `forge doctor export`.
- Inferred: Adding bundle-level catalogue-policy index files is safe because
  they derive only from the already redacted
  `DoctorExportReport.Index.CataloguePolicy` view model.
- Open: Provider version parsing, runtime confirmation, MO2 effective
  visibility, JIP PP LN catalogue policy, and GECK Extender mixed-scope policy
  remain unresolved.

## Implemented

- `forge doctor export --bundle <path>` still writes deterministic ZIP
  archives.
- Every Doctor bundle now includes:
  - `catalogue-policy/index.json`
  - `catalogue-policy/index.md`
- The catalogue-policy index includes redacted source-type groups,
  structured open-question details, diagnostic handoff entries, and raw
  open-question text.
- The catalogue-policy index is included in the bundle manifest and checksums
  file.
- `README.md` links to the catalogue-policy index entries.
- Archive entry ordering and timestamps remain deterministic.
- Catalogue-policy index entries omit local evidence paths and use the already
  redacted Doctor export metadata.

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
| Doctor bundles include deterministic catalogue-policy index JSON and Markdown | Complete |
| Catalogue-policy index derives from redacted Doctor export metadata | Complete |
| README points to catalogue-policy index entries | Complete |
| Catalogue-policy index entries are checksummed and listed in the manifest | Complete |
| Archive ordering and timestamps stay deterministic | Complete |
| Provider policy gaps remain open | Complete |

## Validation

- `dotnet build --no-restore` passed with 0 warnings and 0 errors.
- `dotnet test tests\WastelandForge.GoldenTests\WastelandForge.GoldenTests.csproj --no-build --no-restore --filter "FullyQualifiedName~DoctorExport"` passed 38 tests.
- `dotnet test WastelandForge.sln --no-build --no-restore -m:1` passed 514 tests.
- CLI smoke export inspected `catalogue-policy/index.json`,
  `catalogue-policy/index.md`, `README.md`, `checksums.sha256`, manifest entry
  count, catalogue-policy kind, open-question count, diagnostic handoff count,
  README link, checksum entries, Markdown heading, and absence of the fixture
  project root path in inspected catalogue-policy bundle payloads.
- `git diff --check` exited 0 with only Git LF-to-CRLF working-copy warnings.
- Touched-path protected scan returned no protected-file matches.

## Next gate

Gate 179 should continue documented capability/Doctor value without resolving
open provider policy questions. Provider-version evidence should only start
when documented file, runtime, parser, and catalogue-policy evidence is ready.
