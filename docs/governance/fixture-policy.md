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

Gate 79 validates generated package-verification reports against an embedded
schema using the same synthetic fixture. Public committed fixtures still
exclude generated package verification reports, generated package archives,
and package command output trees.

Gate 80 validates generated package-verification Markdown summaries using the
same synthetic fixture. Public committed fixtures still exclude generated
package verification reports, generated package verification summaries,
generated package archives, and package command output trees.

Gate 81 validates package-verification cross-check evidence using the same
synthetic fixture. Public committed fixtures still exclude generated package
verification reports, summaries, manifests, generated package archives, and
package command output trees.

Gate 82 validates the reusable package-verification evidence validator with
synthetic in-memory JSON evidence. Public committed fixtures still exclude
generated package verification reports, summaries, manifests, generated
package archives, and package command output trees.

Gate 83 validates the file-based package-verification verifier with temp-only
generated files from synthetic fixtures. Public committed fixtures still
exclude generated package verification reports, summaries, manifests,
generated package archives, and package command output trees.

Gate 84 validates payload digest recomputation with temp-only generated files
from synthetic fixtures. Public committed fixtures still exclude generated
package verification reports, summaries, manifests, generated package
archives, and package command output trees.

Gate 85 validates archive digest recomputation with temp-only generated files
from synthetic fixtures. Public committed fixtures still exclude generated
package verification reports, summaries, manifests, generated package
archives, and package command output trees.

Gate 86 validates archive entry-name revalidation with temp-only generated
files from synthetic fixtures. Public committed fixtures still exclude
generated package verification reports, summaries, manifests, generated
package archives, and package command output trees.

Gate 87 adds no fixture corpus changes. It records the future command shape
for verifying existing generated package evidence while preserving the
synthetic-fixture-only policy.

Gate 88 adds golden CLI tests that generate package evidence in temp-only
copies of the synthetic `ExampleMod` fixture. It still adds no generated
package verification reports, summaries, archives, manifests, or package
command output trees to the committed fixture corpus.

Gate 89 adds golden CLI tests for verify-existing SARIF, GitHub annotations,
and GitHub step-summary behavior using temp-only copies of the synthetic
`ExampleMod` fixture. It still adds no generated package verification reports,
summaries, archives, manifests, or package command output trees to the
committed fixture corpus.

Gate 90 adds golden CLI tests for verify-existing Markdown summary output
using temp-only copies of the synthetic `ExampleMod` fixture and temp-only
summary files. It still adds no generated package verification reports,
summaries, archives, manifests, or package command output trees to the
committed fixture corpus.

Gate 91 adds checksum-file revalidation tests using temp-only copies of the
synthetic `ExampleMod` fixture and temp-only generated package outputs. It
still adds no generated package verification reports, checksum files,
archives, manifests, or package command output trees to the committed fixture
corpus.

Gate 92 adds build-manifest content revalidation tests using temp-only copies
of the synthetic `ExampleMod` fixture and temp-only generated package outputs.
It still adds no generated package verification reports, build manifests,
checksum files, archives, or package command output trees to the committed
fixture corpus.

Gate 93 adds install-preview summary content revalidation tests using
temp-only copies of the synthetic `ExampleMod` fixture and temp-only generated
package outputs. It still adds no generated install-preview summaries,
package verification reports, build manifests, checksum files, archives, or
package command output trees to the committed fixture corpus.

Gate 94 adds install-preview/package-manifest entry content cross-check tests
using temp-only copies of the synthetic `ExampleMod` fixture and temp-only
generated package outputs. It still adds no generated install-preview reports,
package manifests, package verification reports, build manifests, checksum
files, archives, or package command output trees to the committed fixture
corpus.

Gate 95 adds package-verification summary content revalidation tests using
temp-only copies of the synthetic `ExampleMod` fixture and temp-only generated
package outputs. It still adds no generated package verification summaries,
package verification reports, build manifests, checksum files, archives, or
package command output trees to the committed fixture corpus.

Gate 96 adds package-verification JSON check content revalidation tests using
temp-only copies of the synthetic `ExampleMod` fixture and temp-only generated
package outputs. It still adds no generated package verification reports,
build manifests, checksum files, archives, or package command output trees to
the committed fixture corpus.

Gate 97 adds package-verification JSON metadata content revalidation tests
using temp-only copies of the synthetic `ExampleMod` fixture and temp-only
generated package outputs. It still adds no generated package verification
reports, build manifests, checksum files, archives, or package command output
trees to the committed fixture corpus.

Gate 98 adds package-verification archive detail content revalidation tests
using temp-only copies of the synthetic `ExampleMod` fixture and temp-only
generated package outputs. It still adds no generated package verification
reports, build manifests, checksum files, archives, or package command output
trees to the committed fixture corpus.

Gate 99 adds install-preview archive detail content revalidation tests using
temp-only copies of the synthetic `ExampleMod` fixture and temp-only generated
package outputs. It still adds no generated package verification reports,
build manifests, checksum files, archives, or package command output trees to
the committed fixture corpus.
