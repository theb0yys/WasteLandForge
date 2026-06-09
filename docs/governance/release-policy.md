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

Gate 9 may implement release dry-run behavior. Public release publishing remains out of scope until verification and governance gates exist.
