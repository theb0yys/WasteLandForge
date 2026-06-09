# WastelandForge Agents

These are project-local specialist agent prompts derived from the research reports in `WasteLandForge/research/`.

Use them as role instructions for subagents or focused Codex threads. Each prompt is intentionally narrow, cites its required research inputs, and treats unanswered research gaps as blockers or open questions rather than invitations to invent design.

## Agents

- `forge-command-agent.md`: Command routing prompt for `/forge ...` and canonical R006 command implementation slices.
- `research-agent.md`: Evidence extraction and research discipline.
- `platform-architect-agent.md`: Hybrid capability platform and module boundaries.
- `contracts-registry-agent.md`: Manifest, schema, registry, diagnostics, immutable schema publication, and provenance contracts.
- `capability-provider-agent.md`: Provider catalogue, detection, scopes, versions, and WF-CAP rules.
- `build-cli-release-agent.md`: Build graph, R006 CLI, R008 CI baseline, packaging, and release workflow.
- `fnv-toolchain-agent.md`: GECK, xEdit, MO2, xNVSE ecosystem boundaries.
- `asset-voice-packaging-agent.md`: Assets, voice, lip files, BSA rules, MCM JSON, package staging.
- `narrative-reactivity-agent.md`: Quest/dialogue/faction/world/event state and reactive narrative.
- `validation-agent.md`: R008/ADR-011 layered validation, tests, CI outputs, SARIF, issue model, and release gates.
- `implementation-planning-agent.md`: v0.1 repository, solution, ADR, schema, C#, CLI, fixture, and CI planning after R008.
- `licensing-governance-agent.md`: Dependency policy, redistribution, AI/human accountability, contribution governance, and WFG-001.
- `ai-orchestration-agent.md`: Governed AI platform layer, specialist agents, memory, approvals, evals.
