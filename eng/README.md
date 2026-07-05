# Engineering

This directory holds shared engineering configuration.

Gate 2 pins the .NET SDK and target framework:

- SDK: `10.0.300`
- Target framework: `net10.0`

Gate 8 adds:

- `ci/New-CiArtifacts.ps1` - local CI governance artifact writer for
  CI build manifest and checksums.

Gate 9 replaces the release-dry-run placeholder with `forge release verify`,
Forge-owned build manifests, and checksums.

Gate 10 replaces placeholder SARIF with `forge validate --format sarif`.

Gate 11 adds Markdown diagnostic summaries and GitHub annotations through the
CLI. CI stores Markdown summaries as artifacts and appends validation summaries
to the GitHub job summary when the runner provides `GITHUB_STEP_SUMMARY`.

Gate 323 adds a source-built Forge runner shim for this repository:

- `forge.ps1` - runs `src/WastelandForge.Cli/WastelandForge.Cli.csproj`
  through `dotnet run` from the repository root and passes arguments through
  unchanged.
- `forge.cmd` - Windows command adapter that invokes `forge.ps1`.

Examples:

```text
.\eng\forge.ps1 --version
.\eng\forge.cmd help
```

The shim is for local source-built development only. It does not publish a
package, create `.config/dotnet-tools.json`, install providers, run external
game tools, mutate generated workflows, or change the ADR-010 CLI command
surface.

Gate 325 adds local-tool package metadata to the CLI project. Local package
smoke output is written under ignored `artifacts/local-tool/`.

```text
dotnet pack src\WastelandForge.Cli\WastelandForge.Cli.csproj -c Release
```

The package installs command `forge` from package ID `WastelandForge.Cli` for
temporary local-tool smoke tests. The repository still does not check in
`.config/dotnet-tools.json` or publish the package.

Gate 326 plans the checked-in local tool manifest flow. Until the package is
published or another stable package feed is gated, restore must be explicit
about local package output:

```text
dotnet pack src\WastelandForge.Cli\WastelandForge.Cli.csproj -c Release
dotnet tool restore --add-source artifacts\local-tool\nupkg
```

The manifest itself remains future work in Gate 327.

Gate 327 checks in `.config/dotnet-tools.json`. Until `WastelandForge.Cli` is
published to a stable package feed, local restore still requires the package
output source:

```text
dotnet pack src\WastelandForge.Cli\WastelandForge.Cli.csproj -c Release
dotnet tool restore --add-source artifacts\local-tool\nupkg
dotnet tool run forge -- --version
dotnet tool run forge -- help
```

No root `NuGet.config` is added.

Gate 328 plans generated workflow and task bootstrap integration without
mutating generated scaffolds. The key decision is that this source repository
can pack and restore the local tool from `src/WastelandForge.Cli`, but
consumer projects created by `forge init` cannot assume that source tree is
present. Gate 329 is routed to add a repo-local restore helper before any
generated workflow or task template is changed.

Gate 329 adds that repo-local restore helper:

- `Restore-ForgeTool.ps1` - packs `WastelandForge.Cli`, writes an ignored
  local-only NuGet config and isolated package cache under
  `artifacts/local-tool/restore/gate-329/`, restores
  `.config/dotnet-tools.json`, and verifies the restored `forge` local tool
  unless `-NoVerify` is passed.

Example:

```text
pwsh -NoLogo -NoProfile -ExecutionPolicy Bypass -File eng/Restore-ForgeTool.ps1
$env:NUGET_PACKAGES = (Resolve-Path -LiteralPath artifacts/local-tool/restore/gate-329/packages).Path
dotnet tool run forge -- help
Remove-Item Env:\NUGET_PACKAGES
```

The helper is for this source repository only. It does not publish a package,
add a root `NuGet.config`, mutate generated workflows or tasks, install
providers, run external game tools, change the ADR-010 CLI command surface,
or add AI behavior.

Gate 330 wires this helper into the repository-owned CI workflow. The Ubuntu
validation lane restores the local tool before running `dotnet tool run forge`
for SARIF and GitHub/Markdown validation output, the mandatory Windows lane
restores the local tool as bootstrap smoke, and the release dry-run lane
restores the local tool before `dotnet tool run forge -- release verify`.
Generated `forge init` workflow and task templates still stay unchanged.

Gate 331 plans repository-owned developer task bootstrap for the same helper.
The planned source-repository `.vscode/tasks.json` should restore the local
tool through `Restore-ForgeTool.ps1`, set `NUGET_PACKAGES` to the ignored
developer restore cache, and run read-only canonical Forge commands through
`dotnet tool run forge`. Gate 331 does not create the task file; Gate 332 is
routed to do that scaffold.
