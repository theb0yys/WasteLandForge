# JIP Script Emission Manifest Schema

`0.1.0/schema.json` describes generated JIP LN text-script emission evidence
under `generated/jip-scripts`. It is generated output evidence, not a source
registry contract and not an install/package format.

Gate 212 embeds the schema and validates emitted
`jip-script-emission-manifest.json` files before checksum sidecars are written.
