# Gate 128 - Redacted Local Doctor Export Bundle

Status: Complete

## Purpose

Gate 128 implements the first real `forge doctor export` command as a
redacted local handoff bundle built from the existing capability scan report.

The command stays within the ADR-010 surface: it implements
`forge doctor export` and does not add `/forge scan`, `forge doctor`, or any
other convenience alias. The bundle is intended for safe local handoff and
support workflows, not for runtime probing or automated game mutation.

## Research grounding

- Documented: R006 and ADR-010 include `forge doctor export` as the canonical
  Doctor handoff command and explicitly reject alternate names such as
  `forge doctor-integration`.
- Documented: R006 says core CLI workflows, including capability scanning and
  Doctor handoff, must be offline-first and AI-optional.
- Documented: R005 and ADR-008 say the Doctor boundary should reuse the common
  capability/provider catalogue and detector evidence while keeping Forge
  focused on developer projects and build environments.
- Documented: ADR-011 requires fixture-backed tests and local deterministic
  machine-readable output.
- Inferred: Until canonical `WF-CAP-*` diagnostics exist, the safest export
  skeleton is a redacted wrapper around the existing `CapabilityScanReport`
  instead of a new diagnostic issue model.

## Implemented

Gate 128 implements:

- `forge doctor export`,
- positional `[project-root]` and `--project <path>` project selection,
- `--game` / `--game-root`, `--data-root`, repeated `--tool-path`, `--output`,
  `-o`, `--format human|plain|json`, and `--no-input`,
- JSON output with command `doctor export`,
- bundle metadata `wastelandforge/doctor-handoff/v1`,
- embedded redacted `capabilities scan` data,
- redacted game root, data root, tool path, project root, and provider
  evidence paths,
- text output through the same redacted scan report,
- output-file writing,
- help text for `forge doctor` and `forge doctor export`,
- golden CLI coverage for the JSON export and output-file path.

## Not implemented

Gate 128 does not implement:

- new `WF-CAP-*` diagnostics,
- SARIF or GitHub projection for capability findings,
- runtime probes,
- MO2 profile or VFS visibility checks,
- provider version parsing,
- GECK automation,
- network checks,
- AI-generated explanation,
- player-install Doctor UX,
- archive/ZIP support for Doctor bundles.

## Exit evidence

| Evidence | Status | Notes |
|---|---|---|
| Command surface | Complete | `forge doctor export` is implemented without aliases. |
| JSON bundle | Complete | Output includes `bundle.kind`, redaction metadata, and embedded capability scan JSON. |
| Redaction | Complete | Local path inputs and provider evidence paths are replaced with placeholders. |
| Project requirements | Complete | Export can read project capability requirements and redacts the project root. |
| Output file | Complete | `--output` and `-o` write the bundle to disk. |
| Runtime mutation avoided | Complete | No game, MO2, GECK, runtime, network, or AI operation is used. |

## Validation

Targeted checks run during implementation:

```text
dotnet build --no-restore
dotnet test tests/WastelandForge.GoldenTests/WastelandForge.GoldenTests.csproj --no-build --no-restore --filter "FullyQualifiedName~DoctorExport|FullyQualifiedName~Capabilities"
dotnet test WastelandForge.sln --no-build --no-restore -m:1
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj --no-build --no-restore -- doctor export fixtures/projects/ExampleMod --format json
```

## Next gate

Gate 129 should implement the first canonical `WF-CAP-*` diagnostic projection
skeleton for capability requirement and provider evidence findings, while
keeping Doctor export redacted and local-first.
