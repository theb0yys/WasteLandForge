# Gate 411 - Dialogue Voice Work-Item Authoring Contract

Status: Complete
Phase: v0.1 product-value definition
Decision base: ADR-004, ADR-007, ADR-009, Gates 17-18, Gates 399-410

## Goal

Define source-backed authoring of a dialogue line voice work item and its
required complete WAV/OGG/LIP asset mapping. Forge records and validates the
work package; it does not record, convert, export, calculate lip data, launch
GECK, or mutate plugin records.

## Research classification

- **Documented:** Dialogue production is coupled to quest/plugin records and
  GECK owns dialogue export and lip calculation.
- **Documented:** Forge owns voice manifests/worklists and deterministic checks
  for WAV/OGG pairs, LIP prerequisites, and voice-folder mappings.
- **Documented:** Dialogue schema `0.23.0` requires `plugin`, `voiceType`, and
  `fileStem` for a voice work item and maps it to
  `sound/voice/<Plugin>/<VoiceType>/<FileStem>`.
- **Documented:** Asset schema `0.1.0` models project-relative source and
  game-Data-relative target paths for `voice` and `lip` assets.
- **Documented:** `WF-SEM-015`, `WF-ASSET-008`, and `WF-ASSET-009` require a
  complete matching `.wav`, `.ogg`, and `.lip` declaration set.
- **Documented:** Gate 400 emits validated voice rows and an unresolved
  `voice-export` action while keeping GECK as record/export authority.
- **Inferred:** The first authoring workflow must add or bind the complete trio
  atomically; partial metadata would intentionally create invalid canonical
  source and is therefore not a supported intermediate state.
- **Open:** Audio metadata policy, recording workflow, codec conversion,
  dialogue export, LIP generation, voice-type discovery from plugins, and
  runtime playback remain later integration work.

## Supported source shape

The workflow supports canonical JSON dialogue `0.23.0` at:

```text
src/registries/dialogue/main.json
```

Asset source is either:

```text
src/registries/assets/main.json  (asset 0.1.0)
```

or absent. When absent, the manifest must not declare a conflicting asset path;
the workflow may create the canonical asset registry and add
`registries.assets = src/registries/assets/` transactionally.

Alternate paths, YAML, multiple documents, unsupported versions, or a failed
canonical project validation are refused rather than migrated.

## Desktop workflow

Narrative Author gains an `Add Voice Work Item` section with:

- a source-backed selector containing dialogue lines without `voice`;
- mode control: `Bind declared trio` or `Declare project files`.

### Bind declared trio

Forge scans the asset registry for complete, case-insensitively matched target
stems containing exactly `.wav`, `.ogg`, and `.lip`. The user selects one stem;
`plugin`, `voiceType`, and `fileStem` are parsed from the validated target path
and cannot be edited independently.

### Declare project files

The user explicitly enters:

- plugin filename ending in `.esm` or `.esp` with no separator;
- voice-type path segment with no separator;
- file stem with no separator or dot;
- project-relative WAV, OGG, and LIP source paths.

All three source files must already exist under the selected project root and
pass existing source-path, extension, signature, voice-pair, and LIP checks.
This workflow declares files; it does not copy, rewrite, convert, or generate
them.

The user performs load, preview, review, `Append, Validate and Rebuild
Handoff`, then reviews validation and voice-worklist evidence.

## Derived canonical data

The selected dialogue line receives:

```json
"voice": {
  "plugin": "<PluginName.esm|esp>",
  "voiceType": "<VoiceType>",
  "fileStem": "<FileStem>"
}
```

Declare-files mode appends three asset entries with IDs derived from the line:

```text
<lineId>.voice.wav
<lineId>.voice.ogg
<lineId>.voice.lip
```

Targets are derived, never user-entered:

```text
sound/voice/<Plugin>/<VoiceType>/<FileStem>.wav
sound/voice/<Plugin>/<VoiceType>/<FileStem>.ogg
sound/voice/<Plugin>/<VoiceType>/<FileStem>.lip
```

WAV/OGG use `assetType: voice`; LIP uses `assetType: lip`; all are explicitly
`required: true`. Source paths are normalized to forward-slash project-relative
paths but source bytes are never modified.

## Preservation and transaction

- Parse and deep-clone manifest, dialogue, and existing/new asset roots with
  structured JSON APIs.
- Modify only the selected line's absent `voice` property, append three assets
  only in declare-files mode, and add the manifest asset declaration only when
  creating the canonical asset registry.
- Preserve all existing nodes and ordering by deep equality.
- Preview/refusals write zero bytes and show exact proposed documents/paths.
- Token binds mode, selected line/stem, normalized inputs, manifest/dialogue/
  asset bytes or absence, source-file paths plus SHA-256 values, and proposed
  JSON bytes.
- Append retains every original byte stream, writes the complete proposal, and
  runs canonical validation.
- Any write or validation failure restores original manifest/dialogue/asset
  bytes and removes only a newly created asset file/directory when safe.
- Successful validation rebuilds `forge package . --target geck-handoff`.
- Package failure keeps valid canonical source and remains retryable.

## Refusals

- Selected line already has voice metadata or no longer exists.
- Existing-trio mode has no complete validated trio or selection changed.
- Plugin, voice type, or file stem violates schema/path rules.
- Any source path is absolute, escapes the project, has the wrong extension,
  does not exist, duplicates another source, or fails existing signature checks.
- Duplicate/case-colliding asset ID or target path.
- Existing asset registry/manifest uses conflicting path, shape, or version.
- Stale token, source-file digest change, parse/write failure, or canonical
  validation failure.

## Safety boundaries

- No audio recording/editing, WAV/OGG conversion, LIP creation, audio metadata
  policy, GECK export, voice-type plugin discovery, executable script, plugin
  mutation, GECK/xEdit launch, game Data/MO2 write, game launch, network,
  release publication, or AI requirement.

## Gate 412 acceptance criteria

- Source-backed unvoiced-line and complete-trio choices load deterministically.
- Bind-existing and declare-files modes both preview with zero writes, append,
  validate, and rebuild the handoff.
- Declare-files mode creates/updates canonical asset registry and manifest only
  when required and preserves all pre-existing nodes.
- Handoff voice row contains exact plugin/type/stem, three target paths, source
  mappings, `validated-declaration`, and unresolved `voice-export` action.
- Partial trio, already voiced line, invalid/escaping/missing/wrong-signature
  source, duplicate target, stale token/digest, write failure, and validation
  rollback receive focused Windows coverage.
- Published desktop automation completes one declaration and proves repeated
  refusal preserves manifest/dialogue/asset hashes and audio bytes.
- Release build and all test suites pass.

## Next route

Gate 412: implement voice choice discovery, complete-trio preview/transaction,
Narrative Author controls, canonical validation/handoff rebuild, focused tests,
and published-app regression.
