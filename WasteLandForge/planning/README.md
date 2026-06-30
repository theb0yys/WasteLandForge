# WastelandForge Planning

Implementation planning is gated. Each gate defines its purpose, exit evidence, and open checks before the next implementation slice starts.

Gate status:

- `gates/gate-000-definition-goals-plan.md` - Gate 0, definition, goals, and plan.
- `gates/gate-001-repository-adr-skeleton.md` - Gate 1, repository and ADR skeleton.
- `gates/gate-002-solution-sdk-baseline.md` - Gate 2, solution and SDK baseline.
- `gates/gate-003-schema-package-skeleton.md` - Gate 3, schema package skeleton.
- `gates/gate-004-core-domain-diagnostics.md` - Gate 4, core domain and diagnostics.
- `gates/gate-005-loader-validation-pipeline.md` - Gate 5, loader and validation pipeline.
- `gates/gate-006-cli-skeleton.md` - Gate 6, CLI skeleton.
- `gates/gate-007-fixtures-tests.md` - Gate 7, fixtures and tests.
- `gates/gate-008-ci-governance-baseline.md` - Gate 8, CI and governance baseline.
- `gates/gate-009-release-dry-run-build-manifest.md` - Gate 9, release dry-run and build manifest evidence.
- `gates/gate-010-sarif-diagnostic-projection.md` - Gate 10, canonical SARIF diagnostic projection.
- `gates/gate-011-markdown-github-diagnostics.md` - Gate 11, Markdown summaries and GitHub annotations.
- `gates/gate-012-yaml-runtime-schema-validation.md` - Gate 12, YAML ingestion and runtime manifest schema validation.
- `gates/gate-013-registry-schema-validation.md` - Gate 13, dependency and capability registry schema validation.
- `gates/gate-014-asset-registry-schema-validation.md` - Gate 14, asset registry schema validation.
- `gates/gate-015-asset-path-semantic-validation.md` - Gate 15, asset path semantic validation.
- `gates/gate-016-asset-type-specific-validation.md` - Gate 16, asset type-specific validation.
- `gates/gate-017-voice-dialogue-asset-validation.md` - Gate 17, voice and dialogue asset validation.
- `gates/gate-018-dialogue-registry-voice-worklist-skeleton.md` - Gate 18, dialogue registry and voice worklist skeleton.
- `gates/gate-019-quest-registry-dialogue-reference-validation.md` - Gate 19, quest registry skeleton and dialogue quest reference validation.
- `gates/gate-020-quest-stages-objectives-skeleton.md` - Gate 20, quest stages and objectives skeleton.
- `gates/gate-021-quest-stage-transition-skeleton.md` - Gate 21, quest stage transition skeleton.
- `gates/gate-022-quest-condition-skeleton.md` - Gate 22, quest condition skeleton.
- `gates/gate-023-quest-result-script-skeleton.md` - Gate 23, quest result-script skeleton.
- `gates/gate-024-quest-variable-skeleton.md` - Gate 24, quest variable skeleton.
- `gates/gate-025-dialogue-condition-skeleton.md` - Gate 25, dialogue condition skeleton.
- `gates/gate-026-dialogue-result-script-skeleton.md` - Gate 26, dialogue result-script skeleton.
- `gates/gate-027-dialogue-topic-link-skeleton.md` - Gate 27, dialogue topic link skeleton.
- `gates/gate-028-dialogue-quest-level-gate-skeleton.md` - Gate 28, dialogue quest-level gate skeleton.
- `gates/gate-029-dialogue-result-script-variable-mutation-skeleton.md` - Gate 29, dialogue result-script variable mutation skeleton.
- `gates/gate-030-dialogue-link-from-skeleton.md` - Gate 30, dialogue Link From skeleton.
- `gates/gate-031-dialogue-link-graph-validation-skeleton.md` - Gate 31, dialogue link graph validation skeleton.
- `gates/gate-032-dialogue-priority-prompt-routing-skeleton.md` - Gate 32, dialogue priority and prompt routing skeleton.
- `gates/gate-033-dialogue-speech-challenge-skeleton.md` - Gate 33, dialogue Speech Challenge skeleton.
- `gates/gate-034-dialogue-skill-gate-skeleton.md` - Gate 34, dialogue skill gate skeleton.
- `gates/gate-035-dialogue-perk-gate-skeleton.md` - Gate 35, dialogue perk gate skeleton.
- `gates/gate-036-dialogue-faction-reputation-gate-skeleton.md` - Gate 36, dialogue faction and reputation gate skeleton.
- `gates/gate-037-dialogue-identity-gate-skeleton.md` - Gate 37, dialogue identity gate skeleton.
- `gates/gate-038-dialogue-local-world-flag-gate-skeleton.md` - Gate 38, dialogue local world flag gate skeleton.
- `gates/gate-039-dialogue-event-history-gate-skeleton.md` - Gate 39, dialogue event history gate skeleton.
- `gates/gate-040-dialogue-companion-state-gate-skeleton.md` - Gate 40, dialogue companion state gate skeleton.
- `gates/gate-041-dialogue-result-script-side-effect-gate-skeleton.md` - Gate 41, dialogue result-script side-effect gate skeleton.
- `gates/gate-042-dialogue-condition-boolean-composition-skeleton.md` - Gate 42, dialogue condition boolean composition skeleton.
- `gates/gate-043-dialogue-condition-logic-reference-validation.md` - Gate 43, dialogue condition logic reference validation.
- `gates/gate-044-dialogue-nested-condition-group-skeleton.md` - Gate 44, dialogue nested condition group skeleton.
- `gates/gate-045-geck-dialogue-export-validation-bridge.md` - Gate 45, GECK dialogue export validation bridge.
- `gates/gate-046-dialogue-condition-negation-skeleton.md` - Gate 46, dialogue condition negation skeleton.
- `gates/gate-047-dialogue-condition-precedence-skeleton.md` - Gate 47, dialogue condition precedence skeleton.
- `gates/gate-048-dialogue-condition-short-circuit-skeleton.md` - Gate 48, dialogue condition short-circuit skeleton.
- `gates/gate-049-dialogue-condition-logic-identity-validation.md` - Gate 49, dialogue condition logic identity validation.
- `gates/gate-050-dialogue-response-route-skeleton.md` - Gate 50, dialogue response route skeleton.
- `gates/gate-051-dialogue-response-route-target-validation.md` - Gate 51, dialogue response route target validation.
- `gates/gate-052-dialogue-response-route-endpoint-validation.md` - Gate 52, dialogue response route endpoint validation.
- `gates/gate-053-dialogue-response-route-identity-validation.md` - Gate 53, dialogue response route identity validation.
- `gates/gate-054-dialogue-response-route-key-ambiguity-validation.md` - Gate 54, dialogue response route key ambiguity validation.
- `gates/gate-055-dialogue-response-route-taxonomy-evidence-checkpoint.md` - Gate 55, dialogue response route taxonomy evidence checkpoint.
- `gates/gate-056-dialogue-response-route-taxonomy-evidence-pack-skeleton.md` - Gate 56, dialogue response route taxonomy evidence pack skeleton.
- `gates/gate-057-capability-catalogue-list-foundation.md` - Gate 57, capability catalogue list foundation.
- `gates/gate-058-capability-scan-detector-skeleton.md` - Gate 58, capability scan detector skeleton.
- `gates/gate-059-capability-explain-scan-evidence-skeleton.md` - Gate 59, capability explain scan evidence skeleton.
- `gates/gate-060-capability-requirement-resolution-skeleton.md` - Gate 60, capability requirement resolution skeleton.
- `gates/gate-061-deterministic-generate-build-report-skeleton.md` - Gate 61, deterministic generate/build report and build manifest skeleton.
- `gates/gate-062-mcm-extender-json-contract-generator-skeleton.md` - Gate 62, MCM Extender JSON contract and generator skeleton.
- `gates/gate-063-mcm-extender-runtime-schema-evidence-output-validation.md` - Gate 63, MCM Extender runtime schema evidence and output validation.
- `gates/gate-064-mcm-extender-requirements-translations-skeleton.md` - Gate 64, MCM Extender requirements and translations skeleton.
- `gates/gate-065-mcm-extender-checkbox-string-toggle-option-skeleton.md` - Gate 65, MCM Extender checkbox and string-toggle option skeleton.
- `gates/gate-066-mcm-extender-keybind-option-skeleton.md` - Gate 66, MCM Extender keybind option skeleton.
- `gates/gate-067-mcm-extender-header-option-skeleton.md` - Gate 67, MCM Extender header option skeleton.
- `gates/gate-068-mcm-extender-image-option-skeleton.md` - Gate 68, MCM Extender image option skeleton.
- `gates/gate-069-mcm-extender-image-asset-validation-skeleton.md` - Gate 69, MCM Extender image asset validation skeleton.
- `gates/gate-070-mcm-extender-loose-file-staging-skeleton.md` - Gate 70, MCM Extender loose-file staging skeleton.
- `gates/gate-071-mcm-extender-package-manifest-skeleton.md` - Gate 71, MCM Extender package manifest skeleton.

Next gate:

- Gate 72 - MCM Extender ZIP package skeleton.

Gate work must follow `AGENTS.md` and classify important claims as `Documented`, `Inferred`, or `Open`.
