# Gate 515 - Forge Init Empty-Directory Recovery Fix

Status: Complete
Phase: v0.1 observation-driven defect resolution
Decision base: Gate 514 and `docs/user-test-v0.1.md`

## Defect

Normal initialization could not establish the selected directory tree in the
reported environment. Pre-creating the empty project/scaffold directories did
not provide a recovery path because `forge init` treated those empty planned
directories as conflicts and returned exit code 6.

## Fix

- `forge init` now reuses existing empty planned scaffold directories and
  implicit ancestor directories such as `src/registries`.
- Existing planned files remain blocking.
- Any unplanned file or directory under a planned scaffold directory remains
  blocking and is preserved unchanged.
- No force/overwrite mode or manual-scaffold bypass was added.

This permits safe retry after a failed directory-creation attempt and permits a
user/parent process to establish writable empty directories before invoking the
bundled backend.

## Validation

- Focused init suite: 9 passed, including recovery and unplanned-content
  refusal with no mutation.
- The attempted full suite hung in the existing unit-test process and was
  terminated; it is not counted as passing evidence. The previous complete
  baseline remains 872 passing tests.
- App/backend publication and unsigned installer rebuild passed.
- Installed backend reused pre-created empty scaffold directories, completed
  initialization, and validated the resulting project.
- The complete installed regression, uninstall, and cleanup passed.
- Installer SHA-256:
  `8109006D08CD5D3E827BAD93A274CECB4A91DBED450B786AE5A290330EA67BF5`.

## Unchanged open evidence

- Exact GECK Editor IDs, FormIDs, coordinates, and container base records remain
  unresolved until local game/GECK evidence is supplied. This fix does not
  invent those values.

## Next route

Resume user testing with the rebuilt installer. Gate 516 opens only for another
reproducible defect or an explicitly authorized deferred compatibility smoke.
