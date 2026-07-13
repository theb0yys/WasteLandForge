# Gate 504 - xEdit Check Lane Closeout and Next Value Routing

Status: Complete
Phase: v0.1 implementation and product-value routing
Decision base: ADR-002, ADR-004, ADR-009, ADR-010, ADR-011 and Gates 501-503

## Goal

Close the xEdit Check ingestion lane at its verified boundary and choose the
next repository-local mod-building slice without adding speculative report
families or requiring an unavailable external provider.

## Lane closeout

The xEdit Check lane is complete at its current deterministic boundary:

- Forge generates a bounded read-only `Check(e)` Pascal script;
- report acceptance is tied to the exact script and opaque subject-plugin bytes;
- malformed or stale evidence is refused with `WF-GEN-015`;
- accepted xEdit errors become source-linked `WF-SEM-045` diagnostics;
- CLI, desktop, Candidate evidence, publication, and installed synthetic proof
  cover the workflow;
- plugin bytes, review status, load order, game Data, and external tools remain
  unchanged.

Real xEdit Pascal compatibility remains explicitly deferred by Gate 503. No
additional xEdit report family should be added until a concrete user workflow
and authoritative format justify it.

## Existing value inventory

Repository inspection confirms that Forge already has separate implemented
lanes for:

- FNV framework project creation;
- MCM Extender JSON generation and packaging;
- JIP LN text-script generation and packaging;
- narrative source authoring and GECK worklists;
- opaque human-authored plugin intake and xEdit review evidence;
- deterministic combined packages and FOMOD distributables;
- Candidate validation, release preparation, and local handoff;
- capability/Doctor inspection and external-tool handoffs.

These capabilities are individually substantial, but the desktop currently
exposes them as specialist workspaces. A new user still has to understand and
coordinate several lanes before producing a first testable mod package.

## Next major value selection

The next slice is a **guided Basic Mod Builder** vertical workflow.

- **Documented:** ADR-004 assigns generation, validation, packaging, release
  automation, and workflow integration to Forge.
- **Documented:** ADR-009 requires deterministic rebuildable outputs and
  provenance; ADR-010 requires an ergonomic offline-first CLI/workflow.
- **Documented:** existing gates already implement the source, generator,
  package, and verification primitives required for a basic MCM/JIP mod.
- **Inferred:** composing those proven primitives into one desktop workflow now
  provides more practical author value than another isolated backend adapter.
- **Open:** the exact minimum fields, supported starter type, transactional
  creation boundary, progress model, and final test/install handoff require a
  dedicated contract before implementation.

## Required boundary

The workflow must orchestrate existing validated services rather than duplicate
their logic. It must produce real source files and a deterministic package, not
a mock dashboard. It remains offline, creates no ESP/ESM, does not write game
Data or MO2, and does not launch GECK/xEdit/the game automatically.

## Next route

Gate 505: research and define the guided Basic Mod Builder contract for creating
one new FNV framework project, authoring a minimal MCM Extender menu and optional
JIP LN startup script, validating it, building a combined package/FOMOD, and
presenting exact outputs and blockers in one desktop workflow.
