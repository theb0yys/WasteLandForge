# Gate 534 - FNVEdit Verifier Compatibility Smoke Deferred

Status: Deferred - synthetic plugin and operator-controlled run evidence unavailable
Phase: post-v0.1 authoring verification
Decision base: ADR-004, ADR-013, R009, and Gates 503, 522, 523, 532, and 533

## Goal

Run the separately approved manual compatibility smoke for Gate 533's
generated read-only Pascal observer only when an explicitly supplied,
redistributable synthetic FNV plugin and exact operator-controlled evidence
destination are available. Otherwise, record the missing evidence without
launching FNVEdit or overstating compatibility.

## Evidence classification

- **Documented:** R009 assigns xEdit the post-save record-aware semantic
  verification role and keeps raw ESP/ESM writing outside Forge core.
- **Documented:** ADR-013 requires subsequent verifier gates with local
  evidence and does not authorize execution by itself.
- **Documented:** Gate 532 requires a human to run the generated observer
  manually against the exact selected plugin and requires a separate Gate 534
  approval for the run and raw-evidence destination.
- **Documented:** Gate 533 allows this smoke only after an explicitly supplied
  redistributable synthetic plugin and exact operator-controlled evidence path
  are available.
- **Previously observed:** Gate 523 identified an FNVEdit 4.1.5.0 provider, but
  Gate 534 has not received or revalidated its exact absolute path in the
  current task.
- **Observed:** the repository and fixture trees contain no tracked `.esp`,
  `.esm`, or `.esl` file that can serve as the required synthetic subject.
- **Open:** Pascal parsing, target-file enumeration, containing-cell traversal,
  raw-observation emission, and the complete manual handoff remain unverified
  against FNVEdit 4.1.5.

## Preflight result

The compatibility smoke cannot start. The current task does not identify:

1. an exact absolute FNVEdit executable path approved for launch;
2. an explicitly supplied synthetic plugin path and redistribution status;
3. the exact Gate 533 project/plan/observer bundle corresponding to that
   plugin;
4. an operator-controlled destination for the raw observations; or
5. approval of the exact executable, plugin digest, load scope, and evidence
   destination preview.

Selecting an arbitrary plugin, generating fake plugin bytes, using a normal
mod or game plugin, inventing an evidence path, installing the script, or
launching FNVEdit would violate the recorded evidence gate. Gate 534 is
therefore deferred rather than passed or failed.

## Resume conditions

Resume Gate 534 only after the operator supplies and approves all of:

1. the exact absolute path to the intended FNVEdit executable;
2. the exact path to a redistributable synthetic FNV plugin, plus confirmation
   that it may be used for this local smoke;
3. the project root whose current plan and observer bundle target that plugin;
4. the exact project-contained raw-observation destination;
5. a preview containing executable version/length/SHA-256, plugin
   length/SHA-256, plan SHA-256, observer SHA-256, selected load scope, and raw
   output path; and
6. explicit approval of that exact preview before launch.

The resumed smoke must remain read-only with respect to plugin bytes and must
capture pre/post plugin digests. It may write only the approved raw-observation
file. Forge must then seal that unchanged evidence and run the existing Gate
522 parser. Any compiler/runtime error or semantic mismatch is evidence of a
failed compatibility smoke, not permission to modify the plugin or silently
change the script contract.

## Validation

- Re-read R009, ADR-013, and Gates 503, 523, 532, and 533.
- Searched tracked repository and fixture paths for `.esp`, `.esm`, and `.esl`
  subjects; none were present.
- Confirmed the current task supplies no plugin path, redistribution statement,
  provider path, project selection, raw-evidence destination, or digest-bound
  launch preview.
- No production code, schema, fixture, generated bundle, or executable changed,
  so the Gate 533 build and 901-test suite were not rerun.

## Actions withheld

- No FNVEdit, xEdit, GECK, MO2, game, provider, or external process launch.
- No script generation, installation, copy, selection, or execution.
- No ESP/ESM/ESL creation, copy, parsing, mutation, save, or promotion.
- No game Data, load order, profile, tool directory, or external-state write.
- No raw observations or sealed verification report was produced.
- No FNVEdit 4.1.5 compatibility claim was made.

## Next route

Gate 534 remains resumable only under the conditions above. No Gate 535 or
authoring-execution route is selected automatically. Any next repository-local
value lane requires an explicit research/planning decision, and Gate 524 plus
ADR-013 continue to block authoring writer/provider execution.
