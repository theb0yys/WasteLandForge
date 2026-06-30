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
