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

Gate 4 creates the core C# model and deterministic JSON serialization.

Gate 10 adds SARIF 2.1.0 projection from the same canonical issue data.

Gate 11 adds Markdown summaries and GitHub workflow-command annotations from
the same canonical issue data. GitHub annotations include line and column only
when canonical `SourceLocation` includes line and column values; JSON Pointer
remains the canonical value-level location.

Gate 12 adds YAML source location mapping for YAML-backed source contracts.
Manifest schema diagnostics are emitted from runtime JSON Schema evaluation
against the normalized canonical JSON object, while locations still use the
canonical JSON Pointer and optional source line and column.

Gate 13 extends runtime schema diagnostics to dependency and capability
registry documents before semantic cross-registry validation runs.

Gate 14 extends runtime schema diagnostics to optional asset registry documents
when the manifest declares an asset registry root.

Gate 15 adds `WF-ASSET-*` semantic diagnostics for asset source and target path
rules after asset registry schema validation succeeds.

Gate 16 adds type-specific `WF-ASSET-*` semantic diagnostics for minimal source
file signatures and target-root conventions after asset path validation
succeeds.

Gate 5 creates the diagnostic report aggregate and uses it for loader and
validation pipeline output.
