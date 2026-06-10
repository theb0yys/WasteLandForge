# Dependency Policy

Status: Skeleton
Research classification: Documented
Source: R002A / WFG-001 and R008 / ADR-011

No proprietary or redistribution-unclear dependency may become a hard requirement of WastelandForge core.

Dependency rules:

- Prefer redistributable open-source libraries for core implementation.
- Do not rehost third-party runtime binaries by default.
- Model ecosystem requirements as capabilities and providers.
- Keep validation, build, release verification, and contribution paths offline-first and AI-optional.
- Record dependency blockers as open implementation checks before changing gate scope.

Gate 2 checks .NET target and package compatibility before solution baseline work is accepted.
Gate 13 validates dependency and capability registry contracts before semantic
capability-reference checks or later provider detection can run.
