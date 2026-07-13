# Gate 505 - Guided Basic Mod Builder Contract

Status: Complete
Phase: v0.1 implementation planning
Decision base: R006, ADR-004, ADR-007, ADR-009, ADR-010, ADR-011 and Gates 386-391, 471-474, 504

## Goal

Define one desktop workflow that creates a new Forge project, authors a useful
starter MCM configuration with an optional inert JIP LN startup script,
validates the canonical source, builds a deterministic combined package and
FOMOD distributable, and exposes the exact result without requiring users to
coordinate five specialist workspaces.

Gate 505 changes no runtime behavior. Gate 506 implements this contract.

## Research grounding

| Claim | Classification | Source |
|---|---|---|
| Forge owns source scaffolding, generation, validation, packaging, release automation, and workflow integration. | Documented | ADR-004 / R006 |
| Canonical source is versioned registry data validated before generation. | Documented | ADR-007 |
| Generated outputs are disposable, deterministic, and provenance-bearing. | Documented | ADR-009 |
| The user workflow must remain offline-first and use the stable command surface. | Documented | ADR-010 |
| `fnv-framework` already creates valid combined MCM/JIP source and packages without repair. | Documented | Gates 389-391 |
| Combined package and fixed-profile FOMOD production are implemented and installed-tested. | Documented | Gates 386-388, 471-474 |
| Joining those services into one guided transaction is the highest current usability value. | Inferred | Gate 504 |

## Product decision

Add a first-class **Basic Mod Builder** desktop workspace. It is the primary
new-project path for a testable text/configuration mod, while the existing New
Project and specialist authoring tabs remain available for advanced use.

This is a real authoring workflow, not a sample loader or command dashboard.
Success leaves a user-owned source project and verified FOMOD ZIP at the chosen
destination.

## Supported starter modes

Gate 506 supports exactly:

1. **MCM configuration**: one MCM menu, one General page, and one INI-backed
   boolean toggle.
2. **MCM plus JIP startup**: the same MCM content plus one `gr_` JIP LN Script
   Runner text script.

The JIP body defaults to a comment-only inert line. User-entered script text is
accepted as opaque source under the existing JIP schema and line/size limits;
Forge does not interpret or certify game-script semantics.

No docs-only, quest, dialogue, voice, plugin, BSA, or release-publication mode
is folded into this first workflow.

## Required inputs

- parent folder selected by the user;
- project/mod name;
- MCM menu title;
- setting label;
- INI section and key;
- enabled-by-default boolean;
- starter mode;
- for JIP mode: script summary and optional opaque body.

IDs, safe slugs, output filenames, INI filename, registry paths, and script
filename derive deterministically from the canonical project ID. They are shown
in preview but are not free-form path inputs.

## Preview contract

Preview is mandatory and write-free. It shows:

- exact destination and starter mode;
- canonical project ID and derived output names;
- source/configuration files to be created;
- ordered pipeline stages;
- expected Data-relative payload entries;
- capability declarations for MCM Extender and, when selected, JIP Script
  Runner/xNVSE;
- explicit statements that no plugin or game installation is produced.

The create token binds every normalized input plus destination state. Any input
change, destination creation, or relevant parent/destination drift invalidates
the token and requires a new preview.

## Transaction and filesystem contract

1. Require an absolute destination whose final project directory does not
   exist and whose existing parent is a regular non-reparse directory.
2. Create a uniquely named sibling work directory under that parent.
3. Invoke the existing `fnv-framework` scaffold service against the work root.
4. Replace only the scaffold-owned starter MCM fields with normalized user
   values.
5. For MCM-only mode, remove the scaffold-owned JIP registry/declaration and
   JIP capability requirement through a typed JSON transformation. For JIP
   mode, replace only the scaffold-owned starter script fields.
6. Run the existing validation pipeline.
7. Build `mod-package`, then `fomod`, using the existing backend command
   services and validators.
8. Revalidate the final source and verify expected FOMOD evidence and archive.
9. Atomically rename the complete work directory to the requested destination.

On any failure before promotion, delete only the transaction-owned sibling work
directory. Never delete or overwrite the requested destination, existing user
files, prior projects, or unrelated generated output. A cancellation follows
the same cleanup rule.

## Ordered result and UI contract

The workspace uses one compact form and a fixed stage list:

```text
Preview
Create source
Validate
Build combined payload
Build and verify FOMOD
Promote project
```

Only one stage runs at a time. Completed, active, blocked, and not-run states
remain visible. The final state shows:

- project root;
- FOMOD ZIP path, length, and SHA-256;
- payload entry count;
- source mode and required capabilities;
- buttons to open the project folder, source registries, distributable folder,
  and FOMOD ZIP;
- exact blocking diagnostic and stage when unsuccessful.

The UI must stay responsive and support cancellation between backend stages.
It must not automatically open folders or launch external applications.

## Service composition boundary

Gate 506 may add a desktop orchestration service and typed preview/result
models. It must call or extract reusable adapters from existing init,
validation, mod-package, and FOMOD behavior rather than duplicate schema,
rendering, archive, checksum, or diagnostic logic.

No new top-level CLI command or alias is added. The underlying canonical
operations remain `forge init`, `forge validate`, and `forge package` targets.

## Refusal behavior

Refuse before promotion for:

- blank/invalid names or invalid INI identifiers;
- unsafe, relative, reparse, inaccessible, or existing destination;
- stale preview token or changed inputs;
- any scaffold/source transformation failure;
- any blocking validation diagnostic;
- combined-package or FOMOD command failure;
- missing, malformed, escaped, stale, or unverifiable final evidence;
- cancellation.

Failures show the exact stage and preserve backend diagnostics. They must not
report a package as ready or leave a partial destination.

## Acceptance criteria for Gate 506

- Preview performs zero writes and binds all inputs and destination state.
- MCM-only creation produces valid source, a combined package containing only
  MCM payload, and a verified FOMOD ZIP.
- MCM+JIP creation produces valid source and a verified FOMOD containing both
  declared component families.
- Derived paths and IDs are deterministic and traversal-safe.
- Existing destination and stale-preview cases refuse without mutation.
- Injected validation/package/FOMOD failure cleans only the owned work root and
  leaves no final project.
- Repeated creation to separate roots produces equivalent canonical source and
  distributable bytes except for documented path-bearing local evidence.
- Windows tests cover preview, both modes, cancellation, failure cleanup,
  containment, and final evidence projection.
- Published and installed UI automation creates one MCM+JIP project through the
  workspace, verifies its FOMOD with the installed backend, then uninstalls and
  cleans isolated state.

## Explicit exclusions

- ESP/ESM creation or mutation, GECK/xEdit execution, conflict resolution, BSA
  packing, game Data writes, MO2 profile/load-order changes, or game launch;
- provider installation, runtime probes, network access, upload/publication,
  signing, attestation, or AI-generated script content;
- modifying an existing project through this workflow;
- hiding raw source or preventing later use of specialist workspaces.

## Next route

Gate 506: implement the Basic Mod Builder orchestration service and desktop
workspace, both starter modes, transactional work-root promotion and cleanup,
typed stage/evidence results, focused Windows coverage, publication, installer
rebuild, and installed end-to-end FOMOD proof.
