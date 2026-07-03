# Gate 186 - Doctor Export Primary Triage Projection

Status: Complete

## Purpose

Extend primary `forge doctor export` JSON, plain text, and Markdown summary
outputs with deterministic triage data so a human or script can see blocking
items, review items, next actions, and primary report sections without needing
to create a ZIP bundle.

## Research grounding

- Documented: R006/ADR-010 defines `forge doctor export` as a canonical
  offline-first CLI command and says CLI output should support humans and
  automation.
- Documented: R006 says JSON should be the primary local automation format and
  kept stable separately from human console output.
- Documented: R008/ADR-011 says JSON is canonical, Markdown is a projection,
  fixture-backed tests should cover stable generated evidence, and local
  workflows must stay offline-first and AI-optional.
- Documented: Gate 128 implements Doctor export as a redacted local handoff
  bundle over capability scan evidence.
- Documented: Gates 135 through 143 add primary Doctor export summary and
  compact index sections derived from redacted capability scan metadata.
- Documented: Gates 166 through 185 add deterministic Doctor bundle archive
  entries, including archive-local triage paths.
- Inferred: Adding a primary triage projection is safe because it derives only
  from already redacted Doctor summary, diagnostic, requirement, action,
  wrong-scope, and open-question metadata.
- Open: Provider version parsing, runtime confirmation, MO2 effective
  visibility, JIP PP LN catalogue policy, and GECK Extender mixed-scope policy
  remain unresolved.

## Implemented

- `forge doctor export --format json` now includes top-level `triage`.
- The primary triage JSON includes:
  - `kind`
  - `summary`
  - `blocking`
  - `review`
  - `actions`
  - `reviewSections`
- Primary triage uses report-section references such as `index.requirements`
  and `index.actions`, not ZIP archive paths.
- Plain `forge doctor export` output now includes a `Triage:` section before
  the Doctor index.
- `forge doctor export --summary <path>` Markdown now includes `## Triage`.
- The existing ZIP bundle `triage/index.*` entries now reuse the same triage
  status and item derivation while keeping archive-path output.
- Local paths remain redacted.

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
| Primary JSON includes top-level triage data | Complete |
| Primary triage references report sections, not bundle paths | Complete |
| Plain output includes a triage section | Complete |
| Markdown summary includes a triage section | Complete |
| Bundle triage status/count derivation is shared with primary triage | Complete |
| Raw local paths remain omitted from triage payloads | Complete |
| Provider policy gaps remain open | Complete |

## Validation

- `dotnet build --no-restore` passed with 0 warnings and 0 errors.
- `dotnet test tests\WastelandForge.GoldenTests\WastelandForge.GoldenTests.csproj --no-build --no-restore --filter "FullyQualifiedName~DoctorExport"` passed.
- `dotnet test WastelandForge.sln --no-build --no-restore -m:1` passed.
- CLI smoke export inspected primary triage JSON, archive triage JSON, archive
  triage Markdown, summary Markdown, section references, bundle path
  references, counts, and absence of the fixture project root path in inspected
  payloads.
- `git diff --check` exited 0 with only Git LF-to-CRLF working-copy warnings.
- Touched-path protected scan returned no protected-file matches.

## Next gate

Gate 187 should continue documented capability/Doctor value from existing
redacted metadata without resolving open provider policy questions.
Provider-version evidence should only start when documented file, runtime,
parser, and catalogue-policy evidence is ready.
