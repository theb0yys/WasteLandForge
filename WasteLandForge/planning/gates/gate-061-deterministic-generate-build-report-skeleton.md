# Gate 61 - Deterministic Generate/Build Report Skeleton

Status: Complete
Phase: v0.1 implementation
Decision base: Gate 0, Gate 60, ADR-008, ADR-009, ADR-010, ADR-011

## Definition

Gate 61 implements the first deterministic `forge generate` and `forge build`
report skeleton.

`forge generate --target reports` validates a project and writes low-risk
metadata outputs under project `generated/reports`. `forge build --target
reports` validates a project and writes the same metadata outputs under
project `dist/build`, plus a local `build-manifest.json` and
`checksums.sha256`.

Gate 61 does not implement MCM Extender JSON generation, JIP text script
generation, package archives, binary plugin generation, runtime process
probes, MO2 VFS launch, MO2 profile inspection, external tool execution,
release prepare/publish, or `WF-CAP-*` diagnostic projection.

## Research grounding

| Claim | Classification | Source |
|---|---|---|
| Outputs are generated through a deterministic, capability-aware build graph. | Documented | R006 generator/build research / ADR-009 |
| Generated artifacts are disposable and rebuildable, and every output must carry provenance. | Documented | R006 generator/build research / ADR-009 |
| `forge generate` and `forge build` are canonical CLI commands. | Documented | R006 CLI research / ADR-010 |
| Metadata outputs, reports, manifests, and staging data are the safest first generator/build outputs before plugin or game-facing generation. | Documented | R006 generator/build research |
| Build manifests and local evidence are mandatory for build/release gates. | Documented | R008 / ADR-011 |
| Public fixtures must be synthetic and redistributable. | Documented | R008 / ADR-011 |
| Keeping MCM Extender JSON generation out of Gate 61 is the smallest safe slice because capability-gated game-facing generation needs its own source contract and output validation. | Inferred | Gate 60 plus R006 generator sequencing |

## Deliverables

- Add a generation-layer metadata report generator.
- Implement `forge generate --target reports`.
- Implement `forge build --target reports`.
- Write validation, dependency, capability, command report, and manifest JSON
  files for generate and build commands.
- Write build checksums for `forge build`.
- Keep `forge generate` outputs constrained to project `generated/`.
- Keep `forge build` outputs constrained to project `dist/`.
- Add `WF-GEN-001` and `WF-BUILD-001` output-boundary diagnostics.
- Support human/plain and JSON command output.
- Support `--project`, positional project root, `--target reports`,
  `--output`, `--dry-run`, and `--no-input`.
- Keep SARIF and GitHub output reserved for canonical diagnostic commands.
- Add unit and golden CLI coverage.
- Update current-gate documentation.

## Validation mapping

Gate 61 adds deterministic report outputs:

```text
project manifest
  -> loader and validation pipeline
  -> dependency requirement read
  -> metadata report planning
  -> output-boundary validation
  -> report file writes
  -> manifest and checksum evidence
```

`forge generate --target reports` writes:

```text
generated/reports/validation.json
generated/reports/dependency-report.json
generated/reports/capability-report.json
generated/reports/generate-report.json
generated/reports/generation-manifest.json
```

`forge build --target reports` writes:

```text
dist/build/validation.json
dist/build/dependency-report.json
dist/build/capability-report.json
dist/build/build-report.json
dist/build/build-manifest.json
dist/build/checksums.sha256
```

## Verification

Planned local checks:

```text
dotnet build WastelandForge.sln -c Release -m:1 --disable-build-servers --no-restore
dotnet test tests/WastelandForge.UnitTests/WastelandForge.UnitTests.csproj -c Release --no-build --no-restore --logger "console;verbosity=minimal"
dotnet test tests/WastelandForge.GoldenTests/WastelandForge.GoldenTests.csproj -c Release --no-build --no-restore --logger "console;verbosity=minimal"
dotnet test WastelandForge.sln -c Release --no-build --no-restore -m:1 --logger "console;verbosity=minimal"
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build --no-restore -- generate <temp-copy-of-ExampleMod> --format json --no-input
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build --no-restore -- build <temp-copy-of-ExampleMod> --format json --no-input
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build --no-restore -- validate <temp-copy-of-ExampleMod>
git diff --check
```

Results:

- Passed: `dotnet build WastelandForge.sln -c Release -m:1
  --disable-build-servers --no-restore` completed with 0 warnings and 0
  errors.
- Passed: `WastelandForge.UnitTests` completed with 14 passed, 0 failed, and
  0 skipped tests.
- Passed: `WastelandForge.GoldenTests` completed with 28 passed, 0 failed,
  and 0 skipped tests.
- Failed then fixed: the first full solution test run exposed an xUnit
  isolation issue where metadata report and release dry-run tests mutated the
  process-wide `SOURCE_DATE_EPOCH` environment variable in parallel. The
  environment-variable tests now share a serialized xUnit collection.
- Passed: full solution tests completed with 246 passed, 0 failed, and 0
  skipped tests after the isolation fix.
- Passed: `forge generate <temp-copy-of-ExampleMod> --format json --no-input`
  returned status `passed`, 0 diagnostics, 6 source digests, and 5 generated
  report outputs under `generated/reports`.
- Passed: `forge build <temp-copy-of-ExampleMod> --format json --no-input`
  returned status `passed`, 0 diagnostics, 6 source digests, and 6 build
  report outputs under `dist/build`, including `build-manifest.json` and
  `checksums.sha256`.
- Passed: CLI validation for the same temp copy of `ExampleMod` returned 0
  errors, 0 warnings, and 0 notes.
- Passed: stale current-doc scan found no active Gate 60 status strings or
  stale generate/build reserved wording outside current accepted docs.
- Passed: `git diff --check` returned exit code 0. Git reported CRLF
  normalization warnings only.
- Passed: protected-term scan across project files found no matches.
- Repository state: no files were staged or committed.

## Open checks

| Check | Status | Notes |
|---|---|---|
| MCM Extender JSON generator | Open | Needs first source contract, capability gate, output schema, and output validation. |
| JIP text scripts | Open | Should follow after text-only MCM JSON generator and script policy. |
| Binary plugin generation | Open | Deferred until text-based generation, provenance, and validation foundations are stronger. |
| Runtime probes | Open | Needed before `confirmed` provider/session status. |
| MO2 profile and VFS scan | Open | Needed before effective visibility claims. |
| `WF-CAP-*` diagnostic projection | Open | Gate 61 consumes declared requirements for reports, but still does not project canonical capability diagnostics. |
| Release prepare/publish | Open | Release verification remains local and explicit; publishing remains gated. |

## Next gate

Gate 62 should implement the first MCM Extender JSON contract and generator
skeleton, gated by declared capabilities and constrained to deterministic
text output, without JIP scripts, binary plugin generation, MO2 VFS launch, or
runtime probes.
