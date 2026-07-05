# Gate 316 - Forge Init Minimal Scaffold Emission

Status: Complete
Phase: CLI onboarding implementation
Decision base: ADR-010, ADR-011, ADR-007, ADR-008, ADR-009, Gate 312

## Goal

Make `forge init` create the first safe source scaffold.

Gate 316 changes `forge init` from planning-only to safe write-by-default
behavior. `--dry-run` still emits the plan without writing files. Normal
execution writes only the root manifest and the minimal dependency and
capability registry files needed for `forge validate` to load the project.

## Research Grounding

| Classification | Decision | Source |
|---|---|---|
| Documented | `forge init` is part of the stable ADR-010 command surface. | ADR-010 / R006 |
| Documented | Source truth is YAML/JSON contracts validated by schemas and deterministic semantic validators. | ADR-007 |
| Documented | Capabilities and providers are registry data, not hard-coded provider assumptions. | ADR-008 |
| Documented | Generated outputs are disposable and separate from source truth. | ADR-009 |
| Documented | Validation and correctness remain offline-first and AI-optional. | ADR-011 |
| Inferred | The first write gate should create only the minimal source contracts that the existing validator can accept. | Gate 312 and current schema contracts |
| Open | Editor tasks, workflow files, README content, Forge config, post-write validation execution, and richer template content remain later gates. | Gate 316 boundary |

## Implemented Behavior

`forge init <project-root>` now writes, when safe:

- `wastelandforge.json`,
- `src/registries/dependencies/main.json`,
- `src/registries/capabilities/runtime.json`,
- required parent directories.

The manifest uses schema `0.2.0` and points only at dependency and capability
registry directories. The dependency registry is empty but schema-valid. The
capability registry contains a project-local baseline placeholder capability.

Existing planned paths are still refused with exit code `6`. `--dry-run`
continues to emit the plan and writes nothing.

## Not Implemented

Gate 316 does not implement:

- generated or distribution root creation,
- `.wastelandforge/config.jsonc` emission,
- `.vscode/tasks.json` emission,
- GitHub Actions workflow emission,
- README emission,
- overwrite or force behavior,
- interactive prompts,
- automatic post-write validation execution,
- provider installation,
- external tool execution,
- MO2 automation,
- GECK automation,
- xEdit execution,
- runtime probes,
- plugin mutation,
- release publication,
- remote repository calls,
- signing or attestation,
- AI behavior.

## Acceptance

| Requirement | Status | Evidence |
|---|---|---|
| `forge init` writes minimal scaffold files when safe | Complete | Golden CLI test creates temp project files. |
| `--dry-run` remains no-write | Complete | Golden CLI dry-run test asserts project root remains absent. |
| Existing planned paths are refused | Complete | Golden CLI refusal test returns exit code `6`. |
| Generated/dist/editor/workflow/README paths remain unwritten | Complete | Golden CLI test asserts they are absent. |
| Created scaffold validates | Complete | Golden CLI test runs `forge validate` on the temp scaffold and gets zero errors. |
| No diagnostic-only format expansion | Complete | `--format sarif` remains a usage error. |

## Validation

Required validation:

```text
dotnet build src\WastelandForge.Cli\WastelandForge.Cli.csproj --no-restore
dotnet test tests\WastelandForge.GoldenTests\WastelandForge.GoldenTests.csproj --no-restore --filter Init
dotnet test WastelandForge.sln --no-restore --logger "console;verbosity=minimal"
git diff --check
```

## Next Forge Init Gate

Gate 317 should extend `forge init` with the next source-adjacent scaffold
items: repo-local Forge config and human README content, while preserving the
same refusal policy and still stopping before editor workflow generation,
provider installation, external tool execution, MO2/GECK automation, runtime
probes, plugin mutation, remote calls, release publication, signing,
attestation, or AI behavior.
