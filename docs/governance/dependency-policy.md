# Dependency Policy

Status: Skeleton
Research classification: Documented
Source: R002A / WFG-001 and R008 / ADR-011

No proprietary or redistribution-unclear dependency may become a hard requirement of WastelandForge core.

Dependency rules:

- Prefer redistributable open-source libraries for core implementation.
- Do not rehost third-party runtime binaries by default.
- Model ecosystem requirements as capabilities and providers.
- Keep validation, build, release verification, and contribution paths offline-first and AI-optional.
- Record dependency blockers as open implementation checks before changing gate scope.

Gate 2 checks .NET target and package compatibility before solution baseline work is accepted.
Gate 13 validates dependency and capability registry contracts before semantic
capability-reference checks or later provider detection can run.
Gate 57 adds a built-in FNV capability/provider catalogue for
`forge capabilities list`; it still does not make any proprietary or
redistribution-unclear ecosystem provider a hard dependency of Forge core.
Gate 58 adds explicit path-based scan evidence for local provider presence.
Detected ecosystem tools and runtime plugins remain optional providers, not
redistributed Forge dependencies.
Gate 59 explains built-in capability and provider status from catalogue and
scan evidence. Explanation output still treats ecosystem tools and runtime
plugins as optional providers and does not redistribute or require their
binaries in Forge core.
Gate 60 resolves declared project capability requirements against local scan
evidence when `forge capabilities scan --project` is used. Detected ecosystem
tools and runtime plugins remain optional providers and are not redistributed
or required by Forge core.
Gate 61 adds deterministic metadata report generation and build manifest
output using only internal Forge projects and .NET APIs. It adds no external
runtime dependency, rehosted binary, or hard provider requirement.
Gate 62 targets MCM Extender as generated JSON output only. It does not bundle
MCM, MCM Extender, xNVSE, JIP LN, JohnnyGuitar, ShowOff, UIO, or any
third-party runtime binary, and it does not make those providers hard
requirements of Forge core.
Gate 63 validates generated MCM Extender JSON against a local schema derived
from upstream documentation evidence, but still does not bundle, modify, or
redistribute MCM Extender core assets or third-party runtime files.
Gate 64 adds generated translation INI files and passes through source-authored
MCM Extender runtime requirement objects, but still does not infer provider
installation, bundle dependencies, or redistribute third-party runtime files.
Gate 65 adds checkbox and string-toggle JSON option generation only; it still
does not bundle MCM Extender, MCM, or any third-party runtime dependency.
Gate 66 adds keybind JSON option generation only; it still does not bundle MCM
Extender, MCM, or any third-party runtime dependency.
Gate 67 adds header JSON option generation only; it still does not bundle MCM
Extender, MCM, or any third-party runtime dependency.
Gate 68 adds image JSON option-map generation only; it still does not bundle
MCM Extender, MCM, referenced image assets, or any third-party runtime
dependency.
Gate 69 validates that referenced MCM image paths resolve to project-declared
texture assets; it still does not bundle MCM Extender, MCM, or any
third-party runtime dependency.
Gate 70 stages only project-declared referenced texture assets into generated
or dist output trees; it still does not bundle MCM Extender, MCM, or any
third-party runtime dependency.
Gate 71 writes package metadata for those generated loose files; it still does
not bundle MCM Extender, MCM, or any third-party runtime dependency.
Gate 72 packages only Forge-generated project output files into a ZIP archive;
it still does not bundle MCM Extender, MCM, or any third-party runtime
dependency.
Gate 73 exposes that package path through `forge package`; it still packages
only Forge-generated project output files and adds no external runtime
dependency.
Gate 74 validates only Forge-generated package metadata and archive entries;
it still does not download, rehost, or require third-party runtime binaries.
Gate 75 writes only a preview report for package install intent; it still does
not copy files into a game install, create an MO2 mod, or bundle third-party
runtime dependencies.
Gate 76 validates only that generated preview report against an embedded
schema; it still does not copy files into a game install, create an MO2 mod,
or bundle third-party runtime dependencies.
Gate 77 writes only a human-readable summary of that preview report; it still
does not copy files into a game install, create an MO2 mod, or bundle
third-party runtime dependencies.
Gate 78 writes only local package verification report evidence; it still does
not copy files into a game install, create an MO2 mod, inspect MO2 VFS
visibility, launch the game, or bundle third-party runtime dependencies.
Gate 79 validates only that generated package verification report against an
embedded schema; it still does not copy files into a game install, create an
MO2 mod, inspect MO2 VFS visibility, launch the game, or bundle third-party
runtime dependencies.
Gate 80 writes only a human-readable summary of that local package
verification report; it still does not copy files into a game install, create
an MO2 mod, inspect MO2 VFS visibility, launch the game, or bundle
third-party runtime dependencies.
Gate 81 cross-checks only Forge-generated local package verification
evidence; it still does not copy files into a game install, create an MO2 mod,
inspect MO2 VFS visibility, launch the game, or bundle third-party runtime
dependencies.
Gate 82 extracts those checks into reusable local validator code; it still
does not add, download, rehost, or require third-party runtime dependencies.
Gate 83 adds only local file-reading verifier code over Forge-generated
evidence; it still does not add, download, rehost, or require third-party
runtime dependencies.
Gate 84 adds only local SHA-256 recomputation over Forge-generated payload
files; it still does not add, download, rehost, or require third-party runtime
dependencies.
Gate 85 adds only local SHA-256 recomputation over Forge-generated package
archives; it still does not add, download, rehost, or require third-party
runtime dependencies.
Gate 86 adds only local ZIP entry-name inspection over Forge-generated package
archives; it still does not add, download, rehost, or require third-party
runtime dependencies.
Gate 87 is a command-surface decision checkpoint only; it still does not add,
download, rehost, or require third-party runtime dependencies.
Gate 88 adds only local file-based verification over Forge-generated package
evidence; it still does not add, download, rehost, or require third-party
runtime dependencies.
Gate 89 adds only SARIF/GitHub output projection over the same local package
evidence diagnostics; it still does not add, download, rehost, or require
third-party runtime dependencies.
Gate 90 adds only Markdown summary projection over the same local package
evidence diagnostics; it still does not add, download, rehost, or require
third-party runtime dependencies.
Gate 91 adds only checksum-file revalidation over the same local package
evidence diagnostics; it still does not add, download, rehost, or require
third-party runtime dependencies.
Gate 92 adds only build-manifest content revalidation over the same local
package evidence diagnostics; it still does not add, download, rehost, or
require third-party runtime dependencies.
Gate 93 adds only install-preview summary content revalidation over the same
local package evidence diagnostics; it still does not add, download, rehost,
or require third-party runtime dependencies.
Gate 94 adds only install-preview/package-manifest entry content cross-checking
over the same local package evidence diagnostics; it still does not add,
download, rehost, or require third-party runtime dependencies.
Gate 95 adds only package-verification summary content revalidation over the
same local package evidence diagnostics; it still does not add, download,
rehost, or require third-party runtime dependencies.
Gate 96 adds only package-verification JSON check content revalidation over
the same local package evidence diagnostics; it still does not add, download,
rehost, or require third-party runtime dependencies.
Gate 97 adds only package-verification JSON metadata content revalidation over
the same local package evidence diagnostics; it still does not add, download,
rehost, or require third-party runtime dependencies.
Gate 98 adds only package-verification archive detail content revalidation
over the same local package evidence diagnostics; it still does not add,
download, rehost, or require third-party runtime dependencies.
Gate 99 adds only install-preview archive detail content revalidation over the
same local package evidence diagnostics; it still does not add, download,
rehost, or require third-party runtime dependencies.
Gate 100 adds only package-manifest archive detail content revalidation over
the same local package evidence diagnostics; it still does not add, download,
rehost, or require third-party runtime dependencies.
Gate 101 adds only archive detail cross-report consistency revalidation over
the same local package evidence diagnostics; it still does not add, download,
rehost, or require third-party runtime dependencies.
Gate 102 adds only package archive presence revalidation over the same local
package evidence diagnostics; it still does not add, download, rehost, or
require third-party runtime dependencies.
Gate 103 adds only checksum unexpected-entry revalidation over the same local
package evidence diagnostics; it still does not add, download, rehost, or
require third-party runtime dependencies.
Gate 104 adds only checksum duplicate-entry revalidation over the same local
package evidence diagnostics; it still does not add, download, rehost, or
require third-party runtime dependencies.
Gate 105 adds only checksum canonical-order revalidation over the same local
package evidence diagnostics; it still does not add, download, rehost, or
require third-party runtime dependencies.
Gate 106 adds only checksum digest canonical-casing revalidation over the same
local package evidence diagnostics; it still does not add, download, rehost, or
require third-party runtime dependencies.
Gate 107 adds only checksum line-ending and trailing-newline revalidation over
the same local package evidence diagnostics; it still does not add, download,
rehost, or require third-party runtime dependencies.
Gate 108 adds only checksum path separator canonicalization revalidation over
the same local package evidence diagnostics; it still does not add, download,
rehost, or require third-party runtime dependencies.
Gate 109 adds only checksum blank-line revalidation over the same local package
evidence diagnostics; it still does not add, download, rehost, or require
third-party runtime dependencies.
Gate 110 adds only checksum entry spacing canonicalization revalidation over
the same local package evidence diagnostics; it still does not add, download,
rehost, or require third-party runtime dependencies.
Gate 111 adds only checksum path casing canonicalization revalidation over the
same local package evidence diagnostics; it still does not add, download,
rehost, or require third-party runtime dependencies.
Gate 112 adds only checksum case-insensitive duplicate revalidation over the
same local package evidence diagnostics; it still does not add, download,
rehost, or require third-party runtime dependencies.
Gate 113 adds only checksum malformed-entry format revalidation over the same
local package evidence diagnostics; it still does not add, download, rehost, or
require third-party runtime dependencies.
Gate 114 adds only checksum path containment revalidation over the same local
package evidence diagnostics; it still does not add, download, rehost, or
require third-party runtime dependencies.
Gate 115 adds only checksum comment-line rejection revalidation over the same
local package evidence diagnostics; it still does not add, download, rehost, or
require third-party runtime dependencies.
Gate 116 adds only schema-validated install-plan evidence over the same local
MCM package output tree; it still does not add, download, rehost, or require
third-party runtime dependencies.
Gate 117 adds only install-plan content revalidation over existing local
package evidence; it still does not add, download, rehost, or require
third-party runtime dependencies.
Gate 118 adds only install-plan schema revalidation over existing local
package evidence; it still does not add, download, rehost, or require
third-party runtime dependencies.
Gate 119 adds only package-manifest schema revalidation over existing local
package evidence; it still does not add, download, rehost, or require
third-party runtime dependencies.
Gate 120 adds only install-preview schema revalidation over existing local
package evidence; it still does not add, download, rehost, or require
third-party runtime dependencies.
Gate 121 adds only package-verification schema revalidation over existing local
package evidence; it still does not add, download, rehost, or require
third-party runtime dependencies.
Gate 122 adds only diagnostic projection tests for existing local package
evidence; it still does not add, download, rehost, or require third-party
runtime dependencies.
Gate 123 adds only missing-evidence diagnostics for existing local package
evidence; it still does not add, download, rehost, or require third-party
runtime dependencies.
Gate 124 adds only malformed-evidence diagnostics for existing local package
evidence; it still does not add, download, rehost, or require third-party
runtime dependencies.
Gate 125 adds only diagnostic projection tests for malformed existing local
package evidence; it still does not add, download, rehost, or require
third-party runtime dependencies.
Gate 126 closes the current MCM Extender lane without adding, downloading,
rehosting, or requiring third-party runtime dependencies. Future
capability/Doctor work must detect local providers and link users to official
acquisition paths rather than bundling restricted runtime assets.
Gate 127 adds only derived Doctor reporting over local path evidence. It does
not add, download, rehost, or require third-party runtime dependencies.
Gate 128 exports that local evidence as a redacted Doctor handoff bundle. It
does not add, download, rehost, or require third-party runtime dependencies,
and it performs no network, runtime, MO2, GECK, or AI operation.
Gate 129 projects existing capability requirement evidence into `WF-CAP-*`
diagnostics. It does not add, download, rehost, or require third-party runtime
dependencies, and it performs no network, runtime, MO2, GECK, or AI operation.
Gate 130 adds provider evidence detail to the same local capability diagnostics.
It does not add, download, rehost, or require third-party runtime dependencies,
and it performs no network, runtime, MO2, GECK, or AI operation.
Gate 131 groups that same local provider evidence in `forge capabilities
explain`. It adds no new dependency, detector, network call, runtime probe,
MO2/GECK integration, or AI requirement.
Gate 132 uses existing local path detectors to identify root-vs-Data
wrong-scope markers. It adds no third-party dependency, download, rehosted
runtime binary, network call, runtime probe, MO2/GECK integration, or AI
requirement.
Gate 133 reuses existing project requirement loading and local capability
resolution inside `forge capabilities explain --project`. It adds no
third-party dependency, download, rehosted runtime binary, network call,
runtime probe, MO2/GECK integration, provider-version parser, or AI
requirement.
Gate 134 reuses the existing capability diagnostic projector inside
`forge capabilities explain --project`. It adds no third-party dependency,
download, rehosted runtime binary, network call, runtime probe, MO2/GECK
integration, provider-version parser, or AI requirement.
Gate 135 derives `forge doctor export` summary and index data from the
existing redacted capability scan report. It adds no third-party dependency,
download, rehosted runtime binary, network call, runtime probe, MO2/GECK
integration, provider-version parser, or AI requirement.
Gate 136 derives `forge doctor export` diagnostics index data from the
existing redacted capability scan diagnostic report. It adds no third-party
dependency, download, rehosted runtime binary, network call, runtime probe,
MO2/GECK integration, provider-version parser, or AI requirement.
Gate 137 derives `forge doctor export` requirements index data from the
existing redacted project requirement resolution report. It adds no
third-party dependency, download, rehosted runtime binary, network call,
runtime probe, MO2/GECK integration, provider-version parser, or AI
requirement.
Gate 138 derives `forge doctor export` action index data from the existing
redacted Doctor area actions. It adds no third-party dependency, download,
rehosted runtime binary, network call, runtime probe, MO2/GECK integration,
provider-version parser, or AI requirement.
Gate 139 derives `forge doctor export` open-question detail data from the
existing built-in catalogue policy gap text. It adds no third-party
dependency, download, rehosted runtime binary, network call, runtime probe,
MO2/GECK integration, provider-version parser, or AI requirement.
Gate 140 derives `forge doctor export` provider-status index data from the
existing redacted capability scan report. It adds no third-party dependency,
download, rehosted runtime binary, network call, runtime probe, MO2/GECK
integration, provider-version parser, or AI requirement.
Gate 141 derives `forge doctor export` capability-status index data from the
existing redacted capability scan report. It adds no third-party dependency,
download, rehosted runtime binary, network call, runtime probe, MO2/GECK
integration, provider-version parser, capability resolver dependency, or AI
requirement.
Gate 142 derives `forge doctor export` Doctor area-status index data from the
existing redacted Doctor report. It adds no third-party dependency, download,
rehosted runtime binary, network call, runtime probe, MO2/GECK integration,
provider-version parser, capability resolver dependency, Doctor planner
dependency, or AI requirement.
Gate 143 derives `forge doctor export` catalogue-policy index data from the
existing structured open-question details. It adds no third-party dependency,
download, rehosted runtime binary, network call, runtime probe, MO2/GECK
integration, provider-version parser, capability resolver dependency, Doctor
planner dependency, or AI requirement.
Gate 144 derives `forge capabilities scan` Doctor readiness index data from
the existing Doctor area results. It adds no third-party dependency, download,
rehosted runtime binary, network call, runtime probe, MO2/GECK integration,
provider-version parser, capability resolver dependency, Doctor planner
dependency, or AI requirement.
Gate 145 derives `forge capabilities scan` provider/capability status index
data from existing scan results. It adds no third-party dependency, download,
rehosted runtime binary, network call, runtime probe, MO2/GECK integration,
provider-version parser, capability resolver dependency, Doctor planner
dependency, or AI requirement.
Gate 146 derives `forge capabilities scan` action index data from existing
Doctor area actions. It adds no third-party dependency, download, rehosted
runtime binary, network call, runtime probe, MO2/GECK integration,
provider-version parser, capability resolver dependency, Doctor planner
dependency, or AI requirement.
Gate 147 derives `forge capabilities scan` requirement index data from
existing project requirement resolution output. It adds no third-party
dependency, download, rehosted runtime binary, network call, runtime probe,
MO2/GECK integration, provider-version parser, capability resolver
dependency, Doctor planner dependency, diagnostic projector dependency, or AI
requirement.
Gate 148 derives `forge capabilities scan` diagnostic index data from existing
diagnostic projection output. It adds no third-party dependency, download,
rehosted runtime binary, network call, runtime probe, MO2/GECK integration,
provider-version parser, capability resolver dependency, Doctor planner
dependency, additional diagnostic projector dependency, or AI requirement.
Gate 149 derives `forge capabilities scan` catalogue-policy index data from
existing Doctor open-question output. It adds no third-party dependency,
download, rehosted runtime binary, network call, runtime probe, MO2/GECK
integration, provider-version parser, capability resolver dependency, Doctor
planner dependency, catalogue-policy decision dependency, or AI requirement.
Gate 150 derives `forge capabilities scan` open-question detail index data
from existing Doctor open-question output. It adds no third-party dependency,
download, rehosted runtime binary, network call, runtime probe, MO2/GECK
integration, provider-version parser, capability resolver dependency, Doctor
planner dependency, catalogue-policy decision dependency, or AI requirement.
Gate 151 derives `forge capabilities explain` catalogue-policy open-question
detail data from existing Doctor open-question output. It adds no third-party
dependency, download, rehosted runtime binary, network call, runtime probe,
MO2/GECK integration, provider-version parser, capability resolver dependency,
Doctor planner dependency, catalogue-policy decision dependency, or AI
requirement.
Gate 152 derives `forge capabilities explain` catalogue-policy diagnostic
handoff data from existing Doctor open-question output and stable question
IDs. It adds no third-party dependency, download, rehosted runtime binary,
network call, runtime probe, MO2/GECK integration, provider-version parser,
capability resolver dependency, Doctor planner dependency, catalogue-policy
decision dependency, or AI requirement.
Gate 153 derives `forge doctor export` catalogue-policy diagnostic handoff
data from existing Doctor open-question output and stable question IDs. It
adds no third-party dependency, download, rehosted runtime binary, network
call, runtime probe, MO2/GECK integration, provider-version parser,
capability resolver dependency, Doctor planner dependency, catalogue-policy
decision dependency, or AI requirement.
Gate 154 derives `forge capabilities scan` catalogue-policy diagnostic
handoff data from existing Doctor open-question output and stable question
IDs. It adds no third-party dependency, download, rehosted runtime binary,
network call, runtime probe, MO2/GECK integration, provider-version parser,
capability resolver dependency, Doctor planner dependency, catalogue-policy
decision dependency, or AI requirement.
Gate 155 derives the same catalogue-policy diagnostic handoff metadata through
shared rendering helpers. It adds no third-party dependency, download,
rehosted runtime binary, network call, runtime probe, MO2/GECK integration,
provider-version parser, capability resolver dependency, Doctor planner
dependency, catalogue-policy decision dependency, or AI requirement.
Gate 156 derives the same catalogue-policy open-question detail and
source-type index metadata through shared rendering helpers. It adds no
third-party dependency, download, rehosted runtime binary, network call,
runtime probe, MO2/GECK integration, provider-version parser, capability
resolver dependency, Doctor planner dependency, catalogue-policy decision
dependency, or AI requirement.
Gate 157 derives the same catalogue-policy metadata through a shared view
model. It adds no third-party dependency, download, rehosted runtime binary,
network call, runtime probe, MO2/GECK integration, provider-version parser,
capability resolver dependency, Doctor planner dependency, catalogue-policy
decision dependency, or AI requirement.
Gate 158 derives compact Doctor action summary metadata from existing Doctor
area actions. It adds no third-party dependency, download, rehosted runtime
binary, network call, runtime probe, MO2/GECK integration, provider-version
parser, capability resolver dependency, Doctor planner dependency,
catalogue-policy decision dependency, or AI requirement.
Gate 159 derives compact provider evidence summary metadata from existing
provider detector evidence. It adds no third-party dependency, download,
rehosted runtime binary, network call, runtime probe, MO2/GECK integration,
provider-version parser, capability resolver dependency, Doctor planner
dependency, catalogue-policy decision dependency, or AI requirement.
Gate 160 derives compact requirement summary metadata from existing project
requirement resolution data. It adds no third-party dependency, download,
rehosted runtime binary, network call, runtime probe, MO2/GECK integration,
provider-version parser, capability resolver dependency, Doctor planner
dependency, catalogue-policy decision dependency, or AI requirement.
Gate 161 derives compact diagnostic summary metadata from existing diagnostic
reports. It adds no third-party dependency, download, rehosted runtime binary,
network call, runtime probe, MO2/GECK integration, provider-version parser,
capability resolver dependency, Doctor planner dependency, catalogue-policy
decision dependency, SARIF/GitHub dependency, or AI requirement.
Gate 162 derives compact provider inventory summary metadata from existing
provider scan results. It adds no third-party dependency, download, rehosted
runtime binary, network call, runtime probe, MO2/GECK integration,
provider-version parser, detector dependency, capability resolver dependency,
Doctor planner dependency, catalogue-policy decision dependency,
SARIF/GitHub dependency, or AI requirement.
Gate 163 derives compact Doctor area capability summary metadata from
existing Doctor, capability, and provider scan results. It adds no third-party
dependency, download, rehosted runtime binary, network call, runtime probe,
MO2/GECK integration, provider-version parser, detector dependency,
capability resolver dependency, Doctor planner dependency, catalogue-policy
decision dependency, SARIF/GitHub dependency, or AI requirement.
Gate 164 derives a Markdown sidecar summary from the existing redacted Doctor
export report. It adds no third-party dependency, download, rehosted runtime
binary, network call, runtime probe, MO2/GECK integration, provider-version
parser, detector dependency, capability resolver dependency, Doctor planner
dependency, catalogue-policy decision dependency, SARIF/GitHub dependency, or
AI requirement.
Gate 165 derives a Markdown sidecar summary from the existing capability scan
report and projected diagnostics. It adds no third-party dependency, download,
rehosted runtime binary, network call, runtime probe, MO2/GECK integration,
provider-version parser, detector dependency, capability resolver dependency,
Doctor planner dependency, catalogue-policy decision dependency,
SARIF/GitHub dependency, GitHub step-summary dependency, or AI requirement.
Gate 166 derives a deterministic ZIP sidecar archive from the existing
redacted Doctor export report and Markdown summary using .NET platform
libraries. It adds no third-party dependency, download, rehosted runtime
binary, network call, runtime probe, MO2/GECK integration, provider-version
parser, detector dependency, capability resolver dependency, Doctor planner
dependency, catalogue-policy decision dependency, SARIF/GitHub dependency,
GitHub step-summary dependency, release-publishing dependency, or AI
requirement.
Gate 167 derives a Markdown sidecar summary from the existing capability
explanation report using .NET platform libraries. It adds no third-party
dependency, download, rehosted runtime binary, network call, runtime probe,
MO2/GECK integration, provider-version parser, detector dependency,
capability resolver dependency, Doctor planner dependency, catalogue-policy
decision dependency, SARIF/GitHub dependency, GitHub step-summary dependency,
or AI requirement.
Gate 168 derives supplemental Markdown archive entries from the existing
capability explanation report using .NET platform libraries. It adds no
third-party dependency, download, rehosted runtime binary, network call,
runtime probe, MO2/GECK integration, provider-version parser, detector
dependency, capability resolver dependency, Doctor planner dependency,
catalogue-policy decision dependency, SARIF/GitHub dependency, GitHub
step-summary dependency, release-publishing dependency, or AI requirement.
Gate 169 derives supplemental JSON archive entries from the existing
capability explanation report using .NET platform libraries. It adds no
third-party dependency, download, rehosted runtime binary, network call,
runtime probe, MO2/GECK integration, provider-version parser, detector
dependency, capability resolver dependency, Doctor planner dependency,
catalogue-policy decision dependency, SARIF/GitHub dependency, GitHub
step-summary dependency, release-publishing dependency, or AI requirement.
Gate 170 derives supplemental index archive entries from existing
requirement-resolution and diagnostic-handoff data using .NET platform
libraries. It adds no third-party dependency, download, rehosted runtime
binary, network call, runtime probe, MO2/GECK integration, provider-version
parser, detector dependency, capability resolver dependency, Doctor planner
dependency, catalogue-policy decision dependency, SARIF/GitHub dependency,
GitHub step-summary dependency, release-publishing dependency, or AI
requirement.
Gate 171 derives a Doctor bundle README archive entry from the existing
redacted Doctor report and archive supplement paths using .NET platform
libraries. It adds no third-party dependency, download, rehosted runtime
binary, network call, runtime probe, MO2/GECK integration, provider-version
parser, detector dependency, capability resolver dependency, Doctor planner
dependency, catalogue-policy decision dependency, SARIF/GitHub dependency,
GitHub step-summary dependency, release-publishing dependency, or AI
requirement.
Gate 172 derives Doctor bundle diagnostic index archive entries from the
existing redacted Doctor diagnostic summary and compact diagnostic entries
using .NET platform libraries. It adds no third-party dependency, download,
rehosted runtime binary, network call, runtime probe, MO2/GECK integration,
provider-version parser, detector dependency, capability resolver dependency,
Doctor planner dependency, catalogue-policy decision dependency, SARIF/GitHub
dependency, GitHub step-summary dependency, release-publishing dependency, or
AI requirement.
Gate 173 derives Doctor bundle action index archive entries from the existing
redacted Doctor action summary and compact action entries using .NET platform
libraries. It adds no third-party dependency, download, rehosted runtime
binary, network call, runtime probe, MO2/GECK integration, provider-version
parser, detector dependency, capability resolver dependency, Doctor planner
dependency, catalogue-policy decision dependency, SARIF/GitHub dependency,
GitHub step-summary dependency, release-publishing dependency, or AI
requirement.
Gate 174 derives Doctor bundle requirement index archive entries from the
existing redacted Doctor requirement summary and compact unavailable
requirement entries using .NET platform libraries. It adds no third-party
dependency, download, rehosted runtime binary, network call, runtime probe,
MO2/GECK integration, provider-version parser, detector dependency,
capability resolver dependency, Doctor planner dependency, catalogue-policy
decision dependency, SARIF/GitHub dependency, GitHub step-summary dependency,
release-publishing dependency, or AI requirement.
Gate 175 derives Doctor bundle provider index archive entries from the
existing redacted Doctor provider summary, provider-status groups, provider
inventory summary, evidence summary, and compact provider scan entries using
.NET platform libraries. It adds no third-party dependency, download,
rehosted runtime binary, network call, runtime probe, MO2/GECK integration,
provider-version parser, detector dependency, capability resolver dependency,
Doctor planner dependency, catalogue-policy decision dependency, SARIF/GitHub
dependency, GitHub step-summary dependency, release-publishing dependency, or
AI requirement.
Gate 176 derives Doctor bundle capability index archive entries from the
existing redacted Doctor capability summary, capability-status groups, Doctor
area capability summary, and compact capability scan entries using .NET
platform libraries. It adds no third-party dependency, download, rehosted
runtime binary, network call, runtime probe, MO2/GECK integration,
provider-version parser, detector dependency, capability resolver dependency,
Doctor planner dependency, catalogue-policy decision dependency, SARIF/GitHub
dependency, GitHub step-summary dependency, release-publishing dependency, or
AI requirement.
Gate 177 derives Doctor bundle Doctor area index archive entries from the
existing redacted Doctor readiness summary, area-status groups, Doctor area
capability summary, and compact Doctor area entries using .NET platform
libraries. It adds no third-party dependency, download, rehosted runtime
binary, network call, runtime probe, MO2/GECK integration, provider-version
parser, detector dependency, capability resolver dependency, Doctor planner
dependency, catalogue-policy decision dependency, SARIF/GitHub dependency,
GitHub step-summary dependency, release-publishing dependency, or AI
requirement.
Gate 178 derives Doctor bundle catalogue-policy index archive entries from the
existing redacted source-type groups, open-question details, diagnostic
handoff entries, and open-question text using .NET platform libraries. It adds
no third-party dependency, download, rehosted runtime binary, network call,
runtime probe, MO2/GECK integration, provider-version parser, detector
dependency, capability resolver dependency, Doctor planner dependency,
catalogue-policy decision dependency, SARIF/GitHub dependency, GitHub
step-summary dependency, release-publishing dependency, or AI requirement.
Gate 179 derives Doctor bundle summary index archive entries from the existing
redacted report summary and already-derived summary metadata using .NET
platform libraries. It adds no third-party dependency, download, rehosted
runtime binary, network call, runtime probe, MO2/GECK integration,
provider-version parser, detector dependency, capability resolver dependency,
Doctor planner dependency, catalogue-policy decision dependency, SARIF/GitHub
dependency, GitHub step-summary dependency, release-publishing dependency, or
AI requirement.
Gate 180 derives Doctor bundle evidence index archive entries from the
existing redacted evidence summary and compact provider evidence entries using
.NET platform libraries. It adds no third-party dependency, download, rehosted
runtime binary, network call, runtime probe, MO2/GECK integration,
provider-version parser, detector dependency, capability resolver dependency,
Doctor planner dependency, catalogue-policy decision dependency, SARIF/GitHub
dependency, GitHub step-summary dependency, release-publishing dependency, or
AI requirement.
Gate 181 derives Doctor bundle redaction index archive entries from the
existing Doctor export redaction metadata using .NET platform libraries. It
adds no third-party dependency, download, rehosted runtime binary, network
call, runtime probe, MO2/GECK integration, provider-version parser, detector
dependency, capability resolver dependency, Doctor planner dependency,
catalogue-policy decision dependency, SARIF/GitHub dependency, GitHub
step-summary dependency, release-publishing dependency, or AI requirement.
Gate 182 derives Doctor bundle open-question index archive entries from the
existing Doctor export open-question metadata using .NET platform libraries.
It adds no third-party dependency, download, rehosted runtime binary, network
call, runtime probe, MO2/GECK integration, provider-version parser, detector
dependency, capability resolver dependency, Doctor planner dependency,
catalogue-policy decision dependency, SARIF/GitHub dependency, GitHub
step-summary dependency, release-publishing dependency, or AI requirement.
Gate 183 derives Doctor bundle scan-input index archive entries from the
existing redacted Doctor export capability scan input metadata using .NET
platform libraries. It adds no third-party dependency, download, rehosted
runtime binary, network call, runtime probe, MO2/GECK integration,
provider-version parser, detector dependency, capability resolver dependency,
Doctor planner dependency, catalogue-policy decision dependency, SARIF/GitHub
dependency, GitHub step-summary dependency, release-publishing dependency, or
AI requirement.
Gate 184 derives Doctor bundle navigation index archive entries from existing
archive supplement paths and redacted bundle metadata using .NET platform
libraries. It adds no third-party dependency, download, rehosted runtime
binary, network call, runtime probe, MO2/GECK integration,
provider-version parser, detector dependency, capability resolver dependency,
Doctor planner dependency, catalogue-policy decision dependency,
SARIF/GitHub dependency, GitHub step-summary dependency,
release-publishing dependency, or AI requirement.
Gate 185 derives Doctor bundle triage index archive entries from existing
redacted Doctor summary, diagnostic, requirement, action, wrong-scope, and
open-question metadata using .NET platform libraries. It adds no third-party
dependency, download, rehosted runtime binary, network call, runtime probe,
MO2/GECK integration, provider-version parser, detector dependency,
capability resolver dependency, Doctor planner dependency, catalogue-policy
decision dependency, SARIF/GitHub dependency, GitHub step-summary dependency,
release-publishing dependency, or AI requirement.
Gate 186 derives primary Doctor export triage JSON, plain text, and Markdown
summary projection from existing redacted Doctor metadata using .NET platform
libraries. It adds no third-party dependency, download, rehosted runtime
binary, network call, runtime probe, MO2/GECK integration,
provider-version parser, detector dependency, capability resolver dependency,
Doctor planner dependency, catalogue-policy decision dependency,
SARIF/GitHub dependency, GitHub step-summary dependency, release-publishing
dependency, or AI requirement.
Gate 187 derives Doctor triage command hints from existing redacted Doctor
metadata using .NET platform libraries. It adds no third-party dependency,
download, rehosted runtime binary, network call, runtime probe, MO2/GECK
integration, provider-version parser, detector dependency, capability
resolver dependency, Doctor planner dependency, catalogue-policy decision
dependency, SARIF/GitHub dependency, GitHub step-summary dependency,
release-publishing dependency, or AI requirement.
Gate 188 derives Doctor triage worklist entries from existing redacted Doctor
metadata and command-hint IDs using .NET platform libraries. It adds no
third-party dependency, download, rehosted runtime binary, network call,
runtime probe, MO2/GECK integration, provider-version parser, detector
dependency, capability resolver dependency, Doctor planner dependency,
catalogue-policy decision dependency, SARIF/GitHub dependency,
GitHub step-summary dependency, release-publishing dependency, or AI
requirement.
Gate 189 derives Doctor worklist summary metadata from existing worklist items
using .NET platform libraries. It adds no third-party dependency, download,
rehosted runtime binary, network call, runtime probe, MO2/GECK integration,
provider-version parser, detector dependency, capability resolver dependency,
Doctor planner dependency, catalogue-policy decision dependency,
SARIF/GitHub dependency, GitHub step-summary dependency, release-publishing
dependency, or AI requirement.
Gate 190 derives the Doctor remediation status header from existing worklist
and command-hint data using .NET platform libraries. It adds no third-party
dependency, download, rehosted runtime binary, network call, runtime probe,
MO2/GECK integration, provider-version parser, detector dependency,
capability resolver dependency, Doctor planner dependency, catalogue-policy
decision dependency, SARIF/GitHub dependency, GitHub step-summary dependency,
release-publishing dependency, or AI requirement.
Gate 191 derives human operator handoff checklist sections from existing
remediation, worklist-summary, and command-hint data using .NET platform
libraries. It adds no third-party dependency, download, rehosted runtime
binary, network call, runtime probe, MO2/GECK integration, provider-version
parser, detector dependency, capability resolver dependency, Doctor planner
dependency, catalogue-policy decision dependency, SARIF/GitHub dependency,
GitHub step-summary dependency, release-publishing dependency, or AI
requirement.
Gate 192 derives `handoff-summary.md` from existing redacted Doctor triage
metadata using .NET platform libraries. It adds no third-party dependency,
download, rehosted runtime binary, network call, runtime probe, MO2/GECK
integration, provider-version parser, detector dependency, capability
resolver dependency, Doctor planner dependency, catalogue-policy decision
dependency, SARIF/GitHub dependency, GitHub step-summary dependency,
release-publishing dependency, or AI requirement.
Gate 193 derives scan-side operator handoff text from existing capability scan
metadata using .NET platform libraries. It adds no third-party dependency,
download, rehosted runtime binary, network call, runtime probe, MO2/GECK
integration, provider-version parser, detector dependency, capability
resolver dependency, Doctor planner dependency, catalogue-policy decision
dependency, SARIF/GitHub dependency, GitHub step-summary dependency,
release-publishing dependency, or AI requirement.
Gate 194 derives explain-side operator handoff text from existing capability
explanation metadata using .NET platform libraries. It adds no third-party
dependency, download, rehosted runtime binary, network call, runtime probe,
MO2/GECK integration, provider-version parser, detector dependency,
capability resolver dependency, Doctor planner dependency, catalogue-policy
decision dependency, SARIF/GitHub dependency, GitHub step-summary dependency,
release-publishing dependency, or AI requirement.
Gate 195 derives Doctor bundle requirement explanation handoff coverage and
generated archive notes from existing capability explanation metadata using
.NET platform libraries. It adds no third-party dependency, download,
rehosted runtime binary, network call, runtime probe, MO2/GECK integration,
provider-version parser, detector dependency, capability resolver dependency,
Doctor planner dependency, catalogue-policy decision dependency, SARIF/GitHub
dependency, GitHub step-summary dependency, release-publishing dependency, or
AI requirement.
Gate 196 derives declaration-only provider-version metadata from the built-in
catalogue using existing .NET platform libraries. It adds no third-party
dependency, download, rehosted runtime binary, network call, runtime probe,
MO2/GECK integration, provider-version parser, detector dependency,
capability resolver dependency, Doctor planner dependency, catalogue-policy
decision dependency, SARIF/GitHub dependency, GitHub step-summary dependency,
release-publishing dependency, or AI requirement.
Gate 197 projects existing declaration-only provider-version metadata through
scan and Doctor provider indexes using existing .NET platform libraries. It
adds no third-party dependency, download, rehosted runtime binary, network
call, runtime probe, MO2/GECK integration, provider-version parser, detector
dependency, capability resolver dependency, Doctor planner dependency,
catalogue-policy decision dependency, SARIF/GitHub dependency, GitHub
step-summary dependency, release-publishing dependency, or AI requirement.
Gate 198 records provider-version parser research only. It adds no
third-party dependency, download, rehosted runtime binary, network call,
runtime probe, MO2/GECK integration, provider-version parser, detector
dependency, capability resolver dependency, Doctor planner dependency,
catalogue-policy decision dependency, SARIF/GitHub dependency, GitHub
step-summary dependency, release-publishing dependency, or AI requirement.
Gate 199 adds the provider-version parser contract using existing .NET
platform libraries and existing WastelandForge domain types. It adds no
third-party dependency, download, rehosted runtime binary, network call,
runtime probe, MO2/GECK integration, detector dependency, capability resolver
dependency, Doctor planner dependency, catalogue-policy decision dependency,
SARIF/GitHub dependency, GitHub step-summary dependency, release-publishing
dependency, or AI requirement.
Gate 200 documents the parser contract and future projection notes using
Markdown only. It adds no third-party dependency, download, rehosted runtime
binary, network call, runtime probe, MO2/GECK integration, detector
dependency, capability resolver dependency, Doctor planner dependency,
catalogue-policy decision dependency, SARIF/GitHub dependency, GitHub
step-summary dependency, release-publishing dependency, or AI requirement.
Gate 201 adds the parsed-evidence model using existing .NET platform
libraries and existing WastelandForge parser types. It adds no third-party
dependency, download, rehosted runtime binary, network call, runtime probe,
MO2/GECK integration, detector dependency, capability resolver dependency,
Doctor planner dependency, catalogue-policy decision dependency,
SARIF/GitHub dependency, GitHub step-summary dependency, release-publishing
dependency, or AI requirement.
Gate 202 adds documentation only for the JIP LN text-script generator
evidence checkpoint. It adds no third-party dependency, download, rehosted
runtime binary, network call, runtime probe, MO2/GECK integration, detector
dependency, capability resolver dependency, Doctor planner dependency,
catalogue-policy decision dependency, SARIF/GitHub dependency, GitHub
step-summary dependency, release-publishing dependency, external tool
execution, or AI requirement.

