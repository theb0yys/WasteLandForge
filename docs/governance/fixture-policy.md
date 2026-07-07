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

Gate 100 adds package-manifest archive detail content revalidation tests using
temp-only copies of the synthetic `ExampleMod` fixture and temp-only generated
package outputs. It still adds no generated package verification reports,
build manifests, checksum files, archives, or package command output trees to
the committed fixture corpus.

Gate 101 adds archive detail cross-report consistency revalidation tests using
temp-only copies of the synthetic `ExampleMod` fixture and temp-only generated
package outputs. It still adds no generated package verification reports,
build manifests, checksum files, archives, or package command output trees to
the committed fixture corpus.

Gate 102 adds package archive presence revalidation tests using temp-only
copies of the synthetic `ExampleMod` fixture and temp-only generated package
outputs. It still adds no generated package verification reports, build
manifests, checksum files, archives, or package command output trees to the
committed fixture corpus.

Gate 103 adds checksum unexpected-entry revalidation tests using temp-only
copies of the synthetic `ExampleMod` fixture and temp-only generated package
outputs. It still adds no generated package verification reports, build
manifests, checksum files, archives, or package command output trees to the
committed fixture corpus.

Gate 104 adds checksum duplicate-entry revalidation tests using temp-only
copies of the synthetic `ExampleMod` fixture and temp-only generated package
outputs. It still adds no generated package verification reports, build
manifests, checksum files, archives, or package command output trees to the
committed fixture corpus.

Gate 105 adds checksum canonical-order revalidation tests using temp-only
copies of the synthetic `ExampleMod` fixture and temp-only generated package
outputs. It still adds no generated package verification reports, build
manifests, checksum files, archives, or package command output trees to the
committed fixture corpus.

Gate 106 adds checksum digest canonical-casing revalidation tests using
temp-only copies of the synthetic `ExampleMod` fixture and temp-only generated
package outputs. It still adds no generated package verification reports, build
manifests, checksum files, archives, or package command output trees to the
committed fixture corpus.

Gate 107 adds checksum line-ending and trailing-newline revalidation tests using
temp-only copies of the synthetic `ExampleMod` fixture and temp-only generated
package outputs. It still adds no generated package verification reports, build
manifests, checksum files, archives, or package command output trees to the
committed fixture corpus.

Gate 108 adds checksum path separator canonicalization revalidation tests using
temp-only copies of the synthetic `ExampleMod` fixture and temp-only generated
package outputs. It still adds no generated package verification reports, build
manifests, checksum files, archives, or package command output trees to the
committed fixture corpus.

Gate 109 adds checksum blank-line revalidation tests using temp-only copies of
the synthetic `ExampleMod` fixture and temp-only generated package outputs. It
still adds no generated package verification reports, build manifests, checksum
files, archives, or package command output trees to the committed fixture
corpus.

Gate 110 adds checksum entry spacing canonicalization revalidation tests using
temp-only copies of the synthetic `ExampleMod` fixture and temp-only generated
package outputs. It still adds no generated package verification reports, build
manifests, checksum files, archives, or package command output trees to the
committed fixture corpus.

Gate 111 adds checksum path casing canonicalization revalidation tests using
temp-only copies of the synthetic `ExampleMod` fixture and temp-only generated
package outputs. It still adds no generated package verification reports, build
manifests, checksum files, archives, or package command output trees to the
committed fixture corpus.

Gate 112 adds checksum case-insensitive duplicate revalidation tests using
temp-only copies of the synthetic `ExampleMod` fixture and temp-only generated
package outputs. It still adds no generated package verification reports, build
manifests, checksum files, archives, or package command output trees to the
committed fixture corpus.

Gate 113 adds checksum malformed-entry format revalidation tests using
temp-only copies of the synthetic `ExampleMod` fixture and temp-only generated
package outputs. It still adds no generated package verification reports, build
manifests, checksum files, archives, or package command output trees to the
committed fixture corpus.

Gate 114 adds checksum path containment revalidation tests using temp-only
copies of the synthetic `ExampleMod` fixture and temp-only generated package
outputs. It still adds no generated package verification reports, build
manifests, checksum files, archives, or package command output trees to the
committed fixture corpus.

Gate 115 adds checksum comment-line rejection revalidation tests using
temp-only copies of the synthetic `ExampleMod` fixture and temp-only generated
package outputs. It still adds no generated package verification reports, build
manifests, checksum files, archives, or package command output trees to the
committed fixture corpus.

Gate 116 adds install-plan generation and schema tests using temp-only copies
of the synthetic `ExampleMod` fixture and temp-only generated package outputs.
It still adds no generated install-plan reports, build manifests, checksum
files, archives, or package command output trees to the committed fixture
corpus.

Gate 117 adds install-plan verify-existing content revalidation tests using
temp-only copies of the synthetic `ExampleMod` fixture and temp-only generated
package outputs. It still adds no generated install-plan reports, build
manifests, checksum files, archives, or package command output trees to the
committed fixture corpus.

Gate 118 adds install-plan verify-existing schema revalidation tests using
temp-only copies of the synthetic `ExampleMod` fixture and temp-only generated
package outputs. It still adds no generated install-plan reports, build
manifests, checksum files, archives, or package command output trees to the
committed fixture corpus.

Gate 119 adds package-manifest verify-existing schema revalidation tests using
temp-only copies of the synthetic `ExampleMod` fixture and temp-only generated
package outputs. It still adds no generated package manifests, build
manifests, checksum files, archives, or package command output trees to the
committed fixture corpus.

Gate 120 adds install-preview verify-existing schema revalidation tests using
temp-only copies of the synthetic `ExampleMod` fixture and temp-only generated
package outputs. It still adds no generated install-preview reports, build
manifests, checksum files, archives, or package command output trees to the
committed fixture corpus.

Gate 121 adds package-verification verify-existing schema revalidation tests
using temp-only copies of the synthetic `ExampleMod` fixture and temp-only
generated package outputs. It still adds no generated package-verification
reports, build manifests, checksum files, archives, or package command output
trees to the committed fixture corpus.

Gate 122 adds schema-gated verify-existing diagnostic projection tests using
temp-only copies of the synthetic `ExampleMod` fixture and temp-only generated
package outputs. It still adds no generated diagnostic projections, generated
package reports, build manifests, checksum files, archives, or package command
output trees to the committed fixture corpus.

Gate 123 adds missing-evidence verification tests using temp-only copies of
the synthetic `ExampleMod` fixture and temp-only generated package outputs. It
still adds no generated package reports, build manifests, checksum files,
archives, or package command output trees to the committed fixture corpus.

Gate 124 adds malformed-evidence verification tests using temp-only copies of
the synthetic `ExampleMod` fixture and temp-only generated package outputs. It
still adds no malformed package reports, build manifests, checksum files,
archives, or package command output trees to the committed fixture corpus.

Gate 125 adds malformed-evidence diagnostic projection tests using temp-only
copies of the synthetic `ExampleMod` fixture and temp-only generated package
outputs. It still adds no generated diagnostic projections, generated package
reports, build manifests, checksum files, archives, or package command output
trees to the committed fixture corpus.

Gate 126 adds no new public fixture files. The MCM Extender fixture lane is
parked; next fixture work should remain synthetic and redistributable while
covering capability scanner and Doctor-style environment evidence.

Gate 127 adds no committed provider fixtures. Golden CLI tests create temp-only
synthetic Fallout: New Vegas roots and tool executables to exercise the Doctor
readiness report.

