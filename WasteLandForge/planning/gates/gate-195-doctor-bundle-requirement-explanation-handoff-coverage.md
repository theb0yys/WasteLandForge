# Gate 195 - Doctor Bundle Requirement Explanation Handoff Coverage

Status: Complete

## Purpose

Make Doctor bundle requirement-explanation Markdown explicitly cover the
explain-side operator handoff projection added by Gate 194, so operators who
open the redacted archive know that per-requirement Markdown entries include
copyable placeholder command checklists.

## Research grounding

- Documented: R006/ADR-010 defines `forge doctor export` and
  `forge capabilities explain` as canonical offline-first CLI commands.
- Documented: R006 says capability failure output should identify what was
  required, what was detected, why it failed, and the next command to run.
- Documented: R008/ADR-011 says Markdown is a projection, fixture-backed tests
  should cover stable generated evidence, and local workflows must remain
  offline-first and AI-optional.
- Documented: Gates 168 through 170 add Doctor bundle per-requirement
  explanation Markdown, JSON, and indexes derived from existing explanation
  reports.
- Documented: Gate 194 adds explain-side operator handoff sections to
  capability explanation Markdown while keeping explain JSON unchanged.
- Inferred: Because Doctor bundle requirement explanations reuse the same
  redacted capability explanation Markdown renderer, archive tests and
  generated bundle docs should explicitly cover the inherited handoff.
- Open: Provider version parsing, runtime confirmation, MO2 effective
  visibility, JIP PP LN catalogue policy, and GECK Extender mixed-scope policy
  remain unresolved.

## Implemented

- `requirement-explanations/index.md` now states that Markdown entries include
  `## Operator Handoff` checklists with placeholder commands.
- Bundle `README.md` now states that requirement explanation Markdown entries
  include operator handoff checklists with placeholder commands and that paired
  JSON entries keep the existing `capabilities explain` contract.
- Golden archive coverage now asserts:
  - `requirement-explanations/<capability>.md` includes `## Operator Handoff`
  - the handoff includes blocked status, priority/source summaries, immediate
    work items, and canonical placeholder commands
  - paired `requirement-explanations/<capability>.json` entries do not add
    `operatorHandoff`
  - Markdown entries omit redacted project-root tokens
  - bundle README and requirement explanation index describe the handoff
    behavior

## Not implemented

- No new command alias.
- No `--format zip` or `--format markdown`.
- No new JSON contract.
- No Doctor export SARIF or GitHub mode.
- No GitHub step-summary behavior for Doctor export.
- No provider detector, runtime probe, resolver behavior, Doctor planning
  behavior, diagnostic rule ID, provider-version parser, MO2 VFS inspection,
  GECK automation, network check, release publishing, catalogue-policy
  decision, archive schema, or AI behavior.

## Exit evidence

| Evidence | Status |
|---|---|
| Requirement explanation Markdown includes operator handoff checklist | Complete |
| Requirement explanation index documents Markdown handoff behavior | Complete |
| Bundle README documents Markdown handoff behavior | Complete |
| Paired explanation JSON keeps existing contract | Complete |
| Markdown entries omit redacted project-root tokens | Complete |
| Provider policy gaps remain open | Complete |

## Validation

- `dotnet build --no-restore` passed with 0 warnings and 0 errors in serial
  validation.
- `dotnet test tests\WastelandForge.GoldenTests\WastelandForge.GoldenTests.csproj --no-build --no-restore --filter "FullyQualifiedName~DoctorExportBundleIncludesRequirementExplanationSummaries"` passed.
- `dotnet test tests\WastelandForge.GoldenTests\WastelandForge.GoldenTests.csproj --no-build --no-restore --filter "FullyQualifiedName~DoctorExport"` passed.
- `dotnet test WastelandForge.sln --no-build --no-restore -m:1` passed.
- CLI smoke export inspected a Doctor bundle ZIP and confirmed:
  - bundle `README.md` documents requirement explanation handoff behavior
  - `requirement-explanations/index.md` documents `## Operator Handoff`
  - `requirement-explanations/runtime.scripting.xnvse.md` includes operator
    handoff commands
  - paired requirement explanation JSON does not add `operatorHandoff`
  - raw fixture project paths are absent from requirement explanation Markdown

## Next gate

Gate 196 should start provider-version value cautiously from documented
metadata. A practical next slice is a provider-version declaration metadata
skeleton in the built-in catalogue, `capabilities list`, and `capabilities
explain` output, without parsing local provider versions, changing
resolution, adding runtime probes, MO2/GECK automation, or deciding unresolved
catalogue policy questions.
