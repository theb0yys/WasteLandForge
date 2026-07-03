# Schemas

This directory holds WastelandForge schema packages.

Gate 3 creates the first schema skeleton and immutable `$id` convention.

## Version Policy

Published schema IDs are immutable and include the full semantic version:

```text
https://schemas.wastelandforge.dev/fnv/<schema-kind>/<schema-version>/schema.json
```

Example:

```text
https://schemas.wastelandforge.dev/fnv/manifest/0.1.0/schema.json
```

Do not overwrite a released schema in place. Schema migrations are explicit tool work, not silent replacement.

## Current Schema Roots

- `manifest/0.1.0/schema.json` - root `wastelandforge.yaml` manifest schema.
- `manifest/0.2.0/schema.json` - manifest schema with optional MCM registry root.
- `dependencies/0.1.0/schema.json` - dependency registry schema.
- `dependencies/0.2.0/schema.json` - dependency registry schema with underscore-capable capability IDs.
- `capabilities/0.1.0/schema.json` - capability registry schema.
- `capabilities/0.2.0/schema.json` - capability registry schema with underscore-capable capability and provider IDs.
- `assets/0.1.0/schema.json` - asset registry schema.
- `mcm/0.1.0/schema.json` - MCM menu source registry schema.
- `jip-scripts/0.1.0/schema.json` - JIP LN text-script source registry schema.
- `jip-script-emission-manifest/0.1.0/schema.json` - generated JIP LN text-script emission evidence manifest schema.
- `mcm-extender-output/0.1.0/schema.json` - minimal MCM Extender runtime JSON output schema emitted by Forge.
- `package-manifest/0.1.0/schema.json` - deterministic generated package manifest schema for the first MCM Extender loose-file package target.
- `install-preview/0.1.0/schema.json` - deterministic generated install-preview report schema for the first MCM Extender loose-file package target.
- `install-plan/0.1.0/schema.json` - deterministic generated install-plan schema for the first MCM Extender loose-file package target.
- `package-verification/0.1.0/schema.json` - deterministic generated package-verification report schema for the first MCM Extender loose-file package target.
- `quests/0.1.0/schema.json` - quest registry schema.
- `quests/0.2.0/schema.json` - quest registry schema with stage and objective skeletons.
- `quests/0.3.0/schema.json` - quest registry schema with transition skeletons.
- `quests/0.4.0/schema.json` - quest registry schema with condition skeletons.
- `quests/0.5.0/schema.json` - quest registry schema with result-script skeletons.
- `quests/0.6.0/schema.json` - quest registry schema with variable skeletons.
- `dialogue/0.1.0/schema.json` - dialogue registry schema.
- `dialogue/0.2.0/schema.json` - dialogue registry schema with condition skeletons.
- `dialogue/0.3.0/schema.json` - dialogue registry schema with result-script skeletons.
- `dialogue/0.4.0/schema.json` - dialogue registry schema with topic/link skeletons.
- `dialogue/0.5.0/schema.json` - dialogue registry schema with quest-level gate skeletons.
- `dialogue/0.6.0/schema.json` - dialogue registry schema with result-script variable mutation skeletons.
- `dialogue/0.7.0/schema.json` - dialogue registry schema with Link From skeletons.
- `dialogue/0.8.0/schema.json` - dialogue registry schema with priority and prompt routing skeletons.
- `dialogue/0.9.0/schema.json` - dialogue registry schema with Speech Challenge skeletons.
- `dialogue/0.10.0/schema.json` - dialogue registry schema with skill gate skeletons.
- `dialogue/0.11.0/schema.json` - dialogue registry schema with perk gate skeletons.
- `dialogue/0.12.0/schema.json` - dialogue registry schema with faction and reputation gate skeletons.
- `dialogue/0.13.0/schema.json` - dialogue registry schema with identity gate skeletons.
- `dialogue/0.14.0/schema.json` - dialogue registry schema with local world flag gate skeletons.
- `dialogue/0.15.0/schema.json` - dialogue registry schema with event history gate skeletons.
- `dialogue/0.16.0/schema.json` - dialogue registry schema with companion state gate skeletons.
- `dialogue/0.17.0/schema.json` - dialogue registry schema with result-script side-effect gate skeletons.
- `dialogue/0.18.0/schema.json` - dialogue registry schema with condition boolean composition skeletons.
- `dialogue/0.19.0/schema.json` - dialogue registry schema with nested condition group skeletons.
- `dialogue/0.20.0/schema.json` - dialogue registry schema with condition negation skeletons.
- `dialogue/0.21.0/schema.json` - dialogue registry schema with condition precedence skeletons.
- `dialogue/0.22.0/schema.json` - dialogue registry schema with condition short-circuit skeletons.
- `dialogue/0.23.0/schema.json` - dialogue registry schema with response route skeletons.

