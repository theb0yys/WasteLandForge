# Gate 320 - Forge Init Editor Schema Association Scaffold

Status: Complete
Phase: CLI onboarding implementation
Decision base: ADR-010, ADR-011, Gate 319, R006 editor integration

## Goal

Extend `forge init` with editor schema association scaffold generation while
keeping the command offline-first and no-execution.

Gate 320 adds `.vscode/settings.json` to the safe `forge init` write set.
`--dry-run` still emits the plan without writing files.

## Research Grounding

| Classification | Decision | Source |
|---|---|---|
| Documented | `forge init` is part of the stable ADR-010 command surface. | ADR-010 / R006 |
| Documented | v0.1 editor integration should ship schemas, VS Code tasks, and problem matchers while deferring a full extension. | R006 / supplemental DX report |
| Documented | JSON files can be associated with schemas through `$schema` or workspace/user `json.schemas`; YAML language-server schema associations can map schemas to file globs. | Supplemental DX report |
| Documented | Published schema URLs are immutable and local validation remains offline-first. | ADR-011 / R008 |
| Inferred | A repo-local `.vscode/settings.json` schema map is safe after source, README, config, VS Code task, and workflow scaffold writes validate. | Gate 319 |
| Open | Runner installation/provisioning, VS Code extension generation, and language-server behavior remain later or separate gates. | Gate 320 boundary |

## Implemented Behavior

`forge init <project-root>` now writes, when safe:

- `wastelandforge.json`,
- `src/registries/dependencies/main.json`,
- `src/registries/capabilities/runtime.json`,
- `.wastelandforge/config.jsonc`,
- `README.md`,
- `.vscode/tasks.json`,
- `.vscode/settings.json`,
- `.github/workflows/wastelandforge.yml`,
- required parent directories.

The VS Code settings scaffold contains:

- `json.schemas` mappings for WastelandForge manifest and registry JSON globs,
- `yaml.schemas` mappings for matching YAML registry globs when a YAML language
  server consumes that setting,
- canonical WastelandForge schema IDs for manifest, dependency, capability,
  asset, dialogue, quest, MCM, JIP script, and xEdit audit source families.

Generating the settings file does not download schemas, run validation, start a
VS Code extension, start a language server, contact a schema host, install
Forge, install providers, or run external tools.

## Not Implemented

Gate 320 does not implement:

- generated or distribution root creation,
- `.wastelandforge/cache/` creation,
- local schema cache copying,
- runner provisioning or Forge CLI installation in CI,
- GitHub remote calls,
- workflow execution,
- GitHub ruleset configuration,
- CODEOWNERS creation,
- Dependabot creation,
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
- signing or attestation,
- AI behavior.

## Acceptance

| Requirement | Status | Evidence |
|---|---|---|
| `forge init` writes `.vscode/settings.json` when safe | Complete | Golden CLI test checks the file. |
| Settings include JSON schema associations for current source contracts | Complete | Golden CLI test checks manifest, dependency, capability, and dialogue mappings. |
| Settings include YAML schema association mappings | Complete | Golden CLI test checks manifest, dependency, and capability YAML mappings. |
| `--dry-run` remains no-write | Complete | Golden CLI dry-run test asserts project root remains absent. |
| Existing planned paths are refused | Complete | Golden CLI refusal test returns exit code `6`. |
| Generated/dist/cache paths remain unwritten | Complete | Golden CLI test asserts they are absent. |
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

Gate 321 should close the current `forge init` onboarding lane and route to the
next highest-value implementation slice, still preserving the same refusal
policy and still stopping before Forge runner installation/provisioning,
provider installation, external tool execution, MO2 automation, GECK
automation, runtime probes, real third-party plugin fixtures, release
publication, remote repository calls, signing, attestation, plugin mutation,
VS Code extension generation, language-server process startup, or AI behavior.
