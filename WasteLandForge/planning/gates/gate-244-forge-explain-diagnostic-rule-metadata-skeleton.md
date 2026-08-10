# Gate 244 - forge explain Diagnostic Rule Metadata Skeleton

Status: Complete

## Purpose

Extend the first executable top-level `forge explain` subject:

```text
forge explain diagnostic <rule-id>
```

This gate adds deterministic rule-specific metadata for documented concrete
rule IDs while preserving the Gate 243 family-level fallback for valid
reserved rule IDs that do not yet have embedded concrete metadata.

## Research grounding

- Documented: R006 and ADR-010 define `forge explain` as a canonical
  offline-first CLI command for diagnostic IDs, targets, output paths,
  capability IDs, provenance, and reasoned explanations.
- Documented: R004 and ADR-007 require stable diagnostic IDs and
  deterministic diagnostic projections from the canonical diagnostic model.
- Documented: R008 and ADR-011 require layered validation, deterministic
  tests, and governance around reserved rule families.
- Documented: Gate 243 implemented `forge explain diagnostic <rule-id>` as a
  deterministic family-level skeleton.
- Inferred: Embedding documented concrete rule metadata is the next safe value
  step because it improves local explanations without reading project
  diagnostics or generated evidence.

## Implemented

Gate 244 implements:

- `ExplainDiagnosticRuleDetail` metadata records,
- documented concrete metadata for current loader, schema, semantic,
  capability, asset, generator, build, and release rule IDs,
- plain output showing documentation status, rule title, summary, and source,
- JSON output with a `rule` object and `rule-specific-metadata-skeleton`
  detail status when documented rule metadata exists,
- JSON fallback with `reserved-family-only` status when a valid reserved rule
  ID has no embedded concrete metadata,
- golden CLI tests for documented rule metadata and reserved-family fallback.

## Not implemented

Gate 244 does not implement:

- project diagnostic report lookup,
- generated manifest reads,
- generated artifact existence checks,
- provenance sidecar reads,
- target explanation execution,
- output explanation execution,
- capability subject execution,
- provenance subject execution,
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
| Documented rule metadata shown | Complete | `WF-CAP-004` plain output includes concrete rule title and source. |
| JSON rule object shown | Complete | `WF-GEN-001` JSON includes documented concrete rule metadata and false execution flags. |
| Reserved fallback preserved | Complete | `WF-GOV-001` remains valid and reports family-level fallback metadata. |
| Runtime mutation avoided | Complete | No project files, generated manifests, provenance sidecars, artifacts, provider evidence, external tools, or AI calls are used. |

## Validation

Gate 244 requires build, targeted explain golden tests, full local tests, and
whitespace checks.

## Next Gate

Gate 245 should implement the next `forge explain` subject skeleton:
`forge explain target <target-id>`. It should explain documented generator,
build, docs, graph, package, and release targets from deterministic local
metadata only, and stop before build planning changes, generator execution,
generated manifest reads, generated artifact existence checks, provider
resolution, capability scan behavior changes, graph visualization formats,
package/release behavior changes beyond explanation metadata, external tool
execution, plugin mutation, MO2 automation, GECK automation, runtime probes,
real third-party plugin fixtures, or AI behavior.