Gate 12 evaluates the embedded manifest schema at runtime. Gate 13 evaluates
embedded dependency and capability registry schemas at runtime. Gate 14
evaluates embedded asset registry schemas at runtime. Gate 18 evaluates
embedded dialogue registry schemas at runtime. Gate 19 evaluates embedded quest
registry schemas at runtime. Gate 20 adds quest registry schema `0.2.0` while
preserving quest registry schema `0.1.0`. Gate 21 adds quest registry schema
`0.3.0` while preserving earlier quest registry schemas. Gate 22 adds quest
registry schema `0.4.0` while preserving earlier quest registry schemas. Gate
23 adds quest registry schema `0.5.0` while preserving earlier quest registry
schemas. Gate 24 adds quest registry schema `0.6.0` while preserving earlier
quest registry schemas. Gate 25 adds dialogue registry schema `0.2.0` while
preserving dialogue registry schema `0.1.0`. Gate 26 adds dialogue registry
schema `0.3.0` while preserving earlier dialogue registry schemas. Gate 27
adds dialogue registry schema `0.4.0` while preserving earlier dialogue
registry schemas. Gate 28 adds dialogue registry schema `0.5.0` while
preserving earlier dialogue registry schemas. Gate 29 adds dialogue registry
schema `0.6.0` while preserving earlier dialogue registry schemas. Gate 30
adds dialogue registry schema `0.7.0` while preserving earlier dialogue
registry schemas. Gate 31 adds no new schema; it preserves dialogue registry
schema `0.7.0` and adds semantic validation over the derived dialogue link
graph. Gate 32 adds dialogue registry schema `0.8.0` while preserving earlier
dialogue registry schemas. Gate 33 adds dialogue registry schema `0.9.0` while
preserving earlier dialogue registry schemas. Gate 34 adds dialogue registry
schema `0.10.0` while preserving earlier dialogue registry schemas. Gate 35
adds dialogue registry schema `0.11.0` while preserving earlier dialogue
registry schemas. Gate 36 adds dialogue registry schema `0.12.0` while
preserving earlier dialogue registry schemas. Gate 37 adds dialogue registry
schema `0.13.0` while preserving earlier dialogue registry schemas. Gate 38
adds dialogue registry schema `0.14.0` while preserving earlier dialogue
registry schemas. Gate 39 adds dialogue registry schema `0.15.0` while
preserving earlier dialogue registry schemas. Gate 40 adds dialogue registry
schema `0.16.0` while preserving earlier dialogue registry schemas. Gate 41
adds dialogue registry schema `0.17.0` while preserving earlier dialogue
registry schemas. Gate 42 adds dialogue registry schema `0.18.0` while
preserving earlier dialogue registry schemas. Gate 43 adds no new schema; it
preserves dialogue registry schema `0.18.0` and adds semantic validation over
schema-valid dialogue condition logic references. Gate 44 adds dialogue
registry schema `0.19.0` while preserving earlier dialogue registry schemas.
Gate 46 adds dialogue registry schema `0.20.0` while preserving earlier
dialogue registry schemas.
Gate 47 adds dialogue registry schema `0.21.0` while preserving earlier
dialogue registry schemas.
Gate 48 adds dialogue registry schema `0.22.0` while preserving earlier
dialogue registry schemas.
Gate 49 adds no new schema; it preserves dialogue registry schema `0.22.0`
and adds semantic validation over schema-valid dialogue condition logic IDs.
Gate 50 adds dialogue registry schema `0.23.0` while preserving earlier
dialogue registry schemas.
Gate 51 adds no new schema; it preserves dialogue registry schema `0.23.0`
and adds semantic validation over schema-valid response route target topics.

