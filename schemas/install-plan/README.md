# Install Plan Schemas

This directory contains generated install-plan evidence schemas.

- `0.1.0/schema.json` defines the first MCM Extender loose-file install-ready
  export plan emitted by `forge generate --target mcm-json`,
  `forge build --target mcm-json`, and `forge package --target mcm-json`.

Install-plan evidence is generated output, not source truth. It records
Data-relative copy intent and explicit non-mutation flags; Forge does not copy
files into a game install or MO2 profile in this gate.