Gate 128 adds no committed provider fixtures. Golden CLI tests reuse temp-only
synthetic provider markers and assert that Doctor export JSON redacts local
paths before writing console or file output.

Gate 129 adds no committed provider fixtures. Golden CLI tests use existing
synthetic project fixtures plus temp-only missing/unknown provider evidence to
exercise `WF-CAP-*` JSON, SARIF, and GitHub diagnostic projection.

Gate 130 adds no committed provider fixtures. Unit and golden CLI tests use
existing synthetic project fixtures plus temp-only local provider evidence to
exercise diagnostic evidence arrays, SARIF evidence properties, GitHub
annotation evidence text, and Doctor export redaction of nested evidence paths.

Gate 131 adds no committed provider fixtures. Golden CLI tests use existing
synthetic provider evidence and temp-only scan inputs to exercise grouped
provider evidence in `forge capabilities explain` JSON and human/plain output.

Gate 132 adds no committed provider fixtures. Golden CLI tests create
temp-only synthetic Fallout: New Vegas roots with misplaced provider markers
to exercise wrong-scope scan and `WF-CAP-004` diagnostic projection.

Gate 133 adds no committed provider fixtures. Golden CLI tests use the
existing synthetic `ExampleMod` fixture and temp-only scan inputs to exercise
`forge capabilities explain --project` requirement context.

Gate 134 adds no committed provider fixtures. Golden CLI tests use the
existing synthetic `ExampleMod` fixture and temp-only scan inputs to exercise
`forge capabilities explain --project` diagnostic handoff context.

Gate 135 adds no committed provider fixtures. Golden CLI tests use the
existing synthetic `ExampleMod` fixture and temp-only scan inputs to exercise
`forge doctor export` summary and index output.

Gate 136 adds no committed provider fixtures. Golden CLI tests use the
existing synthetic `ExampleMod` fixture and temp-only scan inputs to exercise
`forge doctor export` compact diagnostics index output and redaction safety.

Gate 137 adds no committed provider fixtures. Golden CLI tests use the
existing synthetic `ExampleMod` fixture and temp-only scan inputs to exercise
`forge doctor export` compact requirements index output and redaction safety.

Gate 138 adds no committed provider fixtures. Golden CLI tests use the
existing synthetic `ExampleMod` fixture and temp-only scan inputs to exercise
`forge doctor export` compact action index output and redaction safety.

Gate 139 adds no committed provider fixtures. Golden CLI tests use existing
synthetic fixtures and built-in catalogue open questions to exercise
`forge doctor export` structured open-question detail output.

Gate 140 adds no committed provider fixtures. Golden CLI tests use existing
synthetic fixtures and built-in catalogue provider scan results to exercise
`forge doctor export` provider-status index output.

Gate 141 adds no committed provider fixtures. Golden CLI tests use existing
synthetic fixtures and built-in catalogue capability scan results to exercise
`forge doctor export` capability-status index output.

Gate 142 adds no committed provider fixtures. Golden CLI tests use existing
synthetic fixtures and built-in Doctor area results to exercise `forge doctor
export` Doctor area-status index output.

Gate 143 adds no committed provider fixtures. Golden CLI tests use existing
synthetic fixtures and built-in catalogue open-question detail entries to
exercise `forge doctor export` catalogue-policy index output.

Gate 144 adds no committed provider fixtures. Golden CLI tests use existing
synthetic fixtures and built-in Doctor area results to exercise `forge
capabilities scan` Doctor readiness index output.

Gate 145 adds no committed provider fixtures. Golden CLI tests use existing
synthetic fixtures and built-in provider/capability scan results to exercise
`forge capabilities scan` provider/capability status index output.

Gate 146 adds no committed provider fixtures. Golden CLI tests use existing
synthetic fixtures and built-in Doctor area actions to exercise `forge
capabilities scan` action index output.

Gate 147 adds no committed provider fixtures. Golden CLI tests use existing
synthetic fixtures and built-in project requirement resolution output to
exercise `forge capabilities scan` requirement index output.

Gate 148 adds no committed provider fixtures. Golden CLI tests use existing
synthetic fixtures and built-in diagnostic projection output to exercise
`forge capabilities scan` diagnostic index output.

