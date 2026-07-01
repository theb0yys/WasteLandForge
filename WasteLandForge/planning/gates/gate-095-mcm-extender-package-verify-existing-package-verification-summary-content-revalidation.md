# Gate 95 - MCM Extender Package Verify-Existing Package-Verification Summary Content Revalidation

Status: Complete

## Purpose

Gate 95 adds fuller package-verification Markdown summary content
revalidation for existing MCM Extender package evidence verification.

The command shape stays under the Gate 87 decision:

```text
forge package --target mcm-json --verify-existing
```

This gate extends the reusable package-verification evidence validator so
`package-verification.md` is checked against the deterministic
`package-verification.json` evidence it summarizes.

## Research Grounding

- Documented: ADR-009 requires generated artifacts to be deterministic,
  disposable, rebuildable, and traceable through local build evidence.
- Documented: ADR-010 defines the canonical command surface and rejects
  undocumented verifier aliases.
- Documented: ADR-011 requires layered validation and deterministic
  fixture-backed testing.
- Documented: Gate 80 defines `package-verification.md` as deterministic
  human-readable evidence beside the schema-validated
  `package-verification.json` report.
- Inferred: The summary can be fully revalidated in verify-existing mode
  because it is generated evidence and already participates in local
  manifests and checksums.
- Open: Actual Data/MO2 installation and runtime visibility remain separate
  safety/runtime gates.

## Scope

Gate 95 implements:

- header and provenance line checks for `package-verification.md`,
- project, command, target, package root, layout, count, result, and archive
  line checks,
- schema/check section line checks, including install-preview summary and
  package-archive lines,
- local-only limitation line checks,
- targeted tests for edited package-verification summary content,
- updated golden CLI coverage for package-verification summary diagnostics.

Gate 95 does not implement:

- standalone verifier commands or aliases,
- `forge package verify`,
- package regeneration in verify-existing mode,
- Data or MO2 installation,
- MO2 VFS/profile conflict inspection,
- game launch or runtime probe verification,
- FOMOD package creation,
- plugin record generation,
- new diagnostic rule families, rule IDs, or schemas.

## Validation Mapping

The command now:

1. Reads package manifest, install preview, install-preview summary, package
   verification, package-verification summary, checksum, and build-manifest
   evidence without regenerating package outputs.
2. Checks `package-verification.md` header and provenance lines.
3. Checks project, command, target, root, layout, entry, menu, translation,
   asset, result, archive, and archive validation lines against
   `package-verification.json` and computed archive evidence.
4. Checks the `## Checks` section against package evidence paths, summary
   evidence, payload digest count, archive state, and cross-check status.
5. Checks the `## Limitations` section against package-verification JSON.
6. Reports blocking `WF-BUILD-006` diagnostics when package-verification
   summary content is missing, inconsistent, or stale.
7. Preserves exit code `0` for no blocking diagnostics and `1` for blocking
   diagnostics.

## Validation Results

- `dotnet build --no-restore` passed with 0 warnings and 0 errors.
- `dotnet test tests/WastelandForge.UnitTests/WastelandForge.UnitTests.csproj --no-build --no-restore` passed 36 tests.
- `dotnet test tests/WastelandForge.GoldenTests/WastelandForge.GoldenTests.csproj --no-build --no-restore` passed 45 tests.
- `dotnet test WastelandForge.sln --no-build --no-restore -m:1` passed
  the full suite: 316 tests.

## Open Checks

| Check | Status | Notes |
|---|---|---|
| Edited package-verification summary count detection | Complete | Unit and golden tests cover stale `Menus` summary content. |
| Data/MO2 install verification | Open | Requires separate safety and environment model. |
| MO2 profile and VFS conflict inspection | Open | Requires later capability/runtime integration gates. |
| In-game MCM Extender visibility | Open | Requires runtime evidence and launch/probe design. |
| Package-verification JSON check content revalidation | Open | Candidate for Gate 96. |

## Next Gate

Gate 96 should add deeper package-verification JSON check content
revalidation for `forge package --target mcm-json --verify-existing`.
