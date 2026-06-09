# Fixture Policy

Status: Skeleton
Research classification: Documented
Source: R008 / ADR-011

Public WastelandForge fixtures must be synthetic and redistributable.

Allowed public fixture material:

- synthetic YAML/JSON registries,
- synthetic asset paths,
- tiny handcrafted binary or record fragments with no shipped game content,
- golden docs,
- SARIF examples,
- build manifests,
- failure cases.

Disallowed public fixture material:

- Bethesda-owned game assets,
- third-party mod files without explicit permission,
- private user installs,
- generated outputs treated as source truth.

Private extended fixtures may use user-supplied real installs or mods, but they must stay out of public CI and public repositories unless explicit permissions exist.

Gate 7 adds public golden fixtures for CLI help and validation JSON output.
These files are synthetic text/JSON contracts and contain no Bethesda assets,
third-party mod files, private install paths, or generated plugin binaries.
