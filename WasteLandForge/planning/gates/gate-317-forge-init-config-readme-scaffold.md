# Gate 317 - Forge Init Config And README Scaffold

Status: Complete
Phase: CLI onboarding implementation
Decision base: ADR-010, ADR-011, ADR-007, ADR-009, Gate 316

## Goal

Extend `forge init` with the next source-adjacent scaffold files.

Gate 317 adds repo-local Forge config and human README content to the safe
`forge init` write set. `--dry-run` still emits the plan without writing
files.

## Research Grounding

| Classification | Decision | Source |
|---|---|---|
| Documented | `forge init` is part of the stable ADR-010 command surface. | ADR-010 / R006 |
| Documented | Source truth is YAML/JSON contracts and must remain local-first. | ADR-007 |
| Documented | Generated outputs are disposable and separate from source truth. | ADR-009 |
| Documented | Validation and release correctness remain offline-first and AI-optional. | ADR-011 |
| Inferred | Repo-local Forge config and README content are safe onboarding files after the manifest and registry scaffold validates. | Gate 316 |
| Open | Editor task files, GitHub workflow files, output/cache root creation, post-write validation execution, and richer templates remain later gates. | Gate 317 boundary |

## Implemented Behavior

`forge init <project-root>` now writes, when safe:

- `wastelandforge.json`,
- `src/registries/dependencies/main.json`,
- `src/registries/capabilities/runtime.json`,
- `.wastelandforge/config.jsonc`,
- `README.md`,
- required parent directories.

The config records local path defaults and disabled automation flags. The
README records basic Forge commands and the source/generated boundary.

## Not Implemented

Gate 317 does not implement:

- generated or distribution root creation,
- `.wastelandforge/cache/` creation,
- `.vscode/tasks.json` emission,
- GitHub Actions workflow emission,
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
| `forge init` writes config and README when safe | Complete | Golden CLI test checks both files. |
| `--dry-run` remains no-write | Complete | Golden CLI dry-run test asserts project root remains absent. |
| Existing planned paths are refused | Complete | Golden CLI refusal test returns exit code `6`. |
| Generated/dist/editor/workflow/cache paths remain unwritten | Complete | Golden CLI test asserts they are absent. |
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

Gate 318 should extend `forge init` with VS Code task scaffold generation,
still preserving the same refusal policy and still stopping before GitHub
Actions workflow generation, provider installation, external tool execution,
MO2/GECK automation, runtime probes, plugin mutation, remote calls, release
publication, signing, attestation, or AI behavior.
