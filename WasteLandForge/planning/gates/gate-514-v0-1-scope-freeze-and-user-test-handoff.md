# Gate 514 - v0.1 Scope Freeze and User-Test Handoff

Status: Complete
Phase: v0.1 release-candidate freeze
Decision base: ADR-004, ADR-009, ADR-010, ADR-011 and Gates 488, 496, 503, 506-513

## Decision

Freeze v0.1 feature scope. No new generator, registry family, external-tool
adapter, or authoring workflow enters the user-test candidate until observed
testing identifies a blocking defect.

## Ready evidence

- Full solution baseline: 872 passed, zero failed, zero skipped.
- Basic Mod Builder and Plugin Mod Workbench installed vertical slices passed.
- Candidate, package, FOMOD, release preparation, revision reset, grouped
  navigation, minimum-size accessibility, uninstall, and isolated cleanup
  regressions passed.
- Current unsigned installer is documented in `docs/user-test-v0.1.md` with
  exact SHA-256, workflows, safety expectations, and defect-report fields.

## Explicitly deferred

- Gate 488: live Fallout: New Vegas MO2 compatibility; no suitable authorized
  FNV test instance was available.
- Gate 496: real upstream BSArch compatibility; no user-supplied provider was
  available.
- Gate 503: real xEdit Check Pascal compatibility; no configured/supplied
  FNV-capable xEdit executable was available.
- Runtime behavior of user-entered JIP source and generated MCM output remains
  dependent on the user's actual mod stack and in-game testing.
- Signing, timestamping, remote publication, and update channels are outside
  this local user-test candidate.

## Product boundary

The candidate can create and package useful text/configuration mods and
coordinate human-authored plugin mods. It does not create or understand plugin
records and does not replace GECK, xEdit, MO2, or the game runtime.

## Next route

Gate 515 is observation-driven defect triage only. It begins when the user
reports a reproducible problem from `docs/user-test-v0.1.md`, or supplies and
explicitly authorizes one deferred external compatibility environment. No
feature gate should be inferred from a generic continuation request while the
v0.1 scope freeze is active.
