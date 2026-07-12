# Gate 454 - Human-Authored Plugin Artifact Intake Contract

Status: Complete
Phase: v0.1 implementation planning
Decision base: ADR-004, ADR-007, ADR-009, ADR-010, ADR-011, Gates 220-227, 382-390, 399-453

## Goal

Define the first supported path from a human-authored `.esp` or `.esm` saved by
GECK into a deterministic Forge project and combined mod package without Forge
parsing, generating, patching, or mutating plugin records.

## Research grounding

| Claim | Classification | Source |
|---|---|---|
| Binary plugin generation, patching, and record rewrites are deferred high-risk outputs. | Documented | Generator and Build Pipeline report |
| GECK and xEdit remain the human record-authoring and inspection authorities. | Documented | Asset Pipeline report / ADR-004 |
| Forge owns validation, staging, packaging, manifests, checksums, provenance, and workflow integration. | Documented | Asset Pipeline and Generator reports |
| xEdit integration should begin as audit/orchestration rather than a patching backend. | Documented | Generator and Build Pipeline report |
| A byte-for-byte project-owned plugin artifact can be packaged safely when identity, containment, digest, and review state are explicit. | Inferred | ADR-004, ADR-007, ADR-009 |
| Forge cannot prove plugin record correctness without a future record-aware parser or trusted external audit result. | Open | Current research boundary |

## Canonical source contract

Add immutable `plugin-artifact-registry/0.1.0` and optional manifest declaration:

```json
{
  "registries": {
    "pluginArtifacts": "src/registries/plugin-artifacts/"
  }
}
```

Each registry entry contains:

```text
id
file
pluginType: esp | esm
dataPath
sha256
length
authoringTool: geck | xedit | other
reviewStatus: pending | reviewed
reviewEvidence: optional project-relative path
```

Rules:

- `file` resolves inside the selected project and defaults to
  `src/plugins/<filename>` by scaffold convention.
- `dataPath` is exactly one Data-root filename with matching `.esp`/`.esm`
  extension; subdirectories, absolute paths, traversal, and case-only duplicate
  destinations are refused.
- Declared SHA-256, byte length, extension, and filename must match current
  bytes before validation or packaging succeeds.
- Forge treats the binary as opaque. Extension and digest validation do not
  claim TES4 structure, loadability, clean masters, record correctness, or
  conflict safety.
- `reviewed` requires an existing contained `reviewEvidence` file. Forge hashes
  that evidence but does not infer its contents unless it is a Forge-generated
  xEdit audit report with an already supported parser contract.

## Desktop import transaction

Add `Import Plugin Artifact` to the project workflow:

1. User chooses an existing `.esp` or `.esm` using a file picker.
2. Forge reads the file but never writes to its external location.
3. Preview shows source path, destination `src/plugins/<filename>`, exact size,
   SHA-256, plugin type, registry ID, and replacement/refusal state.
4. Apply copies bytes without transformation and writes/updates the plugin
   artifact registry in one rollback-capable transaction.
5. Post-copy digest verification and full project validation must pass or both
   copied bytes and registry changes are restored.

Initial v0.1 intake is create-only. Existing destination, registry ID, or Data
path collisions are refused rather than overwritten. External source remains
untouched on success and failure.

## xEdit review handoff

For each imported plugin, Forge exposes a non-executing review plan containing:

- plugin ID, project source path, Data path, SHA-256, and length;
- expected xEdit inspection target;
- existing `forge generate --target xedit-audit` workflow route;
- pending/reviewed state and contained evidence path when declared;
- explicit statement that xEdit was not launched and plugin validity is not
  established by intake.

No new top-level CLI verb or slash-command alias is introduced. Forge does not
copy the plugin into xEdit, MO2 overwrite, game Data, or a live profile.

## Combined package integration

`forge package <project> --target mod-package` includes validated plugin
artifacts as component `plugin-artifacts`:

- staged path: `staging/Data/<dataPath>`;
- archive entry: `<dataPath>`;
- kind: `plugin-artifact`;
- media type: `application/octet-stream`;
- source file, SHA-256, length, authoring tool, review status, and review
  evidence digest recorded in package/build manifests;
- collision checks shared with MCM/JIP destinations;
- deterministic ZIP ordering and timestamp policy unchanged.

Packaging may proceed with `reviewStatus: pending` for local iteration but must
emit a visible warning and `reviewRequired: true`. Release verification blocks
pending plugin review. This distinction prevents intake from falsely claiming
that opaque bytes are production-ready.

## Diagnostics

- `WF-SCHEMA-*`: registry shape or enum failure.
- `WF-ASSET-008`: plugin file missing, escaped, wrong extension, or Data path
  invalid.
- `WF-ASSET-009`: plugin digest/length mismatch.
- `WF-ASSET-010`: review evidence missing, escaped, or unreadable.
- Existing `WF-BUILD-009`/`WF-BUILD-010`: unsafe or colliding staged Data path.
- `WF-REL-*`: pending plugin review blocks release verification.

No diagnostic states that a plugin is valid merely because intake succeeded.

## Fixture and safety policy

- Public tests use synthetic opaque bytes with `.esp`/`.esm` names and clearly
  state that they are not Bethesda plugin content or valid game plugins.
- No Bethesda assets, game masters, third-party mods, or redistribution-unclear
  binaries enter fixtures or distributions.
- No external tool execution, plugin parsing/mutation, game Data/MO2 write,
  runtime probe, network operation, signing, release publication, or AI.

## Acceptance criteria for Gate 455

- Immutable schema/catalog registration and manifest registry support.
- Preview-gated desktop import copies exact bytes and creates validated source.
- Existing destination/ID/Data path, source drift, traversal, wrong extension,
  and digest mismatch are refused with rollback.
- Validation reports opaque identity and review state without validity claims.
- xEdit review plan is visible and non-executing.
- Combined package includes exact plugin bytes and complete provenance alongside
  existing MCM/JIP components with collision coverage.
- Pending review warns during package and blocks release verification; reviewed
  state requires contained evidence.
- Focused schema, semantic, generation, Windows transaction, golden CLI, full
  suite, and published-app regressions pass.

## Next route

Gate 455: implement the complete human-authored plugin artifact vertical slice,
including schema, validation, preview-gated desktop intake, xEdit review plan,
combined package integration, release-review gating, synthetic fixtures, tests,
and published-app regression.
