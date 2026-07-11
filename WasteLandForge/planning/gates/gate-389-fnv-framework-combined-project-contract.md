# Gate 389 - FNV Framework Combined Project Contract

Status: Complete
Phase: v0.1 implementation planning
Decision base: R006, ADR-007, ADR-009, ADR-010, Gates 362, 386-388

## Goal

Define a useful runtime-enabled `forge init` scaffold that can be validated and
packaged as a combined MCM/JIP mod without relying on the bundled sample.

Gate 389 changes no runtime code or generated template output. It resolves the
contract required for Gate 390.

## Research grounding

| Claim | Classification | Source |
|---|---|---|
| `forge init` owns project/template scaffolding and must remain offline-first. | Documented | R006 / ADR-010 |
| R006 names `fnv-framework` among the accepted onboarding template IDs. | Documented | R006 / Gate 362 |
| A runtime-enabled template category is sufficient for the MVP. | Documented | R006 |
| Canonical source is versioned registry data validated before generation. | Documented | ADR-007 |
| Generated/package output is disposable and must not be created by init. | Documented | ADR-009 |
| Gate 362 found all four existing template IDs identical because specialized contracts were then undefined. | Documented | Gate 362 |
| Gates 386-388 now provide enough implemented MCM/JIP source and package behavior to define `fnv-framework` as the runtime-enabled combined template. | Inferred | Gates 386-388 |

## Template identity decision

`fnv-framework` becomes the combined MCM/JIP runtime-enabled template.

- No fifth template ID or alias is added.
- `fnv-basic`, `fnv-quest-pack`, and `fnv-docs-only` retain the current eight-file
  baseline behavior in Gate 390.
- `fnv-framework` no longer claims identical output after Gate 390.
- This is a pre-1.0 template specialization, not a change to the manifest or
  registry schema versions.

## Source file contract

`forge init <root> --template fnv-framework --name <name>` creates exactly these
12 source/configuration files:

```text
wastelandforge.json
src/registries/dependencies/main.json
src/registries/capabilities/runtime.json
src/registries/capabilities/mcm-json.json
src/registries/capabilities/jip-script-runner.json
src/registries/mcm/main.json
src/registries/jip-scripts/main.json
.wastelandforge/config.jsonc
.vscode/tasks.json
.vscode/settings.json
.github/workflows/wastelandforge.yml
README.md
```

`generated/`, `dist/`, cache directories, game files, plugins, and package
archives remain planned/ignored boundaries and are not created.

## Manifest and registry contract

The manifest remains version `0.2.0` and declares:

```text
registries.dependencies
registries.capabilities
registries.mcm
registries.jipScripts
```

The dependency registry requires these generation capabilities:

```text
runtime.ui.mcm_json
runtime.scripting.jip_script_runner
```

Capability source consists of:

- `runtime.scripting.xnvse` in `runtime.json`;
- `runtime.ui.mcm_json` in `mcm-json.json`;
- `runtime.scripting.jip_script_runner` in
  `jip-script-runner.json`.

These are declaration-only source contracts. Init does not scan for or install
providers and does not claim that the local environment satisfies them.

## Starter content contract

The MCM registry contains one menu, one General page, and one INI-backed
boolean toggle. The JIP registry contains one `gr_` lifecycle script using
`opaqueText`, explicit-reference FormID strategy, and the existing 16 KiB
budget.

All IDs derive from the canonical init project ID. Output filenames derive from
its final sanitized slug:

```text
MCM menu: <slug>.json
MCM INI: Config/<slug>.ini
JIP script: gr_<slug>_bootstrap.txt
```

Defaults remain synthetic and inert: the toggle defaults false and the JIP
body contains a comment-only starter line. The scaffold must not invent plugin
FormIDs, quest stages, game records, runtime side effects, or proprietary data.

## Workflow contract

README and VS Code task guidance adds these canonical commands after the
existing validation/capability steps:

```text
forge package . --target mod-package --dry-run --format json --no-input
forge package . --target mod-package --format json --no-input
```

Init itself runs neither command. Post-create validation remains the existing
app-shell behavior, not an added CLI init side effect.

The generated GitHub Actions scaffold remains validation-only in Gate 390. It
must not start packaging until the generated consumer Forge-availability policy
and artifact-retention contract explicitly permit that change.

## Safety and refusal contract

- Dry-run reports all 12 files and writes nothing.
- Existing planned paths refuse the entire scaffold before any write.
- There is no force/overwrite mode.
- Created source validates with zero blocking diagnostics.
- A created project can run `forge package --target mod-package` and produce
  both MCM and JIP entries without manual source repair.
- JSON/plain init output reports template specialization and exact written
  paths; it does not imply package generation occurred.

## Boundaries

No package output, provider install, capability scan, runtime probe, game/Data
write, MO2 automation, GECK/xEdit execution, plugin creation/mutation, FOMOD,
game launch, network call, release publication, signing, attestation, or AI.

## Acceptance criteria for Gate 390

- `fnv-framework` dry-run plans 12 files and performs no write.
- Create writes exactly those 12 files and refuses any pre-existing planned
  file without partial mutation.
- Created manifest and all registries pass schema and semantic validation.
- Combined package command exits 0 and emits both component families.
- Existing three baseline templates remain byte-for-byte behaviorally stable.
- CLI help and app New Project messaging no longer state that all four
  templates are identical.
- Focused unit/golden/backwards-compatibility coverage protects the split.

## Next route

Gate 390: implement `fnv-framework` specialization across init planning,
scaffold writing, help/JSON output, app template description, and focused
tests, then prove create -> validate -> combined package as one vertical slice.
