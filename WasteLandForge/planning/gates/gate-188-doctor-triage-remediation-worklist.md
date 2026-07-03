# Gate 188 - Doctor Triage Remediation Worklist

Status: Complete

## Purpose

Extend Doctor export triage with an ordered remediation worklist so humans can
see the next operator steps in priority order, linked back to canonical command
hints and existing Doctor report sections or bundle paths.

## Research grounding

- Documented: R006/ADR-010 defines `forge capabilities scan`,
  `forge capabilities explain`, `forge doctor export`, and
  `forge capabilities list` as canonical offline-first workflow commands.
- Documented: R006 says CLI output should help users recover from findings
  and support both human and automation consumers.
- Documented: R008/ADR-011 says JSON is canonical, Markdown is a projection,
  fixture-backed tests should cover stable generated evidence, and local
  workflows must remain offline-first and AI-optional.
- Documented: Gate 187 adds redacted command hints to Doctor triage.
- Inferred: An ordered worklist is safe when it is derived from existing
  redacted triage inputs and references command-hint IDs instead of executing
  commands or resolving new provider evidence.
- Open: Provider version parsing, runtime confirmation, MO2 effective
  visibility, JIP PP LN catalogue policy, and GECK Extender mixed-scope policy
  remain unresolved.

## Implemented

- Primary `forge doctor export --format json` triage now includes:
  - `summary.workItems`
  - `worklist`
- Plain `forge doctor export` output now includes a `Worklist:` subsection in
  `Triage:`.
- `forge doctor export --summary <path>` Markdown now includes
  `### Worklist`.
- `forge doctor export --bundle <path>` triage JSON and Markdown now include
  archive-path-aware worklist entries.
- Worklist items currently derive from existing metadata:
  - refresh capability evidence when local evidence, diagnostics, actions, or
    wrong-scope findings need operator review
  - resolve each unavailable project requirement
  - review diagnostics
  - review Doctor actions
  - inspect wrong-scope evidence
  - review catalogue-policy open questions
- Primary worklist entries use report sections; bundle worklist entries use
  archive paths.

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
| Primary JSON includes triage worklist count and worklist array | Complete |
| Primary worklist references report sections, not bundle paths | Complete |
| Plain output includes a worklist | Complete |
| Markdown summary includes a worklist | Complete |
| Bundle triage includes archive-path-aware worklist entries | Complete |
| Worklist entries link to existing command-hint IDs | Complete |
| Raw local paths remain omitted from worklist payloads | Complete |
| Provider policy gaps remain open | Complete |

## Validation

- `dotnet build --no-restore` passed with 0 warnings and 0 errors.
- `dotnet test tests\WastelandForge.GoldenTests\WastelandForge.GoldenTests.csproj --no-build --no-restore --filter "FullyQualifiedName~DoctorExport"` passed.
- `dotnet test WastelandForge.sln --no-build --no-restore -m:1` passed.
- CLI smoke export inspected primary triage JSON, archive triage JSON, archive
  triage Markdown, summary Markdown, worklist counts, worklist priority/order,
  command-hint links, section/path separation, and absence of the fixture
  project root path in inspected payloads.
- `git diff --check` exited 0 with only Git LF-to-CRLF working-copy warnings.
- Touched-path protected scan returned no protected-file matches.

## Next gate

Gate 189 should continue documented capability/Doctor value from existing
redacted metadata. A practical next slice is priority/source summary metadata
for the Doctor worklist, without adding provider-version, runtime, parser, MO2,
GECK, or catalogue-policy resolution behavior.
