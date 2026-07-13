# Gate 535 - GECK Authoring Verifier Lane Closeout and Desktop Routing

Status: Complete - verifier lane closed at synthetic boundary; next contract selected
Phase: post-v0.1 product-value routing
Decision base: ADR-004, ADR-009, ADR-010, ADR-013, R009, and Gates 511-514,
522, 532-534

## Goal

Close the GECK authoring verifier lane at the strongest currently proven
boundary and select one repository-local next value slice without inventing a
synthetic plugin, bypassing Gate 534, reopening authoring execution, or adding
more backend verifier edge cases.

## Evidence classification

- **Documented:** ADR-004 assigns generation, validation, packaging, release
  automation, and workflow integration to Forge while keeping raw plugin
  editing outside Forge core.
- **Documented:** ADR-009 requires deterministic rebuildable outputs and
  provenance; ADR-010 requires an ergonomic offline-first workflow.
- **Documented:** Gate 533 implements the deterministic authoring plan observer
  bundle, raw-observation validation, report sealing, semantic verification,
  CLI/help/explain surface, and synthetic refusal coverage.
- **Documented:** Gate 534 keeps real FNVEdit compatibility deferred until an
  operator supplies the exact provider, redistributable synthetic plugin,
  matching project/bundle, evidence destination, and digest approval.
- **Documented:** Gates 511-513 require implemented product value to be exposed
  through scalable grouped desktop navigation while preserving specialist
  workflows and accurate boundaries.
- **Observed:** the current WPF Review group exposes xEdit Audit, GECK Handoff,
  Project Outputs, and Release Candidate, but the desktop source and Windows
  tests contain no reference to `geck-authoring-plan`,
  `geck-authoring-verifier`, or `geck-authoring-verification`.
- **Inferred:** exposing the completed deterministic backend through a bounded
  desktop review workflow provides more practical value than adding another
  verifier report family while the real compatibility smoke is blocked.
- **Open:** whether this workflow should extend GECK Handoff and Project
  Outputs or use a dedicated Review workspace must be decided from current
  service/routing ownership and responsive-layout evidence before
  implementation.

## Lane closeout

The authoring verifier lane is complete at its current repository-local
boundary:

- canonical intent can produce a validated, digest-bound preview plan;
- Forge can generate a deterministic read-only Pascal observer bundle;
- explicitly supplied raw observations can be schema-checked and sealed;
- the Gate 522 parser independently verifies exact plan/provider/script/plugin
  provenance and first-slice semantics;
- stale, malformed, incomplete, unsafe, unsupported, and semantically
  mismatched evidence is refused deterministically;
- no external process or plugin mutation is required for the correctness path.

FNVEdit runtime compatibility remains deferred by Gate 534. No further Pascal
or raw-observation contract expansion is selected without real compatibility
evidence or a concrete unsupported user workflow.

## Next major value selection

Select a **desktop GECK authoring review workflow contract** as the next
post-v0.1 slice.

The contract must make the existing backend understandable and operable from
the Windows application without weakening any gate:

1. inspect project/intent/plan readiness and show exact blocking diagnostics;
2. preview and generate the existing authoring plan;
3. preview and generate the existing read-only observer bundle;
4. explain the manual FNVEdit boundary and Gate 534 deferred state without an
   automatic launch or script installation button;
5. accept an explicitly selected project-contained raw-observation file;
6. preview and run the existing Forge report sealer and Gate 522 parser;
7. display exact output paths, digests, safety state, and semantic diagnostics;
8. route to existing GECK Handoff, xEdit Audit, Project Outputs, and diagnostic
   surfaces rather than duplicating their responsibilities.

## Required boundaries

- Reuse the Gate 521-533 generators, parser, CLI contracts, and diagnostics;
  do not create a second correctness implementation in WPF.
- Keep canonical source in the project and generated evidence disposable.
- Do not add a CLI alias, schema family, plugin writer, xEdit launcher, script
  installer, load-order selector, MO2 mutator, game Data writer, or approval
  bypass.
- Do not present synthetic verification as real FNVEdit compatibility.
- Preserve the Gate 514 v0.1 freeze; this is post-v0.1 planning and does not
  alter the current user-test candidate.
- Use existing grouped navigation and avoid another unstructured peer tab.
- Keep all tests synthetic and redistributable.

## Gate 536 research contract

Gate 536 must research and define the desktop workflow before implementation:

- map current GECK Handoff, xEdit Audit, Project Outputs, backend bridge, and
  cancellation ownership;
- decide extension versus dedicated Review workspace using current navigation
  and responsive-layout evidence;
- define phase/readiness states entirely from current project and generated
  evidence;
- define preview/apply boundaries, file pickers, stale-state invalidation,
  cancellation, output opening, and diagnostic projection;
- define UI Automation identities and focused Windows tests;
- define installed/publication proof while keeping external execution and real
  plugin fixtures out of scope.

Gate 536 must stop at an implementation-ready contract. It must not edit WPF,
launch a process, create plugin bytes, install scripts, or claim FNVEdit
compatibility.

## Validation

- Re-read ADR-004, ADR-009, ADR-010, ADR-013, R009, Gates 511-514, and Gates
  522, 532-534.
- Searched current desktop source and Windows tests for all three authoring
  target IDs; no desktop integration exists.
- Confirmed grouped Build/Review/System navigation and the existing GECK
  Handoff, xEdit Audit, and Project Outputs routes remain present.
- No production code, schema, fixture, desktop file, or generated output
  changed, so the Gate 533 901-test result was not rerun.

## Actions withheld

- No WPF, backend bridge, CLI, generator, parser, schema, or test implementation.
- No FNVEdit, xEdit, GECK, MO2, game, provider, or external process launch.
- No plugin, game Data, load order, profile, tool-directory, or external-state
  operation.
- No publication, installer, signing, remote, network, or AI action.

## Next route

Gate 536: research and define the desktop GECK authoring review workflow
contract described above, then stop before implementation or external
execution. Gate 534 remains independently resumable under its recorded exact
operator-controlled prerequisites.
