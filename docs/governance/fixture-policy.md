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

Gate 72 uses disposable local ZIP outputs generated from the same synthetic
fixture. Public committed fixtures still exclude generated archives.

Gate 73 uses disposable local `forge package` outputs generated from the same
synthetic fixture. Public committed fixtures still exclude generated package
archives and package command output trees.

Gate 74 validates generated package manifests and archive entries from the same
synthetic fixture. Public committed fixtures still exclude generated package
manifests, generated archives, and package command output trees.

Gate 75 validates generated install-preview reports from the same synthetic
fixture. Public committed fixtures still exclude generated install previews,
generated package archives, and package command output trees.

Gate 76 validates generated install-preview reports against an embedded schema
using the same synthetic fixture. Public committed fixtures still exclude
generated install previews, generated package archives, and package command
output trees.

Gate 77 validates generated install-preview Markdown summaries using the same
synthetic fixture. Public committed fixtures still exclude generated install
previews, generated package archives, and package command output trees.

Gate 78 validates generated package-verification reports using the same
synthetic fixture. Public committed fixtures still exclude generated package
verification reports, generated package archives, and package command output
trees.
