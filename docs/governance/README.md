# Governance

Governance documents here preserve project rules before enforcement is implemented.

- `dependency-policy.md` - dependency and redistribution rules.
- `diagnostic-model.md` - canonical issue JSON model.
- `fixture-policy.md` - public fixture legality and safety.
- `release-policy.md` - release verification and publish boundaries.
- `repository-rulesets.md` - intended GitHub ruleset baseline.
- `rule-families.md` - reserved diagnostic rule families.
- `schema-version-policy.md` - immutable schema ID and version rules.

Gate 8 creates CI, CODEOWNERS, SECURITY.md, Dependabot, PR template, and
repository ruleset baseline docs.

Gate 9 creates release dry-run evidence, Forge-owned build manifests, and
checksums through `forge release verify`.

Gate 10 creates canonical SARIF 2.1.0 diagnostic projection through
`forge validate --format sarif`.

Gate 11 creates Markdown diagnostic summaries and GitHub workflow-command
annotations through `--summary <path>` and `--format github`.
