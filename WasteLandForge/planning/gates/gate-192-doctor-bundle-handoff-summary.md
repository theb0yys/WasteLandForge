# Gate 192 - Doctor Bundle Handoff Summary

Status: Complete

## Purpose

Add a concise Doctor export bundle sidecar so `README.md` can point a human
operator at remediation, immediate worklist items, command hints, and key
archive paths without requiring them to read every generated index first.

## Research grounding

- Documented: R006/ADR-010 defines `forge doctor export` as an offline-first
  handoff workflow command and requires the CLI command surface to stay small,
  stable, and free of aliases outside the canonical surface.
- Documented: R008/ADR-011 says JSON is canonical, Markdown is a projection,
  generated evidence should be deterministic and fixture-backed, and local
  workflows must remain offline-first and AI-optional.
- Documented: Gate 185 adds bundle triage index entries.
- Documented: Gate 187 adds canonical Doctor command hints.
- Documented: Gate 188 adds ordered Doctor remediation worklist entries.
- Documented: Gate 190 adds remediation status header metadata.
- Documented: Gate 191 adds operator handoff checklists to primary and bundle
  triage Markdown.
- Inferred: A bundle-level Markdown sidecar is safe when it only formats
  existing redacted triage metadata and links to existing archive entries.
- Open: Provider version parsing, runtime confirmation, MO2 effective
  visibility, JIP PP LN catalogue policy, and GECK Extender mixed-scope policy
  remain unresolved.

## Implemented

- `forge doctor export --bundle <path>` archives now include
  `handoff-summary.md`.
- `handoff-summary.md` includes:
  - remediation status and headline
  - work item counts
  - first work item, first command hint, first canonical command, and first
    bundle path when present
  - a short immediate worklist with resolved command strings
  - command hints with their bundle paths and purposes
  - key archive paths for the full report, triage, navigation, summary,
    diagnostic, requirement, evidence, redaction, manifest, and checksum
    entries
- Archive `README.md` links to `handoff-summary.md` in `Start Here`.
- `bundle/index.json` and `bundle/index.md` list `handoff-summary.md`.
- `doctor-bundle-manifest.json` and `checksums.sha256` include
  `handoff-summary.md`.
- Golden tests cover review and blocked bundle cases.

## Not implemented

- No JSON contract change.
- No command execution.
- No new slash command alias.
- No CLI alias outside ADR-010/R006 canonical command names.
- No `--format zip` or `--format markdown`.
- No new primary output format.
- No Doctor export SARIF or GitHub mode.
- No GitHub step-summary behavior for Doctor export.
- No provider detector, runtime probe, resolver behavior, Doctor planning
  behavior, diagnostic rule ID, provider-version parser, MO2 VFS inspection,
  GECK automation, network check, release publishing, catalogue-policy
  decision, or AI behavior.

## Exit evidence

| Evidence | Status |
|---|---|
| Bundle archive includes `handoff-summary.md` | Complete |
| README links `handoff-summary.md` | Complete |
| Bundle index lists `handoff-summary.md` | Complete |
| Manifest and checksums include `handoff-summary.md` | Complete |
| Sidecar derives from existing triage metadata | Complete |
| Sidecar includes remediation, worklist, command hints, and key archive paths | Complete |
| Raw local paths remain omitted from sidecar payloads | Complete |
| Provider policy gaps remain open | Complete |

## Validation

- `dotnet build --no-restore` passed with 0 warnings and 0 errors.
- `dotnet test tests\WastelandForge.GoldenTests\WastelandForge.GoldenTests.csproj --no-build --no-restore --filter "FullyQualifiedName~DoctorExport"` passed.
- `dotnet test WastelandForge.sln --no-build --no-restore -m:1` passed.
- CLI smoke export inspected `handoff-summary.md`, `README.md`, and
  `bundle/index.md`, confirmed sidecar command hints and key archive paths,
  and confirmed the fixture project root path was absent from the sidecar.
- An initial smoke assertion failed because the smoke script used PowerShell
  wildcard matching against backtick-delimited Markdown. The inspected archive
  content was correct, and the smoke was rerun with literal string checks.
- `git diff --check` exited 0 with only Git LF-to-CRLF working-copy warnings.
- Touched-path protected scan returned no protected-file matches.

## Next gate

Gate 193 should continue documented capability/Doctor value from existing
local metadata. A practical next slice is a scan-side operator checklist
projection for `forge capabilities scan` text and Markdown summary output,
derived from existing requirement, diagnostic, action, and command guidance
without adding provider-version, runtime, parser, MO2, GECK, or
catalogue-policy resolution behavior.