Gate 149 adds no committed provider fixtures. Golden CLI tests use existing
synthetic fixtures and built-in Doctor open-question output to exercise
`forge capabilities scan` catalogue-policy index output.
Gate 150 adds no committed provider fixtures. Golden CLI tests use existing
synthetic fixtures and built-in Doctor open-question output to exercise
`forge capabilities scan` open-question detail index output.
Gate 151 adds no committed provider fixtures. Golden CLI tests use existing
synthetic fixtures and built-in Doctor open-question output to exercise
`forge capabilities explain` catalogue-policy open-question detail output.
Gate 152 adds no committed provider fixtures. Golden CLI tests use existing
synthetic fixtures and built-in Doctor open-question output to exercise
`forge capabilities explain` catalogue-policy diagnostic handoff output.
Gate 153 adds no committed provider fixtures. Golden CLI tests use existing
synthetic fixtures and built-in Doctor open-question output to exercise
`forge doctor export` catalogue-policy diagnostic handoff output.
Gate 154 adds no committed provider fixtures. Golden CLI tests use existing
synthetic fixtures and built-in Doctor open-question output to exercise
`forge capabilities scan` catalogue-policy diagnostic handoff output.
Gate 155 adds no committed provider fixtures. Golden CLI tests use existing
synthetic inputs plus a focused helper test to cover shared
catalogue-policy diagnostic handoff JSON shape and text indentation.
Gate 156 adds no committed provider fixtures. Golden CLI tests use existing
synthetic inputs plus a focused helper test to cover shared
catalogue-policy open-question detail JSON shape, source-type index JSON
shape, and text indentation.
Gate 157 adds no committed provider fixtures. Golden CLI tests use existing
synthetic inputs plus a focused helper test to cover shared
catalogue-policy view derivation, copied open-question inputs, and empty
input behavior.
Gate 158 adds no committed provider fixtures. Golden CLI tests use existing
synthetic inputs plus a focused helper test to cover Doctor action summary
grouping, JSON shape, text indentation, and empty output behavior.
Gate 159 adds no committed provider fixtures. Golden CLI tests use existing
synthetic inputs plus a focused helper test to cover provider evidence
summary grouping, JSON shape, text indentation, and empty output behavior.
Gate 160 adds no committed provider or project fixtures. Golden CLI tests use
existing synthetic inputs plus a focused helper test to cover requirement
summary grouping, JSON shape, text indentation, and empty output behavior.
Gate 161 adds no committed provider or project fixtures. Golden CLI tests use
existing synthetic inputs plus a focused helper test to cover diagnostic
summary grouping, JSON shape, text indentation, and empty output behavior.
Gate 162 adds no committed provider or project fixtures. Golden CLI tests use
existing synthetic inputs plus a focused helper test to cover provider
inventory summary grouping, JSON shape, text indentation, and empty output
behavior.
Gate 163 adds no committed provider or project fixtures. Golden CLI tests use
existing synthetic inputs plus a focused helper test to cover Doctor area
capability summary grouping, JSON shape, text indentation, and empty output
behavior.
Gate 164 adds no committed provider or project fixtures. Golden CLI tests use
existing synthetic project fixtures and temporary synthetic local provider
markers to cover the redacted Doctor export Markdown sidecar.
Gate 165 adds no committed provider or project fixtures. Golden CLI tests use
existing synthetic project fixtures and temporary synthetic local provider
markers to cover the path-minimized capability scan Markdown sidecar.
Gate 166 adds no committed provider or project fixtures. Golden CLI tests use
existing synthetic project fixtures and temporary synthetic local provider
markers to cover the redacted Doctor export ZIP sidecar.
Gate 167 adds no committed provider or project fixtures. Golden CLI tests use
existing synthetic project fixtures and temporary synthetic local provider
markers to cover the path-minimized capability explain Markdown sidecar.
Gate 168 adds no committed provider or project fixtures. Golden CLI tests use
existing synthetic project fixtures and temporary synthetic local provider
markers to cover supplemental Doctor bundle requirement explanation entries.
Gate 169 adds no committed provider or project fixtures. Golden CLI tests use
existing synthetic project fixtures and temporary synthetic local provider
markers to cover redacted JSON supplements for Doctor bundle requirement
explanations.
Gate 170 adds no committed provider or project fixtures. Golden CLI tests use
existing synthetic project fixtures and temporary synthetic local provider
markers to cover Doctor bundle requirement explanation index supplements.
Gate 171 adds no committed provider or project fixtures. Golden CLI tests use
existing synthetic project fixtures and temporary synthetic local provider
markers to cover Doctor bundle README entries.
Gate 172 adds no committed provider or project fixtures. Golden CLI tests use
existing synthetic project fixtures and temporary synthetic local provider
markers to cover Doctor bundle diagnostic index entries.
Gate 173 adds no committed provider or project fixtures. Golden CLI tests use
existing synthetic project fixtures and temporary synthetic local provider
markers to cover Doctor bundle action index entries.
Gate 174 adds no committed provider or project fixtures. Golden CLI tests use
existing synthetic project fixtures and temporary synthetic local provider
markers to cover Doctor bundle requirement index entries.
Gate 175 adds no committed provider or project fixtures. Golden CLI tests use
existing synthetic project fixtures and temporary synthetic local provider
markers to cover Doctor bundle provider index entries.
Gate 176 adds no committed provider or project fixtures. Golden CLI tests use
existing synthetic project fixtures and temporary synthetic local provider
markers to cover Doctor bundle capability index entries.
Gate 177 adds no committed provider or project fixtures. Golden CLI tests use
existing synthetic project fixtures and temporary synthetic local provider
markers to cover Doctor bundle area index entries.
Gate 178 adds no committed provider or project fixtures. Golden CLI tests use
existing synthetic project fixtures and temporary synthetic local provider
markers to cover Doctor bundle catalogue-policy index entries.
Gate 179 adds no committed provider or project fixtures. Golden CLI tests use
existing synthetic project fixtures and temporary synthetic local provider
markers to cover Doctor bundle summary index entries.
Gate 180 adds no committed provider or project fixtures. Golden CLI tests use
existing synthetic project fixtures and temporary synthetic local provider
markers to cover Doctor bundle evidence index entries.
Gate 181 adds no committed provider or project fixtures. Golden CLI tests use
existing synthetic project fixtures and temporary synthetic local provider
markers to cover Doctor bundle redaction index entries.
Gate 182 adds no committed provider or project fixtures. Golden CLI tests use
existing synthetic project fixtures and temporary synthetic local provider
markers to cover Doctor bundle open-question index entries.
Gate 183 adds no committed provider or project fixtures. Golden CLI tests use
existing synthetic project fixtures and temporary synthetic local provider
markers to cover Doctor bundle scan-input index entries.
Gate 184 adds no committed provider or project fixtures. Golden CLI tests use
existing synthetic project fixtures and temporary synthetic local provider
markers to cover Doctor bundle navigation index entries.
Gate 185 adds no committed provider or project fixtures. Golden CLI tests use
existing synthetic project fixtures and temporary synthetic local provider
markers to cover Doctor bundle triage index entries.
Gate 186 adds no committed provider or project fixtures. Golden CLI tests use
existing synthetic project fixtures and temporary synthetic local provider
markers to cover primary Doctor export triage JSON, plain text, and Markdown
summary output.
Gate 187 adds no committed provider or project fixtures. Golden CLI tests use
existing synthetic project fixtures and temporary synthetic local provider
markers to cover Doctor triage command hints in primary JSON, plain text,
Markdown summaries, and bundle triage entries.
Gate 188 adds no committed provider or project fixtures. Golden CLI tests use
existing synthetic project fixtures and temporary synthetic local provider
markers to cover Doctor triage worklist entries in primary JSON, plain text,
Markdown summaries, and bundle triage entries.
Gate 189 adds no committed provider or project fixtures. Golden CLI tests use
existing synthetic project fixtures and temporary synthetic local provider
markers to cover Doctor worklist summary metadata in primary JSON, plain text,
Markdown summaries, and bundle triage entries.
Gate 190 adds no committed provider or project fixtures. Golden CLI tests use
existing synthetic project fixtures and temporary synthetic local provider
markers to cover Doctor remediation status headers in primary JSON, plain
text, Markdown summaries, and bundle triage entries.
Gate 191 adds no committed provider or project fixtures. Golden CLI tests use
existing synthetic project fixtures and temporary synthetic local provider
markers to cover human operator handoff checklists in plain text, Markdown
summaries, and bundle triage Markdown.
Gate 192 adds no committed provider or project fixtures. Golden CLI tests use
existing synthetic project fixtures and temporary synthetic local provider
markers to cover `handoff-summary.md` in Doctor bundle archives, including
manifest/checksum coverage and omission of local paths.
Gate 193 adds no committed provider or project fixtures. Golden CLI tests use
existing synthetic project fixtures and temporary synthetic local provider
markers to cover scan-side operator handoff text in plain output and Markdown
summary sidecars.
Gate 194 adds no committed provider or project fixtures. Golden CLI tests use
existing synthetic project fixtures and temporary synthetic local provider
markers to cover explain-side operator handoff text in plain output and
Markdown summary sidecars.
Gate 195 adds no committed provider or project fixtures. Golden CLI tests use
existing synthetic project fixtures and temporary synthetic local provider
markers to cover Doctor bundle requirement explanation Markdown handoff
sections, generated archive notes, JSON non-contract behavior, and path
redaction.
Gate 196 adds no committed provider or project fixtures. Golden CLI tests use
the built-in synthetic catalogue and existing temporary synthetic local
provider markers to cover provider-version declaration metadata in catalogue
list and capability explanation output.
Gate 197 adds no committed provider or project fixtures. Golden CLI tests use
the built-in synthetic catalogue, existing synthetic project fixtures, and
temporary synthetic local provider markers to cover provider-version
declaration metadata in scan output and Doctor provider indexes.
Gate 198 adds no committed provider or project fixtures. Future parser tests
should use synthetic raw version values; real provider DLLs, Bethesda assets,
and third-party mod files remain out of public fixtures unless explicit
permission exists.
Gate 199 adds synthetic parser tests only. It commits no provider DLLs,
Bethesda assets, third-party mod files, local install snapshots, MO2 profiles,
GECK outputs, or external tool fixtures.
Gate 200 adds documentation only. It commits no provider DLLs, Bethesda
assets, third-party mod files, local install snapshots, MO2 profiles, GECK
outputs, external tool fixtures, or generated evidence fixtures.
Gate 201 adds synthetic parsed-evidence unit tests only. It commits no
provider DLLs, Bethesda assets, third-party mod files, local install
snapshots, MO2 profiles, GECK outputs, external tool fixtures, or generated
evidence fixtures.
Gate 202 adds documentation only. It commits no JIP LN script fixtures,
provider DLLs, Bethesda assets, third-party mod files, local install
snapshots, MO2 profiles, GECK outputs, external tool fixtures, generated
script outputs, or generated evidence fixtures.

