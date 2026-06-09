# Gate 0 - Definition, Goals, and Plan

Status: Complete
Phase: v0.1 implementation planning
Decision base: ADR-006, ADR-007, ADR-008, ADR-009, ADR-010, ADR-011

Completion note: Accepted as the active plan before Gate 1 repository and ADR skeleton work began.

## Gate Definition

Gate 0 defines the implementation boundary before repository scaffolding begins. It does not create the .NET solution, schemas, validators, generators, fixtures, CI workflows, or release automation. It locks the research-backed goals, first buildable slice, exit criteria, and open checks that later gates must satisfy.

Gate 0 exists to prevent implementation drift from the completed R001-R008 foundation. Work may move to Gate 1 only after the planned scope, non-goals, command surface, validation model, and governance requirements are explicit.

## Research Decisions Used

| Classification | Decision | Source |
|---|---|---|
| Documented | WastelandForge is a hybrid capability platform with an authoritative external core that owns contracts, registries, validation, generation, and release workflow orchestration. | Platform architecture report / ADR-006 |
| Documented | Source truth is versioned YAML/JSON contracts normalized to canonical JSON and validated with JSON Schema Draft 2020-12 plus deterministic semantic validation. | R004 / ADR-007 |
| Documented | Projects depend on capabilities, not provider names; providers are versioned registry data that satisfy capabilities. | R005 / ADR-008 |
| Documented | Outputs are generated through a deterministic, capability-aware build graph, and generated artifacts are disposable outputs that must carry provenance. | Generator/build report / ADR-009 |
| Documented | Developers use a small, stable, offline-first CLI with the ADR-010/R006 command surface. | R006 / ADR-010 |
| Documented | Validation is layered and maps canonical JSON diagnostics outward to console, JSON, SARIF, Markdown, GitHub annotations, and TRX test records. | R008 / ADR-011 |
| Documented | Public fixtures must be synthetic and redistributable; Bethesda assets and third-party mod files stay out of the public fixture corpus unless explicit permission exists. | R008 / ADR-011 |
| Inferred | Gate 0 should be planning-only because the research says the first defensible implementation spine is validation, schemas, diagnostics, CLI, fixtures, manifests, and CI, not game-facing generation. | R004, R006, R008 |
| Open | The implementation still needs a dependency compatibility check before finalizing .NET 10 LTS as the target. | R008 open implementation question |

## Goals

1. Define a gated implementation sequence for WastelandForge v0.1.
2. Lock the first buildable slice to the research-backed validation-first spine.
3. Keep canonical truth in repository source contracts, never generated outputs, game saves, GECK sessions, agent traces, or AI responses.
4. Preserve the full ADR-010/R006 command surface while implementing it in thin, testable slices.
5. Keep the correctness path offline-first and AI-optional.
6. Establish release and governance requirements before publish flows exist.

## Non-Goals

Gate 0 does not implement:

- binary ESP/ESM generation,
- xEdit patch authoring,
- GECK automation,
- native runtime DLL work,
- AI voice generation,
- AI-required validation or release behavior,
- real Bethesda asset fixtures,
- public release publishing,
- packaging beyond a later release dry-run baseline.

## Canonical Command Surface

The v0.1 CLI plan must preserve these command names:

```text
forge init
forge validate
forge capabilities list
forge capabilities scan
forge capabilities explain
forge generate
forge build
forge package
forge release verify
forge release prepare
forge release publish
forge docs
forge graph
forge explain
forge clean
forge doctor export
forge help
forge --version
```

Early gates may implement only skeleton handlers, but they must not introduce shortcut aliases that spend future command namespace.

## First Buildable Slice

The first implementation slice is:

1. Repository structure.
2. Solution/project layout.
3. ADR files.
4. Schema package skeleton.
5. Core C# domain models.
6. CLI command skeleton.
7. First manifest schema.
8. First validation pipeline.
9. First fixture project.
10. GitHub Actions baseline.

The implementation order is binding for Gate 1 onward unless a later gate records a documented blocker.

## Planned Gates

