# Rule Families

Status: Skeleton
Research classification: Documented
Source: R008 / ADR-011

WastelandForge diagnostic rule IDs use reserved families:

| Family | Scope |
|---|---|
| `WF-LOAD-*` | File discovery, parsing, encoding, duplicate file IDs |
| `WF-SCHEMA-*` | Schema and contract shape |
| `WF-SEM-*` | Semantic and cross-registry rules |
| `WF-CAP-*` | Capability/provider rules |
| `WF-ASSET-*` | Asset and path rules |
| `WF-GEN-*` | Generator rules |
| `WF-BUILD-*` | Build graph and cache rules |
| `WF-REL-*` | Release rules |
| `WF-GOV-*` | Governance rules |
| `WF-SEC-*` | Security and policy rules |

Gate 4 defines the issue model. Gate 5 assigns the first concrete load,
schema, and semantic rule IDs for the loader and validation pipeline.
Gate 12 adds YAML-specific load diagnostics, including unsupported YAML
features and duplicate YAML mapping keys, while preserving the existing
`WF-LOAD-*` family.
Gate 13 applies `WF-SCHEMA-*` diagnostics to dependency and capability registry
contract shape through runtime JSON Schema validation.
Gate 14 applies `WF-SCHEMA-*` diagnostics to asset registry contract shape.
Path existence, file-type, and packaging checks remain future `WF-ASSET-*`
semantic diagnostics.
Gate 15 adds the first concrete `WF-ASSET-*` diagnostics:
`WF-ASSET-001` for asset source escape, `WF-ASSET-002` for missing required
source files, `WF-ASSET-003` for non-game-relative target paths, and
`WF-ASSET-004` for target extension mismatch.
