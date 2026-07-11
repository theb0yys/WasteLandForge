# Gate 414 - Dialogue Branching Authoring Contract

Status: Complete
Phase: v0.1 product-value definition
Decision base: ADR-003, ADR-004, ADR-007, ADR-009, Gates 30-31, Gates 50-55, Gates 405-413

## Goal

Define a source-backed Narrative Author edit that appends one structural topic
link or one authored response route to an existing dialogue line. The edit
records typed branch intent, validates it, and rebuilds the GECK handoff without
deciding runtime selection behavior or mapping the declaration to plugin records.

## Research classification

- **Documented:** Dialogue is a gameplay state query and transition surface,
  not a flat text table.
- **Documented:** Dialogue schema `0.23.0` models line-local `linkTo` and
  `linkFrom` topic references and line-local response routes.
- **Documented:** Semantic validation resolves linked and routed topics,
  requires authored line endpoints, rejects duplicate route IDs, and rejects
  ambiguous duplicate route keys on one line.
- **Documented:** Gate 55 blocks route-key taxonomy, route selection, Speech
  Challenge integration, and GECK mapping until representative evidence exists.
- **Documented:** Gate 400 emits links and response routes to
  `worklists/dialogue-links.tsv`; GECK remains the record authority.
- **Inferred:** One source-backed structural branch append is the smallest useful
  authoring step that exposes the existing validated link/route contracts.
- **Open:** Reciprocity, graph traversal, cycles, ordering, route-key meaning,
  runtime selection, GECK field mapping, and plugin output remain unresolved.

## Supported source shape

The workflow supports the bounded canonical JSON shape already used by the
Narrative Author:

```text
src/registries/quests/main.json    schema 0.6.0
src/registries/dialogue/main.json  schema 0.23.0
```

The manifest must declare the canonical quest and dialogue registry paths, each
directory must contain only `main.json`, and project validation must pass before
choices are loaded or previewed.

## Desktop workflow

Narrative Author gains an `Add Dialogue Branch` section with source-backed
selectors for the source dialogue line and target/source topic. Branch kind is
an explicit segmented choice:

- `Link To`: append a link with `targetTopicId`;
- `Link From`: append a link with `sourceTopicId`;
- `Response Route`: append a route with `routeKey` and `targetTopicId`.

Authored fields are a lowercase ASCII alphanumeric ID slug, optional summary,
and an opaque non-empty route key only for response-route mode. Topic choices
come from declared topics that have at least one authored line endpoint. The
selected line's own topic may be selected; Forge does not infer whether a
self-link is useful because traversal and cycle policy remain open.

The user loads choices, selects the line/kind/topic, previews the complete
proposed dialogue JSON, then appends, validates, and rebuilds the GECK handoff
with an unchanged preview token.

## Derived declarations

For selected line `<lineId>` and authored `<slug>`:

```text
link ID:  <lineId>.link.<slug>
route ID: <lineId>.route.<slug>
```

The selected mode emits exactly one of:

```json
{ "id": "<linkId>", "linkType": "linkTo", "targetTopicId": "<topicId>" }
{ "id": "<linkId>", "linkType": "linkFrom", "sourceTopicId": "<topicId>" }
{ "id": "<routeId>", "routeKey": "<authoredOpaqueKey>", "targetTopicId": "<topicId>" }
```

Optional summary is included only when non-empty. Forge does not normalize,
classify, reserve, or interpret the route key.

## Preservation and transaction

- Parse and deep-clone the dialogue root with structured JSON APIs.
- Read quest source for bounded-shape and validation authority only; do not
  modify it.
- Append one object to only the selected line's `links` or `responseRoutes`
  array, creating that optional array only when absent.
- Preserve all existing values, objects, and array order before the appended
  item.
- Preview and every refusal write zero bytes.
- Bind normalized inputs, selected IDs, manifest/quest/dialogue bytes, and exact
  proposed dialogue bytes into the preview token.
- Append re-runs preview, checks the token, writes the proposal, and restores
  original dialogue bytes after write or canonical validation failure.
- After successful validation, rebuild `forge package . --target geck-handoff`.
  Packaging failure retains valid canonical source and reports a retryable
  output failure.

## Refusals

- Unsupported path, filename, schema, document count, parse state, or failed
  canonical pre-validation.
- Selected line or selected topic no longer exists.
- Selected topic has no authored dialogue line endpoint.
- Invalid mode, invalid lowercase ASCII alphanumeric slug, or blank response
  route key.
- Duplicate link/route ID or duplicate response route key on the selected line.
- Stale selection/input/source/manifest token, path escape, write failure, or
  post-write validation failure.

## Safety boundaries

- No reciprocal-link invention, graph traversal, cycle rejection, ordering,
  route taxonomy, route evaluation, Speech Challenge routing, or condition
  synthesis.
- No raw JSON editor, EditorID/FormID invention, GECK field mapping, executable
  script generation, plugin creation, or plugin mutation.
- No GECK/xEdit launch, game Data/MO2 write, game launch, runtime probe,
  network, release publication, or AI requirement.

## Gate 415 acceptance criteria

- Source-backed line/topic choices load deterministically and exclude topics
  without line endpoints.
- Preview writes zero bytes and displays exact proposed dialogue JSON.
- All three modes preserve existing source and append exactly one declaration.
- Canonical validation and GECK-handoff rebuild succeed; `dialogue-links.tsv`
  contains the exact link type/source/target or opaque route key.
- Duplicate ID/key, missing endpoint, stale token, unsupported shape, write
  failure, and validation rollback have focused Windows coverage.
- Published desktop automation completes one branch append and proves repeated
  duplicate refusal preserves dialogue source SHA-256.
- Release build and all test suites pass.

## Next route

Gate 415: implement the dialogue branch choice loader, preview-token append
engine, validation rollback, Narrative Author controls, GECK-handoff rebuild,
focused tests, and published-app regression.