| Gate | Name | Purpose | Exit Evidence |
|---|---|---|---|
| 0 | Definition, Goals, and Plan | Lock research-backed v0.1 boundary. | This document exists and is accepted as the implementation gate map. |
| 1 | Repository and ADR Skeleton | Create root layout, documentation directories, ADR-006 through ADR-011 files, governance placeholders, and source/output boundaries. | Layout exists; generated and dist paths are clearly disposable; ADR files exist. |
| 2 | Solution and SDK Baseline | Create `WastelandForge.sln`, project skeletons, `global.json`, shared build props, and SDK target check. | Solution builds with empty projects; target framework decision recorded. |
| 3 | Schema Package Skeleton | Create schema directories, immutable `$id` convention, manifest schema stub, and local schema resolver shape. | First schema validates as JSON; schema IDs and version policy documented. |
| 4 | Core Domain and Diagnostics | Add IDs, source locations, version constraints, diagnostics, severities, JSON Pointer locations, and issue serialization. | Unit tests cover core model behavior and issue JSON shape. |
| 5 | Loader and Validation Pipeline | Implement load/source validation, schema validation, semantic placeholder stage, and capability placeholder stage. | `forge validate` can run against synthetic fixture input and emit deterministic issue JSON. |
| 6 | CLI Skeleton | Add canonical commands, help, `--version`, `--format`, exit codes, and command dispatch. | CLI help lists only canonical command surface; skeleton commands return stable output. |
| 7 | Fixtures and Tests | Add synthetic fixture projects, schema tests, semantic tests, golden diagnostics, and Windows path tests. | Public fixtures contain no proprietary assets; test projects run locally. |
| 8 | CI and Governance Baseline | Add GitHub Actions Windows lane, Ubuntu lane, TRX output, SARIF artifact, CODEOWNERS, SECURITY.md, and Dependabot baseline. | CI workflow is thin and calls the CLI; governance files exist. |
| 9 | Build Manifest and Release Dry-Run | Add local build-manifest writer, package staging skeleton, checksum output, and release verification dry-run. | Build/release dry-run emits manifest and checksums without publishing. |

## Gate 0 Exit Criteria

Gate 0 is complete when:

- the Gate 0 document is committed or explicitly accepted as the active plan,
- the command surface is locked to ADR-010/R006,
- the first buildable slice is listed in implementation order,
- non-goals are explicit,
- `.NET target`, dependency compatibility, fixture legality, and CI publishing choices are tracked as checks,
- every later gate has exit evidence,
- the next action is Gate 1 repository and ADR skeleton work.

## Open Implementation Checks

| Check | Status | Gate |
|---|---|---|
| Confirm .NET 10 LTS dependency compatibility for `System.CommandLine`, `YamlDotNet`, `JsonSchema.Net`, test stack, and packaging tools. | Open | 2 |
| Decide whether Draft 7 editor companion schemas are generated in v0.1 or deferred behind an editor-extension plan. | Open | 3 |
| Decide whether JUnit output stays out of core as a derived compatibility adapter. | Open | 8 |
| Decide whether GitHub code scanning upload is enabled by default or opt-in while SARIF remains local and canonical. | Open | 8 |
| Decide how much packaging beyond ZIP belongs in v0.1; FOMOD is not part of Gate 0 scope. | Open | 9 |
| Define signed tag or signed commit enforcement level after contributor friction is understood. | Open | 8 |

## Immediate Plan

Gate 1 starts with repository and ADR skeleton work:

1. Create root implementation directories: `.github/`, `docs/`, `eng/`, `schemas/`, `src/`, `tests/`, `fixtures/`, `generated/`, and `dist/`.
2. Add ignore rules for disposable generated, dist, cache, logs, build, and test artifacts.
3. Add `docs/adr/ADR-006.md` through `docs/adr/ADR-011.md` with decisions copied from the research spine.
4. Add `docs/governance/` placeholders for rule IDs, fixture policy, release policy, and dependency policy.
5. Add `README.md` and `CONTRIBUTING.md` skeletons that state the validation-first, offline-first, AI-optional operating model.
6. Stop before creating the .NET solution; that is Gate 2.

## Gate Rule

No later gate may weaken these Gate 0 constraints without recording:

1. the research section that supports the change,
2. whether the change is `Documented`, `Inferred`, or `Open`,
3. the impact on validation, governance, fixtures, and release safety.
