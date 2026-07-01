# Gate 90 - MCM Extender Package Verify-Existing Markdown Summary

Status: Complete

## Purpose

Gate 90 adds Markdown diagnostic summary file output for existing MCM Extender
package evidence verification.

The command shape stays under the Gate 87 decision:

```text
forge package --target mcm-json --verify-existing --summary <path>
```

This gate reuses the Gate 11 Markdown diagnostic renderer for the same
canonical `DiagnosticReport` used by the verify-existing human, JSON, SARIF,
and GitHub outputs.

## Research Grounding

- Documented: ADR-010 defines the canonical command surface and says not to
  introduce undocumented convenience aliases.
- Documented: ADR-010 and ADR-011 identify Markdown summaries as projections
  from canonical diagnostic issue data.
- Documented: ADR-011 requires stable rule IDs, deterministic fixture-backed
  testing, and offline-first correctness.
- Inferred: `--summary <path>` can safely extend
  `forge package --target mcm-json --verify-existing` because the command is
  already a diagnostic package evidence mode and Gate 11 established Markdown
  summaries as a sidecar output, not a `--format` value.
- Open: Checksum-file revalidation for existing package evidence remains a
  separate later package-verifier gate.

## Scope

Gate 90 implements:

- `forge package --target mcm-json --verify-existing --summary <path>`,
- Markdown summary output for clean and blocking package verification results,
- summary writing alongside human/plain/json/SARIF/GitHub primary output,
- continued GitHub step-summary append for `--format github`,
- duplicate-write avoidance when `--summary` and `GITHUB_STEP_SUMMARY` point
  at the same file,
- continued rejection of `forge package --summary <path>` when
  `--verify-existing` is not set,
- golden CLI coverage for success summaries, blocking diagnostic summaries,
  and non-diagnostic package summary rejection.

Gate 90 does not implement:

- standalone verifier commands or aliases,
- `forge package verify`,
- package regeneration in verify-existing mode,
- Data or MO2 installation,
- MO2 VFS/profile conflict inspection,
- game launch or runtime probe verification,
- FOMOD package creation,
- plugin record generation,
- new diagnostic rule families, rule IDs, or schemas.

## Validation Mapping

The command now:

1. Parses `--summary <path>` for package verify-existing mode.
2. Rejects `--summary <path>` for normal package generation.
3. Runs the existing file-based verifier over generated package evidence.
4. Renders the verifier `DiagnosticReport` through the existing Markdown
   diagnostic projection.
5. Preserves exit code `0` for no blocking diagnostics and `1` for blocking
   diagnostics.

## Validation Results

- Pending final local validation for this gate.

## Open Checks

| Check | Status | Notes |
|---|---|---|
| Package verify-existing Markdown summary file | Complete | Uses existing canonical Markdown diagnostic projection. |
| Clean package summary coverage | Complete | Golden test covers no-diagnostic summary output. |
| Blocking package summary coverage | Complete | Golden test covers `WF-BUILD-006` summary output. |
| Normal package generation summary rejection | Complete | `--verify-existing` is required for package diagnostic summaries. |
| Checksum-file revalidation | Open | Candidate for Gate 91. |
| Data/MO2 install verification | Open | Requires separate safety and environment model. |
| In-game MCM Extender visibility | Open | Requires runtime evidence and launch/probe design. |

## Next Gate

Gate 91 should add checksum-file revalidation for
`forge package --target mcm-json --verify-existing`.