Gate 203 adds a source schema, schema catalog entry, validation read model,
and synthetic JSON fixtures using existing .NET and repository dependencies.
It adds no third-party dependency, download, rehosted runtime binary, network
call, runtime probe, MO2/GECK integration, detector dependency, capability
resolver dependency, Doctor planner dependency, catalogue-policy decision
dependency, SARIF/GitHub dependency, GitHub step-summary dependency,
release-publishing dependency, external tool execution, or AI requirement.

Gate 204 adds semantic checks and synthetic JSON fixtures using existing .NET
and repository dependencies. It adds no third-party dependency, download,
rehosted runtime binary, network call, runtime probe, MO2/GECK integration,
detector dependency, capability resolver dependency, Doctor planner
dependency, catalogue-policy decision dependency, SARIF/GitHub dependency,
GitHub step-summary dependency, release-publishing dependency, external tool
execution, or AI requirement.

Gate 205 adds opaque source-line schema/read-model support and synthetic JSON
fixtures using existing .NET and repository dependencies. It adds no
third-party dependency, download, rehosted runtime binary, network call,
runtime probe, MO2/GECK integration, detector dependency, capability resolver
dependency, Doctor planner dependency, catalogue-policy decision dependency,
SARIF/GitHub dependency, GitHub step-summary dependency,
release-publishing dependency, external tool execution, or AI requirement.