Gate 52 adds no new schema; it preserves dialogue registry schema `0.23.0`
and adds semantic validation over schema-valid response route target
endpoints.

Gate 53 adds no new schema; it preserves dialogue registry schema `0.23.0`
and adds semantic validation over schema-valid response route IDs.

Gate 54 adds no new schema; it preserves dialogue registry schema `0.23.0`
and adds semantic validation over schema-valid response route keys.

Gate 55 adds no new schema; it preserves dialogue registry schema `0.23.0`
and records that response route taxonomy remains open pending an evidence
pack.

Gate 56 adds no new schema; it creates a docs-only evidence pack skeleton and
continues to preserve dialogue registry schema `0.23.0`.

Gate 57 adds no new schema. The built-in capability catalogue is C# registry
data for `forge capabilities list`; local scan report schemas remain future
work.

Gate 58 adds no new schema. `forge capabilities scan` emits a CLI JSON report
from C# scan records; published scan report schemas remain future work.

Gate 59 adds no new schema. `forge capabilities explain` emits a CLI JSON
report from C# explanation records; published capability scan and explanation
report schemas remain future work.

Gate 60 adds no new schema. `forge capabilities scan --project` extends the
CLI JSON scan report with requirement resolution from C# records; published
capability scan and explanation report schemas remain future work.

Gate 61 adds no new published schema. `forge generate --target reports` and
`forge build --target reports` emit metadata report and manifest JSON from C#
records; published generation/build report schemas remain future work.

Gate 62 adds manifest schema `0.2.0`, dependency schema `0.2.0`, capability
schema `0.2.0`, and MCM registry schema `0.1.0`. The `0.2.0` dependency and
capability schemas preserve earlier `0.1.0` schemas and add underscore-capable
logical IDs for researched capability/provider IDs such as
`runtime.ui.mcm_json` and `provider.runtime.mcm_extender`.

Gate 63 adds `mcm-extender-output/0.1.0/schema.json` as a generated-output
schema, not a source registry schema. Forge uses it to validate the minimal
runtime-shaped MCM Extender JSON subset emitted by `forge generate --target
mcm-json` and `forge build --target mcm-json`.

Gate 64 extends the MCM source registry schema with optional MCM Extender
runtime `requirements` arrays and menu `translations` maps. It preserves the
same schema IDs because these schemas have not been published as immutable
packages yet in this gated implementation branch.

Gate 65 extends the same unpublished MCM source and MCM Extender output schema
surfaces with source `checkbox` and `stringToggle` settings, output option
types `5` and `6`, and optional string-toggle `textOn`/`textOff` labels.

Gate 66 extends the same unpublished schema surfaces with source `keybind`
settings and output option type `3`.

Gate 67 extends the same unpublished schema surfaces with source `header`
settings that emit output option type `0`.

Gate 68 extends the same unpublished schema surfaces with source `image`
settings and generated type `0` image maps. The schemas validate image map
shape; Gate 69 adds semantic validation that MCM image filenames resolve to
required texture asset targets and existing DDS source-file checks.

Gate 70 changes generated output staging and manifest evidence only. It does
not introduce a new schema ID or change the published source contract shape.

Gate 71 adds generated package-manifest metadata only. It does not introduce a
new source schema ID; a formal package-manifest schema remains future work.

Gate 72 adds ZIP archive output and archive digest evidence only. It does not
introduce a new source schema ID or a formal package/archive schema.

Gate 73 adds canonical `forge package --target mcm-json` execution only. It
does not introduce a new source schema ID or a formal package/archive schema.

Gate 74 adds `package-manifest/0.1.0/schema.json` as a generated package
evidence schema, not a source registry schema. Forge uses it to validate the
package manifest emitted by `forge generate --target mcm-json`, `forge build
--target mcm-json`, and `forge package --target mcm-json`.

