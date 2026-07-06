# Gate 333 - Repository-Owned Local-Tool Bootstrap Lane Closeout

Status: Complete
Phase: CLI bootstrap closeout and next-value routing
Decision base: ADR-010, ADR-011, Gates 322-332, R006, R008

## Goal

Close the repository-owned Forge local-tool bootstrap lane and route the next
value slice to the unresolved consumer-project package-source policy.

Gate 333 is documentation and prompt routing only. It does not change CLI
runtime behavior, generated `forge init` task or workflow templates, package
publication, external tool execution, or provider detection.

## Research Grounding

| Classification | Decision | Source |
|---|---|---|
| Documented | Git hooks, editor tasks, and CI should be thin wrappers around the stable `forge` CLI. | R006 / ADR-010 |
| Documented | Repository-pinned tooling through local tool manifests reduces version drift and supports reproducible CI. | R006 / ADR-010 |
| Documented | CI and validation must stay offline-first, AI-optional, fixture-backed, and auditable. | R008 / ADR-011 |
| Inferred | Gates 322 through 332 now cover the source-repository bootstrap path well enough to stop adding repository-owned wrappers by default. | Gates 322-332 |
| Open | Consumer-project generated task and workflow templates still need a published package or explicit package-source policy before they can restore Forge without the WastelandForge source tree. | Gate 328 / Gate 332 |

## Closeout

The repository-owned local-tool bootstrap lane now covers:

- source-built runner shims through `eng/forge.ps1` and `eng/forge.cmd`,
- CLI .NET tool package metadata for command `forge`,
- checked-in `.config/dotnet-tools.json`,
- local package-source restore through `eng/Restore-ForgeTool.ps1`,
- repository-owned CI restore and `dotnet tool run forge --` invocation,
- repository-owned VS Code task restore and read-only Forge task invocation.

That is enough for this source repository. Further source-repository bootstrap
work should be reopened only for a documented product gap, such as version
upgrade policy, signed package verification, or a task/workflow failure found
in real use.

## Next Route

Gate 334 should plan the consumer-project Forge package-source policy.

The target is to decide how generated projects created by `forge init` should
obtain a known `forge` command without assuming the WastelandForge source tree
exists. The planning gate should compare:

- published NuGet local-tool restore,
- explicit package-source configuration,
- standalone executable distribution,
- source-repository-only developer bootstrap,
- offline-first constraints,
- generated task/workflow mutation boundaries,
- version pinning and upgrade policy.

Gate 334 should stay planning-only and stop before mutating generated task or
workflow templates.

## Not Implemented

Gate 333 does not implement:

- CLI runtime behavior changes,
- new command names or aliases,
- generated `.vscode/tasks.json` mutation,
- generated `.github/workflows/wastelandforge.yml` mutation,
- root `NuGet.config`,
- NuGet publication,
- package signing,
- package attestation,
- provider installation,
- external tool execution,
- MO2 automation,
- GECK automation,
- xEdit execution,
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
| Repository-owned bootstrap lane is explicitly closed | Complete | This gate documents the closeout boundary. |
| Existing source-repository bootstrap artifacts are listed | Complete | Closeout section records shims, local tool metadata, manifest, restore helper, CI, and VS Code tasks. |
| Consumer-project package-source gap remains explicit | Complete | Next route is Gate 334 package-source policy planning. |
| Canonical command surface is preserved | Complete | No new command or alias is introduced. |
| Runtime behavior is unchanged | Complete | This gate is docs and prompt routing only. |

## Validation

Required validation:

```text
git diff --check
rg -n "Route the next development step to Gate 333|Gate 333: repository-owned local-tool bootstrap lane closeout" WasteLandForge/skills WasteLandForge/agents .agents
rg -n "Route the next development step to Gate 334|Gate 334" WasteLandForge/skills WasteLandForge/agents .agents WasteLandForge/planning/README.md
rg -n "Tales from the Age of Men|Tales From The Age Of Men|Age of Men|overhaul" .agents WasteLandForge docs src tests eng .github .vscode
```

Runtime build/test is not required for Gate 333 because it does not change
source code, project metadata, task behavior, workflow behavior, or CLI
runtime behavior.

## Next Gate

Gate 334 should plan consumer-project Forge package-source policy, still
stopping before generated workflow/task mutation, NuGet publication, root
`NuGet.config`, provider installation, external tool execution, MO2/GECK
automation, runtime probes, real third-party plugin fixtures, release
publication, remote repository calls, signing or attestation, plugin mutation,
VS Code extension generation, language-server process startup, or AI behavior.
