using System.Runtime.InteropServices;
using System.Text.Json;
using WastelandForge.Validation;

namespace WastelandForge.WindowsTests;

public sealed class WindowsPathTests
{
    [Fact]
    public void RegistryPathsCannotEscapeProjectRoot()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return;
        }

        var projectRoot = CreateProject(
            "escaping",
            """
            {
              "schemaVersion": "0.1.0",
              "kind": "manifest",
              "id": "io.github.theboyyss.escape",
              "name": "Escaping Paths",
              "version": "0.1.0",
              "game": "falloutnv",
              "registries": {
                "dependencies": "..\\outside\\dependencies",
                "capabilities": "src\\registries\\capabilities"
              }
            }
            """);

        Directory.CreateDirectory(Path.Combine(projectRoot, "src", "registries", "capabilities"));
        File.WriteAllText(
            Path.Combine(projectRoot, "src", "registries", "capabilities", "runtime.json"),
            """
            {
              "schemaVersion": "0.1.0",
              "kind": "capability",
              "id": "runtime.scripting.xnvse",
              "title": "xNVSE runtime scripting",
              "satisfiedBy": [
                {
                  "id": "provider.xnvse",
                  "providerType": "runtime"
                }
              ],
              "requires": {
                "capabilities": []
              },
              "scope": "runtime-session",
              "stability": "stable",
              "features": [
                "script-extender"
              ]
            }
            """);

        var report = new ProjectValidationPipeline().Validate(projectRoot);

        var issue = Assert.Single(report.Issues);
        Assert.Equal("WF-LOAD-005", issue.RuleId.ToString());
        Assert.Equal("load", issue.Category);
    }

    [Fact]
    public void WindowsRegistrySeparatorsProduceStableForwardSlashLocations()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return;
        }

        var projectRoot = CreateProject(
            "stable",
            """
            {
              "schemaVersion": "0.1.0",
              "kind": "manifest",
              "id": "io.github.theboyyss.windows",
              "name": "Windows Paths",
              "version": "0.1.0",
              "game": "falloutnv",
              "registries": {
                "dependencies": "src\\registries\\dependencies",
                "capabilities": "src\\registries\\capabilities"
              }
            }
            """);

        Directory.CreateDirectory(Path.Combine(projectRoot, "src", "registries", "dependencies"));
        Directory.CreateDirectory(Path.Combine(projectRoot, "src", "registries", "capabilities"));
        File.WriteAllText(
            Path.Combine(projectRoot, "src", "registries", "dependencies", "main.json"),
            """
            {
              "schemaVersion": "0.1.0",
              "kind": "dependency",
              "id": "io.github.theboyyss.windows.dependencies",
              "requires": {
                "capabilities": [
                  {
                    "id": "runtime.ui.fakeprovider"
                  }
                ]
              }
            }
            """);
        File.WriteAllText(
            Path.Combine(projectRoot, "src", "registries", "capabilities", "runtime.json"),
            """
            {
              "schemaVersion": "0.1.0",
              "kind": "capability",
              "id": "runtime.scripting.xnvse",
              "title": "xNVSE runtime scripting",
              "satisfiedBy": [
                {
                  "id": "provider.xnvse",
                  "providerType": "runtime"
                }
              ],
              "requires": {
                "capabilities": []
              },
              "scope": "runtime-session",
              "stability": "stable",
              "features": [
                "script-extender"
              ]
            }
            """);

        var report = new ProjectValidationPipeline().Validate(projectRoot);

        var issue = Assert.Single(report.Issues);
        Assert.Equal("src/registries/dependencies/main.json", issue.PrimaryLocation.File);
    }

    private static string CreateProject(string name, string manifest)
    {
        var root = Path.Combine(Path.GetTempPath(), "WastelandForge.Tests", Guid.NewGuid().ToString("N"), name);
        Directory.CreateDirectory(root);
        File.WriteAllText(Path.Combine(root, "wastelandforge.json"), manifest);
        return root;
    }
}
