# Gate 368 - In-App Generated Markdown Preview

Status: Complete

## Goal

Preview a selected generated Markdown reference inside Mod Builder as plain,
read-only text without invoking external applications or interpreting content.

## Research grounding

- **Documented:** ADR-009 keeps generated docs disposable and prevents them
  from becoming canonical source truth.
- **Documented:** ADR-010 requires local, deterministic, offline-first docs
  workflows.
- **Documented:** Gate 367 routes an uninterpreted read-only preview over the
  selected contained Markdown reference.

## Implemented

- Added a fixed-height, scrollable Markdown preview to the Docs Index detail
  panel.
- Preview loading occurs only after canonical entry mapping plus `.md`
  extension, generated/docs containment, existence, and path validation pass.
- Loads the generated file as text into a WPF read-only TextBox.
- Does not render Markdown, HTML, images, links, scripts, or embedded content.
- Invalid or unreadable selections show an unavailable state and keep Open
  Reference disabled.
- Starting a new project resets the preview.

## Verification

- Release desktop build/publish passed with zero warnings and zero errors.
- Published-app UI Automation selected the generated asset schema reference.
- Preview contained `# asset 0.1.0 Schema Reference` and the canonical schema
  ID.
- UI Automation reported the preview as read-only.
- SHA-256 values for both the Markdown file and reference index were unchanged
  after previewing.
- Settings remained unchanged and the synthetic project was removed.

## Boundaries

- No Markdown/HTML interpretation, link navigation, process launch, generated
  mutation, source mutation, network calls, provider installation, external
  game-tool execution, runtime probes, release behavior, plugin mutation, or
  AI behavior.

## Next route

Gate 369: Docs Index search and filter. Add local in-memory filtering across
entry title, ID, kind, source, and description while preserving the immutable
parsed index and current read-only selection behavior.
