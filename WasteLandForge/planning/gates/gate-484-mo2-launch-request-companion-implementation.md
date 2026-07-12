# Gate 484 - MO2 Launch Request and Companion Implementation

Status: Complete
Phase: v0.1 implementation
Decision base: ADR-002, ADR-004, ADR-008, ADR-009, ADR-010, ADR-011 and Gate 483

## Goal

Implement the optional, user-mediated MO2 launch bridge defined by Gate 483
without making MO2 or Python a dependency of WastelandForge core.

## Research grounding

- **Documented:** R005 and ADR-008 require local-first capability behavior and
  identify MO2 profiles/VFS as environment-dependent state.
- **Documented:** Gate 483 pins the official MO2 plugin API evidence and defines
  the exact request, launch, handle-ownership, receipt, and safety contracts.
- **Inferred:** A private short-lived request plus optional in-process companion
  is the smallest supported profile-aware launch bridge.
- **Open:** Compatibility of the Python UI entrypoint with a user's exact MO2
  and Python plugin versions requires a future live installation test.

## Implemented

- The desktop can preview and create digest-bound, 15-minute GECK and xEdit MO2
  launch requests after the existing direct-launch approval gates.
- Request creation is atomic and create-only under the private LocalAppData
  request root. It does not launch MO2 or mutate project/generated evidence.
- The optional Python companion strictly validates request shape, duplicate
  keys, containment, expiry, tool bytes, empty arguments, safety flags,
  explicit profile selection, replay, drift, and invalid handles.
- The companion invokes only the Gate 483 `startApplication` signature, obtains
  the PID through an injected native adapter, closes the owned handle, and
  writes a process-created receipt.
- The desktop publication includes companion source under
  `Integrations/MO2/wastelandforge_bridge` while retaining no Python/MO2 runtime
  dependency.
- Installed desktop automation creates both request kinds and verifies that no
  receipt or MO2 execution occurs in the desktop request-generation step.

## Verification

- Full .NET test suite: 824 passed, 0 failed.
- Focused GECK/xEdit launch and request tests: 20 passed, 0 failed.
- Isolated Python companion tests: 3 passed, 0 failed using fake organizer,
  native-handle, and in-memory filesystem adapters.
- Python source syntax and installed-regression PowerShell syntax passed.
- Self-contained `win-x64` publication and unsigned Inno Setup installer build
  passed. The isolated installed regression created both request kinds, found
  the companion source, proved no receipt/MO2 execution, and cleaned its files.

## Boundaries

- No real MO2, GECK, xEdit, game, Bethesda asset, or third-party mod binary was
  executed by the automated companion tests.
- No profile, load order, executable registration, Overwrite, or mod state is
  changed.
- A process-created receipt is not editor readiness, VFS correctness, plugin
  review, save, game-runtime, or release evidence.
- The concrete Python filesystem adapter remains covered by source review; the
  current test host denied Python filesystem writes, so deterministic tests use
  the injected in-memory adapter.

## Next route

Gate 485: package the optional MO2 companion as a deterministic, separately
installable artifact with checksums and installation/removal guidance, then
perform a compatibility smoke only when a user-controlled MO2 test instance is
explicitly available. No live profile or load-order mutation is authorized.
