# Gate 191 - Doctor Operator Handoff Checklist

Status: Complete

## Purpose

Add a concise operator handoff checklist to Doctor text and Markdown triage
output so a human can copy the immediate recovery steps without reading every
derived index first.

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
- Documented: Gate 190 adds a compact remediation status header with the first
  recommended canonical command.
- Inferred: A human operator handoff is safe when it formats existing
  remediation, worklist, and command-hint data without executing commands or
  resolving new provider evidence.
- Open: Provider version parsing, runtime confirmation, MO2 effective
  visibility, JIP PP LN catalogue policy, and GECK Extender mixed-scope policy
  remain unresolved.

## Implemented

- Plain `forge doctor export` output now includes `Operator handoff:` inside
  `Triage:`.
- `forge doctor export --summary <path>` Markdown now includes
  `### Operator Handoff`.
- `forge doctor export --bundle <path>` triage Markdown now includes
  `## Operator Handoff`.
- The handoff combines:
  - remediation status and headline
  - priority summary
  - section/path source summary
  - a short checklist of the first remediation work items
  - canonical command strings resolved from command-hint IDs
- Primary output uses report sections; bundle output uses archive paths.

## Not implemented

- No JSON contract change.
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
| Plain output includes operator handoff checklist | Complete |
| Markdown summary includes operator handoff checklist | Complete |
| Bundle triage Markdown includes path-based operator handoff checklist | Complete |
| Checklist commands derive from existing command-hint data | Complete |
| Primary handoff references report sections, not bundle paths | Complete |
| Bundle handoff references archive paths | Complete |
| Raw local paths remain omitted from handoff payloads | Complete |
| Provider policy gaps remain open | Complete |

## Validation

- `dotnet build --no-restore` passed with 0 warnings and 0 errors.
- `dotnet test tests\WastelandForge.GoldenTests\WastelandForge.GoldenTests.csproj --no-build --no-restore --filter "FullyQualifiedName~DoctorExport"` passed.
- `dotnet test WastelandForge.sln --no-build --no-restore -m:1` passed.
- CLI smoke export inspected plain output, summary Markdown, archive triage
  Markdown, operator handoff checklist content, section/path separation, and
  absence of the fixture project root path in inspected payloads.
- `git diff --check` exited 0 with only Git LF-to-CRLF working-copy warnings.
- Touched-path protected scan returned no protected-file matches.

## Next gate

Gate 192 should continue documented capability/Doctor value from existing
redacted metadata. A practical next slice is a Doctor export concise summary
sidecar for bundle READMEs that points at remediation, worklist, command hints,
and key archive indexes without adding provider-version, runtime, parser, MO2,
GECK, or catalogue-policy resolution behavior.
