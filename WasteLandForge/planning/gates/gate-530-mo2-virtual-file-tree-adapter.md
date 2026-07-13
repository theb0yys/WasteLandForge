# Gate 530 - MO2 Virtual-File-Tree Adapter

Status: Complete - repository-local adapter and deterministic package only
Phase: post-v0.1 provider discovery
Decision base: ADR-004, ADR-008, ADR-009, ADR-013, R005, and Gates 483-485 and
526-529

## Goal

Connect Gate 529's pure effective-provider evaluator to the documented MO2
Python read-only virtual-tree surface, verify the adapter with API-faithful
synthetic doubles, and refresh the separately installable deterministic
companion package without installing it or changing an MO2 environment.

## Evidence classification

- **Documented:** R005 and ADR-008 distinguish physical provider presence from
  effective profile visibility and identify MO2's virtual file tree as the
  supported effective-scope surface.
- **Documented:** the official MO2 Python API exposes
  `IOrganizer.virtualFileTree()`, `IFileTree.find()`,
  `IFileTree.walk(callback, sep)`, `FileTreeEntry.name()`/`isFile()`, and
  `IOrganizer.resolvePath()` for resolving a virtual Data-relative path to its
  winning physical file.
- **Documented:** the virtual tree represents the current MO2 session/profile;
  the API does not accept a profile argument.
- **Inferred:** fail-closed equality between the explicitly requested profile
  and `profileName()` is the smallest non-mutating way to bind inspection to an
  operator-selected profile.
- **Open:** compatibility with the user's installed MO2/Python versions and a
  real initialized FNV profile remains unproven because no live companion
  installation or profile inspection is authorized in this gate.

Official API evidence:

- `https://www.modorganizer.org/python-plugins-doc/autoapi/mobase/index.html`

## Implementation

`Mo2EffectiveProviderAdapter`:

1. requires a non-empty explicit profile already reported as current;
2. reads `virtualFileTree()` and finds the `NVSE/Plugins` subtree;
3. walks that subtree read-only and resolves each effective file through
   `resolvePath()`;
4. reads each winning physical file once and records its exact length and
   SHA-256 without exposing the physical path in the snapshot;
5. refuses missing subtrees, unresolved winners, reparse points, malformed
   paths, API errors, and instance/profile drift; and
6. delegates the completed snapshot to Gate 529's exact-provider evaluator.

The synthetic doubles implement the documented `find`, `walk`, `name`,
`isFile`, `isDir`, `virtualFileTree`, and `resolvePath` shapes. Regressions prove
successful winning-file hashing, one-read behavior, profile mismatch refusal,
unresolved-path refusal, extra native-provider refusal, and mid-inspection
profile drift refusal.

The Gate 485 package remains version `0.1.0`; this gate changes its source
payload but does not claim a new released version. The existing deterministic
builder regenerates checksums and build evidence with `liveMo2Executed=false`
and `mo2StateChanged=false`.

## Validation

- Passed all seven isolated Python companion tests, including API-shaped
  virtual-tree traversal, exact one-read winner hashing, current-profile
  binding, unresolved winner refusal, invalid digest refusal, extra native DLL
  refusal, and profile-drift refusal.
- Passed the Gate 485 deterministic package regression. Two isolated builds
  produced the same ZIP SHA-256:
  `527ffff36b936428b0eafdce680d709a0c2cf5f90a5e8d84cf36629c2cb12ccd`.
- Refreshed the retained ignored `0.1.0` companion package with that digest and
  build-manifest flags `liveMo2Executed=false` and `mo2StateChanged=false`.
- Passed the complete .NET solution suite: 892 tests across unit, schema,
  semantic, golden, Windows, and backwards-compatibility assemblies.
- NuGet vulnerability-feed checks emitted `NU1900` warnings because
  `https://api.nuget.org/v3/index.json` was unavailable; restore, build, and all
  tests still completed successfully from the available package state.
- The first focused Python run exposed a stale fixture read-log assertion. The
  test was corrected to clear setup reads before measuring adapter reads, and
  the complete focused suite then passed.
- No MO2/GECK/xEdit/game process, profile operation, staging copy, launch
  request, receipt, plugin read/write, or external-tool execution occurred.

## Boundary result

Gate 530 provides a packaged adapter for a future approved live preflight. It
does not install the companion, choose or modify a profile, stage the probe,
create a launch request, start MO2/GECK/xEdit, inspect a real virtual tree,
write game files, or authorize plugin record mutation.

## Next route

Gate 531: perform a separately approved, read-only compatibility preflight of
the packaged adapter in an already initialized FNV MO2 instance and already
selected probe profile. If those operator-controlled prerequisites remain
unavailable, record the live check as deferred. Keep probe staging, request
creation, GECK launch, and plugin mutation blocked behind later approvals.
