# Dialogue Response Route Taxonomy Evidence Pack

Status: Skeleton
Gate: 56
Research classification: Open
Decision base: Gate 50, Gate 51, Gate 52, Gate 53, Gate 54, Gate 55, ADR-003, ADR-007, ADR-011

## Purpose

This evidence pack defines the evidence Forge needs before it implements a
dialogue response route taxonomy.

Gate 56 does not define allowed `responseRoutes[].routeKey` values. It does
not define response selection ordering, Speech Challenge success/failure
routing, condition-aware route execution, GECK export behavior, plugin output
shape, or any new diagnostic rule.

The pack exists because Gate 55 recorded route taxonomy as evidence-blocked.
Forge can validate local response route shape and ambiguity today, but route
meaning still needs GECK documentation and representative shipped-record
inspection before implementation.

## Evidence Boundary

Allowed public evidence:

- structural summaries of GECK dialogue fields,
- references to public documentation used by the research reports,
- project-authored notes from local GECK or xEdit inspection,
- plugin names, Editor IDs, FormIDs, and hashes when used as implementation
  references,
- paraphrased findings about field relationships, ordering, and branching,
- synthetic examples that do not copy shipped game or third-party mod content.

Disallowed public evidence:

- copied shipped dialogue text,
- Bethesda-owned asset files or plugin records,
- third-party mod files without explicit permission,
- private user install paths,
- screenshots or exports that disclose copyrighted record content unless
  explicit permission exists.

Private extended evidence may be kept outside the public repository when it
depends on a local install or user-owned tools. Public docs should summarize
only the structural conclusions needed for Forge design.

## Required Evidence Classes

The narrative research says exact schema and generator details should be
verified against representative records before implementation. For response
route taxonomy, the minimum evidence classes are:

| Evidence class | Status | Required observation |
|---|---|---|
| GECK dialogue documentation | Open | Topic/info fields that affect route meaning: prompt, priority, link fields, conditions, Speech Challenge, and result scripts. |
| Major faction arc | Open | Response routes that branch a quest-owned conversation through faction or main-arc state. |
| Regional side-faction hub | Open | Response routes that branch a local quest hub through reputation, faction, skill, or quest state. |
| Companion quest | Open | Response routes that branch or record companion-specific memory, trust, history, or observation state. |
| Radio quest | Open | Dialogue route behavior for radio/news delivery where spoken dialogue and sound playback differ from normal topics. |
| Multi-ending side quest | Open | Response routes that expose mutually exclusive outcomes, fallback paths, or late consequences. |
| Synthetic Forge fixture mapping | Open | Redistributable example that mirrors the structural evidence without copying shipped content. |

## Evidence Item Template

Use this template for each evidence item:

```text
Evidence ID:
Status: Open | Reviewed | Rejected | Superseded
Classification: Documented | Inferred | Open
Source type: GECK docs | local GECK inspection | xEdit inspection | synthetic fixture
Representative class:
Source reference:
Implementation references:
Observed fields:
Route pattern observed:
Condition interaction:
Prompt and priority interaction:
Speech Challenge interaction:
Result-script interaction:
Fallback or default behavior:
Contradictions or uncertainty:
Taxonomy implication:
Selection-behavior implication:
Generator/plugin implication:
Can support implementation: No | Partly | Yes
Reviewer:
Reviewed date:
```

Do not mark `Can support implementation` as `Yes` until the item is specific
enough to justify a schema, validator, generator, or CLI behavior.

## Current Evidence Items

No evidence items are populated in Gate 56.

| Evidence ID | Status | Classification | Source type | Notes |
|---|---|---|---|---|
| EVID-RR-001 | Open | Open | GECK docs | Reserve for GECK dialogue field documentation summary. |
| EVID-RR-002 | Open | Open | Local inspection | Reserve for major faction arc inspection notes. |
| EVID-RR-003 | Open | Open | Local inspection | Reserve for regional side-faction hub inspection notes. |
| EVID-RR-004 | Open | Open | Local inspection | Reserve for companion quest inspection notes. |
| EVID-RR-005 | Open | Open | Local inspection | Reserve for radio quest inspection notes. |
| EVID-RR-006 | Open | Open | Local inspection | Reserve for multi-ending side quest inspection notes. |
| EVID-RR-007 | Open | Open | Synthetic fixture | Reserve for a redistributable structural fixture after evidence is reviewed. |

## Implementation Blockers

Forge must not implement these from this skeleton alone:

- allowed or reserved `routeKey` vocabulary,
- route-key enum validation,
- response route selection ordering or tie-breaking,
- Speech Challenge success or failure route semantics,
- prompt-route and response-route interaction,
- GECK `Link To` or `Link From` output mapping,
- plugin record generation from response routes,
- new `WF-SEM-*`, `WF-GEN-*`, or `WF-BUILD-*` rules about route meaning.

## Unlock Checklist

Before route taxonomy implementation starts, the evidence pack should show:

- [ ] GECK dialogue field documentation has been summarized with source
  references.
- [ ] At least one representative record has been inspected for every required
  evidence class.
- [ ] Structural findings are paraphrased without committed shipped content.
- [ ] Candidate route meanings are classified as `Documented`, `Inferred`, or
  `Open`.
- [ ] Contradictory or ambiguous observations are listed explicitly.
- [ ] Synthetic examples are derived from the observed structure, not copied
  content.
- [ ] The proposed implementation boundary says whether the next step is a
  schema change, semantic validator, generator mapping, CLI behavior, or
  another evidence pass.

## Open Questions

| Question | Status | Notes |
|---|---|---|
| Which route keys should Forge reserve? | Open | Gate 56 records no vocabulary decision. |
| Does route selection depend on GECK priority, condition order, link order, or another field? | Open | Requires documentation and record inspection. |
| How should Speech Challenge success and failure map to response routes? | Open | Gate 33 kept evaluation and routing semantics open. |
| Are route keys authored by Forge only, or do they map directly to GECK fields? | Open | Requires generator/export evidence. |
| Should route taxonomy be schema-level, semantic-level, or generator-level validation? | Open | Requires evidence from inspected records and implementation risk. |

## Next Evidence Pass

A later dialogue evidence gate should populate the GECK documentation evidence
item first. It should still avoid route taxonomy implementation unless the
evidence pack satisfies the unlock checklist.
