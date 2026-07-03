# Gate 190 - Doctor Remediation Status Header

Status: Complete

## Purpose

Add a compact remediation status header to Doctor triage output so humans and
scripts can quickly see the blocked/review/ready state, work item counts, and
the first recommended canonical command before reading the full worklist.

## Research grounding

- Documented: R006/ADR-010 defines `forge doctor export` as an offline-first
  workflow command and says CLI output should support human recovery and
  automation consumers.
- Documented: R008/ADR-011 says JSON is canonical, Markdown is a projection,
  fixture-backed tests should cover stable generated evidence, and local
  workflows must remain offline-first and AI-optional.
- Documented: Gate 188 adds ordered Doctor triage worklist entries linked to
  command-hint IDs.
- Documented: Gate 189 adds priority and source summary metadata for the
  Doctor worklist.
- Inferred: A compact remediation header is safe when it is derived from the
  existing worklist and command hints without executing commands or resolving
  new provider evidence.
- Open: Provider version parsing, runtime confirmation, MO2 effective
  visibility, JIP PP LN catalogue policy, and GECK Extender mixed-scope policy
  remain unresolved.

## Implemented

- Primary `forge doctor export --format json` triage now includes
  `remediation`.
- Primary remediation header includes:
  - `status`
  - `headline`
  - `workItems`
  - `blockerItems`
  - `reviewItems`
  - `firstWorkItem`
  - `firstCommandHint`
  - `firstCommand`
  - `section`
- Plain `forge doctor export` output now includes a compact `Remediation:`
  header inside `Triage:`.
- `forge doctor export --summary <path>` Markdown now includes
  `### Remediation`.
- `forge doctor export --bundle <path>` triage JSON and Markdown now include
  the same header with a bundle `path` instead of a primary report `section`.

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
| Primary JSON includes remediation status header | Complete |
| Primary remediation header references a report section, not a bundle path | Complete |
| Plain output includes remediation status header | Complete |
| Markdown summary includes remediation status header | Complete |
| Bundle triage includes path-based remediation status header | Complete |
| First command derives from existing command-hint data | Complete |
| Raw local paths remain omitted from remediation payloads | Complete |
| Provider policy gaps remain open | Complete |

## Validation

- `dotnet build --no-restore` passed with 0 warnings and 0 errors.
- `dotnet test tests\WastelandForge.GoldenTests\WastelandForge.GoldenTests.csproj --no-build --no-restore --filter "FullyQualifiedName~DoctorExport"` passed.
- `dotnet test WastelandForge.sln --no-build --no-restore -m:1` passed.
- CLI smoke export inspected primary triage JSON, archive triage JSON, archive
  triage Markdown, summary Markdown, remediation status, first command,
  section/path separation, and absence of the fixture project root path in
  inspected payloads.
- `git diff --check` exited 0 with only Git LF-to-CRLF working-copy warnings.
- Touched-path protected scan returned no protected-file matches.

## Next gate

Gate 191 should continue documented capability/Doctor value from existing
redacted metadata. A practical next slice is a plain-text and Markdown
operator handoff section that combines remediation header, worklist summary,
and command hints into a concise copyable checklist without adding
provider-version, runtime, parser, MO2, GECK, or catalogue-policy resolution
behavior.
