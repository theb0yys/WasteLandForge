# Gate 359 - App-Shell New Project over `forge init`

Status: Complete

## Goal

Expose the canonical `forge init` scaffold workflow through the Windows app
without duplicating scaffold logic or weakening its refusal policy.

## Research grounding

- **Documented:** ADR-010 defines `forge init` as the canonical project creation
  command and keeps the correctness path local, deterministic, and AI-optional.
- **Documented:** Gates 312 and 316-320 define the four templates, scaffold
  files, dry-run contract, and refusal of existing planned paths.
- **Documented:** Gate 358 routes the next product slice to this app-shell
  surface.
- **Inferred:** A successful preview tied to the current inputs is an
  appropriate human confirmation boundary before filesystem writes.

## Implemented

- Added a `New Project` app tab with project name, target folder, and the four
  documented templates: `fnv-basic`, `fnv-framework`, `fnv-quest-pack`, and
  `fnv-docs-only`.
- Preview invokes canonical `forge init <root> --template <id> --name <name>
  --game falloutnv --format json --no-input --dry-run`.
- Create remains disabled until preview succeeds for the exact current input
  signature. Editing any input invalidates that preview.
- Create invokes the same command without `--dry-run`; the app displays the
  backend JSON and selects the created project for the current session.
- Existing planned paths retain canonical exit-code-6 refusal. No overwrite or
  force option was added.
- Invalid target paths are rejected locally before backend invocation.

## Verification evidence

- Release desktop build completed with zero warnings and zero errors.
- Published-app UI Automation verified preview produced no target directory.
- Creation produced all eight canonical files: manifest, dependency registry,
  capability registry, Forge config, README, VS Code tasks/settings, and the
  GitHub Actions workflow.
- A second preview against the created target returned exit code 6 and did not
  change the manifest hash.
- The test left persisted settings unchanged and removed its synthetic project.

## Boundaries and risk

- No provider installation, external tool execution, MO2/GECK automation,
  runtime probe, plugin mutation, release publication, remote call, or AI call.
- This machine blocks the unsigned CLI from creating folders under the current
  protected `D:` workspace while normal user-writable `%TEMP%` creation passes.
  The app reports backend exit 8 for that environment restriction; Gate 360
  should preserve the error detail during post-create handoff work.

## Next route

Gate 360: post-create validation and project handoff. After successful init,
run canonical `forge validate` against the new scaffold, present the structured
result, and route the user to Mod Builder without additional scaffold writes.
