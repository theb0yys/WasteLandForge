# Gate 471 - Deterministic FOMOD Packaging Adapter Contract

Status: Complete
Phase: v0.1 research and implementation planning
Decision base: ADR-004, ADR-009, ADR-010, ADR-011, Gates 387-394 and
467-470

## Goal

Define the first deterministic, manager-consumable FOMOD package generated from
validated Forge source without executing a mod manager or introducing a second
canonical payload model.

Gate 471 adds no runtime behavior. Gate 472 implements this contract.

## External research checked

- The maintained Nexus Mods FOMOD installer describes FOMOD as an XML/C# mod
  archive format and supports XML script versions 1.0 through 5.0. Its native
  path supports XML only, while C# scripts require the dynamic Windows path:
  <https://github.com/Nexus-Mods/fomod-installer>.
- The FOMOD 5.x documentation defines a package-level `fomod` directory with
  `info.xml` and `ModuleConfig.xml`; `info.xml` supplies package metadata and
  `requiredInstallFiles` in `ModuleConfig.xml` expresses unconditional files:
  <https://fomod-docs.readthedocs.io/en/latest/tutorial.html>.
- The same documentation shows that element ordering is significant and that
  richer installers add groups, selection rules, flags, visibility conditions,
  and conditional file matrices.
- Mod Organizer 2 ships dedicated FOMOD installer plugins rather than treating
  the format as a generic loose-file copy:
  <https://github.com/ModOrganizer2/modorganizer-installer_fomod> and
  <https://github.com/ModOrganizer2/modorganizer-installer_fomod_csharp>.

## Research conclusion

The first Forge adapter should target the common declarative XML 5.0 subset
only. It should package one already validated combined payload as unconditional
required files. It must not attempt optional choices, scripted installers, or
manager-specific execution in the first slice.

This gives authors a distributable archive immediately while keeping output
deterministic, auditable, and compatible with Forge's current single-payload
model.

## Canonical command

```text
forge package <project> --target fomod [--dry-run] [--format human|plain|json]
```

- `fomod` is a package target under the existing canonical verb.
- No `forge install`, `deploy`, or manager-specific alias is added.
- Normal execution rebuilds and validates `mod-package` from canonical source
  before FOMOD assembly.
- A future explicit existing-package reuse option may be added only by reusing
  the Gate 468 verifier; Gate 472 does not need it for the first slice.
- `--dry-run` validates metadata, plans exact XML/archive entries, and writes
  nothing.

## Source contract

Add one optional manifest registry declaration:

```json
{
  "registries": {
    "fomod": "src/registries/fomod/main.json"
  }
}
```

The immutable `fomod/0.1.0` source contract contains:

- `schemaVersion`, `kind`, and logical `id`;
- display `name`;
- `author`;
- SemVer-compatible machine `version` plus optional display version;
- non-empty `description`;
- optional absolute HTTP(S) `website`;
- `moduleName`, defaulting to `name` only when omitted;
- explicit `installerVersion: "5.0"`.

No source field contains raw XML, arbitrary element names, XPath, scripts,
manager commands, source filesystem paths, or destination paths. Payload paths
come only from validated `mod-package` evidence.

## Generated layout

Default output:

```text
dist/fomod/
  staging/
    fomod/
      info.xml
      ModuleConfig.xml
    <Data-relative payload files>
  package.zip
  fomod-manifest.json
  build-manifest.json
  checksums.sha256
```

The ZIP root matches `staging/`; it does not add an extra project, package, or
`Data` directory. Existing mod-package entries therefore remain game
Data-relative in the archive.

## XML contract

`info.xml` uses UTF-8 without BOM and emits elements in fixed order:

```text
Name, Author, Version, Description, Website (when present)
```

`Version` carries the machine version attribute and display text. XML values
are written through `System.Xml` APIs with escaping; no string-built markup is
allowed.

`ModuleConfig.xml` targets the FOMOD 5.0 schema location, emits `moduleName`,
then `requiredInstallFiles`. Every validated mod-package entry becomes one
explicit `<file>` in canonical ordinal Data-path order. Source and destination
are the same normalized Data-relative path. Gate 472 must confirm the exact
attribute form against the selected FOMOD 5.0 schema fixture before freezing
goldens.

## Determinism and validation

- All XML has stable declaration, indentation, newline, encoding, element
  order, and attribute order.
- ZIP entries are ordinal-sorted, stored without compression, and use Forge's
  existing deterministic ZIP timestamp policy.
- XML is parsed after writing and validated against checked-in,
  redistribution-compatible FOMOD 5.0 schema evidence.
- Archive contents are reopened and compared with the plan, XML, payload
  lengths, and SHA-256 values.
- Repeated builds from identical source produce byte-identical XML and ZIP.
- Every output is covered by checksums and a Forge build manifest.

## FOMOD manifest

Add immutable `fomod-manifest/0.1.0` evidence recording:

- project, tool, source-contract, and mod-package evidence digests;
- installer version and metadata projection;
- exact XML paths/digests/lengths;
- ordered payload entries with component, Data path, length, and SHA-256;
- ZIP path, length, SHA-256, entry count, and timestamp policy;
- validation status;
- all disabled execution/mutation flags.

## Refusal rules

Block before output promotion when:

- the FOMOD registry is missing, malformed, or schema-invalid;
- metadata is blank, the version is invalid, or website is non-HTTP(S);
- combined package generation/validation fails;
- payload is empty, unsafe, duplicated, case-colliding, linked, or stale;
- metadata/XML/payload paths collide case-insensitively;
- generated XML is invalid or does not satisfy the selected FOMOD schema;
- archive or output containment/digest verification fails.

Generation uses a Forge-owned temporary output and atomic replacement under
`dist/fomod`. Failure removes only that temporary output and preserves any
previous successful FOMOD package.

## Explicit first-slice exclusions

- install steps, optional groups, plugins/options, flags, dependencies,
  visibility conditions, conditional file installs, images, and type patterns;
- C# install scripts or any executable content;
- external FOMOD libraries as a core runtime dependency;
- MO2/Vortex/NMM launch, install simulation, profile mutation, or compatibility
  claims beyond XML/schema/archive correctness;
- plugin generation/mutation, game Data writes, network calls, signing,
  publication, runtime probes, or AI.

## Gate 472 acceptance criteria

Gate 472 must implement and prove:

- immutable source and FOMOD-manifest schemas are registered and tested;
- one synthetic combined project produces deterministic `info.xml`,
  `ModuleConfig.xml`, staging tree, ZIP, manifest, build manifest, and checksums;
- archive payload bytes exactly match validated mod-package entries;
- dry-run writes nothing and reports the exact plan;
- malformed metadata, unsafe/colliding payloads, invalid XML, output escape,
  and injected failure are refusal/rollback tested;
- repeated builds are byte-identical;
- CLI help and JSON/plain output are covered;
- full solution tests, publication, and installed desktop output workflow pass
  with synthetic redistributable fixtures and cleanup.

## Next route

Gate 472: implement the minimal deterministic required-files FOMOD 5.0 adapter,
source/evidence schemas, CLI package target, Project Outputs workflow, fixture
coverage, publication, and installed regression.
