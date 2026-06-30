# Gate 58 - Capability Scan Detector Skeleton

Status: Complete
Phase: v0.1 implementation
Decision base: Gate 0, Gate 13, Gate 57, ADR-006, ADR-008, ADR-010, ADR-011

## Definition

Gate 58 implements the first local capability scan skeleton for
`forge capabilities scan`.

The gate scans explicit local paths with deterministic root-file, data-file,
and executable-tool detectors. It reports provider and capability evidence
against the built-in FNV catalogue from Gate 57.

Gate 58 does not implement runtime process probes, MO2 VFS launch, MO2 profile
inspection, provider version parsing, declared project capability resolution,
Doctor export bundles, `WF-CAP-*` diagnostics, generator/build gating, MCM
Extender JSON generation, or build manifests. `forge capabilities explain`
remains reserved.

## Research grounding

| Claim | Classification | Source |
|---|---|---|
| Detection should be local-first and deterministic, with runtime probes as secondary enrichment. | Documented | R005 / ADR-008 |
| Root-file detectors are appropriate for xNVSE because it is installed beside `FalloutNV.exe` and launched through `nvse_loader.exe`. | Documented | R005 |
| Data-file detectors are appropriate for NVSE plugins, UI frameworks, and other Data-managed providers. | Documented | R005 |
| Executable-tool detectors are appropriate for external tools such as xEdit, MO2, and GECK. | Documented | R005 |
| Runtime-only confirmation, MO2 effective visibility, wrong-scope diagnostics, and version gating require later evidence and resolver work. | Documented | R005 / ADR-008 |
| A path-based scan report is the smallest safe step after Gate 57 because it exercises detector families without claiming runtime certainty. | Inferred | Gate 57 plus R005 detector taxonomy |

## Deliverables

- Add capability scan domain records for inputs, summary, provider evidence,
  provider results, and capability results.
- Add a built-in FNV scanner for deterministic root-file, data-file, and
  executable-tool probes.
- Implement `forge capabilities scan`.
- Support `--game <path>`, `--game-root <path>`, `--data-root <path>`,
  repeated `--tool-path <path>`, `--format human|plain|json`,
  `--output <path>`, and `--no-input`.
- Keep `forge capabilities explain` reserved.
- Add JSON and output-file CLI tests using synthetic temp-only folders.
- Update current-gate documentation.

## Validation mapping

Gate 58 adds a scan report, not a diagnostic report:

```text
explicit local paths
  -> root-file / data-file / executable-tool probes
  -> provider scan results
  -> capability scan results
  -> text or JSON command output
```

Scan statuses are evidence statuses:

- `probable` means a deterministic Gate 58 marker exists.
- `missing` means a configured detector ran and the marker was absent.
- `unknown` means Gate 58 lacks enough input or a safe marker for that
  provider.

No `WF-CAP-*` diagnostics are emitted in Gate 58.

## Verification

Planned local checks:

```text
dotnet build WastelandForge.sln -c Release -m:1 --disable-build-servers --no-restore
dotnet test tests/WastelandForge.GoldenTests/WastelandForge.GoldenTests.csproj -c Release --no-build --no-restore --logger "trx;LogFileName=golden.trx" --results-directory TestResults/Gate58/Golden
dotnet test WastelandForge.sln -c Release --no-build --no-restore -m:1 --logger "console;verbosity=minimal"
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build --no-restore -- capabilities scan --format json
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build --no-restore -- capabilities scan --game-root <synthetic-root> --tool-path <synthetic-xedit> --tool-path <synthetic-mo2> --format json
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build --no-restore -- validate fixtures/projects/ExampleMod
git diff --check
```

Results:

- Passed: `dotnet build WastelandForge.sln -c Release -m:1
  --disable-build-servers --no-restore` completed with 0 warnings and 0
  errors.
- Passed: `WastelandForge.GoldenTests` completed with 21 passed, 0 failed,
  and 0 skipped tests. This includes no-input scan, temp-only synthetic path
  evidence, scan output-file writing, and reserved `capabilities explain`
  status coverage.
- Passed: full solution tests completed with 236 passed, 0 failed, and 0
  skipped tests.
- Passed: `forge capabilities scan --format json` returned 15 unknown
  providers and 19 unknown capabilities with runtime probes and MO2 VFS
  disabled.
- Passed: synthetic path CLI scan returned 13 probable providers, 0 missing
  providers, 2 unknown providers, 17 probable capabilities, and 2 unknown
  capabilities.
- Passed: `forge capabilities explain --format json` returned the reserved
  command JSON status. The shell reports this as nonzero, as expected for a
  reserved command.
- Passed: CLI validation for `fixtures/projects/ExampleMod` returned 0
  errors.
- Passed: temp synthetic scan folders were removed after the smoke check.
- Passed: `git diff --check` returned exit code 0. Git reported CRLF
  normalization warnings only.
- Passed: protected-term scan across project docs, planning, schemas,
  fixtures, source, and tests found no matches.
- Passed: stale current-gate scan found no current `capabilities scan`
  reserved references; remaining matches are historical gate records or
  future `WF-CAP-*` diagnostic reservations.
- Repository state: no files were staged or committed.

## Open checks

| Check | Status | Notes |
|---|---|---|
| Runtime probes | Open | Needed before `confirmed` runtime/session status. |
| MO2 profile and VFS scan | Open | Needed before effective visibility claims. |
| Wrong-scope and unsupported-version diagnostics | Open | Requires resolver and version policy. |
| Capability explanation command | Open | Proposed Gate 59 work. |
| Project requirement resolution | Open | Needed before generator/build gating. |
| MCM Extender JSON generator | Open | Depends on scan and build manifest foundations. |

## Next gate

Gate 59 should implement `forge capabilities explain` over the built-in
catalogue and scan evidence shape. It should still avoid runtime probes,
MO2 VFS launch, project requirement gating, and generated mod outputs.
