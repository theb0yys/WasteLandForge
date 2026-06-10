# Source

This directory holds WastelandForge implementation projects.

Gate 2 creates empty buildable project skeletons:

- `WastelandForge.Core`
- `WastelandForge.Schema`
- `WastelandForge.Registry`
- `WastelandForge.Validation`
- `WastelandForge.Generation`
- `WastelandForge.Provenance`
- `WastelandForge.Cli`

Gate 6 adds the first package-free CLI skeleton in `WastelandForge.Cli`.
Only `forge validate` performs real validation work at this stage; the rest of
the ADR-010 command surface is reserved with stable help and status output until
later gates implement each workflow.

Gate 10 and Gate 11 add diagnostic projections in `WastelandForge.Core`:
SARIF, Markdown summaries, and GitHub workflow-command annotations.

Gate 12 adds YAML source contract ingestion and runtime manifest JSON Schema
evaluation in `WastelandForge.Validation`, backed by embedded schema text from
`WastelandForge.Schema`.

Gate 13 adds dependency and capability registry JSON Schema evaluation through
the same validation path.

Gate 14 adds optional asset registry JSON Schema evaluation through the same
validation path.

Gate 15 adds asset path semantic validation in `WastelandForge.Validation`,
including source containment, required source existence, target traversal, and
basic asset-type target extension checks.

Gate 16 adds asset type-specific semantic validation in
`WastelandForge.Validation`, including minimal source signature checks for DDS,
WAV, OGG, NIF, and KF targets plus target-root convention checks by declared
asset type.

Gate 17 adds voice and dialogue asset validation in
`WastelandForge.Validation`, including voice/lip target shape checks plus
WAV/OGG/LIP pair checks by game-relative voice target stem.

Gate 18 adds dialogue registry runtime schema validation and dialogue voice
worklist semantic validation in `WastelandForge.Validation`, backed by the
embedded dialogue schema in `WastelandForge.Schema`.

Gate 19 adds quest registry runtime schema validation and dialogue `questId`
semantic validation in `WastelandForge.Validation`, backed by the embedded
quest schema in `WastelandForge.Schema`.

Gate 20 adds quest registry schema `0.2.0` runtime validation and quest
objective stage-reference semantic validation in `WastelandForge.Validation`,
while preserving quest registry schema `0.1.0` in `WastelandForge.Schema`.

Gate 21 adds quest registry schema `0.3.0` runtime validation and quest
transition stage-reference semantic validation in `WastelandForge.Validation`,
while preserving earlier quest registry schema versions in
`WastelandForge.Schema`.

Gate 22 adds quest registry schema `0.4.0` runtime validation and quest
condition stage-reference semantic validation in `WastelandForge.Validation`,
while preserving earlier quest registry schema versions in
`WastelandForge.Schema`.

Gate 23 adds quest registry schema `0.5.0` runtime validation and quest
result-script condition-reference semantic validation in
`WastelandForge.Validation`, while preserving earlier quest registry schema
versions in `WastelandForge.Schema`.

Gate 24 adds quest registry schema `0.6.0` runtime validation and quest
condition variable-reference semantic validation in
`WastelandForge.Validation`, while preserving earlier quest registry schema
versions in `WastelandForge.Schema`.

Gate 25 adds dialogue registry schema `0.2.0` runtime validation and dialogue
condition quest-state reference semantic validation in
`WastelandForge.Validation`, while preserving dialogue registry schema
`0.1.0` in `WastelandForge.Schema`.

Gate 26 adds dialogue registry schema `0.3.0` runtime validation for
line-local dialogue result-script skeletons in `WastelandForge.Validation`,
while preserving earlier dialogue registry schema versions in
`WastelandForge.Schema`.

Gate 27 adds dialogue registry schema `0.4.0` runtime validation for dialogue
topic and `linkTo` skeletons in `WastelandForge.Validation`. It adds semantic
topic-reference checks for line `topicId` values and link target topics while
preserving earlier dialogue registry schema versions in `WastelandForge.Schema`.

Gate 28 adds dialogue registry schema `0.5.0` runtime validation for
quest-level dialogue gate skeletons in `WastelandForge.Validation`. It adds
semantic checks for gate quest references plus gate condition stage and
variable references while preserving earlier dialogue registry schema versions
in `WastelandForge.Schema`.

Gate 29 adds dialogue registry schema `0.6.0` runtime validation for dialogue
result-script quest-variable mutation skeletons in `WastelandForge.Validation`.
It adds semantic checks for mutation variable references while preserving
earlier dialogue registry schema versions in `WastelandForge.Schema`.

Gate 30 adds dialogue registry schema `0.7.0` runtime validation for dialogue
Link From skeletons in `WastelandForge.Validation`. It adds semantic checks for
`linkFrom` source topic references while preserving earlier dialogue registry
schema versions in `WastelandForge.Schema`.

Gate 31 adds semantic dialogue link graph endpoint validation in
`WastelandForge.Validation`. It adds checks for declared `linkTo` target topics
and declared `linkFrom` source topics that have no authored dialogue line
endpoint, without adding a new schema version.

Gate 32 adds dialogue registry schema `0.8.0` runtime validation for dialogue
priority and prompt routing skeletons in `WastelandForge.Validation`. It adds
semantic duplicate prompt route validation for schema-valid dialogue documents.
