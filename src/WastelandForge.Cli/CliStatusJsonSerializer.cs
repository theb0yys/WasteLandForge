using System.Text.Json;
using System.Text.Json.Nodes;

namespace WastelandForge.Cli;

internal static class CliStatusJsonSerializer
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    public static string SerializeReservedCommand(string commandPath)
    {
        return CreateStatus(commandPath, "reserved", $"forge {commandPath} is reserved by ADR-010 but is not implemented in Gate 6.", 1)
            .ToJsonString(SerializerOptions);
    }

    public static string SerializeUsageError(string commandPath, string message)
    {
        return CreateStatus(commandPath, "usage-error", message, 1)
            .ToJsonString(SerializerOptions);
    }

    private static JsonObject CreateStatus(string commandPath, string status, string message, int errors)
    {
        return new JsonObject
        {
            ["formatVersion"] = CliConstants.JsonFormatVersion,
            ["tool"] = new JsonObject
            {
                ["name"] = CliConstants.ToolName,
                ["version"] = CliConstants.Version
            },
            ["command"] = commandPath,
            ["status"] = status,
            ["summary"] = new JsonObject
            {
                ["errors"] = errors,
                ["warnings"] = 0,
                ["notes"] = 0
            },
            ["message"] = message
        };
    }
}
