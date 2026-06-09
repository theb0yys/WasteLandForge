namespace WastelandForge.Cli;

internal static class CliConstants
{
    public const string ToolName = "WastelandForge";
    public const string Version = "0.1.0";
    public const string JsonFormatVersion = "1.0";

    public static bool IsMachineFormat(string format)
    {
        return StringComparer.Ordinal.Equals(format, "json");
    }

    public static bool IsTextFormat(string format)
    {
        return StringComparer.Ordinal.Equals(format, "human") ||
            StringComparer.Ordinal.Equals(format, "plain");
    }

    public static bool IsKnownFormat(string format)
    {
        return IsTextFormat(format) ||
            IsMachineFormat(format) ||
            StringComparer.Ordinal.Equals(format, "sarif") ||
            StringComparer.Ordinal.Equals(format, "github");
    }
}
