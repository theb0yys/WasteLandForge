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
