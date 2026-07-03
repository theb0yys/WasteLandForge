# Gate 189 - Doctor Worklist Summary Metadata

Status: Complete

## Purpose

Extend Doctor triage worklist output with deterministic priority and source
summary metadata so humans and scripts can quickly see how many remediation
items are blockers, how many are review items, and which redacted report
sections or bundle paths they come from.

## Research grounding

- Documented: R006/ADR-010 defines `forge doctor export` as an offline-first
  workflow command and says CLI output should support human recovery and
  automation consumers.
- Documented: R008/ADR-011 says JSON is canonical, Markdown is a projection,
  fixture-backed tests should cover stable generated evidence, and local
  workflows must remain offline-first and AI-optional.
- Documented: Gate 188 adds ordered Doctor triage worklist entries linked to
  command-hint IDs and report sections or bundle paths.
- Inferred: Priority and source summaries are safe when they group existing
  worklist items without executing commands or resolving new provider evidence.
- Open: Provider version parsing, runtime confirmation, MO2 effective
  visibility, JIP PP LN catalogue policy, and GECK Extender mixed-scope policy
  remain unresolved.

## Implemented

- Primary `forge doctor export --format json` triage now includes:
  - `summary.worklistPriorityGroups`
  - `summary.worklistSourceGroups`
  - `worklistSummary.priorities`
  - `worklistSummary.sources`
- Primary worklist sources use report sections.
- Plain `forge doctor export` output now includes `Worklist summary:`.
- `forge doctor export --summary <path>` Markdown now includes
  `### Worklist Summary`.
- `forge doctor export --bundle <path>` triage JSON and Markdown now include
  the same priority summary and path-based source summary.

## Not implemented

- No command execution.
- No new slash command alias.
- No CLI alias outside ADR-010/R006 canonical command names.
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
| Primary JSON includes worklist priority/source group counts | Complete |
| Primary JSON includes section-based worklist summary metadata | Complete |
| Plain output includes worklist summary metadata | Complete |
| Markdown summary includes worklist summary metadata | Complete |
| Bundle triage includes path-based worklist summary metadata | Complete |
| Raw local paths remain omitted from summary payloads | Complete |
| Provider policy gaps remain open | Complete |

## Validation

- `dotnet build --no-restore` passed with 0 warnings and 0 errors.
- `dotnet test tests\WastelandForge.GoldenTests\WastelandForge.GoldenTests.csproj --no-build --no-restore --filter "FullyQualifiedName~DoctorExport"` passed.
- `dotnet test WastelandForge.sln --no-build --no-restore -m:1` passed.
- CLI smoke export inspected primary triage JSON, archive triage JSON, archive
  triage Markdown, summary Markdown, priority/source summary counts,
  section/path separation, and absence of the fixture project root path in
  inspected payloads.
- `git diff --check` exited 0 with only Git LF-to-CRLF working-copy warnings.
- Touched-path protected scan returned no protected-file matches.

## Next gate

Gate 190 should continue documented capability/Doctor value from existing
redacted metadata. A practical next slice is a compact remediation status
header for Doctor output that summarizes blocked/review/ready state, work item
counts, and first recommended command without adding provider-version,
runtime, parser, MO2, GECK, or catalogue-policy resolution behavior.
