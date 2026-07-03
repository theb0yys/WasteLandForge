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
