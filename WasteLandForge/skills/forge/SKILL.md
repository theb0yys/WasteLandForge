---
name: forge
description: Use when the WastelandForge /forge slash command plugin needs routing support, or when the user asks how a canonical Forge command maps to research-bound implementation/planning behavior. Dispatches ADR-010/R006 forge commands without inventing aliases.
argument-hint: "<command> [args]"
---

# Forge Command Routing

## Purpose

This is the supporting routing skill for the project-local `/forge` slash command plugin at `.agents/plugins/plugins/wastelandforge/commands/forge.md`.

The skill is not itself the UI slash command. It is loaded by the command so Codex uses the same research-bound routing every time.

Use it when the user types or asks for:

```text
/forge init
/forge validate
/forge capabilities list
/forge capabilities scan
/forge capabilities explain
/forge generate
/forge build
/forge package
/forge release verify
/forge release prepare
/forge release publish
/forge docs
/forge graph
/forge explain
/forge clean
/forge doctor export
/forge help
/forge --version
```

The slash command is a Codex prompt command, not a replacement for the future `forge` binary. R006 says Git hooks, editor tasks, and CI should be thin wrappers around a thick CLI. Until that CLI exists, `/forge` should implement or plan the requested command slice in the repo using the same names and semantics.

## Required sources

Start with:

- `WasteLandForge/skills/wastelandforge-research-grounding/SKILL.md`.
- `WasteLandForge/skills/wastelandforge-build-cli-release/SKILL.md`.
- `WasteLandForge/skills/wastelandforge-validation-release-governance/SKILL.md`.
- `WasteLandForge/agents/forge-command-agent.md`.

Then load the narrower skill or agent needed for the specific command:

- Contracts/schemas/diagnostics: `wastelandforge-contracts-registries`.
- Capabilities/providers: `wastelandforge-capabilities-providers` or `capability-provider-agent.md`.
- Validation/testing/release gates: `wastelandforge-validation-release-governance` or `validation-agent.md`.
- Implementation planning: `wastelandforge-implementation-planning` or `implementation-planning-agent.md`.
- Licensing/public fixtures/governance: `wastelandforge-licensing-dependencies` or `licensing-governance-agent.md`.

## Dispatch rules

Parse the command after `/forge`. Keep names aligned with ADR-010/R006. Do not create undocumented aliases such as `/forge scan` for `capabilities scan`.

If the requested command is missing or unclear, show the canonical command list and ask for the exact command.

If a real `forge` CLI exists in the workspace, prefer using it for read-only or explicitly requested operations. If it does not exist yet, implement the requested command skeleton or produce the implementation plan for that slice, depending on the user's wording.

For implementation work, classify important claims as `Documented`, `Inferred`, or `Open`. Do not resolve research gaps from general modding assumptions.

## Command routing

Use this routing:

| Slash form | Route |
|---|---|
| `/forge init` | scaffold/project layout work through implementation planning, contracts, and build CLI release prompts |
| `/forge validate` | layered validation through validation and contracts prompts |
| `/forge capabilities list` | capability catalogue through capability provider prompts |
| `/forge capabilities scan` | local deterministic provider detection through capability provider prompts |
| `/forge capabilities explain` | capability/provider diagnostic explanation through capability provider prompts |
| `/forge generate` | deterministic generator planning/execution through build CLI release prompts |
| `/forge build` | full validation-first build graph through build CLI release and validation governance prompts |
| `/forge package` | deterministic staging/ZIP/checksum work through build CLI release and content pipeline prompts |
| `/forge release verify` | release gates, manifests, checksums, and governance through validation release governance prompts |
| `/forge release prepare` | release dry-run and packaging preparation through build CLI release and governance prompts |
| `/forge release publish` | protected publish flow; require explicit approval and complete release governance |
| `/forge docs` | deterministic docs generation through contracts and build CLI release prompts |
| `/forge graph` | build/capability/registry graph explanation through build CLI release and platform prompts |
| `/forge explain` | diagnostic or build explanation through validation, contracts, or capability prompts |
| `/forge clean` | generated/dist cleanup only; require explicit approval for anything else |
| `/forge doctor export` | diagnostic export boundary through validation, capability, and licensing prompts |
| `/forge help` | show this command surface and research-backed constraints |
| `/forge --version` | report actual CLI version if implemented; otherwise report planned version state and open implementation status |

Gate 45 option routing:

- `/forge validate --geck-dialogue-export <path>` maps to the real
  `forge validate --geck-dialogue-export <path>` CLI behavior when the CLI is
  available. It is file-based only: validate the user-saved GECK dialogue
  export text file and do not control, automate, import into, or mutate an
  open GECK session.

Gate 71 option routing:

- `/forge generate --target mcm-json` and `/forge build --target mcm-json`
  map to the real `forge` CLI behavior when available. The output is the
  Gate 71 validated MCM Extender JSON runtime subset under `MCM/<menu>.json`,
  plus `MCM/Translations/<modName>.ini` when translations are declared, and
  staged referenced texture assets under their game-relative target paths.
  The output also includes `package-manifest.json` for the loose-file package
  root. It supports header, image, toggle, keybind, checkbox, string-toggle,
  slider, choice, and text settings. MCM image filenames are validated against
  required texture asset targets and existing DDS source-file checks. Do not
  claim callbacks, multi-slider, color picker, actual `forge package`
  execution, ZIP/FOMOD package creation,
  capability-derived runtime requirements, or in-game verification exist until
  later gates implement them.

## Safety gates

Never make slash commands bypass the research:

- `validate` must not rewrite source contracts.
- `clean` targets generated outputs only unless the user explicitly authorizes more.
- `release publish` requires explicit user approval and completed governance checks.
- Public fixtures must be synthetic and redistributable.
- AI cannot be required for validation, build, release, or contribution.
- Generated outputs are disposable and must carry provenance.
