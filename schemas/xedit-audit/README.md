# xEdit Audit Schema

Status: Gate 218 adapter evidence checkpoint

The xEdit audit schema defines source-controlled audit and inspection intent
for future xEdit script/report support. The initial contract is evidence-only:
it records script and report output paths, required tool capabilities, target
plugin names, record signatures, and explicit non-mutation safety flags.

Gate 218 does not execute xEdit, parse real xEdit output, generate patches,
mutate plugins, automate MO2, automate GECK, run runtime probes, or use real
third-party plugin fixtures.
