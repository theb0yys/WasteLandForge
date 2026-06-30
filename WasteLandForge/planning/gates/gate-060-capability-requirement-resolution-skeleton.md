# Gate 60 - Capability Requirement Resolution Skeleton

Status: Complete
Phase: v0.1 implementation
Decision base: Gate 0, Gate 57, Gate 58, Gate 59, ADR-008, ADR-010, ADR-011

## Definition

Gate 60 implements the first project capability requirement resolution
skeleton.

`forge capabilities scan --project <path>` reads declared dependency
capability requirements from the project, validates the manifest and
dependency registry source needed for that read, runs the existing Gate 58
path-based scan, and resolves declared requirements against the built-in FNV
catalogue and scan evidence.

Gate 60 does not implement runtime process probes, MO2 VFS launch, MO2 profile
inspection, provider version parsing, wrong-scope diagnostics, `WF-CAP-*`
diagnostics, generator/build gating, Doctor export bundles, MCM Extender JSON
generation, or build manifests.

## Research grounding

| Claim | Classification | Source |
|---|---|---|
| Projects should depend on capabilities, not provider names. | Documented | R005 / ADR-008 |
| Projects declare required and optional capabilities, and Forge resolves whether installed providers satisfy them. | Documented | R005 / ADR-008 |
| Requirement classes should be phase-aware. | Documented | R005 |
| `forge capabilities scan --project .` is the documented CLI shape for project-aware environment scans. | Documented | R006 / ADR-010 |
| Runtime probes should enrich local deterministic detection rather than define correctness. | Documented | R005 / ADR-008 |
| Returning exit code `4` for capability/environment resolution failure follows the CLI exit-code contract. | Documented | R006 / ADR-010 |
| Keeping `WF-CAP-*` diagnostics out of Gate 60 is the smallest safe slice because Gate 59 left canonical diagnostic projection open. | Inferred | Gate 59 plus R005 diagnostic roadmap |

## Deliverables

- Add project capability requirement records and a built-in FNV requirement
  resolver over Gate 58 scan results.
- Expose a deterministic dependency requirement reader from
  `ProjectValidationPipeline`.
- Implement `forge capabilities scan --project <path>`.
- Extend scan JSON and text output with an optional project requirement
  section.
- Return exit code `4` when any non-optional project requirement is not
  satisfied.
- Keep plain `forge capabilities scan` behavior unchanged.
- Keep `forge validate` source-only for capability requirements; it still does
  not require a local FNV install.
- Add CLI tests for satisfied and unavailable project requirements using
  synthetic temp-only scan evidence.
- Update current-gate documentation.

## Validation mapping

Gate 60 adds requirement resolution to the existing scan report:

```text
project dependency registry
  -> requirement read
  -> built-in FNV capability catalogue
  -> Gate 58 path-based provider scan
  -> capability scan result
  -> requirement status
  -> optional scan report section
```

Requirement statuses are:

- `satisfied` when at least one satisfying provider is `probable`.
- `missing` when all configured satisfying providers are `missing`.
- `unknown` when the capability is unknown to the built-in catalogue, the scan
  lacks enough evidence, or a version constraint is declared.

No `WF-CAP-*` diagnostics are emitted in Gate 60.

## Verification

Planned local checks:

```text
dotnet build WastelandForge.sln -c Release -m:1 --disable-build-servers --no-restore
dotnet test tests/WastelandForge.GoldenTests/WastelandForge.GoldenTests.csproj -c Release --no-build --no-restore --logger "console;verbosity=minimal"
dotnet test WastelandForge.sln -c Release --no-build --no-restore -m:1 --logger "console;verbosity=minimal"
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build --no-restore -- capabilities scan --project fixtures/projects/ExampleMod --format json
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build --no-restore -- validate fixtures/projects/ExampleMod
git diff --check
```

Results:

- Passed: `dotnet build WastelandForge.sln -c Release -m:1
  --disable-build-servers --no-restore` completed with 0 warnings and 0
  errors.
- Passed: `WastelandForge.GoldenTests` completed with 26 passed, 0 failed,
  and 0 skipped tests. This includes project requirement resolution for a
  satisfied xNVSE requirement and an unavailable required xNVSE requirement.
- Passed: full solution tests completed with 241 passed, 0 failed, and 0
  skipped tests.
- Passed: `forge capabilities scan --project fixtures/projects/ExampleMod
  --format json` returned a scan report with one `unknown` required
  requirement and `requiredUnavailable: 1`. PowerShell surfaced the native
  non-zero as process exit 1 when run directly, so `$LASTEXITCODE` was checked
  with a plain-format rerun and confirmed the Forge app exit code was `4`.
- Passed: CLI validation for `fixtures/projects/ExampleMod` returned 0
  errors, 0 warnings, and 0 notes.
- Passed: `git diff --check` returned exit code 0. Git reported CRLF
  normalization warnings only.
- Passed: protected-term scan across project docs, planning, schemas,
  fixtures, source, tests, and `.agents` found no matches.
- Repository state: no files were staged or committed.

## Open checks

| Check | Status | Notes |
|---|---|---|
| Runtime probes | Open | Needed before `confirmed` runtime/session status. |
| MO2 profile and VFS scan | Open | Needed before effective visibility claims. |
| Wrong-scope diagnostics | Open | Requires scope-aware resolver evidence. |
| Unsupported provider version diagnostics | Open | Requires provider version parsing and comparison policy. |
| `WF-CAP-*` diagnostic projection | Open | Gate 60 emits report status, not canonical diagnostics. |
| Generator/build gating | Open | Build and generate commands still do not consume requirement resolution. |
| Doctor export bundle | Open | Should reuse the shared catalogue, scan, explanation, and requirement model later. |
| MCM Extender JSON generator | Open | Depends on build/generate and capability gating foundations. |

## Next gate

Gate 61 should implement the first deterministic `forge generate` / `forge
build` report skeleton with local build manifests for low-risk metadata
outputs, without MCM Extender JSON generation, runtime probes, MO2 VFS launch,
or binary plugin generation.
