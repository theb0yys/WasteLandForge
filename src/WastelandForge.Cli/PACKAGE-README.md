# WastelandForge CLI

WastelandForge.Cli provides the `forge` command for WastelandForge projects.

The CLI is designed for offline-first validation, capability inspection,
generation, packaging, release dry-run evidence, and project documentation
workflows.

Example commands:

```text
forge --version
forge help
forge validate fixtures/projects/ExampleMod --format json
forge capabilities scan --help
```

This package metadata scaffold is for local-tool validation. Public package
publication, checked-in tool manifests, signing, attestation, provider
installation, external game-tool execution, and AI behavior are handled by
separate gates.
