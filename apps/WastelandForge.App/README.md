# WastelandForge.App

Unity app shell for the premium Windows `WastelandForge.exe` experience.

This project is a GUI lane over the existing deterministic `forge.exe`
backend. It must not replace Forge core, the canonical CLI command surface, or
the JSON report contracts.

## Local Heat Import

Heat - Complete Modern UI 1.1.8 is imported locally at:

```text
Assets/ThirdParty/Heat - Complete Modern UI/
```

That path is ignored by git. Do not commit or redistribute the raw Heat source
asset pack from this repository.

## First Backend Bridge

The app shell starts with:

```text
forge.exe --version
forge.exe capabilities list --format json
forge.exe validate <project-root> --format json
```

The editor setup and build hook live in:

```text
Assets/WastelandForge/App/Editor/WastelandForgeAppShellSetup.cs
```

