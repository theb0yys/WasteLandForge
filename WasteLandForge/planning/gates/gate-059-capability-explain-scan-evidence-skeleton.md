# Gate 59 - Capability Explain Scan Evidence Skeleton

Status: Complete
Phase: v0.1 implementation
Decision base: Gate 0, Gate 57, Gate 58, ADR-008, ADR-010, ADR-011

## Definition

Gate 59 implements `forge capabilities explain` for built-in Fallout: New
Vegas capability and provider IDs.

The command reuses the Gate 58 explicit-path scan evidence model and explains
one requested target at a time. Capability explanations show the target
capability status, satisfying providers, and provider evidence. Provider
explanations show the provider status, provider evidence, and capabilities the
provider can satisfy.

Gate 59 does not implement runtime process probes, MO2 VFS launch, MO2 profile
inspection, provider version parsing, declared project capability resolution,
Doctor export bundles, `WF-CAP-*` diagnostics, generator/build gating, MCM
Extender JSON generation, or build manifests.

## Research grounding

| Claim | Classification | Source |
|---|---|---|
| Projects should depend on capabilities, not provider names. | Documented | R005 / ADR-008 |
| Providers should be versioned registry data that satisfy capabilities and carry detection/explanation data. | Documented | R005 / ADR-008 |
| Detection should be local-first and deterministic, with runtime probes as secondary enrichment. | Documented | R005 / ADR-008 |
| Capability scanning and explanation are first-class CLI UX. | Documented | R006 / ADR-010 |
| Machine-facing CLI output should be stable JSON for automation. | Documented | R006 / ADR-010 |
| Explanation over Gate 58 scan results is the smallest safe next step before project requirement resolution. | Inferred | Gate 58 plus R005 detector/resolution sequence |

## Deliverables

- Add capability explanation records for inputs, target, provider evidence,
  and capability relationships.
- Add a built-in FNV capability explainer over the Gate 58 scanner.
- Implement `forge capabilities explain <capability-or-provider-id>`.
- Support `--game <path>`, `--game-root <path>`, `--data-root <path>`,
  repeated `--tool-path <path>`, `--format human|plain|json`,
  `--output <path>`, and `--no-input`.
- Reject unknown capability or provider IDs with usage JSON/text.
- Keep SARIF and GitHub output reserved for diagnostic commands.
- Add JSON and output-file CLI tests using synthetic temp-only folders.
- Update current-gate documentation.

## Validation mapping

Gate 59 adds an explanation report, not a diagnostic report:

```text
target capability/provider ID
  -> built-in FNV catalogue lookup
  -> optional explicit local path scan
  -> provider and capability scan results
  -> target explanation
  -> text or JSON command output
```

Explanation statuses reuse Gate 58 scan statuses:

- `probable` means a deterministic Gate 58 marker exists.
- `missing` means a configured detector ran and the marker was absent.
- `unknown` means Gate 58 lacks enough input or a safe marker for that
  provider or capability.

No `WF-CAP-*` diagnostics are emitted in Gate 59.

## Verification

Planned local checks:

```text
dotnet build WastelandForge.sln -c Release -m:1 --disable-build-servers --no-restore
dotnet test tests/WastelandForge.GoldenTests/WastelandForge.GoldenTests.csproj -c Release --no-build --no-restore --logger "console;verbosity=minimal"
dotnet test WastelandForge.sln -c Release --no-build --no-restore -m:1 --logger "console;verbosity=minimal"
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build --no-restore -- capabilities explain runtime.ui.mcm_json --format json
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build --no-restore -- capabilities explain runtime.fake.missing --format json
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build --no-restore -- validate fixtures/projects/ExampleMod
git diff --check
```

Results:

- Passed: `dotnet build WastelandForge.sln -c Release -m:1
  --disable-build-servers --no-restore` completed with 0 warnings and 0
  errors.
- Passed: `WastelandForge.GoldenTests` completed with 24 passed, 0 failed,
  and 0 skipped tests. This includes capability explanations, provider
  explanations, unknown capability/provider usage JSON, and explanation
  output-file writing.
- Passed: full solution tests completed with 239 passed, 0 failed, and 0
  skipped tests.
- Passed: `forge capabilities explain runtime.ui.mcm_json --format json`
  returned an `unknown` capability explanation when no explicit paths were
  provided, with runtime probes and MO2 VFS disabled.
- Passed: synthetic explicit-path CLI explanation returned
  `runtime.ui.mcm_json` as `probable` through
  `provider.runtime.mcm_extender` with `probable` evidence.
- Passed: `forge capabilities explain runtime.fake.missing --format json`
  returned usage JSON with exit code 2 for an unknown ID.
- Passed: CLI validation for `fixtures/projects/ExampleMod` returned 0
  errors, 0 warnings, and 0 notes.
- Passed: `git diff --check` returned exit code 0. Git reported CRLF
  normalization warnings only.
- Passed: stale current-doc scan found no active `capabilities explain`
  reserved references or Gate 58 status strings outside the historical Gate 58
  planning record.
- Passed: protected-term scan across project docs, planning, schemas,
  fixtures, source, and tests found no matches.
- Repository state: no files were staged or committed.

## Open checks

| Check | Status | Notes |
|---|---|---|
| Runtime probes | Open | Needed before `confirmed` runtime/session status. |
| MO2 profile and VFS scan | Open | Needed before effective visibility claims. |
| Wrong-scope and unsupported-version diagnostics | Open | Requires resolver and version policy. |
| Project requirement resolution | Open | Needed before generator/build gating. |
| Doctor export bundle | Open | Should reuse the shared catalogue, scan, and explanation model later. |
| MCM Extender JSON generator | Open | Depends on build/generate and capability gating foundations. |

## Next gate

Gate 60 should implement a capability requirement resolution skeleton for
declared project requirements against the built-in catalogue and scan evidence,
without generation, build execution, runtime probes, or MO2 VFS launch.
