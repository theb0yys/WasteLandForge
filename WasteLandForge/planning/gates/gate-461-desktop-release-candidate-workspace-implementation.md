# Gate 461 - Desktop Release Candidate Workspace Implementation

Status: Complete
Phase: v0.1 desktop release-candidate workflow
Decision base: Gate 460, ADR-004, ADR-005, ADR-009, ADR-010, ADR-011

## Implemented behavior

- Added a dedicated desktop **Release Candidate** workspace.
- One action runs canonical validation, combined `mod-package` packaging, and
  release verification in order through the bundled backend.
- Blocking stages short-circuit the pipeline and leave later stages visibly
  `Not run`.
- Stage rows expose status, exit code, duration, diagnostic counts, and the
  canonical command.
- Exact structured diagnostic identifiers, titles, severities, and messages
  remain visible without desktop policy reinterpretation.
- Package plugin rows expose packaged Data paths and digests plus validated
  review, evidence, and report-snapshot provenance.
- Candidate projections become stale when selected project inputs change.
- Package, archive, release-evidence, and handoff open actions require existing
  paths contained under the selected project's `dist` tree.
- Runs are single-instance and cancellable; cancellation terminates the backend
  process tree and prevents later stages from running.

## Automated coverage

Focused Windows tests prove:

- canonical stage order and successful readiness;
- validation short-circuiting and exact diagnostic preservation;
- package failure short-circuiting;
- source fingerprint staleness and path containment;
- cancellation without later command execution.

The complete solution build passed. All six test projects passed individually:

- Unit: 118
- Schema: 140
- Semantic: 87
- Backwards compatibility: 45
- Golden: 304
- Windows: 81
- Total: 775

## Published and installed evidence

- Standalone backend and desktop app-shell publication passed.
- The published bundled backend completed validate, combined package, and
  release verification against an isolated synthetic project with exit 0.
- `package.zip` and `release-evidence-handoff.md` were present.
- Inno Setup 6.7.3 built the unsigned local installer.
- Installer size: 3,699,857 bytes.
- Installer SHA-256:
  `cdc791985da2db5d93796d88e9158ebdbf27b073a0a30466c7cfe7005dbe6e9c`.
- Isolated per-user silent install passed; the installed WPF executable remained
  running during launch smoke.
- The installed bundled backend completed all three release-candidate stages
  with exit 0.
- Silent uninstall passed and isolated install/project roots were removed.

## Boundaries

- No release was prepared or published.
- No network correctness path, provider installation, external game tool,
  GECK/xEdit/MO2 automation, game launch, plugin parsing/mutation, signing,
  attestation, or AI behavior was added.
- Fixtures and installed regression inputs were synthetic and redistributable.

## Environment note

External package runs inside the actively monitored repository artifact tree
experienced file-event races. The same published and installed executables
passed in isolated Windows Local Temp, which matches the application's normal
per-user project workflow. All diagnostic trees created during investigation
were removed.

## Next route

Gate 462: automate the installed Release Candidate tab interaction itself,
including project selection, stage-row readiness, exact blocked diagnostics,
evidence action enablement, stale projection, and uninstall cleanup.