Gate 206 adds source-line byte-budget semantic validation and synthetic JSON
fixtures using existing .NET and repository dependencies. It adds no
third-party dependency, download, rehosted runtime binary, network call,
runtime probe, MO2/GECK integration, detector dependency, capability resolver
dependency, Doctor planner dependency, catalogue-policy decision dependency,
SARIF/GitHub dependency, GitHub step-summary dependency,
release-publishing dependency, external tool execution, or AI requirement.

Gate 207 adds duplicate output filename semantic validation and synthetic
JSON fixtures using existing .NET and repository dependencies. It adds no
third-party dependency, download, rehosted runtime binary, network call,
runtime probe, MO2/GECK integration, detector dependency, capability resolver
dependency, Doctor planner dependency, catalogue-policy decision dependency,
SARIF/GitHub dependency, GitHub step-summary dependency,
release-publishing dependency, external tool execution, or AI requirement.

Gate 208 adds a non-emitting JIP generation planner and unit tests using
existing .NET and repository dependencies. It adds no third-party dependency,
download, rehosted runtime binary, network call, runtime probe, MO2/GECK
integration, detector dependency, capability resolver dependency, Doctor
planner dependency, catalogue-policy decision dependency, SARIF/GitHub
dependency, GitHub step-summary dependency, release-publishing dependency,
external tool execution, or AI requirement.

