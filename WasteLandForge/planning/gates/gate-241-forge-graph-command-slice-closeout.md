# Gate 241 - forge graph Command Slice Closeout

Status: Complete

## Purpose

Close the current `forge graph` command metadata lane and prevent drift into
another chain of graph-only metadata micro-gates.

The graph slice now has enough deterministic value for v0.1 planning: Forge can
emit local project source graph evidence, declaration-only capability
requirement links, catalogue capability/provider links, generator target links,
generated artifact expectation links, manifest provenance reference links, and
generated/dist output boundary links under `generated/graph`.

The next implementation lane should move to the already-canonical top-level
`forge explain` command. That lane should start with a subject contract and
planning skeleton for diagnostic, target, output, capability, and provenance
explanations rather than adding more graph edges.

## Research grounding

- Documented: ADR-010 and R006 define both `forge graph` and `forge explain`
  as canonical CLI commands and reserve `forge explain` for provenance and
  reasoned explanation of diagnostics, targets, outputs, and capabilities.
- Documented: R006 frames `forge explain` as the human "why/how" companion to
  graph and build-plan workflows while keeping JSON and SARIF contracts stable
  for automation.
- Documented: ADR-009 says generated artifacts are disposable and rebuildable
  and must carry provenance through local manifest evidence.
- Documented: ADR-011 requires deterministic fixture-backed testing,
  offline-first behavior, and no required AI.
- Inferred: Further graph-only metadata layers now have lower v0.1 value than
  starting the `forge explain` lane, because the existing graph already exposes
  source, capability, generator, artifact expectation, manifest reference, and
  output-boundary relationships.

## Implemented

Gate 241 implements:

- planning closeout for the Gate 236-240 `forge graph` command slice,
- documentation updates that mark graph metadata work as parked unless
  explicitly reopened,
- routing updates that keep `/forge graph` mapped to the real CLI while
  preventing accidental drift into graph visualization, execution, provider
  resolution, manifest reads, or artifact existence checks,
- a next-gate direction focused on top-level `forge explain` subject planning.

## Not implemented

Gate 241 does not implement:

- new `forge graph` behavior,
- graph visualization formats,
- `forge graph --subject`,
- generated manifest reads,
- generated artifact existence checks,
- generator execution from `forge graph`,
- build planning or execution changes,
- capability scan behavior changes,
- runtime provider resolution,
- provider status claims,
- top-level `forge explain` command behavior,
- `forge explain` subject parsing,
- package/release behavior changes,
- xEdit process execution,
- plugin patch generation,
- plugin mutation,
- MO2 automation,
- GECK automation,
- runtime probes,
- real third-party plugin fixtures,
- AI behavior.

## Exit Evidence

| Evidence | Status | Notes |
|---|---|---|
| Graph slice closeout recorded | Complete | Gate 236-240 are treated as the complete current graph metadata lane. |
| Next development direction changed | Complete | Next gate moves to top-level `forge explain` subject planning. |
| Command surface preserved | Complete | No command or alias is introduced. |
| Runtime mutation avoided | Complete | No Data, MO2, GECK, game runtime state, xEdit process, plugin file, generated manifest, or generated artifact payload is touched. |
| Verification scope unchanged | Complete | Existing graph behavior remains as implemented by earlier gates. |

## Validation

Gate 241 is a planning/routing closeout gate. Required validation is doc and
routing consistency plus normal local build/test smoke checks.

## Next Gate

Gate 242 starts the top-level `forge explain` lane with a subject contract and
planning skeleton as help and reserved JSON metadata. Gate 243 implements the
`forge explain diagnostic <rule-id>` subject skeleton. Gate 244 should add
rule-specific diagnostic explanation metadata while stopping before generated
manifest reads, generated artifact existence checks, build planning changes,
generator execution changes, runtime provider resolution, capability scan
behavior changes, graph visualization formats, package/release behavior, xEdit
process execution, plugin mutation, MO2 automation, GECK automation, runtime
probes, real third-party plugin fixtures, or AI behavior.
