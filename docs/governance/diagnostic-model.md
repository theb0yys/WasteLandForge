# Diagnostic Model

Status: Skeleton
Research classification: Documented
Source: R004 / ADR-007 and R008 / ADR-011

WastelandForge diagnostic issue JSON is the canonical diagnostic model. Other outputs such as console text, Markdown, SARIF, and GitHub annotations are projections from the same issue data.

## Fields

- `ruleId`: stable WastelandForge rule ID, such as `WF-SEM-014`.
- `severity`: `error`, `warning`, or `note`.
- `category`: diagnostic category, such as `semantic`.
- `title`: short human-readable title.
- `message`: detailed diagnostic message.
- `projectId`: optional stable dotted lowercase project ID.
- `primaryLocation`: required source location.
- `relatedLocations`: optional supporting source locations.
- `suggestedFix`: optional remediation text.
- `docsUri`: optional documentation URI for the rule.
- `fingerprint`: optional stable identity for repeat diagnostics.

## Locations

JSON Pointer is the canonical location format for values inside source contracts. Human renderers may add friendlier displays later, but those are not the source of truth.

Locations can include:

- `file`
- `pointer`
- `line`
- `column`

## Diagnostic Reports

Gate 5 adds a deterministic report wrapper for validation runs:

- `formatVersion`: machine-readable output contract version.
- `tool`: tool metadata for the emitting command, including version once defined.
- `command`: command name that produced the payload.
- `project`: optional project metadata, including stable dotted lowercase project ID when available.
- `summary`: error, warning, and note counts.
- `issues`: sorted diagnostic issue objects.

Reports are the machine-readable output for `forge validate --format json`.

## Gate Ownership

Gate 4 creates the core C# model and deterministic JSON serialization. SARIF output is a later projection, not part of Gate 4.

Gate 5 creates the diagnostic report aggregate and uses it for loader and
validation pipeline output.
