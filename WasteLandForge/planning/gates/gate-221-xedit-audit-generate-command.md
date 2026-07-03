# Gate 221 - xEdit Audit Generate Command

Status: Complete

## Purpose

Wire canonical `forge generate --target xedit-audit` to the local xEdit audit
script scaffold emitter.

This gate exposes the existing local generated evidence path through the
ADR-010 CLI without adding aliases, xEdit process execution, xEdit report
parsing, plugin patch generation, plugin mutation, MO2 automation, GECK
automation, runtime probes, real third-party plugin fixtures,
`forge build --target xedit-audit`, or package/release behavior.

## Research grounding

- Documented: ADR-010 defines `forge generate` as part of the canonical
  command surface.
- Documented: R006 says xEdit support should stay narrow: audit scripts,
  inspection scripts, scaffolds, and report parsers, not silent high-risk
  patch authoring.
- Documented: ADR-009 requires generated artifacts to be disposable,
  rebuildable, and traceable.
- Inferred: the first xEdit CLI target should expose already-local scaffold
  evidence only, matching the JIP script generation command pattern while
  keeping xEdit execution out of the correctness path.

## Implemented

Gate 221 implements:

- `forge generate --target xedit-audit`,
- JSON output for generated scaffold, manifest, checksum, diagnostic, and
  output digest evidence,
- human/plain output for generated scaffold evidence,
- generate help text for `xedit-audit`,
- usage rejection for `--output` and `--dry-run` on this target,
- usage rejection for `forge build --target xedit-audit`,
- golden CLI coverage for successful generation and unsupported boundaries.

## Not implemented

Gate 221 does not implement:

- xEdit process execution,
- xEdit report parsing,
- plugin patch generation,
- plugin mutation,
- MO2 automation,
- GECK automation,
- runtime probes,
- real third-party plugin fixtures,
- `forge build --target xedit-audit`,
- `forge package --target xedit-audit`,
- release behavior.

## Exit Evidence

| Evidence | Status | Notes |
|---|---|---|
| `forge generate --target xedit-audit` writes scaffold evidence | Complete | Uses `XEditAuditScriptScaffoldEmitter`. |
| JSON output reports scaffold, manifest, checksum, and digest paths | Complete | Uses root `generated/xedit-audit`. |
| Help lists the canonical target | Complete | No alias added. |
| `--output` is rejected | Complete | Target remains fixed to `generated/xedit-audit` in this gate. |
| `forge build --target xedit-audit` is rejected | Complete | Build/package remain future work. |
| xEdit is not executed | Complete | CLI only writes local generated evidence. |

## Validation

Required validation:

- golden CLI tests for the new target,
- .NET build/test smoke,
- protected-file scan.

## Next Gate

Gate 222 should add a non-executing xEdit audit report parser contract using
synthetic JSON report fixtures only. It should stop before xEdit process
execution, plugin patch generation, plugin mutation, MO2 automation, GECK
automation, runtime probes, real third-party plugin fixtures,
`forge build --target xedit-audit`, package/release behavior, or applying
parsed report findings to plugins.
