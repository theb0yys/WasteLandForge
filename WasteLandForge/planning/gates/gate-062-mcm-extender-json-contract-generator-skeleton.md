# Gate 62 - MCM Extender JSON Contract And Generator Skeleton

Status: Complete
Phase: v0.1 implementation
Decision base: Gate 0, Gate 61, ADR-007, ADR-008, ADR-009, ADR-010, ADR-011

## Definition

Gate 62 implements the first MCM Extender JSON source contract and generator
skeleton.

`forge generate --target mcm-json` reads manifest-declared MCM source intent,
requires a non-optional generation dependency on `runtime.ui.mcm_json`, and
writes deterministic Forge-marked MCM JSON skeleton files under project
`generated/mcm-json`. `forge build --target mcm-json` writes the same skeleton
files under project `dist/mcm-json`, plus a local `build-manifest.json` and
`checksums.sha256`.

Gate 62 does not implement the exact upstream MCM Extender runtime JSON schema,
runtime process probes, MO2 VFS launch, MO2 profile inspection, provider
version parsing, `WF-CAP-*` diagnostic projection, JIP text script generation,
package archives, binary plugin generation, or external tool execution.

## Research grounding

| Claim | Classification | Source |
|---|---|---|
| MCM Extender JSON is the best first game-facing generator because it is deterministic text output and does not require ESP/ESM generation. | Documented | R006 generator/build research |
| MCM JSON generation is capability-gated and targets a stack including MCM, xNVSE, JIP LN, JohnnyGuitar, ShowOff, and UIO. | Documented | R005 / R006 |
| Projects should depend on capabilities, not provider names. | Documented | R005 / ADR-008 |
| Generated outputs are disposable and must carry provenance. | Documented | R006 generator/build research / ADR-009 |
| Public fixtures must be synthetic and redistributable. | Documented | R008 / ADR-011 |
| Adding manifest `0.2.0`, dependency `0.2.0`, and capability `0.2.0` is required because existing `0.1.0` schemas are immutable and do not allow underscore-bearing researched IDs such as `runtime.ui.mcm_json`. | Inferred | ADR-007/ADR-011 plus R005 ID examples |
| The exact upstream MCM Extender runtime JSON shape is still open in local evidence, so Gate 62 output is marked as a Forge skeleton. | Open | Local research confirms JSON-driven menus but does not include the concrete runtime file schema |

## Deliverables

- Add manifest schema `0.2.0` with optional `registries.mcm`.
- Add dependency schema `0.2.0` and capability schema `0.2.0` with
  underscore-capable logical IDs while preserving `0.1.0` schemas.
- Add MCM registry schema `0.1.0`.
- Extend validation to schema-check manifest-declared MCM registries.
- Add typed MCM menu reads for generator use.
- Implement `forge generate --target mcm-json`.
- Implement `forge build --target mcm-json`.
- Write deterministic skeleton files under `Menus/Prefabs/MCMExtender/`.
- Write generation/build manifests and build checksums.
- Gate the target on declared non-optional `runtime.ui.mcm_json` generation
  dependency.
- Add `WF-GEN-002`, `WF-GEN-003`, and `WF-GEN-004` generator diagnostics.
- Update ExampleMod with synthetic MCM intent only.
- Add schema, back-compat, unit, and golden CLI coverage.
- Update current-gate documentation.

## Validation mapping

Gate 62 adds this path:

```text
manifest 0.2.0
  -> mcm registry 0.1.0 schema validation
  -> dependency/capability 0.2.0 schema validation
  -> declared runtime.ui.mcm_json generation dependency gate
  -> MCM skeleton generation
  -> manifest/checksum evidence
```

`forge generate --target mcm-json` writes:

```text
generated/mcm-json/Menus/Prefabs/MCMExtender/<menu>.json
generated/mcm-json/generation-manifest.json
```

`forge build --target mcm-json` writes:

```text
dist/mcm-json/Menus/Prefabs/MCMExtender/<menu>.json
dist/mcm-json/build-manifest.json
dist/mcm-json/checksums.sha256
```

## Verification

Planned local checks:

```text
dotnet build WastelandForge.sln -c Release -m:1 --disable-build-servers --no-restore
dotnet test tests/WastelandForge.SchemaTests/WastelandForge.SchemaTests.csproj -c Release --no-build --no-restore --logger "console;verbosity=minimal"
dotnet test tests/WastelandForge.UnitTests/WastelandForge.UnitTests.csproj -c Release --no-build --no-restore --logger "console;verbosity=minimal"
dotnet test tests/WastelandForge.GoldenTests/WastelandForge.GoldenTests.csproj -c Release --no-build --no-restore --logger "console;verbosity=minimal"
dotnet test tests/WastelandForge.BackCompatTests/WastelandForge.BackCompatTests.csproj -c Release --no-build --no-restore --logger "console;verbosity=minimal"
dotnet test WastelandForge.sln -c Release --no-build --no-restore -m:1 --logger "console;verbosity=minimal"
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build --no-restore -- generate <temp-copy-of-ExampleMod> --target mcm-json --format json --no-input
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build --no-restore -- build <temp-copy-of-ExampleMod> --target mcm-json --format json --no-input
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build --no-restore -- validate fixtures/projects/ExampleMod
git diff --check
```

Results:

- `dotnet build WastelandForge.sln -c Release -m:1 --disable-build-servers --no-restore`
  passed with 0 warnings and 0 errors.
- `dotnet test WastelandForge.sln -c Release --no-build --no-restore -m:1 --logger "console;verbosity=minimal"`
  passed: 265 tests, 0 failed, 0 skipped.
- Focused schema, unit, golden, and back-compat test passes were run while
  implementing the gate. Initial focused MCM tests exposed that immutable
  dependency/capability `0.1.0` logical ID patterns rejected researched
  underscore IDs such as `runtime.ui.mcm_json`; this was fixed by adding
  `0.2.0` dependency and capability schemas instead of mutating `0.1.0`.
- CLI smoke on a fresh `%TEMP%` copy of `fixtures/projects/ExampleMod` passed:
  `forge validate`, `forge generate --target mcm-json --format json
  --no-input`, and `forge build --target mcm-json --format json --no-input`.
  The generate smoke wrote 2 outputs; the build smoke wrote 3 outputs.
- Direct `forge validate fixtures/projects/ExampleMod` passed with 0 errors,
  0 warnings, and 0 notes.
- A repo-local temporary smoke copy under the Codex workspace hit a sandbox
  .NET file I/O `FileNotFoundException` while creating/writing disposable
  output directories. The same CLI commands passed on the `%TEMP%` copy, so
  the repo-local smoke copy was not used as gate evidence.

## Open checks

| Check | Status | Notes |
|---|---|---|
| Upstream MCM Extender runtime JSON schema | Open | Gate 62 output is explicitly marked as a Forge skeleton until primary runtime schema evidence is captured. |
| Runtime/provider capability confirmation | Open | Requires runtime probes, version parsing, and `WF-CAP-*` diagnostic projection. |
| MO2 profile and VFS scan | Open | Needed before effective visibility claims. |
| JIP text scripts | Open | Should follow after MCM JSON output validation and script policy. |
| Package staging | Open | Needed before generated MCM output becomes a distributable mod package. |
| Binary plugin generation | Open | Deferred. |

## Next gate

Gate 63 should research and record the concrete upstream MCM Extender runtime
JSON shape, then add output validation for Forge-generated MCM files without
adding JIP scripts, package archives, MO2 VFS launch, or runtime probes.
