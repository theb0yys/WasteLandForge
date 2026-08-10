# Gate 243 - forge explain Diagnostic Subject Skeleton

Status: Complete

## Purpose

Implement the first executable top-level `forge explain` subject:

```text
forge explain diagnostic <rule-id>
```

This gate explains a diagnostic rule ID at the reserved rule-family level. It
does not introduce a full rule catalogue, inspect project diagnostics, read
generated manifests, or infer rule-specific causes from local files.

## Research grounding

- Documented: R006 and ADR-010 define `forge explain` as a canonical
  offline-first CLI command for diagnostic IDs, targets, output paths,
  capability IDs, provenance, and reasoned explanations.
- Documented: R004 and ADR-007 require stable diagnostic IDs and
  JSON/console/SARIF projections from a canonical diagnostic model.
- Documented: R008 and ADR-011 reserve rule families including `WF-LOAD-*`,
  `WF-SCHEMA-*`, `WF-SEM-*`, `WF-CAP-*`, `WF-ASSET-*`, `WF-GEN-*`,
  `WF-BUILD-*`, `WF-REL-*`, `WF-GOV-*`, and `WF-SEC-*`.
- Documented: Gate 242 defined the top-level `forge explain` subject contract
  and selected diagnostic as the first subject skeleton.
- Inferred: A rule-family explanation is the safe first executable slice
  because it is deterministic, local, and useful without requiring a full rule
  catalogue or project file inspection.

## Implemented

Gate 243 implements:

- `forge explain diagnostic <rule-id>` parsing,
- rule ID validation through the existing `RuleId` reserved-family parser,
- family-level diagnostic explanation for every reserved rule family,
- plain/human and JSON output,
- usage JSON for invalid diagnostic rule IDs,
- help text that marks `diagnostic` implemented and the other top-level
  explain subjects reserved,
- golden CLI tests for plain output, JSON output, invalid rule IDs, and the
  existing reserved-subject JSON path.

## Not implemented

Gate 243 does not implement:

- rule-specific catalogue lookup,
- project diagnostic report lookup,
- target explanation execution,
- output explanation execution,
- capability subject execution,
- provenance subject execution,
- generated manifest reads,
- generated artifact existence checks,
- provenance sidecar reads,
- build planning changes,
- generator execution changes,
- runtime provider resolution,
- capability scan behavior changes,
- graph visualization formats,
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
| Diagnostic subject executes | Complete | `forge explain diagnostic <rule-id>` returns exit code 0 for valid reserved rule IDs. |
| Reserved rule family metadata used | Complete | Output is derived from the existing rule-family scopes and command surface only. |
| Invalid rule IDs rejected | Complete | Invalid IDs return usage exit code 2. |
| Other explain subjects stay reserved | Complete | `target`, `output`, `capability`, and `provenance` still use reserved status metadata. |
| Runtime mutation avoided | Complete | No Data, MO2, GECK, game runtime state, xEdit process, plugin file, generated manifest, provenance sidecar, or generated artifact payload is touched. |

## Validation

Gate 243 requires build, targeted explain golden tests, full local tests, and
whitespace checks.

## Next Gate

Gate 244 should add a rule-specific diagnostic explanation metadata skeleton
for currently documented concrete rule IDs, still using deterministic local
metadata only. It should stop before project diagnostic report lookup,
generated manifest reads, generated artifact existence checks, build planning
changes, generator execution changes, runtime provider resolution, capability
scan behavior changes, graph visualization formats, package/release behavior,
xEdit process execution, plugin mutation, MO2 automation, GECK automation,
runtime probes, real third-party plugin fixtures, or AI behavior.
