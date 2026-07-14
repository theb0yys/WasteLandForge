using System.IO;

namespace WastelandForge.Desktop;

internal static class WastelandForgeLocalData
{
    internal const string OverrideVariable = "WASTELANDFORGE_LOCAL_APP_DATA";
    internal const string FnvUserStateOverrideVariable = "WASTELANDFORGE_FNV_USER_STATE";

    internal static string Root => ResolveRoot(Environment.GetEnvironmentVariable(OverrideVariable));
    internal static string FnvUserStateRoot => ResolveFnvUserStateRoot(Environment.GetEnvironmentVariable(FnvUserStateOverrideVariable));

    internal static string Combine(params string[] segments)
    {
        return Path.Combine([Root, .. segments]);
    }

    internal static string ResolveRoot(string? configuredOverride)
    {
        if (string.IsNullOrWhiteSpace(configuredOverride))
        {
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "WastelandForge");
        }

        if (!Path.IsPathRooted(configuredOverride))
        {
            throw new InvalidOperationException($"{OverrideVariable} must be an absolute path.");
        }

        var root = Path.GetFullPath(configuredOverride);
        if (string.Equals(root.TrimEnd(Path.DirectorySeparatorChar), Path.GetPathRoot(root)?.TrimEnd(Path.DirectorySeparatorChar), StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"{OverrideVariable} cannot be a filesystem root.");
        }

        return root;
    }

    internal static string ResolveFnvUserStateRoot(string? configuredOverride)
    {
        if (string.IsNullOrWhiteSpace(configuredOverride))
        {
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "FalloutNV");
        }

        if (!Path.IsPathRooted(configuredOverride))
        {
            throw new InvalidOperationException($"{FnvUserStateOverrideVariable} must be an absolute path.");
        }

        var root = Path.GetFullPath(configuredOverride);
        if (string.Equals(root.TrimEnd(Path.DirectorySeparatorChar), Path.GetPathRoot(root)?.TrimEnd(Path.DirectorySeparatorChar), StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"{FnvUserStateOverrideVariable} cannot be a filesystem root.");
        }

        return root;
    }

    internal static string SuggestFnvIniPath(string? documentsRoot = null)
    {
        try
        {
            var root = documentsRoot ?? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            if (string.IsNullOrWhiteSpace(root)) return string.Empty;
            var candidate = Path.GetFullPath(Path.Combine(root, "My Games", "FalloutNV", "Fallout.ini"));
            if (!File.Exists(candidate)) return string.Empty;
            var attributes = File.GetAttributes(candidate);
            return (attributes & (FileAttributes.Directory | FileAttributes.ReparsePoint)) == 0 ? candidate : string.Empty;
        }
        catch (Exception exception) when (exception is ArgumentException or IOException or UnauthorizedAccessException or NotSupportedException)
        {
            return string.Empty;
        }
    }
}
