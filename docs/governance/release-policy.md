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

Gate 8 adds a `release-dry-run` CI status check that validates the synthetic
fixture and emits CI governance artifacts. It is not the full Forge release
dry-run.

Gate 9 implements release dry-run behavior, Forge-owned build manifests, and
release verification evidence. Public release publishing remains out of scope
until verification and governance gates exist.
