# Gate 463 - Release Candidate Remediation Navigation Contract

Status: Complete
Phase: v0.1 desktop remediation workflow definition
Decision base: ADR-004, ADR-005, ADR-009, ADR-010, ADR-011, Gates 460-462

## Purpose

Define the next operator action after a Release Candidate stage blocks. The
desktop must explain the selected diagnostic through canonical
`forge explain diagnostic <rule-id> --format json`, preserve the exact issue
context from the failed run, and offer bounded navigation to an existing Forge
workspace. It must not invent fixes or mutate source.

## Documented behavior

- `forge explain diagnostic` is part of the ADR-010 command surface.
- Its structured output supplies the diagnostic family, validation stage,
  family recovery commands, optional concrete rule metadata, and execution
  boundaries.
- The explain command is deterministic and local. It does not read project
  files, generated evidence, provider output, or external-tool output.
- Recovery commands are informational metadata. Displaying them does not grant
  permission to execute them.
- AI is not required for explanation, validation, remediation, or navigation.

## Inferred desktop contract

The Release Candidate diagnostics grid gains single selection. A remediation
panel beside or below it exposes:

- the original rule ID, severity, title, and exact message from the selected
  stage result;
- **Explain diagnostic**, which invokes the bundled backend with the canonical
  rule ID and JSON format;
- explanation state: `Not loaded`, `Loading`, `Ready`, or `Unavailable`;
- family scope, validation stage, concrete rule title/summary when documented,
  recovery-command text, and backend-declared boundaries;
- the exact canonical explain command used;
- **Go to workspace**, enabled only when a deterministic local route exists and
  the result is not stale.

The original issue and generic rule explanation remain visually distinct.
Generic metadata must never overwrite or paraphrase the exact run diagnostic.

Selecting another diagnostic clears the prior explanation projection. Only one
explanation request may be active. A stale response from an earlier selection
must not replace the current selection's panel.

## Deterministic workspace routes

Rule-family routing changes only the selected desktop tab:

| Rule family | Existing workspace |
| --- | --- |
| `WF-LOAD-*`, `WF-SCHEMA-*`, `WF-SEM-*`, `WF-ASSET-*` | Validation Report |
| `WF-CAP-*` | Capabilities |
| `WF-GEN-*`, `WF-BUILD-*` | Project Outputs |
| `WF-REL-*`, `WF-GOV-*`, `WF-SEC-*` | Release Candidate |

The route is based on the parsed reserved rule-family prefix, never diagnostic
message text. Unknown or malformed identifiers have no route. Plugin review
remains visible through Release Candidate provenance; Gate 464 must not infer a
Plugin Intake route by scraping a message.

Navigation does not rerun validation, package, release verification, provider
scans, generation, or any recovery command. It does not focus or edit a source
file because current diagnostic projections do not carry a complete,
contracted editor location.

## Freshness and failure handling

- Generic explanation remains available for a previously selected rule when a
  candidate projection becomes stale.
- **Go to workspace** is disabled for stale projections until the candidate is
  rerun, because the original issue may no longer describe current source.
- Backend exit 2, malformed JSON, missing required identity, or a mismatched
  returned rule ID produces `Unavailable` and preserves the original issue.
- Explanation failure does not change the candidate state or evidence actions.
- Candidate-ready and no-selection states disable remediation controls.

## Gate 464 acceptance criteria

Gate 464 must implement this complete panel and prove:

- selected `WF-SCHEMA-*` diagnostics invoke the exact canonical explain command
  and render family, validation-stage, rule summary, recovery commands, and
  boundaries from backend JSON;
- exact run messages remain unchanged and separate from explanation metadata;
- family routes select Validation Report, Capabilities, Project Outputs, and
  Release Candidate correctly;
- route selection never executes a backend recovery command;
- changed diagnostic selection cannot receive an older asynchronous response;
- malformed, mismatched, and failed explain output becomes `Unavailable`;
- stale projections allow generic explanation but disable workspace routing;
- candidate-ready/no-selection states expose no actionable remediation;
- focused Windows tests, full solution build/tests, published app smoke, and an
  installed UI regression pass.

Installed fixtures must remain synthetic and cleanup must remove isolated
install, settings, project, and generated-output roots.

## Explicit non-goals

- automatic fixes or source rewrites;
- automatic execution of recovery commands;
- editor/file/JSON-pointer navigation;
- GECK, xEdit, MO2, game, or provider execution;
- release prepare or publish;
- project-specific explain lookup beyond the already captured diagnostic;
- AI-generated remediation.

## Next route

Gate 464: implement and verify the Release Candidate diagnostic explanation
panel and deterministic workspace navigation defined here.
