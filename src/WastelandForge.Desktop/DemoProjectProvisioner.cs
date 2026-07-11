using System.IO;

namespace WastelandForge.Desktop;

internal sealed record DemoProjectProvisionResult(bool Success, string? ProjectPath, string? Error);

internal static class DemoProjectProvisioner
{
    public static DemoProjectProvisionResult Prepare(string sourceProject, string demoRoot, bool reset, string projectName = "ExampleMod")
    {
        try
        {
            if (!Directory.Exists(sourceProject)) return new(false, null, "Bundled demo project was not found.");
            var fullRoot = Path.GetFullPath(demoRoot);
            var project = Path.Combine(fullRoot, projectName);
            if (!IsUnderDirectory(project, fullRoot)) return new(false, null, "Refusing to prepare demo outside its local root.");

            if (reset && Directory.Exists(project)) Directory.Delete(project, recursive: true);
            if (!Directory.Exists(project)) CopyProjectDirectory(sourceProject, project, skipProjectOutputRoots: true);
            return new(true, project, null);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException or NotSupportedException)
        {
            return new(false, null, ex.Message);
        }
    }

    private static void CopyProjectDirectory(string sourceDirectory, string targetDirectory, bool skipProjectOutputRoots)
    {
        Directory.CreateDirectory(targetDirectory);
        foreach (var file in Directory.EnumerateFiles(sourceDirectory))
            File.Copy(file, Path.Combine(targetDirectory, Path.GetFileName(file)), overwrite: true);

        foreach (var directory in Directory.EnumerateDirectories(sourceDirectory))
        {
            var name = Path.GetFileName(directory);
            if (skipProjectOutputRoots && (name.Equals("generated", StringComparison.OrdinalIgnoreCase) ||
                                           name.Equals("dist", StringComparison.OrdinalIgnoreCase))) continue;
            CopyProjectDirectory(directory, Path.Combine(targetDirectory, name), skipProjectOutputRoots: false);
        }
    }

    private static bool IsUnderDirectory(string path, string parent)
    {
        var normalizedPath = Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var normalizedParent = Path.GetFullPath(parent).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        return normalizedPath.Equals(normalizedParent, StringComparison.OrdinalIgnoreCase) ||
               normalizedPath.StartsWith(normalizedParent + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) ||
               normalizedPath.StartsWith(normalizedParent + Path.AltDirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);
    }
}
