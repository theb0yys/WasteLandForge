# Repository Rulesets

Status: Gate 8 baseline
Research classification: Documented
Source: R008 / ADR-011

R008 recommends GitHub rulesets rather than relying only on classic branch
protection. Rulesets are repository settings, so this file records the intended
baseline for maintainers to apply in GitHub.

## Protected References

- `main`
- `release/*`

## Required Pull Request Rules

- Require pull requests before merge.
- Require code-owner review for paths covered by `.github/CODEOWNERS`.
- Block force pushes and branch deletion.
- Require conversation resolution before merge.

## Required Status Checks

Use unique job names as required checks:

- `validate-ubuntu`
- `build-test-windows`
- `release-dry-run` on `main` and `release/*`

## CI Permissions

Workflow permissions are read-only by default.

Escalations are job-scoped:

- `validate-ubuntu` grants `security-events: write` only to upload SARIF.
- `build-test-windows` grants only `contents: read`.
- `release-dry-run` grants only `contents: read`.

## Gate 8 Open Checks

- Keep solution-level `dotnet test` serial with `-m:1` until the Gate 7
  parallel VSTest/xUnit hang is resolved.
- GitHub SARIF upload remains an optional publishing surface. Gate 10 local
  `forge validate --format sarif` generation is the correctness path.
- Gate 11 adds GitHub annotations and Markdown job summaries as convenience
  projections; they do not replace local validation or SARIF artifacts.
- Gate 9 replaced the release-dry-run fixture validation with `forge release
  verify`, Forge-owned `build-manifest.json`, and checksums.
