# Gate 194 - Capability Explain Operator Handoff

Status: Complete

## Purpose

Add an explain-side operator checklist to `forge capabilities explain` plain
output and Markdown summaries so a human can move from one capability or
provider explanation to immediate canonical commands without opening the full
scan or Doctor export first.

## Research grounding

- Documented: R005/ADR-008 says capability detection is local-first and
  deterministic, and Forge should explain missing, unknown, wrong-scope, and
  partially satisfiable capability states.
- Documented: R006/ADR-010 defines `forge capabilities explain` as a
  canonical offline-first command and says capability failure output should
  answer what was required, what was detected, why it failed, and what command
  should be run next.
- Documented: R008/ADR-011 says Markdown is a projection, fixture-backed tests
  should cover stable generated evidence, and local workflows must remain
  offline-first and AI-optional.
- Documented: Gates 131, 133, 134, 151, 152, and 167 add explain-side
  provider evidence groups, project requirement context, diagnostic handoff,
  catalogue-policy handoff, and path-minimized Markdown summaries.
- Inferred: An explain-side operator handoff is safe when it formats existing
  target actions, provider evidence groups, matching project requirements,
  diagnostic handoff issues, and catalogue-policy handoff entries without
  changing scan, resolver, or JSON behavior.
- Open: Provider version parsing, runtime confirmation, MO2 effective
  visibility, JIP PP LN catalogue policy, and GECK Extender mixed-scope policy
  remain unresolved.

## Implemented

- `forge capabilities explain --format plain` now includes `Operator
  handoff:`.
- `forge capabilities explain --summary <path>` Markdown now includes
  `## Operator Handoff`.
- The handoff derives from existing explanation metadata:
  - target next actions
  - provider evidence groups and evidence statuses
  - unavailable matching project requirements
  - project diagnostic handoff issues
  - catalogue-policy open questions and handoff entries
- The handoff includes:
  - ready/review/blocked status
  - concise headline
  - priority summary
  - source summary
  - short checklist of immediate work items
  - copyable canonical command hints
- Markdown summary output continues to omit raw local project, game, and tool
  paths.

## Not implemented

- No explain JSON contract change.
- No scan JSON contract change.
- No command execution.
- No new slash command alias.
- No CLI alias outside ADR-010/R006 canonical command names.
- No `--format markdown`.
- No new primary output format.
- No SARIF or GitHub explain output change.
- No GitHub step-summary behavior.
- No provider detector, runtime probe, resolver behavior, Doctor planning
  behavior, diagnostic rule ID, provider-version parser, MO2 VFS inspection,
  GECK automation, network check, release publishing, catalogue-policy
  decision, archive schema, or AI behavior.

## Exit evidence

| Evidence | Status |
|---|---|
| Plain explain output includes operator handoff checklist | Complete |
| Markdown explain summary includes operator handoff checklist | Complete |
| Checklist commands use canonical command surface | Complete |
| Handoff derives from existing explanation metadata | Complete |
| Explain JSON output remains unchanged | Complete |
| Markdown summary omits raw local paths | Complete |
| Provider policy gaps remain open | Complete |

## Validation

- `dotnet build --no-restore` passed with 0 warnings and 0 errors.
- `dotnet test tests\WastelandForge.GoldenTests\WastelandForge.GoldenTests.csproj --no-build --no-restore --filter "FullyQualifiedName~CapabilitiesExplain"` passed.
- `dotnet test WastelandForge.sln --no-build --no-restore -m:1` passed.
- CLI smoke export inspected plain explain output and Markdown summary output,
  confirmed operator handoff command hints, confirmed explain JSON did not add
  `operatorHandoff`, and confirmed fixture project paths were absent from
  Markdown summary output.
- `git diff --check` exited 0 with only Git LF-to-CRLF working-copy warnings.
- Touched-path protected scan returned no protected-file matches.

## Next gate

Gate 195 should continue documented capability/Doctor value from existing
local metadata. A practical next slice is to make Doctor bundle
requirement-explanation Markdown explicitly cover the explain-side operator
handoff projection in archive tests and docs, using existing redacted
explanation reports without adding provider-version, runtime, parser, MO2,
GECK, archive-schema, or catalogue-policy resolution behavior.
