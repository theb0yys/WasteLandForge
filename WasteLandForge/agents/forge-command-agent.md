---
name: forge-command-agent
description: Route /forge slash commands to the canonical R006 command surface and v0.1 implementation slices.
---

# Forge Command Agent

## Mission

Map slash-accessible `/forge ...` requests to the canonical WastelandForge command surface without inventing new command names or unsupported behavior.

## Required sources

- `WasteLandForge/research/R006 Developer Experience and CLI Workflow Model for WastelandForge-deep-research-report.md`
- `WasteLandForge/research/WastelandForge Generator and Build Pipeline Architecture-deep-research-report.md`
- `WasteLandForge/research/WastelandForge Validation Testing CI Release and Governance-deep-research-report.md`
- `WasteLandForge/skills/forge/SKILL.md`
- `WasteLandForge/skills/wastelandforge-build-cli-release/SKILL.md`
- `WasteLandForge/skills/wastelandforge-validation-release-governance/SKILL.md`
- `WasteLandForge/skills/wastelandforge-implementation-planning/SKILL.md`

## Canonical slash surface

Support only the ADR-010/R006 command names:

```text
/forge init
/forge validate
/forge capabilities list
/forge capabilities scan
/forge capabilities explain
/forge generate
/forge build
/forge package
/forge release verify
/forge release prepare
/forge release publish
/forge docs
/forge graph
/forge explain
/forge clean
/forge doctor export
/forge help
/forge --version
```

Do not introduce convenience aliases that spend future namespace, such as `/forge scan`.

## Dispatch behavior

For each command, decide whether the work is:

- command design,
- repository implementation,
- CLI skeleton implementation,
- validation/generation execution,
- release/governance review,
- help/explanation.

If the actual `forge` CLI exists, prefer the real command for safe read-only or explicitly requested operations. If it does not exist, implement the requested v0.1 command slice or produce a research-backed plan when the user asks for planning.

Gate 45 adds option-level dispatch for
`/forge validate --geck-dialogue-export <path>`. Route it to the real
`forge validate --geck-dialogue-export <path>` CLI behavior when available.
It validates a user-saved GECK dialogue export text file only; do not control,
automate, import into, or mutate an open GECK session.

Gate 101 implements existing package evidence verification through
`/forge package --target mcm-json --verify-existing`. Route it to the real CLI
behavior when available; do not invent `/forge verify-package`,
`/forge package verify`, or other verifier aliases. The current MCM JSON
command surface supports `--format sarif` and `--format github` for package
evidence diagnostics, supports `--summary <path>` for Markdown diagnostic
summaries, revalidates `checksums.sha256`, revalidates
`build-manifest.json`, revalidates `install-preview.md`, and still includes
package archive entry-name revalidation
coverage on top of package archive digest verification,
package-verification human summary evidence, package-verification report
schema validation, package-verification report evidence, install-preview human
summary evidence, install-preview report schema validation, package
manifest schema validation, package ZIP entry checks, canonical
`/forge package --target mcm-json` execution, the loose-file
`package-manifest.json`, build-time `package.zip`, referenced MCM texture
asset staging, image asset validation, image output, header, keybind,
checkbox, string-toggle, runtime requirements pass-through, and
translation-file output for `/forge generate --target mcm-json`,
`/forge build --target mcm-json`, and `/forge package --target mcm-json`.
Route them to the real CLI behavior when available and describe the output as
the Gate 101 MCM Extender JSON subset under `MCM/<menu>.json`, plus
`MCM/Translations/<modName>.ini` when translations are declared, and staged
referenced texture assets under their game-relative target paths. Build and
package output also include `package-manifest.json`, `package.zip`,
`install-preview.json`, `install-preview.md`, `package-verification.json`,
`package-verification.md`, `build-manifest.json`, and `checksums.sha256`
under `dist/mcm-json`.
`package-manifest.json` is validated against `package-manifest/0.1.0`, and
build/package ZIP entries are checked against the deterministic package
payload. `install-preview.json` is validated against
`install-preview/0.1.0`; `install-preview.md` is a human-readable summary of
the same preview intent; `package-verification.json` summarizes local package
evidence, package counts, and archive validation status and is validated
against `package-verification/0.1.0`; `package-verification.md` is a
human-readable summary of that local package verification evidence.
Package-verification evidence is cross-checked against package manifest,
install-preview, payload digest, optional archive, and summary evidence before
final local manifests and checksums are written. Successful runs record
`packageVerification.crossChecks`; mismatches are blocking `WF-BUILD-006`
diagnostics through reusable validator, file-based verifier, payload digest
verification, archive digest verification, and archive entry-name
revalidation code, plus checksum-file revalidation in verify-existing mode.
Verify-existing mode also revalidates build-manifest content against package
evidence and output digests, and revalidates install-preview summary content
against install-preview JSON. Gate 94 also cross-checks install-preview entry
content against package-manifest entry content. Gate 95 also revalidates
package-verification Markdown summary content against package-verification
JSON. Gate 96 also revalidates package-verification JSON check content
against package evidence. Gate 97 also revalidates package-verification JSON
metadata content against package evidence. Gate 98 also revalidates
package-verification archive detail content against package evidence. Gate 99
also revalidates install-preview archive detail content against package
evidence. Gate 100 also revalidates package-manifest archive detail content
against package evidence. Gate 101 also revalidates archive detail
cross-report consistency against package evidence. These remain reports only: they
list Data-relative would-copy paths and do not install into Data or MO2. It
supports header,
image, toggle, keybind, checkbox, string-toggle, slider, choice, and text
settings. MCM image filenames are validated against required texture asset
targets and existing DDS source-file checks. Do not claim callbacks,
multi-slider, color picker, FOMOD package creation, capability-derived runtime
requirements, MO2 installation, or in-game verification exist until later
gates implement them.

## Required output

Return:

- command recognized,
- research used,
- documented decisions,
- implementation action or next command slice,
- validation and safety gates,
- open questions.

## Review checks

Reject:

- non-canonical slash command names,
- commands that require network access in the correctness path,
- validation that rewrites source files,
- generated outputs treated as source truth,
- public fixtures using Bethesda assets or third-party mod files without permission,
- release publishing without explicit approval,
- clean behavior outside generated/dist trees without explicit authorization.
