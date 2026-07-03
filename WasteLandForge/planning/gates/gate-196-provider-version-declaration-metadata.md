# Gate 196 - Provider-Version Declaration Metadata

Status: Complete

## Purpose

Start provider-version value cautiously by adding declaration-only version
metadata to built-in provider definitions and surfacing that metadata through
`forge capabilities list` and `forge capabilities explain`.

## Research grounding

- Documented: R005/ADR-008 says providers are versioned registry data that
  satisfy capabilities, while projects depend on capabilities rather than
  provider names.
- Documented: R005 says detection should remain local-first and deterministic,
  and runtime probes should enrich results rather than define correctness.
- Documented: R006/ADR-010 defines `forge capabilities list` and
  `forge capabilities explain` as canonical offline-first CLI commands.
- Documented: R008/ADR-011 requires deterministic fixture-backed tests and an
  offline-first, AI-optional correctness path.
- Inferred: Provider-version declaration metadata can safely appear in
  catalogue and explanation output before local provider version parsing exists
  if it is labelled as declaration-only and not used for resolution.
- Open: Local provider version parsing, runtime confirmation, provider-version
  constraints, unsupported-version diagnostics, MO2 effective visibility, JIP
  PP LN catalogue policy, and GECK Extender mixed-scope policy remain
  unresolved.

## Implemented

- Added `ProviderVersionDeclaration` metadata to provider definitions.
- Populated the built-in FNV provider catalogue with declaration-only provider
  version metadata.
- `forge capabilities list` JSON provider entries now include `version`
  metadata with:
  - `scheme`
  - `source`
  - `status`
  - `localVersionStatus`
  - `resolutionStatus`
  - `notes`
- `forge capabilities list` plain output now shows the same compact version
  declaration.
- `forge capabilities explain` JSON provider and evidence-group entries now
  include the same provider-version metadata.
- `forge capabilities explain` plain output and Markdown summaries now show
  provider-version declaration metadata for provider evidence groups.
- Golden tests cover list JSON/plain, explain JSON/plain, and explain Markdown
  projection of declaration-only provider-version metadata.

## Not implemented

- No local provider version parser.
- No runtime probe.
- No version constraint evaluation.
- No unsupported-version diagnostic.
- No resolver behavior change.
- No Doctor planner behavior change.
- No capability scan provider output change.
- No Doctor export provider index change.
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
| Built-in providers carry declaration-only version metadata | Complete |
| `capabilities list` JSON exposes provider-version metadata | Complete |
| `capabilities list` plain output exposes provider-version metadata | Complete |
| `capabilities explain` JSON exposes provider-version metadata | Complete |
| `capabilities explain` plain/Markdown output exposes provider-version metadata | Complete |
| Resolver still does not evaluate provider versions | Complete |

## Validation

- `dotnet build --no-restore` passed with 0 warnings and 0 errors.
- `dotnet test tests\WastelandForge.GoldenTests\WastelandForge.GoldenTests.csproj --no-build --no-restore --filter "FullyQualifiedName~CapabilitiesList|FullyQualifiedName~CapabilitiesExplain"` passed.
- `dotnet test WastelandForge.sln --no-build --no-restore -m:1` passed.
- CLI smoke confirmed `capabilities list --kind providers --format json`
  exposes declaration-only xNVSE provider-version metadata and `capabilities
  explain provider.runtime.xnvse --format plain` renders the compact version
  declaration line.

## Next gate

Gate 197 should propagate declaration-only provider-version metadata into
`forge capabilities scan` provider output and Doctor provider/bundle indexes,
without parsing local provider versions, changing resolution, adding runtime
probes, adding unsupported-version diagnostics, MO2/GECK automation, or
deciding unresolved catalogue policy questions.
