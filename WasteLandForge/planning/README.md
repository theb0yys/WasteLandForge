# WastelandForge Planning

Implementation planning is gated. Each gate defines its purpose, exit evidence, and open checks before the next implementation slice starts.

Gate status:

- `gates/gate-000-definition-goals-plan.md` - Gate 0, definition, goals, and plan.
- `gates/gate-001-repository-adr-skeleton.md` - Gate 1, repository and ADR skeleton.
- `gates/gate-002-solution-sdk-baseline.md` - Gate 2, solution and SDK baseline.
- `gates/gate-003-schema-package-skeleton.md` - Gate 3, schema package skeleton.
- `gates/gate-004-core-domain-diagnostics.md` - Gate 4, core domain and diagnostics.
- `gates/gate-005-loader-validation-pipeline.md` - Gate 5, loader and validation pipeline.
- `gates/gate-006-cli-skeleton.md` - Gate 6, CLI skeleton.
- `gates/gate-007-fixtures-tests.md` - Gate 7, fixtures and tests.
- `gates/gate-008-ci-governance-baseline.md` - Gate 8, CI and governance baseline.
- `gates/gate-009-release-dry-run-build-manifest.md` - Gate 9, release dry-run and build manifest evidence.
- `gates/gate-010-sarif-diagnostic-projection.md` - Gate 10, canonical SARIF diagnostic projection.
- `gates/gate-011-markdown-github-diagnostics.md` - Gate 11, Markdown summaries and GitHub annotations.
- `gates/gate-012-yaml-runtime-schema-validation.md` - Gate 12, YAML ingestion and runtime manifest schema validation.
- `gates/gate-013-registry-schema-validation.md` - Gate 13, dependency and capability registry schema validation.
- `gates/gate-014-asset-registry-schema-validation.md` - Gate 14, asset registry schema validation.
- `gates/gate-015-asset-path-semantic-validation.md` - Gate 15, asset path semantic validation.

Next gate:

- Gate 16 - asset type-specific validation.

Gate work must follow `AGENTS.md` and classify important claims as `Documented`, `Inferred`, or `Open`.