Gate 203 commits only synthetic JSON registry fixtures for JIP LN text-script
source-contract validation. It commits no generated JIP script files, provider
DLLs, Bethesda assets, third-party mod files, local install snapshots, MO2
profiles, GECK outputs, external tool fixtures, generated script outputs, or
generated evidence fixtures.

Gate 204 commits only synthetic JSON registry fixtures for JIP source semantic
validation. It commits no generated JIP script files, provider DLLs, Bethesda
assets, third-party mod files, local install snapshots, MO2 profiles, GECK
outputs, external tool fixtures, generated script outputs, or generated
evidence fixtures.

Gate 205 commits only synthetic JSON registry fixtures for opaque JIP
body/source-line contract validation. It commits no generated JIP script
files, provider DLLs, Bethesda assets, third-party mod files, local install
snapshots, MO2 profiles, GECK outputs, external tool fixtures, generated
script outputs, or generated evidence fixtures.

Gate 206 commits only synthetic JSON registry fixtures for JIP source-line
byte-budget validation. It commits no generated JIP script files, provider
DLLs, Bethesda assets, third-party mod files, local install snapshots, MO2
profiles, GECK outputs, external tool fixtures, generated script outputs, or
generated evidence fixtures.

Gate 207 commits only synthetic JSON registry fixtures for duplicate JIP
output filename validation. It commits no generated JIP script files, provider
DLLs, Bethesda assets, third-party mod files, local install snapshots, MO2
profiles, GECK outputs, external tool fixtures, generated script outputs, or
generated evidence fixtures.

Gate 208 commits no new fixture files. It reuses existing synthetic JSON
fixtures for non-emitting planner unit tests and commits no generated JIP
script files, provider DLLs, Bethesda assets, third-party mod files, local
install snapshots, MO2 profiles, GECK outputs, external tool fixtures,
generated script outputs, or generated evidence fixtures.

Gate 209 commits no new fixture files. It reuses existing synthetic JSON
fixtures for in-memory renderer unit tests and commits no generated JIP script
files, provider DLLs, Bethesda assets, third-party mod files, local install
snapshots, MO2 profiles, GECK outputs, external tool fixtures, generated
script outputs, or generated evidence fixtures.

Gate 210 commits no new fixture files. It uses temp-copied synthetic JSON
fixtures for generated-file emission unit tests and commits no generated JIP
script outputs, provider DLLs, Bethesda assets, third-party mod files, local
install snapshots, MO2 profiles, GECK outputs, external tool fixtures, or
generated evidence fixtures.

Gate 211 commits no new fixture files. It uses temp-copied synthetic JSON
fixtures for manifest, checksum, and digest emission unit tests and commits no
generated JIP script outputs, provider DLLs, Bethesda assets, third-party mod
files, local install snapshots, MO2 profiles, GECK outputs, external tool
fixtures, or generated evidence fixtures.

Gate 212 commits no new fixture files. It uses temp-copied synthetic JSON
fixtures for generated manifest schema validation tests and commits no
generated JIP script outputs, provider DLLs, Bethesda assets, third-party mod
files, local install snapshots, MO2 profiles, GECK outputs, external tool
fixtures, or generated evidence fixtures.

Gate 213 commits no new fixture files. It uses temp-copied synthetic JSON
fixtures for generated checksum sidecar revalidation tests and commits no
generated JIP script outputs, provider DLLs, Bethesda assets, third-party mod
files, local install snapshots, MO2 profiles, GECK outputs, external tool
fixtures, or generated evidence fixtures.

Gate 214 commits no new fixture files. It uses temp-copied synthetic JSON
fixtures for JIP generate CLI coverage and commits no generated JIP script
outputs, provider DLLs, Bethesda assets, third-party mod files, local install
snapshots, MO2 profiles, GECK outputs, external tool fixtures, or generated
evidence fixtures.

Gate 215 commits no new fixture files. It uses temp-copied synthetic JSON
fixtures for JIP build unit and CLI coverage and commits no built JIP script
outputs, provider DLLs, Bethesda assets, third-party mod files, local install
snapshots, MO2 profiles, GECK outputs, external tool fixtures, or generated
evidence fixtures.

Gate 226 commits no new fixture files. It uses temp-copied synthetic xEdit
audit fixtures and temp-generated handoff sidecar evidence for revalidation
tests, and commits no xEdit outputs, plugin files, provider DLLs, Bethesda
assets, third-party mod files, local install snapshots, MO2 profiles, GECK
outputs, external tool fixtures, or generated evidence fixtures.

Gate 227 commits no new fixture files. It uses temp-copied synthetic xEdit
audit fixtures and temp-generated handoff CLI evidence for golden tests, and
commits no xEdit outputs, plugin files, provider DLLs, Bethesda assets,
third-party mod files, local install snapshots, MO2 profiles, GECK outputs,
external tool fixtures, or generated evidence fixtures.

Gate 228 commits no new fixture files. It is a planning/routing closeout gate
and commits no generated docs output, xEdit outputs, plugin files, provider
DLLs, Bethesda assets, third-party mod files, local install snapshots, MO2
profiles, GECK outputs, external tool fixtures, or generated evidence
fixtures.

Gate 229 commits no generated docs output. It uses temp-copied synthetic
project fixtures for `forge docs` golden tests and commits no generated
reference indexes, static site output, xEdit outputs, plugin files, provider
DLLs, Bethesda assets, third-party mod files, local install snapshots, MO2
profiles, GECK outputs, external tool fixtures, or generated evidence
fixtures.

Gate 230 commits no generated docs output. It uses temp-copied synthetic
project fixtures for `forge docs` golden tests and commits no generated schema
reference pages, generated reference indexes, static site output, xEdit
outputs, plugin files, provider DLLs, Bethesda assets, third-party mod files,
local install snapshots, MO2 profiles, GECK outputs, external tool fixtures,
or generated evidence fixtures.

Gate 231 commits no generated docs output. It uses temp-copied synthetic
project fixtures for `forge docs` golden tests and commits no generated
registry reference pages, generated schema reference pages, generated
reference indexes, static site output, xEdit outputs, plugin files, provider
DLLs, Bethesda assets, third-party mod files, local install snapshots, MO2
profiles, GECK outputs, external tool fixtures, or generated evidence
fixtures.

Gate 232 commits no generated docs output. It uses temp-copied synthetic
project fixtures for `forge docs` golden tests and commits no generated rule
reference pages, generated registry reference pages, generated schema
reference pages, generated reference indexes, static site output, xEdit
outputs, plugin files, provider DLLs, Bethesda assets, third-party mod files,
local install snapshots, MO2 profiles, GECK outputs, external tool fixtures,
or generated evidence fixtures.

Gate 233 commits no generated docs output. It uses temp-copied synthetic
project fixtures for `forge docs` golden tests and commits no generated
capability reference pages, generated rule reference pages, generated registry
reference pages, generated schema reference pages, generated reference
indexes, static site output, xEdit outputs, plugin files, provider DLLs,
Bethesda assets, third-party mod files, local install snapshots, MO2 profiles,
GECK outputs, external tool fixtures, or generated evidence fixtures.

Gate 234 commits no generated docs output. It uses temp-copied synthetic
project fixtures for `forge docs` golden tests and commits no generated
provider reference pages, generated capability reference pages, generated rule
reference pages, generated registry reference pages, generated schema
reference pages, generated reference indexes, static site output, xEdit
outputs, plugin files, provider DLLs, Bethesda assets, third-party mod files,
local install snapshots, MO2 profiles, GECK outputs, external tool fixtures,
or generated evidence fixtures.

