# Gate 164 - Doctor Export Markdown Summary

Status: Complete

## Purpose

Gate 164 adds a redacted Markdown sidecar summary for `forge doctor export`
through `--summary <path>`. The summary is derived from the already redacted
Doctor export report, so authors can attach or inspect a compact handoff
without opening the full JSON or plain bundle.

This is a reporting gate only. It does not add Doctor export SARIF/GitHub
mode, new diagnostics, new provider detection, provider-version parsing,
runtime probes, MO2 VFS checks, GECK automation, network operations, or AI
behavior.

## Research grounding

- Documented: R006/ADR-010 says machine-facing output should stay stable and
  explicit, with Markdown summaries as an integration/reporting surface.
- Documented: R006 says `forge doctor export` is a canonical command for
  redacted handoff bundles.
- Documented: R005/ADR-008 says capability/provider detection remains
  local-first and deterministic; runtime probes are enrichment, not the
  correctness path.
- Documented: ADR-011 requires deterministic fixture-backed tests and
  offline-first, AI-optional governance.
- Inferred: A Markdown Doctor export sidecar is safe because it is derived
  only from the redacted `DoctorExportReport` already produced by the command.
- Open: Provider-version parsing, runtime confirmation, MO2 effective-scope
  visibility, JIP PP LN alias policy, and GECK Extender safe marker policy
  remain unresolved.

## Implemented

Gate 164 implements:

- `forge doctor export --summary <path>`,
- a redacted `DoctorExportMarkdownRenderer`,
- Markdown sections for summary counts, Doctor areas, next actions, project
  requirements, diagnostics, and open questions,
- CLI help text for the new sidecar option,
- golden CLI coverage for Markdown sidecar writing and missing summary path
  parsing,
- documentation and `/forge` routing updates.

## Not implemented

Gate 164 does not implement:

- `--format markdown`,
- Doctor export SARIF output,
- Doctor export GitHub annotation output,
- GitHub step-summary emission for Doctor export,
- new provider detectors,
- new capability resolution behavior,
- new requirement resolution behavior,
- new Doctor planning behavior,
- new diagnostic projection behavior,
- catalogue policy resolution,
- provider version parsing,
- runtime probes,
- MO2 profile or VFS visibility checks,
- mixed/effective-scope diagnostics,
- GECK automation,
- new `WF-CAP-*` rule IDs,
- AI-generated explanations.

## Exit evidence

| Evidence | Status | Notes |
|---|---|---|
| Markdown renderer | Complete | The renderer consumes the redacted Doctor export report only. |
| CLI option | Complete | `forge doctor export --summary <path>` writes the Markdown sidecar beside primary human/plain/json output. |
| Redaction retained | Complete | Summary tests assert local project, game, and tool paths do not appear. |
| Runtime mutation avoided | Complete | No game, MO2, GECK, runtime, network, or AI operation is used. |

## Validation

Targeted checks run during implementation:

```text
dotnet build --no-restore
dotnet test tests/WastelandForge.GoldenTests/WastelandForge.GoldenTests.csproj --no-build --no-restore --filter "FullyQualifiedName~DoctorExport"
```

## Next gate

Gate 165 should continue documented capability/Doctor value without resolving
open provider policy questions. Provider-version evidence should only start
when documented file, runtime, parser, and catalogue-policy evidence is ready.
