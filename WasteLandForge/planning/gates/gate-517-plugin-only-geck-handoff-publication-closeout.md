# Gate 517 - Plugin-Only GECK Handoff Publication Closeout

Status: Complete
Phase: v0.1 observation-driven defect remediation
Decision base: ADR-004, ADR-009, ADR-010, ADR-011 and Gates 399-400, 449, 508-510, 514-516

## Goal

Close the Gate 516 publication gap by proving the plugin-only GECK handoff in
the installed product, without changing opaque plugin bytes or claiming GECK
record semantics.

## Research grounding

| Claim | Classification | Source |
|---|---|---|
| Forge may coordinate GECK authoring but must not create or mutate plugin records. | Documented | ADR-004 and Gate 399 |
| Handoff sources and generated output require deterministic digest provenance. | Documented | ADR-009 and Gate 399 |
| The desktop GECK launch is explicit, preview-gated, and requires a fresh handoff. | Documented | Gates 449 and 480 |
| Installed regression must exercise a project with plugin artifacts and no quest/dialogue registries. | Inferred from reproduced user defect | Gates 515-516 |
| Exact placed-container records remain outside Forge evidence. | Open | Requires user-owned GECK/plugin inspection |

## Installed proof

The installed regression now creates a synthetic `PlacedContainer.esp`, records
its exact length and SHA-256 in a plugin-artifact registry, removes quest and
dialogue declarations, and runs the installed backend package target.

It then proves:

- `evidence/source-index.json` records `scope: plugin-only`;
- exactly one plugin artifact is indexed;
- the handoff manifest and unresolved-action worklist remain unchanged through
  the controlled GECK launch;
- the opaque plugin SHA-256 remains unchanged through packaging and launch;
- the installed desktop enables preview and explicit launch from the fresh
  plugin-only handoff.

The controlled test executable is the bundled Forge binary renamed inside an
isolated temporary directory. No real GECK, game Data, MO2 instance, or plugin
editor is executed or modified.

## Validation

- Unit: 135 passed.
- Schema: 148 passed.
- Semantic: 87 passed.
- Golden: 309 passed.
- Backwards compatibility: 45 passed.
- Windows: 151 passed.
- Total: 875 passed, 0 failed, 0 skipped.
- Self-contained app publication passed.
- Inno Setup 6.7.3 unsigned local installer build passed.
- Installed release-candidate UI regression passed, including Gate 517.

## User-test artifact

```text
artifacts/installer/inno/local/WastelandForge-Setup-local.exe
length: 48634306
sha256: 960A5EA96F8E93C95D957ECE0109EE095FB564233E22A0403EB1EFFD89F394F4
```

This is an unsigned local test installer. It is not a signed or published
release.