Gate 235 commits no generated docs output. It uses temp-copied synthetic
project fixtures for `forge docs` golden tests and commits no generated
command reference pages, generated provider reference pages, generated
capability reference pages, generated rule reference pages, generated registry
reference pages, generated schema reference pages, generated reference
indexes, static site output, xEdit outputs, plugin files, provider DLLs,
Bethesda assets, third-party mod files, local install snapshots, MO2 profiles,
GECK outputs, external tool fixtures, or generated evidence fixtures.

Gate 236 commits no generated graph output. It uses temp-copied synthetic
project fixtures for `forge graph` golden tests and commits no generated graph
JSON, generated graph Markdown, graph manifests, checksum sidecars, static
site output, xEdit outputs, plugin files, provider DLLs, Bethesda assets,
third-party mod files, local install snapshots, MO2 profiles, GECK outputs,
external tool fixtures, or generated evidence fixtures.

Gate 237 commits no generated graph output. It uses temp-copied synthetic
project fixtures for `forge graph` golden tests and commits no generated graph
JSON, generated graph Markdown, graph manifests, checksum sidecars, provider
status fixtures, scanner output fixtures, static site output, xEdit outputs,
plugin files, provider DLLs, Bethesda assets, third-party mod files, local
install snapshots, MO2 profiles, GECK outputs, external tool fixtures, or
generated evidence fixtures.

Gate 238 commits no generated graph output. It uses temp-copied synthetic
project fixtures for `forge graph` golden tests and commits no generated graph
JSON, generated graph Markdown, graph manifests, checksum sidecars, generated
target outputs, provider status fixtures, scanner output fixtures, static site
output, xEdit outputs, plugin files, provider DLLs, Bethesda assets,
third-party mod files, local install snapshots, MO2 profiles, GECK outputs,
external tool fixtures, or generated evidence fixtures.

Gate 239 commits no generated graph output. It uses temp-copied synthetic
project fixtures for `forge graph` golden tests and commits no generated graph
JSON, generated graph Markdown, graph manifests, checksum sidecars, generated
artifact output fixtures, provider status fixtures, scanner output fixtures,
static site output, xEdit outputs, plugin files, provider DLLs, Bethesda
assets, third-party mod files, local install snapshots, MO2 profiles, GECK
outputs, external tool fixtures, or generated evidence fixtures.

Gate 240 commits no generated graph output. It uses temp-copied synthetic
project fixtures for `forge graph` golden tests and commits no generated graph
JSON, generated graph Markdown, graph manifests, checksum sidecars, generated
manifest fixtures, generated artifact output fixtures, provider status
fixtures, scanner output fixtures, static site output, xEdit outputs, plugin
files, provider DLLs, Bethesda assets, third-party mod files, local install
snapshots, MO2 profiles, GECK outputs, external tool fixtures, or generated
evidence fixtures.

Gate 241 adds no public fixture, private fixture, generated graph golden
payload, or real provider sample. It is validated by documentation/routing
consistency plus normal local build/test smoke checks.

Gate 242 adds no public fixture, private fixture, generated evidence payload,
or real provider sample. It uses existing golden CLI tests for `forge help
explain` and reserved JSON metadata only.

Gate 243 adds no public fixture, private fixture, generated evidence payload,
or real provider sample. It uses golden CLI tests only for
`forge explain diagnostic <rule-id>` plain, JSON, and usage output.

Gate 244 adds no public fixture, private fixture, generated evidence payload,
or real provider sample. It extends golden CLI tests only for documented rule
metadata and reserved-family fallback output from
`forge explain diagnostic <rule-id>`.

Gate 245 adds no public fixture, private fixture, generated evidence payload,
or real provider sample. It extends golden CLI tests only for documented target
metadata, unknown-target usage output, and reserved-subject behavior from
`forge explain target <target-id>`.

Gate 246 adds no public fixture, private fixture, generated evidence payload,
or real provider sample. It extends golden CLI tests only for documented output
path classification, unknown-output usage output, and reserved-subject
behavior from `forge explain output <generated-or-dist-path>`.

Gate 247 adds no public fixture, private fixture, generated evidence payload,
or real provider sample. It extends golden CLI tests only for built-in
capability catalogue metadata, unknown-capability usage output, and
reserved-subject behavior from `forge explain capability <capability-id>`.

Gate 248 adds no public fixture, private fixture, generated evidence payload,
or real provider sample. It extends golden CLI tests only for deterministic
provenance boundary planning, unknown-provenance usage output, and
unknown-subject reserved JSON from `forge explain provenance
<manifest-or-output-path>`.

Gate 249 adds no public fixture, private fixture, generated evidence payload,
or real provider sample. It is a documentation and routing closeout for the
top-level `forge explain` lane and a transition to `forge clean` planning.

Gate 250 adds no public fixture, private fixture, generated evidence payload,
or real provider sample. It uses a temporary synthetic generated file in golden
CLI tests only to prove the reserved `forge clean` planning skeleton does not
delete files.

Gate 251 adds no public fixture, private fixture, generated evidence payload,
or real provider sample. It uses temporary synthetic generated, dist, and cache
files in golden CLI tests only to prove dry-run clean path planning does not
delete files.

Gate 252 adds no public fixture, private fixture, generated evidence payload,
or real provider sample. It uses temporary synthetic generated, dist, and cache
files in golden CLI tests only to prove unconfirmed and confirmed all-scope
clean planning does not delete files.

Gate 253 adds no public fixture, private fixture, generated evidence payload,
or real provider sample. It uses temporary synthetic generated-root directories
and files in golden CLI tests only to prove explicit generated clean removes
the synthetic target root, reports missing roots, and preserves dry-run and
default-scope non-mutation behavior.

Gate 254 adds no public fixture, private fixture, generated evidence payload,
or real provider sample. It uses temporary synthetic dist-root directories and
files in golden CLI tests only to prove explicit dist clean removes the
synthetic target root, reports missing roots, and preserves dry-run and
all-scope non-mutation behavior.

Gate 255 adds no public fixture, private fixture, generated evidence payload,
or real provider sample. It uses temporary synthetic cache-root directories and
files in golden CLI tests only to prove explicit cache clean removes the
synthetic target root, reports missing roots, and preserves dry-run and
all-scope non-mutation behavior.

Gate 256 adds no public fixture, private fixture, generated evidence payload,
or real provider sample. It uses temporary synthetic generated, dist, and
cache directories and files in golden CLI tests only to prove confirmed
all-scope clean removes documented synthetic target roots, reports missing
roots, and preserves dry-run and refusal behavior.

Gate 257 adds no public fixture, private fixture, generated evidence payload,
or real provider sample. It uses temporary synthetic JSON/YAML root manifests
and temporary generated, dist, and cache directories in golden CLI tests only
to prove project-ID confirmation matching, mismatch refusal, missing-manifest
refusal, and preservation of dry-run behavior.

Gate 258 adds no public fixture, private fixture, generated evidence payload,
or real provider sample. It uses temporary synthetic generated, dist, and
cache directories plus `.wastelandforge/cache/build.lock` files in golden CLI
tests only to prove cache-affecting clean refusal and filesystem preservation.

Gate 259 adds no public fixture, private fixture, generated evidence payload,
or real provider sample. It is a planning/routing closeout gate and relies on
existing clean golden coverage as regression evidence.

