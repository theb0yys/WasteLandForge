# Gate 197 - Provider-Version Declarations in Scan and Doctor Indexes

Status: Complete

## Purpose

Propagate the declaration-only provider-version metadata introduced in Gate
196 into `forge capabilities scan` provider output and Doctor provider bundle
indexes.

## Research grounding

- Documented: R005/ADR-008 says providers are versioned registry data that
  satisfy capabilities, while projects depend on capabilities rather than
  provider names.
- Documented: R005 says detection remains local-first and deterministic, with
  runtime probes enriching results rather than defining correctness.
- Documented: R006/ADR-010 defines `forge capabilities scan` and
  `forge doctor export` as canonical offline-first CLI commands.
- Documented: R008/ADR-011 requires deterministic fixture-backed tests and an
  offline-first, AI-optional correctness path.
- Inferred: The same declaration-only provider-version object can be surfaced
  in scan and Doctor provider indexes if it is labelled as catalogue metadata
  and not used for local detection or capability resolution.
- Open: Local provider version parsing, runtime confirmation, version
  constraints, unsupported-version diagnostics, MO2 effective visibility, JIP
  PP LN catalogue policy, and GECK Extender mixed-scope policy remain
  unresolved.

## Implemented

- Added a shared CLI projection helper for provider-version JSON and compact
  text rendering.
- `forge capabilities scan --format json` provider entries now include
  declaration-only `version` metadata.
- `forge capabilities scan --format plain` provider sections now include the
  compact provider-version declaration line.
- `forge capabilities scan --summary <path>` Markdown now includes a provider
  table with compact provider-version declarations and no local paths.
- `forge doctor export --format json` carries the updated scan provider
  metadata through its embedded capability scan payload.
- Doctor bundle `providers/index.json` entries now include declaration-only
  provider-version metadata.
- Doctor bundle `providers/index.md` now includes a provider-version column.
- Golden tests cover scan JSON/plain/Markdown projection and Doctor
  provider-index JSON/Markdown projection.

## Not implemented

- No local provider version parser.
- No runtime probe.
- No version constraint evaluation.
- No unsupported-version diagnostic.
- No resolver behavior change.
- No Doctor planner behavior change.
- No requirement-resolution provider evidence version field.
- No MO2 VFS inspection.
- No GECK automation.
- No network check.
- No catalogue-policy decision.
- No command alias.
- No new output format.
- No AI behavior.

## Exit evidence

| Evidence | Status |
|---|---|
| `capabilities scan` JSON provider entries expose declaration-only version metadata | Complete |
| `capabilities scan` plain output exposes compact provider-version metadata | Complete |
| `capabilities scan` Markdown summary exposes compact provider-version metadata | Complete |
| Doctor embedded scan JSON carries provider-version metadata | Complete |
| Doctor bundle provider JSON/Markdown indexes expose provider-version metadata | Complete |
| Resolver still does not evaluate provider versions | Complete |

## Validation

- `dotnet build --no-restore` passed with 0 warnings and 0 errors.
- `dotnet test tests\WastelandForge.GoldenTests\WastelandForge.GoldenTests.csproj --no-build --no-restore --filter "FullyQualifiedName~CapabilitiesScan|FullyQualifiedName~DoctorExport|FullyQualifiedName~CapabilitiesList|FullyQualifiedName~CapabilitiesExplain"` passed.
- `dotnet test WastelandForge.sln --no-build --no-restore -m:1` passed.

## Next gate

Gate 198 should perform a provider-version parser research checkpoint for the
first locally parseable providers, documenting authoritative version sources
and fixture requirements before adding parser code, runtime probes, resolver
changes, unsupported-version diagnostics, MO2/GECK automation, or catalogue
policy decisions.
