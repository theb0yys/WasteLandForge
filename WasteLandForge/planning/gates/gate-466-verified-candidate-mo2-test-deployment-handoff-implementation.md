# Gate 466 - Verified-Candidate MO2 Test-Deployment Handoff Implementation

Status: Complete
Phase: v0.1 desktop workflow implementation
Decision base: Gate 465, ADR-004, ADR-008, ADR-009, ADR-010, ADR-011

## Implemented behavior

- The Release Candidate workspace now exposes **Prepare MO2 Test Copy** only
  for a fresh `Candidate ready` projection.
- Preview and creation use the canonical `forge package --target mod-package`
  named-MO2 export path; the desktop does not copy payload files itself.
- Preview binds project root, source fingerprint, candidate run identity and
  state, backend version, all four required candidate evidence files, explicit
  MO2 root/name/destination, ordered entries with lengths and SHA-256 digests,
  and the complete disabled-side-effect declaration.
- Creation reruns the dry preview and compares the complete approval token
  before invoking the mutating backend command.
- Completed output is checked for direct-child containment, loose payload
  layout without nested `Data`, exact file lengths/digests, export manifest and
  checksum evidence, and absence of Forge temporary siblings.
- Input, source, evidence, backend, destination, entry, or safety drift blocks
  the approval.
- Existing destinations and all Gate 392 unsafe roots/names remain backend
  refusals.
- A successful package/export command may refresh package provenance bytes.
  The test copy remains verified, but the Release Candidate projection becomes
  stale and must be rerun before another handoff.
- The UI says explicitly that enabling and ordering the mod remains manual in
  MO2. Forge does not launch MO2 or the game.

## Automated evidence

- Full solution build passed.
- All six test projects passed: Unit 118, Schema 140, Semantic 87, Golden 304,
  Backwards compatibility 45, and Windows 96; 790 tests total.
- Focused Release Candidate tests passed, including exact preview binding and
  pre-mutation refusal after evidence drift.
- NuGet audit emitted only the known unavailable network-feed warning.

## Published and installed evidence

- Self-contained `win-x64` desktop publication and bundled backend verification
  passed.
- Inno Setup 6.7.3 built the unsigned local installer.
- Installer size: 48,513,217 bytes.
- Installer SHA-256:
  `7ec0d24c76d349e1ad964d139ff2ee9134aaa693dcfcedd30724e00ccff89427`.
- Isolated installed UI Automation reached `Candidate ready`, proved preview
  created no destination, created one direct-child synthetic MO2 mod, found no
  nested `Data` directory, found payload files, enabled **Open Test Copy**, and
  exposed manifest/checksum evidence.
- Silent uninstall and isolated install/settings/project/MO2-root cleanup
  completed.

## Boundaries

- No MO2 launch, profile mutation, mod enablement, priority, load order,
  plugin activation, VFS inspection, game Data write, game launch, overwrite,
  external tool execution, network correctness path, or AI behavior was added.
- No proprietary or redistribution-unclear files were used; all test data was
  synthetic and isolated.

## Next route

Gate 467: define an existing-candidate package reuse contract for named MO2
export so a verified test-copy handoff can preserve byte-identical candidate
package evidence instead of refreshing package provenance during creation.