Gate 260 adds no public fixture, private fixture, generated evidence payload,
or real provider sample. It uses temporary synthetic project directories in
golden CLI tests only to prove release-prepare planning output, output-root
refusal, and no filesystem writes.

Gate 261 adds no public fixture, private fixture, committed generated evidence
payload, or real provider sample. It uses temporary synthetic project
directories in golden CLI tests only to prove release-plan emission,
`--dry-run` no-write behavior, and unsafe output-root refusal.

Gate 262 adds no public fixture, private fixture, committed generated evidence
payload, or real provider sample. It uses temporary synthetic project
directories in golden CLI tests only to prove release-summary emission beside
the release plan, `--dry-run` no-write behavior, and unsafe output-root
refusal.

Gate 263 adds no public fixture, private fixture, committed generated evidence
payload, or real provider sample. It uses temporary synthetic project
directories in golden CLI tests only to prove release-prepare build-manifest
emission beside the release plan and summary, `--dry-run` no-write behavior,
and unsafe output-root refusal.

Gate 264 adds no public fixture, private fixture, committed generated evidence
payload, or real provider sample. It uses temporary synthetic project
directories in golden CLI tests only to prove release-prepare checksum sidecar
emission beside the release plan, summary, and build manifest, `--dry-run`
no-write behavior, and unsafe output-root refusal.

Gate 265 adds no public fixture, private fixture, committed generated evidence
payload, or real provider sample. It uses temporary synthetic project
directories in golden CLI tests only to prove release-prepare staging payload
skeleton emission beside the release plan, summary, build manifest, and
checksum sidecar, `--dry-run` no-write behavior, and unsafe output-root
refusal.

Gate 266 adds no public fixture, private fixture, committed generated evidence
payload, or real provider sample. It uses temporary synthetic project
directories in golden CLI tests only to prove release-prepare archive planning
metadata beside the staging payload, release plan, summary, build manifest,
and checksum sidecar, planned archive no-write behavior, `--dry-run` no-write
behavior, and unsafe output-root refusal.

Gate 267 adds no public fixture, private fixture, committed generated evidence
payload, or real provider sample. It uses temporary synthetic project
directories in golden CLI tests only to prove deterministic release archive
creation beside the archive plan, staging payload, release plan, summary,
build manifest, and checksum sidecar, repeated-run archive digest stability,
`--dry-run` no-write behavior, and unsafe output-root refusal.

Gate 268 adds no public fixture, private fixture, committed generated evidence
payload, or real provider sample. It uses temporary synthetic project
directories in golden CLI tests only to prove release archive evidence
revalidation beside the archive, archive plan, staging payload, release plan,
summary, build manifest, and checksum sidecar, `--dry-run` no-write behavior,
and unsafe output-root refusal.

Gate 269 adds no public fixture, private fixture, committed generated evidence
payload, or real provider sample. It is a planning and routing closeout gate
for the release-prepare lane and relies on existing synthetic release-prepare
coverage from Gates 260 through 268.

Gate 270 adds no public fixture, private fixture, committed generated evidence
payload, or real provider sample. Release-publish governance preflight tests
use temporary synthetic project roots and assert that no `dist/` output is
created.

Gate 271 adds no public fixture, private fixture, committed generated evidence
payload, or real provider sample. Release-publish local evidence discovery
tests use temporary synthetic project roots and temporary release-prepare
outputs generated by the CLI during golden tests.

Gate 272 adds no public fixture, private fixture, committed generated evidence
payload, or real provider sample. Release-publish content-shape tests use
temporary synthetic project roots, temporary release-prepare outputs generated
by the CLI, and a temp-only malformed JSON rewrite.

Gate 273 adds no public fixture, private fixture, committed generated evidence
payload, or real provider sample. Release-publish checksum sidecar entry
classification tests use temporary synthetic project roots, temporary
release-prepare outputs generated by the CLI, and temp-only checksum sidecar
line rewrites.

Gate 274 adds no public fixture, private fixture, committed generated evidence
payload, or real provider sample. Release-publish build-manifest output
cross-reference tests use temporary synthetic project roots, temporary
release-prepare outputs generated by the CLI, and temp-only build-manifest
output path rewrites.

Gate 275 adds no public fixture, private fixture, committed generated evidence
payload, or real provider sample. Release-publish release-archive-evidence
metadata cross-reference tests use temporary synthetic project roots,
temporary release-prepare outputs generated by the CLI, and temp-only
release-archive-evidence metadata path rewrites.

Gate 276 adds no public fixture, private fixture, committed generated evidence
payload, or real provider sample. Release-publish checksum sidecar digest
revalidation tests use temporary synthetic project roots, temporary
release-prepare outputs generated by the CLI, and temp-only checksum sidecar
digest rewrites.

Gate 277 adds no public fixture, private fixture, committed generated evidence
payload, or real provider sample. Release-publish build-manifest output digest
revalidation tests use temporary synthetic project roots, temporary
release-prepare outputs generated by the CLI, and temp-only build-manifest
digest rewrites.

Gate 278 adds no public fixture, private fixture, committed generated evidence
payload, or real provider sample. Release-publish release-archive-evidence
archive digest metadata revalidation tests use temporary synthetic project
roots, temporary release-prepare outputs generated by the CLI, and temp-only
archive metadata rewrites.

Gate 279 adds no public fixture, private fixture, committed generated evidence
payload, or real provider sample. Release-publish archive
reopening/revalidation tests use temporary synthetic project roots, temporary
release-prepare outputs generated by the CLI, and temp-only archive entry
mutation plus evidence digest refreshes.

Gate 280 adds no public fixture, private fixture, committed generated evidence
payload, or real provider sample. Release-publish semantic release-evidence
validation tests use temporary synthetic project roots, temporary
release-prepare outputs generated by the CLI, and temp-only release-summary
counter mutation plus checksum and build-manifest digest refreshes.

Gate 281 adds no public fixture, private fixture, committed generated evidence
payload, or real provider sample. Release-publish governance-check evaluation
tests use temporary synthetic project roots plus temp-only governance policy,
workflow, CODEOWNERS, fixture-policy, and AI-optional policy files.

Gate 282 adds no public fixture, private fixture, committed generated evidence
payload, or real provider sample. Release-publish schema-validation evidence
tests use temporary synthetic project roots plus temp-only
`dist/release-dry-run/validation.json` diagnostic report files.

Gate 283 adds no public fixture, private fixture, committed generated evidence
payload, or real provider sample. Release-publish capability/environment
evidence tests use temp-copied synthetic project fixtures, temp-only synthetic
provider marker layouts, and temp-only
`dist/release-dry-run/capabilities-scan.json` reports generated by the
existing `forge capabilities scan --project --format json` command.

Gate 284 adds no public fixture, private fixture, committed generated evidence
payload, or real provider sample. Release-publish package-validation evidence
tests use temp-copied synthetic project fixtures, temp-only package outputs
generated by the existing `forge package --target mcm-json` command, and
temp-only `dist/release-dry-run/package-verify.json` reports generated by the
existing `forge package --target mcm-json --verify-existing --format json`
command.

Gate 285 adds no public fixture, private fixture, committed generated evidence
payload, or real provider sample. Release-publish release-verification
evidence tests use temp-copied synthetic project fixtures and temp-only
`dist/release-dry-run/release-verify.json` reports generated by the existing
`forge release verify --format json` command.

Gate 286 adds no public fixture, private fixture, committed generated evidence
payload, or real provider sample. Release-publish human-approval tests use
temp-copied synthetic project fixtures and the synthetic root project manifest
ID already present in `fixtures/projects/ExampleMod/wastelandforge.json`.

