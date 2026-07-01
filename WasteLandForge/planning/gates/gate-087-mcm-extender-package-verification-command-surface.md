# Gate 87 - MCM Extender Package Verification Command Surface

Status: Complete

## Purpose

Gate 87 decides how the internal MCM Extender package-verification verifier
should become user-accessible without violating the ADR-010 command surface.

This is a decision checkpoint only. It does not add a new CLI flag, command,
alias, schema, generated artifact, installer behavior, MO2 inspection, runtime
probe, or plugin-record output.

## Research Grounding

- Documented: ADR-010 defines the canonical command surface and says not to
  introduce undocumented convenience aliases.
- Documented: ADR-009 says `forge package` assembles distributable staging
  trees and package evidence, while generated artifacts remain disposable and
  traceable through local manifests.
- Documented: ADR-011 requires layered validation, deterministic
  fixture-backed testing, local build manifests, and offline-first release
  evidence.
- Inferred: Existing generated package evidence verification belongs under
  the canonical `forge package` command because it validates a package staging
  tree and package evidence, not source contracts or release publication.
- Open: The exact JSON response envelope and whether package verification
  should later emit SARIF/GitHub diagnostics remain implementation questions
  for the command skeleton gate.

## Decision

Future user-accessible MCM package evidence verification must use the existing
canonical command surface:

```text
forge package --target mcm-json --verify-existing
```

The planned mode should:

- default to the existing package output root `dist/mcm-json`,
- accept `--output <path>` to select a generated package evidence root under
  project `dist/`,
- accept `--project <path>`,
- accept `--format human|plain|json` for the first public skeleton,
- read existing `package-manifest.json`, `install-preview.json`,
  `package-verification.json`, `package-verification.md`, and optional
  `package.zip` evidence from the selected package root,
- call `McmPackageVerificationEvidenceFileVerifier`,
- return exit code `0` when no blocking diagnostics are found,
- return exit code `1` when blocking package-verification diagnostics are
  found,
- return exit code `2` for parse or usage errors.

The planned mode must not:

- regenerate package outputs,
- install files into Data or MO2,
- inspect MO2 VFS/profile state,
- launch the game,
- claim in-game MCM Extender visibility,
- create FOMOD installers,
- generate plugin records.

## Rejected Shapes

| Shape | Decision | Reason |
|---|---|---|
| `forge verify-package` | Rejected | Adds a non-canonical top-level command. |
| `forge package verify` | Rejected for v0.1 | Adds a new subcommand under a command that ADR-010 currently names as a single verb. |
| `forge validate --package` | Rejected | Blurs source-contract validation with generated package evidence verification. |
| `forge release verify` | Rejected for this use | Release verification is broader release/readiness evidence, not package staging evidence only. |
| `/forge verify-package` | Rejected | Slash routing must mirror the future real CLI and must not invent aliases. |

## Scope

Gate 87 implements:

- command-surface decision documentation,
- planning index update,
- CLI and prompt documentation alignment,
- next-gate definition for the command skeleton.

Gate 87 does not implement:

- `--verify-existing`,
- a standalone verifier command,
- SARIF/GitHub output for package verification,
- Data or MO2 installation,
- MO2 VFS/profile conflict inspection,
- game launch or runtime probe verification,
- FOMOD package creation,
- plugin record generation.

## Validation Mapping

The next implementation gate can now wire the existing file-based verifier
without opening command-surface questions:

1. Parse `forge package --target mcm-json --verify-existing`.
2. Resolve the package evidence root under `dist/`.
3. Build a `McmPackageVerificationEvidenceFileVerificationRequest` from the
   selected root.
4. Run `McmPackageVerificationEvidenceFileVerifier`.
5. Render human/plain/json output and return the documented exit code.

## Validation Results

- `dotnet build --no-restore` passed with 0 warnings and 0 errors after an
  initial parallel build/test file-lock warning was rerun cleanly.
- `dotnet test tests/WastelandForge.GoldenTests/WastelandForge.GoldenTests.csproj --no-build --no-restore` passed 31 tests.
- `dotnet test WastelandForge.sln --no-build --no-restore -m:1` passed 295 tests.
- `git diff --check` passed with Git line-ending normalization warnings only.
- Targeted trailing-whitespace scan over changed files returned no matches.
- Stale current-Gate-86 wording scan returned no matches.
- Protected-file scan for Tales from the Age of Men / Age of Men / overhaul
  terms returned no matches outside ignored build output trees.

## Open Checks

| Check | Status | Notes |
|---|---|---|
| Command surface decision | Complete | Use `forge package --target mcm-json --verify-existing`. |
| Standalone package verifier command | Rejected | Would violate the canonical command surface. |
| Public command skeleton | Open | Candidate for Gate 88. |
| SARIF/GitHub package diagnostics | Open | Defer until the JSON/human skeleton is stable. |
| Data/MO2 install verification | Open | Requires separate safety and environment model. |
| In-game MCM Extender visibility | Open | Requires runtime evidence and launch/probe design. |

## Next Gate

Gate 88 should add the `forge package --target mcm-json --verify-existing`
command skeleton using the existing file-based verifier.
