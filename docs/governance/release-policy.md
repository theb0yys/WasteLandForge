# Release Policy

Status: Skeleton
Research classification: Documented
Source: R008 / ADR-011

Release behavior is gated and validation-first.

Required before release publish exists:

- schema validation,
- semantic validation,
- capability/environment validation,
- package validation,
- release verification,
- local `build-manifest.json`,
- checksums,
- governance checks,
- explicit human approval.

Gate 9 implements release dry-run behavior through `forge release verify`.

The command validates first, stages synthetic release evidence under project
`dist/`, writes Forge-owned `build-manifest.json`, writes `checksums.sha256`,
and emits release summary and validation reports.

Gate 11 adds release diagnostic projections through `forge release verify
--format github` and `forge release verify --summary <path>`. These projections
report diagnostics only; release evidence still lives under project `dist/`.

Public release publishing remains out of scope until later verification and
governance gates exist.
