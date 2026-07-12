# Gate 472 - Deterministic FOMOD Packaging Adapter Implementation

Status: Implemented with compatibility closeout routed to Gate 473
Phase: v0.1 implementation
Decision base: ADR-004, ADR-007, ADR-009, ADR-010, ADR-011 and Gate 471

## Delivered

- Immutable manifest 0.4.0, FOMOD source 0.1.0, and FOMOD evidence 0.1.0 schemas.
- `registries.fomod` source metadata on the synthetic combined-mod fixture.
- `forge package <project> --target fomod` with dry-run, plain, and JSON output.
- Deterministic required-files FOMOD 5.0 XML and ZIP output under `dist/fomod`.
- Local FOMOD manifest, build manifest, checksums, staging tree, and payload digests.
- A visible FOMOD installer workflow in the desktop Project Outputs workspace.
- Determinism, XML structure, archive layout, evidence, dry-run, and catalog tests.
- Windows output handling through the repository's existing filesystem fallback.

## Proven boundary

The adapter emits only unconditional `requiredInstallFiles`. It does not emit
installer pages, choices, conditions, executable scripts, or manager commands.
It does not launch MO2/Vortex, write game Data, mutate plugins, or use AI or the
network.

The synthetic archive contains Data-relative payload entries at ZIP root plus
`fomod/info.xml` and `fomod/ModuleConfig.xml`. Repeated builds produce the same
ZIP bytes.

## Compatibility closeout still required

Gate 472 does not claim external installer compatibility. The checked-in test
currently parses and structurally asserts the generated XML, but it does not
validate against a redistribution-cleared FOMOD 5.0 XSD or run the archive
through MO2's installer plugin. Malformed metadata, collision, injected
failure/rollback, complete CLI help/golden coverage, full-suite publication,
and installed-app automation also remain to be closed.

## Next route

Gate 473: FOMOD schema compatibility, refusal/rollback, publication, and
installed desktop workflow closeout. No richer FOMOD feature work enters that
gate.