Gate 75 adds generated `install-preview.json` output but no schema. Gate 76
adds `install-preview/0.1.0/schema.json` as generated install-preview
evidence schema, not a source registry schema. Forge uses it to validate the
install-preview report emitted by `forge generate --target mcm-json`, `forge
build --target mcm-json`, and `forge package --target mcm-json`.

Gate 116 adds `install-plan/0.1.0/schema.json` as generated install-plan
evidence schema, not a source registry schema. Forge uses it to validate the
install-ready export plan emitted by `forge generate --target mcm-json`,
`forge build --target mcm-json`, and `forge package --target mcm-json`.

Gate 78 adds generated `package-verification.json` output but no schema. Gate
79 adds `package-verification/0.1.0/schema.json` as generated
package-verification evidence schema, not a source registry schema. Forge uses
it to validate the package-verification report emitted by `forge generate
--target mcm-json`, `forge build --target mcm-json`, and `forge package
--target mcm-json`.

Gate 80 adds generated `package-verification.md` summary output but no schema.
The Markdown summary is human review evidence derived from the validated JSON
report path and local package evidence.

Gate 81 adds package-verification evidence cross-checks but no schema. The
checks compare generated package-verification evidence against existing
package manifest, install-preview, payload digest, archive, and summary
evidence before local manifests and checksums are finalized.

Gate 82 extracts those checks into reusable validator code but still adds no
schema. The package-verification evidence schema remains
`package-verification/0.1.0`.

Gate 83 adds a file-based verifier over existing generated evidence files but
still adds no schema. The package-verification evidence schema remains
`package-verification/0.1.0`.

Gate 84 adds payload digest recomputation to that verifier but still adds no
schema. The package-verification evidence schema remains
`package-verification/0.1.0`.

Gate 85 adds archive digest recomputation to that verifier but still adds no
schema. The package-verification evidence schema remains
`package-verification/0.1.0`.

Gate 86 adds archive entry-name revalidation to that verifier but still adds
no schema. The package-verification evidence schema remains
`package-verification/0.1.0`.

Gate 87 records the future package-verification command shape but still adds
no schema. The package-verification evidence schema remains
`package-verification/0.1.0`.

Gate 88 implements that package-verification command skeleton but still adds
no schema. The package-verification evidence schema remains
`package-verification/0.1.0`.

Gate 203 adds `jip-scripts/0.1.0/schema.json` as a source registry schema and
adds optional manifest `registries.jipScripts` loading. It records JIP LN
text-script intent only; generated script output schemas remain future work.

Gate 204 adds no schema. It validates schema-valid JIP source contracts
semantically and preserves `jip-scripts/0.1.0/schema.json`.

Gate 205 extends the unpublished `jip-scripts/0.1.0/schema.json` source
contract with required opaque body/source-line records. It does not add a
generated output schema.

Gate 206 adds no schema. It validates schema-valid JIP source lines against
the source-level byte budget declared in `sizePolicy.maxBytes`.

Gate 207 adds no schema. It validates schema-valid JIP `outputFile` values
for duplicate generated filename intent.

Gate 208 adds no schema. It derives non-emitting JIP generation plan metadata
from validated `jip-scripts/0.1.0` source contracts; generated output schemas
remain future work.

Gate 209 adds no schema. It renders validated `jip-scripts/0.1.0` source
lines in memory only; generated output schemas remain future work.

Gate 210 adds no schema. It writes rendered JIP text files under
`generated/jip-scripts` only; generated manifest or evidence schemas remain
future work.

Gate 211 adds no schema. It writes a generated JIP emission manifest and
checksum sidecar under `generated/jip-scripts`; the manifest schema and
revalidation rules remain future work.

Gate 212 adds `jip-script-emission-manifest/0.1.0/schema.json` as a generated
evidence schema, not a source registry schema. Forge uses it to validate the
JIP emission manifest before writing checksum evidence; checksum sidecar
revalidation remains future work.

Gate 213 adds no schema. It revalidates the generated JIP emission
`checksums.sha256` sidecar against `jip-script-emission-manifest/0.1.0`
evidence and local generated files.
