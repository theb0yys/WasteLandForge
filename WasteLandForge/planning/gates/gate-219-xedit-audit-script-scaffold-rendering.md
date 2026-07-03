# Gate 219 - xEdit Audit Script Scaffold Rendering

Status: Complete

## Purpose

Render non-executing xEdit audit script scaffolds from the validated Gate 218
audit plan and write them only under `generated/xedit-audit/scripts`.

This gate turns the xEdit lane from evidence planning into a first generated
text artifact while staying before xEdit process execution, xEdit report
parsing, plugin patch generation, plugin mutation, MO2 automation, GECK
automation, runtime probes, and real third-party plugin fixtures.

## Research grounding

- Documented: the generator/build research says xEdit support is worthwhile
  only when kept narrow: audit scripts, inspection scripts, scaffolds, and
  report parsers, not silent high-risk patch authoring.
- Documented: ADR-009 says generated artifacts are disposable and rebuildable
  and every output must be traceable.
- Documented: ADR-011 keeps public fixtures synthetic and redistributable.
- Inferred: the first generated xEdit artifact should be a scaffold file that
  records audit intent and safety limits without asserting full xEdit runtime
  syntax or executing the tool.

## Implemented

Gate 219 implements:

- `XEditAuditScriptScaffoldEmitter`,
- generated scaffold document/file/result records,
- UTF-8 no-BOM `.pas` scaffold output under `generated/xedit-audit/scripts`,
- output digest evidence for generated scaffold files,
- tests proving generated file placement, scaffold content, no report output,
  no `Data` writes, and validation-error no-write behavior.

## Not implemented

Gate 219 does not implement:

- `forge generate --target xedit-audit`,
- generated manifest or checksum sidecar files,
- xEdit process execution,
- xEdit report parsing,
- plugin patch generation,
- plugin mutation,
- MO2 automation,
- GECK automation,
- runtime probes,
- real third-party plugin fixtures.

## Exit Evidence

| Evidence | Status | Notes |
|---|---|---|
| Scaffold file is written under `generated/xedit-audit/scripts` | Complete | Uses the validated `outputs.script` path. |
| Scaffold content records audit intent | Complete | Includes audit id, intent, mode, report path, target plugins, record types, and required capabilities. |
| xEdit is not executed | Complete | The emitter only writes text files. |
| Report path remains metadata | Complete | No report file is created. |
| Runtime/plugin mutation avoided | Complete | No `Data`, plugin, MO2, GECK, or runtime path is touched. |
| Validation errors stop output | Complete | Broken fixture produces no generated directory. |

## Validation

Required validation:

- unit tests for scaffold emission and no-write failures,
- .NET build/test smoke,
- protected-file scan.

## Next Gate

Gate 220 should add local generated evidence for xEdit audit scaffolds:
`xedit-audit-script-manifest.json` and `checksums.sha256` under
`generated/xedit-audit`. It should stop before `forge generate --target
xedit-audit`, xEdit process execution, xEdit report parsing, plugin patch
generation, plugin mutation, MO2 automation, GECK automation, runtime probes,
or real third-party plugin fixtures.
