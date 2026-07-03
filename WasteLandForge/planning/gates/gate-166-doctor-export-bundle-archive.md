# Gate 166 - Doctor Export Bundle Archive

Status: Complete

## Purpose

Add `forge doctor export --bundle <path>` as a deterministic redacted ZIP
sidecar for Doctor handoff. The archive contains the existing redacted Doctor
JSON report, the redacted Markdown summary, a local bundle manifest, and
checksums. It is a local handoff artifact only.

## Research grounding

- Documented: R006/ADR-010 defines `forge doctor export` as the canonical
  Doctor handoff command and states it can arrive early when implemented as a
  pure local bundle exporter using shared contracts.
- Documented: R006 requires the CLI to remain small, offline-first,
  AI-optional, and stable for scripts.
- Documented: ADR-011 requires deterministic fixture-backed testing and local
  evidence for release/governance workflows.
- Inferred: A deterministic ZIP sidecar is safe in this gate because it
  archives already-redacted Doctor export JSON and Markdown rather than
  adding detector behavior, provider policy, or release publishing.
- Open: Provider version parsing, runtime confirmation, MO2 effective
  visibility, JIP PP LN catalogue policy, and GECK Extender mixed-scope policy
  remain unresolved.

## Implemented

- `forge doctor export --bundle <path>` writes a deterministic ZIP archive.
- Archive entries are stable and path-minimized:
  - `doctor-export.json`
  - `doctor-export.md`
  - `doctor-bundle-manifest.json`
  - `checksums.sha256`
- The archive uses sorted ZIP entries and stable entry timestamps.
- The manifest records the archive kind, source Doctor export kind, redaction
  mode, and checksummed entry metadata.
- CLI help documents the new sidecar option.
- Golden CLI tests open the ZIP and verify entry names, timestamps, manifest
  metadata, checksums, and local path redaction.

## Not implemented

- No new command alias.
- No `--format zip` or `--format markdown`.
- No Doctor export SARIF or GitHub mode.
- No GitHub step-summary behavior for Doctor export.
- No provider detector, runtime probe, resolver behavior, Doctor planning
  behavior, diagnostic rule ID, provider-version parser, MO2 VFS inspection,
  GECK automation, network check, release publishing, or AI behavior.

## Exit evidence

| Evidence | Status |
|---|---|
| `forge doctor export --bundle <path>` writes a ZIP archive | Complete |
| Archive entries are deterministic and sorted | Complete |
| Archive includes JSON, Markdown, manifest, and checksums | Complete |
| Local paths are redacted inside archive payloads | Complete |
| Missing bundle path is a usage error | Complete |
| Provider policy gaps remain open | Complete |

## Validation

- `dotnet build --no-restore`
- `dotnet test tests\WastelandForge.GoldenTests\WastelandForge.GoldenTests.csproj --no-build --no-restore --filter "FullyQualifiedName~DoctorExport"`
- Full solution test suite and final hygiene checks are required before
  reporting the gate complete.

## Next gate

Gate 167 should continue documented capability/Doctor value without resolving
open provider policy questions. Provider-version evidence should only start
when documented file, runtime, parser, and catalogue-policy evidence is ready.
