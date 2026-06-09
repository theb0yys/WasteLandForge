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

Gate 4 defines the issue model. Gate 5 and later assign concrete rule IDs.
