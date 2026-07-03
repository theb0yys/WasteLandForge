# Gate 193 - Capability Scan Operator Handoff

Status: Complete

## Purpose

Add a scan-side operator checklist to `forge capabilities scan` plain output
and Markdown summaries so a human can move from scan findings to immediate
canonical commands without opening Doctor export first.

## Research grounding

- Documented: R005/ADR-008 says capability detection is local-first and
  deterministic, and Forge should explain when capabilities are absent,
  unknown, wrong-scope, or only partially satisfiable.
- Documented: R006/ADR-010 defines `forge capabilities scan` as a canonical
  offline-first command and says recoverable workflows should remain
  script-safe without top-level aliases.
- Documented: R008/ADR-011 says Markdown is a projection, fixture-backed tests
  should cover stable generated evidence, and local workflows must remain
  offline-first and AI-optional.
- Documented: Gates 146-150 add scan-side action, requirement, diagnostic,
  catalogue-policy, and open-question indexes.
- Documented: Gate 165 adds path-minimized scan Markdown summaries.
- Inferred: A scan-side operator handoff is safe when it formats existing scan
  requirements, diagnostics, Doctor actions, wrong-scope counts, and
  catalogue-policy open questions without changing scan JSON or running new
  provider checks.
- Open: Provider version parsing, runtime confirmation, MO2 effective
  visibility, JIP PP LN catalogue policy, and GECK Extender mixed-scope policy
  remain unresolved.

## Implemented

- `forge capabilities scan --format plain` now includes `Operator handoff:`
  inside the scan status index.
- `forge capabilities scan --summary <path>` Markdown now includes
  `## Operator Handoff`.
- The handoff derives from existing scan metadata:
  - unavailable project requirements
  - already-projected scan diagnostics
  - Doctor next actions
  - wrong-scope provider/capability counts
  - catalogue-policy open questions
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

- No scan JSON contract change.
- No command execution.
- No new slash command alias.
- No CLI alias outside ADR-010/R006 canonical command names.
- No `--format markdown`.
- No new primary output format.
- No SARIF or GitHub scan output change.
- No GitHub step-summary behavior.
- No provider detector, runtime probe, resolver behavior, Doctor planning
  behavior, diagnostic rule ID, provider-version parser, MO2 VFS inspection,
  GECK automation, network check, release publishing, catalogue-policy
  decision, or AI behavior.

## Exit evidence

| Evidence | Status |
|---|---|
| Plain scan output includes operator handoff checklist | Complete |
| Markdown scan summary includes operator handoff checklist | Complete |
| Checklist commands use canonical command surface | Complete |
| Handoff derives from existing scan metadata | Complete |
| Scan JSON output remains unchanged | Complete |
| Markdown summary omits raw local paths | Complete |
| Provider policy gaps remain open | Complete |

## Validation

- `dotnet build --no-restore` passed with 0 warnings and 0 errors.
- `dotnet test tests\WastelandForge.GoldenTests\WastelandForge.GoldenTests.csproj --no-build --no-restore --filter "FullyQualifiedName~CapabilitiesScan"` passed.
- `dotnet test WastelandForge.sln --no-build --no-restore -m:1` passed.
- CLI smoke export inspected plain scan output and Markdown summary output,
  confirmed operator handoff command hints, and confirmed fixture project,
  game-root, and tool-path strings were absent from Markdown summary output.
- `git diff --check` exited 0 with only Git LF-to-CRLF working-copy warnings.
- Touched-path protected scan returned no protected-file matches.

## Next gate

Gate 194 should continue documented capability/Doctor value from existing
local metadata. A practical next slice is an explain-side operator checklist
projection for `forge capabilities explain` text and Markdown summary output,
derived from existing target actions, provider evidence groups, matching
project requirements, diagnostic handoff, and catalogue-policy handoff without
adding provider-version, runtime, parser, MO2, GECK, or catalogue-policy
resolution behavior.
