# Engineering

This directory holds shared engineering configuration.

Gate 2 pins the .NET SDK and target framework:

- SDK: `10.0.300`
- Target framework: `net10.0`

Gate 8 adds:

- `ci/New-CiArtifacts.ps1` - local CI governance artifact writer for
  placeholder SARIF, CI build manifest, and checksums.

Gate 9 replaces the release-dry-run placeholder with Forge-owned release
verification and build-manifest behavior.
