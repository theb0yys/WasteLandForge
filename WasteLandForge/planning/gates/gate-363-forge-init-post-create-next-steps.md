# Gate 363 - Forge Init Post-Create Next Steps

Status: Complete

## Goal

Make `forge init` tell users what to do next through both human-readable and
structured output, without adding writes or executing follow-up commands.

## Research grounding

- **Documented:** R006 recommends that init print three post-create commands:
  `forge validate .`, `forge capabilities scan --project .`, and
  `forge docs .`.
- **Documented:** ADR-010 requires canonical commands, examples-first help,
  deterministic local behavior, and stable machine-readable output.
- **Documented:** Gate 362 routes this exact implementation slice.

## Implemented

- Added one ordered `NextSteps` collection to the init plan result contract.
- JSON output now contains:

  ```json
  {
    "nextSteps": [
      "forge validate .",
      "forge capabilities scan --project .",
      "forge docs ."
    ]
  }
  ```

- Human/plain creation output labels the commands `Next steps`.
- Dry-run output labels them `Planned next steps` and remains no-write.
- Refused init plans do not instruct the user to proceed.
- The commands are guidance only; init does not execute them.

## Verification

- Full Release solution build passed with zero warnings and zero errors.
- All six init-focused golden tests passed.
- Tests verify exact JSON order and plain dry-run numbering/content.
- Standalone Forge and the app-shell bundled backend were republished and
  verified by the existing publish helpers.

## Boundaries

- No additional scaffold files, validation execution, capability scan, docs
  generation, provider installation, external tools, runtime probes, network
  calls, release behavior, or AI behavior.

## Next route

Gate 364: app-shell post-create action surface. Read the canonical init
`nextSteps` result and expose the remaining capability-scan and docs actions
after automatic validation, without creating command aliases or auto-running
external tools.
