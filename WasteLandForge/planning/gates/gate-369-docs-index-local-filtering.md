# Gate 369 - Docs Index Local Filtering

Status: Complete

## Goal

Make the generated reference browser usable at 100-plus entries through local,
read-only search without changing generated evidence.

## Research grounding

- **Documented:** ADR-009 keeps generated evidence disposable and immutable
  from browsing operations.
- **Documented:** ADR-010 requires local, offline-first user workflows.
- **Documented:** Gate 368 routes in-memory filtering over parsed entry
  metadata.

## Implemented

- Added a Docs Index filter box with an explicit accessibility/tooltip label.
- Filtering is case-insensitive across entry title, ID, kind, source, version,
  and description/detail text.
- Uses the immutable parsed `DocsReferenceIndexView` as the source and creates
  filtered section projections in memory.
- Omits empty sections and reports matching entries, total entries, and matching
  section count.
- Empty input restores the original section ordering and all entries.
- Query changes reset selected details, preview, and Open Reference state so a
  hidden stale entry cannot remain actionable.
- No-match input displays zero entries without changing the source index.

## Verification

- Release desktop build/publish passed with zero warnings and zero errors.
- Published-app UI Automation used mixed-case `FoRgE InIt` and found exactly
  one result in the Commands section.
- The filtered entry remained selectable and previewed the canonical command
  reference Markdown.
- A no-match query produced zero entries and disabled stale Open state.
- Clearing the query restored six sections and all 109 entries.
- The generated reference-index SHA-256 remained unchanged.
- Settings remained unchanged and the synthetic project was removed.

## Boundaries

- No file reread during filtering, generated mutation, source mutation, network
  calls, process launch, provider installation, external game-tool execution,
  runtime probes, release behavior, plugin mutation, or AI behavior.

## Next route

Gate 370: first app-shell MCM source authoring surface. Use the published MCM
schema and existing synthetic fixtures to create a minimal page/setting source
registry, save only inside the selected project's source registry tree, run
canonical validation, and generate through the existing MCM pipeline. This
closes the current docs-browser polish slice.
