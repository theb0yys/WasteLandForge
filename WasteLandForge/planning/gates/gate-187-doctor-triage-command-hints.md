# Gate 187 - Doctor Triage Command Hints

Status: Complete

## Purpose

Extend Doctor export triage with deterministic command hints so a human can
move from redacted Doctor evidence to the next canonical `forge` commands
without guessing which capability or Doctor command to run.

## Research grounding

- Documented: R006/ADR-010 defines a small, stable, offline-first CLI and
  names `forge capabilities scan`, `forge capabilities explain`, and
  `forge doctor export` as canonical workflow commands.
- Documented: R006 says CLI output should help users recover and should
  support both human and automation consumers.
- Documented: R008/ADR-011 says JSON is canonical, Markdown is a projection,
  fixture-backed tests should cover stable generated evidence, and local
  workflows must remain offline-first and AI-optional.
- Documented: Gate 186 adds primary Doctor export triage using report-section
  references while keeping bundle triage path-based.
- Inferred: Command hints are safe when they are derived from already redacted
  Doctor metadata and use placeholders such as `<project-root>`,
  `<game-root>`, and `<tool-path>` instead of raw local paths.
- Open: Provider version parsing, runtime confirmation, MO2 effective
  visibility, JIP PP LN catalogue policy, and GECK Extender mixed-scope policy
  remain unresolved.

## Implemented

- Primary `forge doctor export --format json` triage now includes:
  - `summary.commandHints`
  - `commands`
- Plain `forge doctor export` output now includes command hints in `Triage:`.
- `forge doctor export --summary <path>` Markdown now includes
  `### Command Hints`.
- `forge doctor export --bundle <path>` triage JSON and Markdown now include
  archive-path-aware command hints.
- Command hints currently derive from existing metadata:
  - `rescan-capabilities`
  - `explain-requirement-*` for unavailable project requirements
  - `review-diagnostics`
  - `review-actions`
  - `review-catalogue-policy`
- Command strings use canonical commands and redacted placeholders only.

## Not implemented

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
| Primary JSON includes triage command hint count and command array | Complete |
| Primary command hints use report sections, not bundle paths | Complete |
| Plain output includes command hints | Complete |
| Markdown summary includes command hints | Complete |
| Bundle triage includes archive-path-aware command hints | Complete |
| Command strings use canonical commands and placeholders | Complete |
| Raw local paths remain omitted from command hints | Complete |
| Provider policy gaps remain open | Complete |

## Validation

- `dotnet build --no-restore` passed with 0 warnings and 0 errors.
- `dotnet test tests\WastelandForge.GoldenTests\WastelandForge.GoldenTests.csproj --no-build --no-restore --filter "FullyQualifiedName~DoctorExport"` passed.
- `dotnet test WastelandForge.sln --no-build --no-restore -m:1` passed.
- CLI smoke export inspected primary triage JSON, archive triage JSON, archive
  triage Markdown, summary Markdown, command hint counts, canonical command
  strings, placeholder usage, bundle path references, section references, and
  absence of the fixture project root path in inspected payloads.
- `git diff --check` exited 0 with only Git LF-to-CRLF working-copy warnings.
- Touched-path protected scan returned no protected-file matches.

## Next gate

Gate 188 should continue documented capability/Doctor value from existing
redacted metadata. A practical next slice is a Doctor remediation worklist or
operator checklist derived from the same triage and command-hint data, without
resolving provider-version, runtime, parser, MO2, GECK, or catalogue-policy
open questions.
