# Gate 336 - Generated Consumer-Project Forge Command Availability Guidance Closeout

Status: Complete
Phase: CLI bootstrap closeout and next-value routing
Decision base: ADR-009, ADR-010, ADR-011, Gates 334-335, R006, R008

## Goal

Close the generated consumer-project Forge command availability guidance lane
and route the next value slice.

Gate 336 is documentation and prompt routing only. It does not change CLI
runtime behavior, generated `forge init` scaffolds, package restore behavior,
publication policy, install policy, executable packaging, provider handling, or
release behavior.

## Research Grounding

| Classification | Decision | Source |
|---|---|---|
| Documented | Git hooks, editor tasks, and GitHub Actions should remain thin wrappers around the same stable `forge` CLI. | R006 / ADR-010 |
| Documented | Core workflows must stay offline-first and AI-optional. | R006 / ADR-011 |
| Documented | Release and build outputs must be local, deterministic, manifest-backed, and governed before publication. | R008 / ADR-011 |
| Documented | Generated consumer projects remain source-agnostic and use an existing `forge` command until package/feed governance is gated. | Gate 334 |
| Documented | Generated consumer scaffolds now explain and check that `forge` is available on `PATH`. | Gate 335 |
| Open | Public NuGet publication, signed restore, package feed selection, standalone executable distribution, installer technology, signing, and update channels remain unresolved. | Gate 334 / Gate 335 / ADR-012 |

## Closeout

Gates 334-335 resolved the consumer-project scaffold honesty gap:

- generated projects no longer imply they can restore Forge from WastelandForge
  source;
- generated README, VS Code task, and workflow scaffolds now say or check that
  `forge` must already be available;
- generated tasks and workflows still invoke canonical Forge commands directly;
- package-source, publication, signing, standalone distribution, installer, and
  update policy remain deferred.

The next value slice should therefore address how a user obtains a local
`forge` command without requiring the WastelandForge source repository.

## Next Route

Gate 337 should plan local standalone Forge executable distribution.

The planning target is a source-agnostic, offline-friendly handoff for a local
`forge` executable package that can satisfy the generated `forge init`
`PATH` prerequisite before public package/feed policy is ready.

Gate 337 should evaluate:

- local standalone `forge.exe` artifact shape,
- checksum and manifest requirements,
- how it differs from NuGet/local-tool restore,
- how it interacts with the existing `dist/local/forge.exe` manual-test path,
- whether and how `WastelandForge.exe` app-shell packaging stays separate,
- what must wait for installer, signing, update-channel, and publication gates.

## Boundary

Gate 336 does not implement:

- CLI runtime behavior changes,
- generated `forge init` template changes,
- generated `.config/dotnet-tools.json`,
- root `NuGet.config`,
- package restore,
- NuGet publication,
- standalone executable packaging,
- installer generation,
- update-channel logic,
- signing,
- attestation,
- provider installation,
- external game-tool execution,
- MO2 automation,
- GECK automation,
- runtime probes,
- plugin mutation,
- release publication,
- remote repository calls,
- VS Code extension generation,
- language-server process startup,
- AI behavior.

## Acceptance

| Requirement | Status | Evidence |
|---|---|---|
| Command availability guidance lane is closed | Complete | This gate records Gates 334-335 as the completed consumer-project scaffold honesty lane. |
| Remaining install/distribution gap is explicit | Complete | Public package/feed policy and standalone executable distribution remain open. |
| Next value route is selected | Complete | Gate 337 is routed to local standalone Forge executable distribution planning. |
| Runtime behavior is unchanged | Complete | This gate changes docs and prompt routing only. |

## Validation

Required validation:

```text
git diff --check
rg -n "Route the next development step to Gate 336|Gate 336: generated consumer-project Forge command availability guidance closeout" WasteLandForge/skills WasteLandForge/agents .agents
rg -n "Route the next development step to Gate 337|Gate 337" WasteLandForge/skills WasteLandForge/agents .agents WasteLandForge/planning/README.md
changed-file path scan for protected Tales from the Age of Men / Age of Men / overhaul paths
```

Runtime build/test is not required for Gate 336 because it does not change
source code, generated templates, project metadata, task behavior, workflow
behavior, or CLI runtime behavior.

## Next Gate

Gate 337 should plan local standalone Forge executable distribution, still
stopping before executable packaging changes, installer creation, update-channel
logic, signing, attestation, NuGet publication, package restore, generated local
tool manifest emission, root package-source config, provider installation,
external tool execution, MO2/GECK automation, runtime probes, real third-party
plugin fixtures, release publication, remote repository calls, plugin mutation,
VS Code extension generation, language-server process startup, or AI behavior.
