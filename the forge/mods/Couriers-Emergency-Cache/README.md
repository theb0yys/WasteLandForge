# Courier's Emergency Cache

WastelandForge project scaffold for Fallout: New Vegas.

## Commands

```text
forge validate . --format json --no-input
forge capabilities list --format plain
forge build . --target reports --format json --no-input
code . # optional: open VS Code tasks
```

## Forge Command Availability

Generated tasks and workflows expect `forge` to already be available on `PATH`.
Use your chosen install method before running them. This scaffold does not
restore Forge, publish packages, add `NuGet.config`, or assume the
WastelandForge source repository exists.

## Layout

- `wastelandforge.json` is the root project manifest.
- `src/registries/dependencies/main.json` declares project capability requirements.
- `src/registries/capabilities/runtime.json` declares project-local capability records.
- `.wastelandforge/config.jsonc` stores repo-local Forge settings.
- `.vscode/tasks.json` contains local Forge validate, capability scan, and reports build tasks.
- `.vscode/settings.json` associates Forge source files with published schema IDs for editor validation.
- `.github/workflows/wastelandforge.yml` contains a GitHub Actions validation workflow scaffold.

Generated outputs are disposable and belong under `generated/` or `dist/`.
Forge correctness remains offline-first and AI-optional.
