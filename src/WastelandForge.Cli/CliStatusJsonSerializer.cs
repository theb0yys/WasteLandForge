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
        var status = CreateStatus(commandPath, "reserved", $"forge {commandPath} is reserved by ADR-010 but is not implemented in the current gate.", 1);
        if (StringComparer.Ordinal.Equals(commandPath, "explain"))
        {
            status["plannedSubjects"] = CreateExplainSubjectArray();
            status["execution"] = new JsonObject
            {
                ["subjectParsing"] = false,
                ["manifestRead"] = false,
                ["artifactExistenceCheck"] = false,
                ["provenanceSidecarRead"] = false,
                ["buildPlanning"] = false,
                ["generatorExecution"] = false,
                ["packageExecution"] = false,
                ["releaseExecution"] = false,
                ["providerResolution"] = false,
                ["capabilityScanBehaviorChange"] = false,
                ["externalToolExecution"] = false,
                ["aiRequired"] = false
            };
        }

        return status.ToJsonString(SerializerOptions);
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

    private static JsonArray CreateExplainSubjectArray()
    {
        var subjects = new JsonArray();
        foreach (var subject in ExplainSubjectContracts.All)
        {
            subjects.Add(new JsonObject
            {
                ["subject"] = subject.Subject,
                ["usage"] = subject.Usage,
                ["purpose"] = subject.Purpose,
                ["boundary"] = subject.Boundary
            });
        }

        return subjects;
    }
}