Gate 287 adds no public fixture, private fixture, committed generated evidence
payload, or real provider sample. Release-publish readiness aggregation tests
use temp-copied synthetic project fixtures, temp-only local evidence reports,
and temp-only governance files.

Gate 288 adds no public fixture, private fixture, committed generated evidence
payload, or real provider sample. Release-publish no-publish lane closeout
tests reuse temp-copied synthetic project fixtures and temp-only local release
evidence from the existing release-publish readiness coverage.

Gate 289 adds no public fixture, private fixture, committed generated evidence
payload, or real provider sample. Doctor export release-readiness handoff
tests reuse the synthetic `fixtures/projects/ExampleMod` project and
temporary Doctor export output/archive paths only.

Gate 290 adds no public fixture, private fixture, committed generated evidence
payload, or real provider sample. Doctor export release-readiness
triage/worklist tests reuse the synthetic `fixtures/projects/ExampleMod`
project, temp-only synthetic capability marker layouts, and temporary Doctor
export output/archive paths only.

Gate 291 adds no public fixture, private fixture, committed generated evidence
payload, or real provider sample. Doctor export release-readiness lane
closeout is planning, routing, and documentation only.

Gate 292 adds no public fixture, private fixture, committed generated evidence
payload, or real provider sample. Build-plan and build-report-index tests
reuse the synthetic `fixtures/projects/ExampleMod` project and temporary
generated/dist output paths only.

Gate 293 adds no public fixture, private fixture, committed generated evidence
payload, or real provider sample. Build-plan and build-report-index Markdown
tests reuse the synthetic `fixtures/projects/ExampleMod` project and
temporary generated/dist output paths only.

Gate 294 adds no public fixture, private fixture, committed generated evidence
payload, or real provider sample. Reports build-evidence closeout is
planning, routing, and documentation only.

Gate 295 adds no public fixture, private fixture, committed generated evidence
payload, or real provider sample. Reports package tests reuse the synthetic
`fixtures/projects/ExampleMod` project through temporary copies and commit no
`dist/reports-package` outputs, staged package payloads, archives, provider
DLLs, Bethesda assets, third-party mod files, local install snapshots, MO2
profiles, GECK outputs, external tool fixtures, or generated evidence
fixtures.

Gate 296 adds no public fixture, private fixture, committed generated evidence
payload, or real provider sample. Reports package input-discovery tests reuse
the synthetic `fixtures/projects/ExampleMod` project through temporary copies
and create any `dist/build` and `dist/reports-package` evidence only in temp
test directories.

Gate 297 adds no public fixture, private fixture, committed generated evidence
payload, or real provider sample. Reports package staging-copy tests reuse the
synthetic `fixtures/projects/ExampleMod` project through temporary copies and
create/copy any `dist/build` and `dist/reports-package` evidence only in temp
test directories.

Gate 298 adds no public fixture, private fixture, committed generated evidence
payload, or real provider sample. Reports package archive tests reuse the
synthetic `fixtures/projects/ExampleMod` project through temporary copies and
create any `dist/build`, `dist/reports-package`, and `package.zip` archive
evidence only in temp test directories.

Gate 299 adds no public fixture, private fixture, committed generated evidence
payload, or real provider sample. Reports package archive-evidence tests
reuse the synthetic `fixtures/projects/ExampleMod` project through temporary
copies and create any `dist/build`, `dist/reports-package`, `package.zip`, and
`package-archive-evidence.json` evidence only in temp test directories.

Gate 300 adds no public fixture, private fixture, committed generated evidence
payload, or real provider sample. It is a planning/routing closeout and uses
no Bethesda assets, third-party mod files, local install snapshots, MO2
profiles, GECK outputs, external tool fixtures, or generated evidence
fixtures.

Gate 301 adds no public fixture, private fixture, committed generated evidence
payload, or real provider sample. Release verify self-report tests reuse the
synthetic `fixtures/projects/ExampleMod` project through temporary copies and
create `dist/release-dry-run/release-verify.json` only in temp test
directories.

Gate 302 adds no public fixture, private fixture, committed generated evidence
payload, or real provider sample. Release verify evidence-index tests reuse
the synthetic `fixtures/projects/ExampleMod` project through temporary copies
and create `dist/release-dry-run/release-evidence-index.json` only in temp
test directories.

Gate 303 adds no public fixture, private fixture, committed generated evidence
payload, or real provider sample. Release verify evidence-handoff tests reuse
the synthetic `fixtures/projects/ExampleMod` project through temporary copies
and create `dist/release-dry-run/release-evidence-handoff.md` only in temp
test directories.

Gate 304 adds no public fixture, private fixture, committed generated evidence
payload, or real provider sample. Release verify evidence-status tests reuse
the synthetic `fixtures/projects/ExampleMod` project through temporary copies
and create `dist/release-dry-run/release-evidence-status.json` only in temp
test directories.

Gate 305 adds no public fixture, private fixture, committed generated evidence
payload, or real provider sample. Release verify missing-evidence action
tests reuse the synthetic `fixtures/projects/ExampleMod` project through
temporary copies and create `dist/release-dry-run/release-evidence-actions.json`
only in temp test directories.

Gate 306 adds no public fixture, private fixture, committed generated evidence
payload, or real provider sample. Release verify evidence collection-plan
tests reuse the synthetic `fixtures/projects/ExampleMod` project through
temporary copies and create
`dist/release-dry-run/release-evidence-collection-plan.json` only in temp test
directories.

Gate 307 adds no public fixture, private fixture, committed generated
evidence payload, or real provider sample. Release publish collection-plan
evidence tests reuse the synthetic `fixtures/projects/ExampleMod` project
through temporary copies and read
`dist/release-dry-run/release-evidence-collection-plan.json` only from temp
test directories after local release-verify dry-run generation.

Gate 308 adds no public fixture, private fixture, committed generated
evidence payload, or real provider sample. Release publish cross-link tests
reuse the synthetic `fixtures/projects/ExampleMod` project through temporary
copies and read the local release dry-run evidence files only from temp test
directories after local release-verify dry-run generation.

Gate 309 adds no public fixture, private fixture, committed generated
evidence payload, or real provider sample. Release publish remediation tests
reuse the synthetic `fixtures/projects/ExampleMod` project through temporary
copies, mutate generated temp-only release dry-run evidence to exercise
malformed and mismatched states, and commit no generated evidence.

Gate 216 commits no new fixture files. It uses temp-copied synthetic JSON
fixtures for JIP package unit and CLI coverage and commits no staged package
outputs, provider DLLs, Bethesda assets, third-party mod files, local install
snapshots, MO2 profiles, GECK outputs, external tool fixtures, or generated
evidence fixtures.

Gate 312 commits no new public fixture, private fixture, generated evidence
payload, provider sample, Bethesda asset, third-party mod file, local install
snapshot, MO2 profile, GECK output, external tool fixture, or AI-generated
fixture. `forge init` coverage creates only temporary synthetic directories
under the test temp root and verifies that planning output does not write the
planned scaffold files.

Gate 316 commits no new public fixture, private fixture, generated evidence
payload, provider sample, Bethesda asset, third-party mod file, local install
snapshot, MO2 profile, GECK output, external tool fixture, or AI-generated
fixture. `forge init` coverage creates temporary synthetic project
directories, writes only minimal source scaffold contracts in those temp
directories, and validates them locally before test cleanup by the OS temp
area.

