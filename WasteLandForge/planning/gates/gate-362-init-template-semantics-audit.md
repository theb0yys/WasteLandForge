# Gate 362 - Init Template Semantics Audit

Status: Complete

## Goal

Determine what the four supported `forge init` template IDs actually do and
align product messaging with the documented implementation boundary.

## Research grounding

- **Documented:** R006 names `fnv-basic`, `fnv-framework`, `fnv-quest-pack`,
  and `fnv-docs-only` as onboarding templates.
- **Documented:** R006 requires init to create a valid scaffold containing a
  manifest, minimal registries, VS Code tasks, a GitHub Actions workflow, and a
  README.
- **Documented:** R006 says minimal, docs-only, and runtime-enabled categories
  are sufficient for the MVP.
- **Open:** The research does not define distinct file or registry contracts
  for `fnv-framework` or `fnv-quest-pack`, nor exact specialized docs-only or
  runtime-enabled contents.

## Audit evidence

Each template was created with the same synthetic project name and then
validated through the Release CLI:

| Template | Init | Validate | Files | Same paths | Same content |
| --- | ---: | ---: | ---: | --- | --- |
| `fnv-basic` | 0 | 0 | 8 | yes | yes |
| `fnv-framework` | 0 | 0 | 8 | yes | yes |
| `fnv-quest-pack` | 0 | 0 | 8 | yes | yes |
| `fnv-docs-only` | 0 | 0 | 8 | yes | yes |

The current IDs are accepted selectors recorded in init output, but they do
not select distinct scaffold content.

## Implemented correction

- `forge help init` now states that all supported IDs create the same validated
  baseline scaffold and that specialized contents remain undefined.
- The app-shell New Project form displays the same boundary below template
  selection.
- CLI documentation records the current baseline behavior.
- A golden help test protects this disclosure.
- No template ID, scaffold file, output schema, or validation behavior changed.

## Verification

- Full Release solution build passed with zero warnings and zero errors.
- Four targeted init/help golden tests passed.
- All four synthetic template projects validated successfully and were removed.
- The local app distribution was refreshed with the corrected New Project text.

## Next route

Gate 363: implement the documented `forge init` post-create next-step guidance:
`forge validate .`, `forge capabilities scan --project .`, and `forge docs .`,
with equivalent structured JSON fields and no additional filesystem writes.