Gate 209 adds an in-memory JIP text renderer and unit tests using existing
.NET and repository dependencies. It adds no third-party dependency,
download, rehosted runtime binary, network call, runtime probe, MO2/GECK
integration, detector dependency, capability resolver dependency, Doctor
planner dependency, catalogue-policy decision dependency, SARIF/GitHub
dependency, GitHub step-summary dependency, release-publishing dependency,
external tool execution, or AI requirement.

Gate 210 adds generated JIP file emission and unit tests using existing .NET
and repository dependencies. It adds no third-party dependency, download,
rehosted runtime binary, network call, runtime probe, MO2/GECK integration,
detector dependency, capability resolver dependency, Doctor planner
dependency, catalogue-policy decision dependency, SARIF/GitHub dependency,
GitHub step-summary dependency, release-publishing dependency, external tool
execution, or AI requirement.

Gate 211 adds generated JIP manifest, checksum, and digest emission using
existing .NET and repository dependencies. It adds no third-party dependency,
download, rehosted runtime binary, network call, runtime probe, MO2/GECK
integration, detector dependency, capability resolver dependency, Doctor
planner dependency, catalogue-policy decision dependency, SARIF/GitHub
dependency, GitHub step-summary dependency, release-publishing dependency,
external tool execution, or AI requirement.