Gate 317 commits no new public fixture, private fixture, generated evidence
payload, provider sample, Bethesda asset, third-party mod file, local install
snapshot, MO2 profile, GECK output, external tool fixture, or AI-generated
fixture. `forge init` coverage creates temporary synthetic project
directories, writes config and README scaffold files only in those temp
directories, and validates the resulting source contracts locally.

Gate 318 commits no new public fixture, private fixture, generated evidence
payload, provider sample, Bethesda asset, third-party mod file, local install
snapshot, MO2 profile, GECK output, external tool fixture, or AI-generated
fixture. `forge init` coverage creates temporary synthetic project
directories, writes VS Code task scaffold files only in those temp directories,
parses the task JSON, and validates the resulting source contracts locally.

Gate 319 commits no new public fixture, private fixture, generated evidence
payload, provider sample, Bethesda asset, third-party mod file, local install
snapshot, MO2 profile, GECK output, external tool fixture, or AI-generated
fixture. `forge init` coverage creates temporary synthetic project
directories, writes GitHub Actions workflow scaffold files only in those temp
directories, inspects the workflow text for expected CI/governance markers,
and validates the resulting source contracts locally.

Gate 320 commits no new public fixture, private fixture, generated evidence
payload, provider sample, Bethesda asset, third-party mod file, local install
snapshot, MO2 profile, GECK output, external tool fixture, or AI-generated
fixture. `forge init` coverage creates temporary synthetic project
directories, writes editor schema association scaffold files only in those
temp directories, parses the settings JSON, inspects schema association
mappings, and validates the resulting source contracts locally.

Gate 321 commits no new public fixture, private fixture, generated evidence
payload, provider sample, Bethesda asset, third-party mod file, local install
snapshot, MO2 profile, GECK output, external tool fixture, or AI-generated
fixture. It is a docs and prompt routing closeout only.

Gate 322 commits no new public fixture, private fixture, generated evidence
payload, provider sample, Bethesda asset, third-party mod file, local install
snapshot, MO2 profile, GECK output, external tool fixture, or AI-generated
fixture. It is a docs and prompt routing planning gate only.

Gate 323 commits no new public fixture, private fixture, generated evidence
payload, provider sample, Bethesda asset, third-party mod file, local install
snapshot, MO2 profile, GECK output, external tool fixture, or AI-generated
fixture. It adds source-built runner scripts and documentation only.

Gate 324 commits no new public fixture, private fixture, generated evidence
payload, provider sample, Bethesda asset, third-party mod file, local install
snapshot, MO2 profile, GECK output, external tool fixture, local tool package
fixture, or AI-generated fixture. It is a docs and prompt routing planning
gate only.

Gate 325 commits no new public fixture, private fixture, generated evidence
payload, provider sample, Bethesda asset, third-party mod file, local install
snapshot, MO2 profile, GECK output, external tool fixture, checked-in local
tool package fixture, or AI-generated fixture. Local `.nupkg` and temporary
tool-manifest smoke files are generated under ignored `artifacts/local-tool/`.

Gate 326 commits no new public fixture, private fixture, generated evidence
payload, provider sample, Bethesda asset, third-party mod file, local install
snapshot, MO2 profile, GECK output, external tool fixture, checked-in local
tool package fixture, checked-in tool manifest fixture, or AI-generated
fixture. It is a docs and prompt routing planning gate only.

Gate 327 commits no new public fixture, private fixture, generated evidence
payload, provider sample, Bethesda asset, third-party mod file, local install
snapshot, MO2 profile, GECK output, external tool fixture, checked-in local
tool package fixture, or AI-generated fixture. The checked-in tool manifest is
bootstrap source metadata; generated `.nupkg`, NuGet config, and package cache
smoke files stay under ignored `artifacts/local-tool/`.

Gate 328 commits no new public fixture, private fixture, generated evidence
payload, provider sample, Bethesda asset, third-party mod file, local install
snapshot, MO2 profile, GECK output, external tool fixture, generated
workflow/task fixture, checked-in local tool package fixture, or AI-generated
fixture. It is a docs and prompt routing planning gate only.

Gate 329 commits no new public fixture, private fixture, generated evidence
payload, provider sample, Bethesda asset, third-party mod file, local install
snapshot, MO2 profile, GECK output, external tool fixture, generated
workflow/task fixture, checked-in local tool package fixture, or AI-generated
fixture. Restore smoke artifacts stay under ignored `artifacts/local-tool/`.

Gate 330 commits no new public fixture, private fixture, generated evidence
payload, provider sample, Bethesda asset, third-party mod file, local install
snapshot, MO2 profile, GECK output, external tool fixture, generated
workflow/task fixture, checked-in local tool package fixture, or AI-generated
fixture. Workflow-equivalent smoke artifacts stay under ignored `artifacts/`.

Gate 331 commits no new public fixture, private fixture, generated evidence
payload, provider sample, Bethesda asset, third-party mod file, local install
snapshot, MO2 profile, GECK output, external tool fixture, repository task
fixture, generated workflow/task fixture, checked-in local tool package
fixture, or AI-generated fixture. It is a planning gate only.

Gate 332 commits no new public fixture, private fixture, generated evidence
payload, provider sample, Bethesda asset, third-party mod file, local install
snapshot, MO2 profile, GECK output, external tool fixture, generated
workflow/task fixture, checked-in local tool package fixture, or AI-generated
fixture. The repository-owned `.vscode/tasks.json` is developer bootstrap
metadata, and restore smoke artifacts stay under ignored
`artifacts/local-tool/`.

Gate 333 commits no new public fixture, private fixture, generated evidence
payload, provider sample, Bethesda asset, third-party mod file, local install
snapshot, MO2 profile, GECK output, external tool fixture, generated
workflow/task fixture, checked-in local tool package fixture, or AI-generated
fixture. It is a docs and prompt routing closeout only.

Gate 334 commits no new public fixture, private fixture, generated evidence
payload, provider sample, Bethesda asset, third-party mod file, local install
snapshot, MO2 profile, GECK output, external tool fixture, generated
workflow/task fixture, checked-in local tool package fixture, or AI-generated
fixture. It is a docs and prompt routing planning gate only.

Gate 335 commits no new public fixture, private fixture, generated evidence
payload, provider sample, Bethesda asset, third-party mod file, local install
snapshot, MO2 profile, GECK output, external tool fixture, checked-in local tool
package fixture, or AI-generated fixture. It updates generated scaffold
templates and exercises them through a temporary synthetic project in golden
tests.

Gate 336 commits no new public fixture, private fixture, generated evidence
payload, provider sample, Bethesda asset, third-party mod file, local install
snapshot, MO2 profile, GECK output, external tool fixture, executable package
fixture, installer fixture, checked-in local tool package fixture, or
AI-generated fixture. It is a docs and prompt routing closeout only.

Gate 337 commits no new public fixture, private fixture, generated evidence
payload, provider sample, Bethesda asset, third-party mod file, local install
snapshot, MO2 profile, GECK output, external tool fixture, executable package
fixture, installer fixture, app-shell package fixture, checked-in local tool
package fixture, or AI-generated fixture. It is a docs and prompt routing
planning gate only.

Gate 338 commits no new public fixture, private fixture, generated evidence
payload, provider sample, Bethesda asset, third-party mod file, local install
snapshot, MO2 profile, GECK output, external tool fixture, checked-in local
tool package fixture, installer fixture, app-shell package fixture, or
AI-generated fixture. Its standalone publish outputs are ignored local
distribution artifacts under `dist/local/forge` or
`artifacts/standalone-forge/<name>`.
