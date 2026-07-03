# JIP LN Text Script Registry Schema

`0.1.0/schema.json` defines source-controlled intent for future JIP LN Script
Runner text-script generation.

Gate 203 adds the schema skeleton only. It records script identity,
lifecycle prefix, output filename intent, required capabilities, size policy,
and FormID-resolution policy. It does not generate text files, wire a
`forge generate` or `forge build` target, run runtime probes, automate GECK,
inspect MO2 VFS state, mutate a live Data tree, or execute external tools.
