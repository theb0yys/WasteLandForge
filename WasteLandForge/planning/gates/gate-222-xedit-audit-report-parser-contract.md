# Gate 222 - xEdit Audit Report Parser Contract

Status: Complete

## Purpose

Add a non-executing xEdit audit report parser contract using synthetic JSON
report fixtures only.

This gate starts the report-reader half of the xEdit lane while staying before
xEdit process execution, plugin patch generation, plugin mutation, MO2
automation, GECK automation, runtime probes, real third-party plugin fixtures,
`forge build --target xedit-audit`, package/release behavior, or applying
parsed report findings to plugins.

## Research grounding

- Documented: R006 says xEdit support should stay narrow: audit scripts,
  inspection scripts, scaffolds, and report parsers, not silent high-risk
  patch authoring.
- Documented: R006 says high-risk outputs such as binary plugin generation,
  record merge patches, dialogue record rewrites, quest patching, and complex
  conflict-fixing patches should be deferred.
- Documented: ADR-011 requires public fixtures to be synthetic and
  redistributable.
- Inferred: the first report parser should consume a synthetic JSON contract
  fixture from `generated/xedit-audit/reports` and return typed report
  evidence plus diagnostics without invoking xEdit.

## Implemented

Gate 222 implements:

- `XEditAuditReportParser`,
- typed report, safety, summary, record, and finding records,
- `XEditAuditReportParserResult`,
- synthetic JSON report fixture under `fixtures/xedit-audit-reports`,
- `WF-GEN-009` diagnostics for missing or invalid synthetic report evidence,
- tests proving report parsing, missing report diagnostics, malformed JSON
  diagnostics, no scaffold emission, no manifest/checksum writes, and no
  `Data` writes.

## Not implemented

Gate 222 does not implement:

- xEdit process execution,
- xEdit report generation,
- report schema publication,
- parser CLI wiring,
- plugin patch generation,
- plugin mutation,
- MO2 automation,
- GECK automation,
- runtime probes,
- real third-party plugin fixtures,
- `forge build --target xedit-audit`,
- package/release behavior,
- applying parsed findings to plugins.

## Exit Evidence

| Evidence | Status | Notes |
|---|---|---|
| Synthetic JSON report fixture parses | Complete | Uses `fixtures/xedit-audit-reports/synthetic-record-inspection.json`. |
| Parsed records and findings are typed | Complete | Includes safety, summary, record, and finding data. |
| Missing reports are diagnostic | Complete | Uses `WF-GEN-009`. |
| Malformed JSON is diagnostic | Complete | Uses `WF-GEN-009`. |
| Parser does not emit scaffolds or sidecars | Complete | No script, manifest, or checksum writes. |
| Runtime/plugin mutation avoided | Complete | No `Data`, plugin, MO2, GECK, or runtime path is touched. |

## Validation

Required validation:

- unit tests for parser success and failure diagnostics,
- .NET build/test smoke,
- protected-file scan.

## Next Gate

Gate 223 should add xEdit audit report parser evidence projection for
machine/human handoff without CLI command wiring. It should stop before xEdit
process execution, report generation, plugin patch generation, plugin
mutation, MO2 automation, GECK automation, runtime probes, real third-party
plugin fixtures, `forge build --target xedit-audit`, package/release behavior,
or applying parsed report findings to plugins.
