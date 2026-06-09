# GitHub Configuration

Gate 8 establishes the GitHub governance baseline:

- `.github/workflows/ci.yml` - Windows mandatory lane, Ubuntu fast-validation lane, TRX artifacts, SARIF upload surface, and release dry-run placeholder.
- `.github/CODEOWNERS` - code-owner review baseline for sensitive paths.
- `.github/dependabot.yml` - NuGet and GitHub Actions update checks.
- `.github/pull_request_template.md` - research grounding, validation, fixture, and AI-assist disclosure checklist.

Repository rulesets are settings, not files. The intended baseline is recorded in `docs/governance/repository-rulesets.md`.
