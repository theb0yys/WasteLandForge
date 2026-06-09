# Gate 1 - Repository and ADR Skeleton

Status: Complete
Phase: v0.1 implementation
Decision base: Gate 0, ADR-006, ADR-007, ADR-008, ADR-009, ADR-010, ADR-011

## Gate Definition

Gate 1 creates the repository and documentation skeleton needed before any .NET solution work starts.

This gate is intentionally limited to layout, ADR records, governance placeholders, source/output boundaries, and contributor-facing project documentation.

## Research Decisions Used

| Classification | Decision | Source |
|---|---|---|
| Documented | The repository must distinguish canonical source, generated output, distribution output, and local state. | R006 / ADR-010 |
| Documented | ADR-006 through ADR-011 form the implementation architecture spine. | R003-R008 |
| Documented | Public fixtures must be synthetic and redistributable. | R008 / ADR-011 |
| Documented | Governance includes CODEOWNERS, SECURITY.md, dependency policy, release policy, and least-privilege CI later. | R008 / ADR-011 |
| Inferred | Gate 1 can create docs and placeholders before CODEOWNERS, SECURITY.md, workflows, and solution files because those have later dedicated gates. | Gate 0 |

## Deliverables

- Root `README.md`.
- Root `CONTRIBUTING.md`.
- Root `.gitignore`.
- `.github/` skeleton.
- `docs/adr/ADR-006.md` through `docs/adr/ADR-011.md`.
- `docs/governance/` policy placeholders.
- `eng/`, `schemas/`, `src/`, `tests/`, and `fixtures/` skeletons.
- `generated/` and `dist/` disposable output boundaries.

## Exit Evidence

- Repository layout exists.
- Generated and distribution paths are ignored except for sentinel files.
- ADR files preserve the accepted decision text.
- Governance policy placeholders exist.
- No .NET solution, project files, package references, or `global.json` were created.

## Next Gate

Gate 2 creates the solution and SDK baseline:

1. choose and record the SDK target,
2. create `global.json`,
3. create `WastelandForge.sln`,
4. create empty project skeletons,
5. verify the empty solution builds.
