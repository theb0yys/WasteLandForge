# Engineering

This directory holds shared engineering configuration.

Gate 2 pins the .NET SDK and target framework:

- SDK: `10.0.300`
- Target framework: `net10.0`

Gate 8 adds:

- `ci/New-CiArtifacts.ps1` - local CI governance artifact writer for
  CI build manifest and checksums.

Gate 9 replaces the release-dry-run placeholder with `forge release verify`,
Forge-owned build manifests, and checksums.

Gate 10 replaces placeholder SARIF with `forge validate --format sarif`.

Gate 11 adds Markdown diagnostic summaries and GitHub annotations through the
CLI. CI stores Markdown summaries as artifacts and appends validation summaries
to the GitHub job summary when the runner provides `GITHUB_STEP_SUMMARY`.
