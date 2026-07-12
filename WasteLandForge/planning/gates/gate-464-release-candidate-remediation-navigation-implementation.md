# Gate 464 - Release Candidate Remediation Navigation Implementation

Status: Complete
Phase: v0.1 desktop remediation workflow
Decision base: Gate 463, ADR-004, ADR-005, ADR-009, ADR-010, ADR-011

## Implemented behavior

- Release Candidate diagnostics now support single selection.
- The selected issue retains its exact rule ID, severity, title, and backend
  message in a dedicated original-context area.
- **Explain Diagnostic** invokes only
  `forge explain diagnostic <rule-id> --format json` through the bundled
  backend.
- A typed parser verifies command identity, selected-rule identity, family
  metadata, rule metadata, recovery commands, and declared boundaries.
- The explanation panel renders family scope, validation stage, concrete rule
  title/summary, display-only recovery commands, boundaries, and the canonical
  command.
- Malformed or mismatched JSON becomes `Unavailable` without changing the
  original issue or candidate state.
- A request revision gate prevents an older asynchronous explanation from
  replacing a newer diagnostic selection.
- **Go to Workspace** routes by reserved rule family only:
  validation families to Validation Report, capability rules to Capabilities,
  generation/build rules to Project Outputs, and release/governance/security
  rules to Release Candidate.
- Navigation changes only the selected tab. It does not execute recovery
  commands.
- Stale candidate projections retain generic explanation but disable workspace
  routing.

## Automated coverage

Focused Windows coverage proves canonical parsing, display-only recovery
metadata, all reserved family routes, malformed JSON refusal, mismatched rule
identity refusal, and superseded-request rejection.

All six test projects passed:

- Unit: 118
- Schema: 140
- Semantic: 87
- Backwards compatibility: 45
- Golden: 304
- Windows: 94
- Total: 788

The full solution build passed. NuGet audit emitted only the known unavailable
network-feed warning.

## Published and installed evidence

- App-shell publication and installer input preflight passed.
- Inno Setup 6.7.3 built the unsigned local installer.
- Installer size: 3,704,419 bytes.
- Installer SHA-256:
  `14f9baa170394c22cb96d85e651d294d778238df37ccf456704b7689b29ebf70`.
- The installed UI regression proved ready candidate state, contained evidence
  actions, external-source staleness, blocked schema diagnostics, diagnostic
  selection, canonical explanation metadata, separate exact issue context,
  Validation Report routing, and stale-route disabling.
- Silent uninstall and isolated install/settings/project cleanup passed.

## Regression correction

Early installed automation attempts retained UI Automation elements across a
long backend run and recursively enumerated virtualized DataGrid descendants.
Those operations made the PowerShell automation host unreliable. The reusable
regression now reacquires the installed window after asynchronous work and
limits Gate 464 assertions to stable named controls and selected rows. Gate 462
already owns the detailed stage-grid regression. Failed isolated installs were
removed before the passing run.

## Boundaries

- No source file or generated evidence is modified by explanation/navigation.
- Recovery commands are never executed by the panel.
- No editor location inference, automatic fixes, provider execution, GECK,
  xEdit, MO2 automation, game launch, release prepare/publish, signing,
  attestation, network correctness path, or AI behavior was added.

## Next route

Gate 465: define a verified-candidate MO2 test-deployment handoff that reuses
the existing preview-token and new-destination export safety model, requires a
fresh Candidate-ready result, and still does not enable profiles, change
priority/load order, write game Data, launch the game, or overwrite an existing
MO2 mod.
