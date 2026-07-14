# Gate 554 - Redistributable Synthetic FNV Fixture Acquisition

Status: Complete - acquisition contract defined; matching fixture not yet supplied
Phase: post-v0.1 authoring verification
Decision base: ADR-004, ADR-005, ADR-009, ADR-011, ADR-013, WFG-001, R009,
and Gates 520-553

## Goal

Resolve Gate 553's missing-subject blocker without committing proprietary game
data, selecting an arbitrary mod, teaching Forge core to write plugin bytes, or
using the unproved verifier to approve its own bootstrap fixture.

## Evidence classification

- **Documented:** public fixtures must be synthetic and redistributable and
  must not contain Bethesda assets or third-party mod files without explicit
  permission.
- **Documented:** ADR-004 keeps GECK record editing and raw plugin writing
  outside Forge core. ADR-013 requires independent verification before bounded
  provider execution.
- **Documented:** Gate 553 requires a valid synthetic FNV plugin that matches
  the first-slice plan. Opaque text named `.esp` is not acceptable.
- **Observed:** the official xEdit `dev-4.1.5` tree contains seven tracked
  hardcoded game plugins and is published under MPL-2.0.
- **Observed:** the checked xEdit tree is commit
  `9058a79437367c2cb1b757a9021ab443d743012d` with tree
  `0796e8dffde814bdbb95d2800c8739f627953227`.
- **Observed:** its `Core/Hardcoded/FalloutNV.esp` candidate is 4,226 bytes,
  starts with `TES4`, and has SHA-256
  `eb6eb5d3dd9753d90e454a41ae49a1d41df592a2ec314c94d4413ed2532191ce`.
- **Observed:** bounded byte-token inspection finds `TES4`, `GRUP`, and `MICN`
  records but no `MAST`, `CONT`, `REFR`, `CELL`, or `WRLD` signature.
- **Inferred:** that upstream file is xEdit hardcoded-definition support data,
  not a semantic first-slice authoring fixture.
- **Open:** no matching, explicitly licensed one-container/one-reference FNV
  plugin has been supplied.

Primary upstream evidence:

- [xEdit repository and MPL-2.0 license](https://github.com/TES5Edit/TES5Edit/tree/dev-4.1.5)
- [xEdit 4.1.5f release](https://github.com/TES5Edit/TES5Edit/releases/tag/xedit-4.1.5f)

## Candidate decision

`Core/Hardcoded/FalloutNV.esp` is rejected as the Gate 553 subject. Its license
and valid TES4 header do not make it semantically compatible with the existing
Gate 521 plan or Gate 522 verifier. Running the observer against a knowingly
wrong record set would violate Gate 534 rather than prove compatibility.

No upstream game plugin was copied into WastelandForge.

## Accepted fixture source

The accepted bootstrap route is a human-authored synthetic plugin created in
GECK outside Forge's correctness implementation. The contributor must create
it from an exact Gate 521 plan and explicitly license the resulting plugin for
redistribution in the public fixture corpus.

The fixture must:

1. be a structurally valid Fallout: New Vegas ESP produced and saved by GECK;
2. use exactly `FalloutNV.esm` as its ordered master set;
3. contain the plan's one new `CONT` base and one placed `REFR` only, apart
   from editor-required grouping/header structures;
4. contain exactly the declared synthetic inventory, ownership, persistence,
   respawn, encounter-zone, cell, and transform semantics;
5. contain no embedded Bethesda assets, scripts copied from the game, voice,
   meshes, textures, audio, navmesh edits, or third-party mod content;
6. include no additional authored records or undeclared masters;
7. use a plugin filename that exactly matches the plan;
8. be accompanied by an explicit redistribution license or dedication from
   the fixture author;
9. include the canonical intent, evidence references, creation notes, file
   length, and SHA-256 supplied for Gate 553 Approval A; and
10. remain outside the tracked fixture corpus until Gate 553 independently
    verifies it and a maintainer approves its license and inclusion.

## Rejected bootstrap routes

- Writing handcrafted ESP bytes in Forge, a test, PowerShell, Python, or C#.
- Adding a plugin-writing library to Forge core.
- Using FNVEdit to create the subject before its verifier compatibility is
  independently established.
- Copying a normal mod, game master, game-generated file, or unlicensed plugin.
- Reusing xEdit's hardcoded-definition plugin despite semantic mismatch.
- Promoting opaque test bytes or synthetic JSON observations to plugin status.
- Weakening the plan or verifier until an unrelated available plugin passes.

These routes either violate the ownership boundary, create circular verifier
evidence, lack redistribution authority, or knowingly test the wrong contract.

## Intake package

Gate 553 Approval A should receive one operator-controlled directory containing:

```text
subject/
  CouriersEmergencyCache.esp
  LICENSE.txt
  creation-notes.md
```

The directory name is illustrative; the exact plugin filename remains plan-
controlled. `creation-notes.md` must identify the GECK version, selected active
file workflow, ordered masters, declared record identities, and the author's
statement that no proprietary assets or third-party plugin bytes were copied.

Forge may inspect paths, regular-file status, containment, length, digests, and
text evidence during Approval A. It must not parse, edit, normalize, repair, or
resave the plugin before the independently approved FNVEdit observer run.

## Validation

- Inspected the official xEdit `dev-4.1.5` Git tree and MPL-2.0 license.
- Enumerated its tracked plugin files without copying them into the repository.
- Inspected the exact `Core/Hardcoded/FalloutNV.esp` length, SHA-256, TES4
  header, and bounded record-signature tokens.
- Confirmed the candidate does not contain the first-slice signature set.
- Rechecked Gate 553's subject and licensing requirements.
- No GECK, FNVEdit, xEdit, MO2, game, or provider process ran.
- No plugin was created, changed, copied into the repository, or approved.

## External state

A sparse, read-only audit checkout of official xEdit source was created under:

```text
C:/Users/kane0/AppData/Local/Temp/WastelandForge-xedit-fixture-audit
```

Only the repository metadata, `LICENSE.txt`, and
`Core/Hardcoded/FalloutNV.esp` were materialized. It is research evidence, not
a Forge dependency, fixture, generated output, or redistribution source.

## Next route

Return to Gate 553 Approval A after the operator supplies the three-file intake
package and the remaining exact INI/project/output/provider inputs. Gate 553
then calculates and displays the complete preview digest and stops for Approval
B before one no-retry read-only FNVEdit run.

Until that package exists, no further repository implementation can establish
real verifier compatibility or unblock the bounded GECK writer.
