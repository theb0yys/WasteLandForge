# Gate 318 - Forge Init VS Code Task Scaffold

Status: Complete
Phase: CLI onboarding implementation
Decision base: ADR-010, ADR-011, ADR-007, Gate 317, R006 editor integration

## Goal

Extend `forge init` with the first editor integration scaffold while keeping
the command offline-first and no-execution.

Gate 318 adds `.vscode/tasks.json` to the safe `forge init` write set.
`--dry-run` still emits the plan without writing files.

## Research Grounding

| Classification | Decision | Source |
|---|---|---|
| Documented | `forge init` is part of the stable ADR-010 command surface. | ADR-010 / R006 |
| Documented | VS Code tasks and problem matchers are the first editor integration target for v0.1. | R006 editor integrations |
| Documented | The correctness path remains offline-first and AI-optional. | ADR-011 / R004 |
| Inferred | A repo-local `.vscode/tasks.json` is safe after the source scaffold, config, and README write set validates. | Gate 317 |
| Open | GitHub Actions workflow emission, schema associations, watch tasks, a full VS Code extension, and language-server behavior remain later gates. | Gate 318 boundary |

## Implemented Behavior

`forge init <project-root>` now writes, when safe:

- `wastelandforge.json`,
- `src/registries/dependencies/main.json`,
- `src/registries/capabilities/runtime.json`,
- `.wastelandforge/config.jsonc`,
- `README.md`,
- `.vscode/tasks.json`,
- required parent directories.

The VS Code task file contains deterministic local tasks for:

- `Forge: Validate`,
- `Forge: Capabilities Scan`,
- `Forge: Build Reports`.

The validate task includes a local problem matcher for Forge plain `ERR`
diagnostic lines. Generating the task file does not execute any task.

## Not Implemented

Gate 318 does not implement:

- generated or distribution root creation,
- `.wastelandforge/cache/` creation,
- GitHub Actions workflow emission,
- VS Code schema associations,
- VS Code extension generation,
- language-server behavior,
- watch/background tasks,
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
| `forge init` writes `.vscode/tasks.json` when safe | Complete | Golden CLI test parses the task file. |
| `--dry-run` remains no-write | Complete | Golden CLI dry-run test asserts project root remains absent. |
| Existing planned paths are refused | Complete | Golden CLI refusal test returns exit code `6`. |
| Generated/dist/cache/workflow paths remain unwritten | Complete | Golden CLI test asserts they are absent. |
| Created scaffold validates | Complete | Golden CLI test runs `forge validate` on the temp scaffold and gets zero errors. |

## Validation

Required validation:

```text
dotnet build src\WastelandForge.Cli\WastelandForge.Cli.csproj --no-restore
dotnet test tests\WastelandForge.GoldenTests\WastelandForge.GoldenTests.csproj --no-restore --filter Init
dotnet test WastelandForge.sln --no-restore --logger "console;verbosity=minimal"
git diff --check
```

## Next Forge Init Gate

Gate 319 should extend `forge init` with GitHub Actions workflow scaffold
generation, still preserving the same refusal policy and still stopping before
provider installation, external tool execution, MO2/GECK automation, runtime
probes, real third-party plugin fixtures, release publication, remote
repository calls, signing, attestation, plugin mutation, or AI behavior.
