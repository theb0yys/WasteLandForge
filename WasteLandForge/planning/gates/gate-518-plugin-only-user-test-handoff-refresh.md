# Gate 518 - Plugin-Only User-Test Handoff Refresh

Status: Complete
Phase: v0.1 release-candidate freeze
Decision base: Gates 514-517 and `docs/user-test-v0.1.md`

## Goal

Correct the user-test handoff after the Gate 516 defect fix and Gate 517
publication closeout. This gate changes documentation only and does not reopen
v0.1 feature scope.

## Research grounding

| Claim | Classification | Source |
|---|---|---|
| Generic continuation cannot open a feature gate during the v0.1 freeze. | Documented | Gate 514 |
| The Gate 517 installer passed 875 tests and installed plugin-only GECK regression. | Documented | Gate 517 |
| The existing user-test document still names the superseded Gate 515 SHA-256. | Documented repository state | `docs/user-test-v0.1.md` before this gate |
| Plugin-only projects no longer require fabricated quest/dialogue registries. | Documented implementation evidence | Gates 516-517 |

## Changes

- Replaced the superseded installer SHA-256 with the Gate 517 artifact hash.
- Updated the human-authored plugin workflow to cover validated plugin-only
  projects and the explicit GECK handoff preview/launch sequence.
- Added plugin-only handoff support to the documented v0.1 capability list.
- Preserved every safety limitation and deferred real-tool compatibility item.

## Validation

- Recomputed installer SHA-256:
  `960A5EA96F8E93C95D957ECE0109EE095FB564233E22A0403EB1EFFD89F394F4`.
- Confirmed the documented path exists and points to the Gate 517 installer.
- No code, schema, test fixture, executable, plugin, external tool, or game Data
  file changed in this gate.

## Next route

Resume user testing with the Gate 517 installer and this corrected handoff.
Gate 519 opens only for a new reproducible defect or an explicitly authorized
deferred compatibility smoke. The v0.1 feature freeze remains active.

