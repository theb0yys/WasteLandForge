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

Gate 69 adds a tiny handcrafted DDS-header fixture for an MCM image reference.
It is synthetic test material, not a Bethesda asset or third-party mod file.

Gate 70 uses that synthetic DDS fixture to test loose-file staging. Generated
and dist outputs remain disposable local artifacts and are not committed as
fixture source truth.

Gate 71 uses disposable local package-manifest outputs generated from the same
synthetic fixture. Public committed fixtures still exclude generated outputs
and archives.
