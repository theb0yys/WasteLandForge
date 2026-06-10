# Quest Registry Schemas

Quest registry schemas validate source-controlled quest declarations for
WastelandForge projects.

Gate 19 adds `0.1.0/schema.json` as an immutable Draft 2020-12 public schema.
The schema is a skeleton: it captures stable quest identity and a small amount
of descriptive metadata so dialogue registries can validate `questId`
references. Stages, objectives, conditions, result scripts, lockouts, and
branching semantics remain later narrative gates.

Gate 20 adds `0.2.0/schema.json` as a new immutable Draft 2020-12 schema. It
adds minimal stage and objective declarations plus objective references to
start and completion stages. Stage transitions, conditions, result scripts,
quest variables, lockouts, fallback paths, and branch semantics remain later
narrative gates.

Gate 21 adds `0.3.0/schema.json` as a new immutable Draft 2020-12 schema. It
adds minimal transition declarations with optional `fromStageId` and required
`toStageId` references. Transition conditions, result scripts, lockouts,
fallback paths, and branch semantics remain later narrative gates.

Gate 22 adds `0.4.0/schema.json` as a new immutable Draft 2020-12 schema. It
adds minimal quest-local `stageDone` condition declarations with `stageId`
references. Full GECK condition language, condition evaluation, result scripts,
quest variables, lockouts, fallback paths, and branch semantics remain later
narrative gates.

Gate 23 adds `0.5.0/schema.json` as a new immutable Draft 2020-12 schema. It
adds minimal stage `resultScripts` declarations with `stageResult` type and an
optional `conditionId` reference to quest-local conditions. Raw script bodies,
side-effect language, result-script execution, quest variables, lockouts,
fallback paths, and branch semantics remain later narrative gates.

Gate 24 adds `0.6.0/schema.json` as a new immutable Draft 2020-12 schema. It
adds minimal quest-local integer `variables` declarations and `variableEquals`
condition skeletons. Variable mutation, non-integer variable typing, dialogue
condition compilation, result-script side effects, lockouts, fallback paths,
and branch semantics remain later narrative gates.
