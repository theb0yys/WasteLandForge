# Gate 198 - Provider-Version Parser Research Checkpoint

Status: Complete

## Purpose

Decide the first safe provider-version parser direction before adding parser
code, runtime probes, resolver behavior, unsupported-version diagnostics,
MO2/GECK automation, or catalogue-policy decisions.

## Research grounding

- Documented: R005/ADR-008 says providers are versioned registry data,
  detection is local-first and deterministic, and runtime probes should enrich
  results rather than define the entire correctness path.
- Documented: R005 says provider-version parsing is not uniform enough for one
  parser. It identifies xNVSE semver-like release labels, `GetNVSEVersion`
  integer-style runtime values, `GetPluginVersion` plugin values, JIP LN
  decimal-style checks, and ShowOff scaled-integer checks as separate schemes.
- Documented: R006/ADR-010 keeps `forge capabilities scan` as the canonical
  command and keeps core workflows offline-first and AI-optional.
- Documented: R008/ADR-011 requires deterministic, redistributable fixtures
  and keeps real local installs in private extended test lanes.
- Documented: GECK Wiki documents `GetNVSEVersion`, `GetPluginVersion`, and
  `IsPluginInstalled` as runtime functions; `GetPluginVersion` uses registered
  plugin names rather than DLL filenames.
- Documented: xNVSE GitHub releases use release/archive labels such as
  `nvse_6_4_7.7z` and install into the Fallout New Vegas base folder.
- Inferred: First implementation should be a pure version parser contract with
  synthetic raw inputs before any runtime probe is added.
- Open: Whether PE file version metadata from local DLLs is authoritative
  enough for offline provider-version parsing remains unresolved.

## Source matrix

| Provider | Current marker | Authoritative version source for future parser | Scheme note | Gate 198 status |
|---|---|---|---|---|
| `provider.runtime.xnvse` | `nvse_loader.exe` in game root | Runtime `GetNVSEVersion`; release labels from xNVSE GitHub | Release labels are semver-like; runtime value is integer-style | Runtime-source documented; no offline parser selected |
| `provider.runtime.jip_ln` | `Data/NVSE/Plugins/jip_nvse.dll` | Runtime `GetPluginVersion "JIP NVSE Plugin"` | JIP is explicitly a `GetPluginVersion` exception where decimal values such as `57.30` are checked as-is | Eligible for parser-contract fixture, runtime probe later |
| `provider.runtime.showoff` | `Data/NVSE/Plugins/ShowOffNVSE.dll` | Runtime `GetPluginVersion "ShowOffNVSE Plugin"` | GECK documents scaled integer values, for example `1.80` as `180` | Eligible for parser-contract fixture, runtime probe later |
| `provider.runtime.johnnyguitar` | `Data/NVSE/Plugins/JohnnyGuitarNVSE.dll` | Runtime `GetPluginVersion "JohnnyGuitarNVSE"` | Registration name is documented; return scaling is not settled in project research | Defer parser semantics |
| `provider.runtime.knvse` | `Data/NVSE/Plugins/kNVSE.dll` | Runtime `GetPluginVersion "kNVSE"` | Registration name is documented; return scaling is not settled in project research | Defer parser semantics |
| `provider.runtime.mcm` | `Data/Menus/Prefabs/MCM` | Runtime `GetPluginVersion "MCM Extensions"` | Registration name is documented; return scaling is not settled in project research | Defer parser semantics |
| `provider.runtime.uio` | `Data/UIO/Public` | Runtime `GetPluginVersion "UI Organizer Plugin"` | Registration name is documented; return scaling is not settled in project research | Defer parser semantics |
| `provider.runtime.mcm_extender` | `Data/Menus/Prefabs/MCMExtender` | Open | GitHub source documents JSON MCM purpose, but no authoritative runtime version query is recorded here | Open |
| `provider.editor.geck_extender` | mixed root/Data/editor evidence | Not `GetPluginVersion`/`IsPluginInstalled` | GECK Wiki says `ZeGaryHax` cannot be used with those runtime functions | Blocked until separate editor-source evidence |
| `provider.tool.xedit`, `provider.tool.mo2`, `provider.editor.geck` | executable tools | Open | CLI/file metadata version source is not established by current research | Open |

## Decisions

- Gate 199 should add a pure provider-version parser contract skeleton using
  synthetic raw values only.
- Gate 199 may cover `semver`, `integer`, and `scaled-integer` normalization
  because those schemes already exist in schema contracts and R005.
- Gate 199 should preserve raw provider-version values so JIP-style decimal
  values can remain lossless until the project decides whether to add a
  dedicated decimal scheme or use `custom`.
- Runtime calls to `GetNVSEVersion`, `GetPluginVersion`, or
  `IsPluginInstalled` remain out of scope until a later runtime-probe gate.
- PE file-version parsing from local DLLs is out of scope until authoritative
  source evidence proves it is valid for a provider.
- Requirement resolution must continue to ignore provider-version declarations
  and parsed values until a later resolver gate.

## Not implemented

- No parser code.
- No provider-version scan evidence field.
- No runtime probe.
- No DLL or EXE file-version inspection.
- No version constraint evaluation.
- No unsupported-version diagnostic.
- No resolver behavior change.
- No Doctor planner behavior change.
- No MO2 VFS inspection.
- No GECK automation.
- No external tool execution.
- No committed third-party provider fixtures.
- No catalogue-policy decision.
- No command alias.
- No new output format.
- No AI behavior.

## Sources checked

- [GECK Wiki: GetNVSEVersion](https://geckwiki.com/index.php/GetNVSEVersion)
- [GECK Wiki: GetPluginVersion](https://geckwiki.com/index.php/GetPluginVersion)
- [GECK Wiki: IsPluginInstalled](https://geckwiki.com/index.php/IsPluginInstalled)
- [xNVSE GitHub releases](https://github.com/xNVSE/NVSE/releases/)
- [GECK Wiki: ShowOff NVSE functions](https://geckwiki.com/index.php?title=Category%3AFunctions_%28ShowOff_NVSE%29)
- [JohnnyGuitarNVSE GitHub repository](https://github.com/carxt/JohnnyGuitarNVSE)
- [GECK Wiki: kNVSE functions](https://geckwiki.com/index.php?title=Category%3AFunctions_%28kNVSE%29)
- [MCM Extender GitHub repository](https://github.com/Stentorious/MCMExtender)

## Validation

- Documentation-only gate.
- `dotnet build --no-restore` passed.
- `dotnet test WastelandForge.sln --no-build --no-restore -m:1` passed.
- `git diff --check` passed with Git line-ending normalization warnings only.

## Next gate

Gate 199 should add a provider-version parser contract skeleton for
`semver`, `integer`, and `scaled-integer` raw values with synthetic unit tests,
without runtime probes, local DLL/EXE metadata parsing, resolver changes,
unsupported-version diagnostics, MO2/GECK automation, or catalogue-policy
decisions.
