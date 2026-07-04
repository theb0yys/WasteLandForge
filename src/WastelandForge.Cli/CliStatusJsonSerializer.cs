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

    public static string SerializeReservedCommand(string commandPath, string? cleanScope = null)
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
        else if (StringComparer.Ordinal.Equals(commandPath, "clean"))
        {
            status["plannedScopes"] = CreateCleanScopeArray();
            if (!string.IsNullOrWhiteSpace(cleanScope) &&
                CleanScopeContracts.TryGetByScope(cleanScope, out var contract))
            {
                status["selectedScope"] = CreateCleanScope(contract);
            }

            status["reportContract"] = new JsonObject
            {
                ["status"] = "planned",
                ["summary"] = "Clean execution reports removed outputs, missing roots, and refused unsafe operations.",
                ["canonicalFormat"] = "json",
                ["mutatesFilesystemInCurrentGate"] = false
            };
            status["execution"] = new JsonObject
            {
                ["deleteBehavior"] = false,
                ["filesystemMutation"] = false,
                ["generatedManifestRead"] = false,
                ["buildManifestRead"] = false,
                ["provenanceSidecarRead"] = false,
                ["checksumRead"] = false,
                ["artifactExistenceCheck"] = false,
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

    private static JsonArray CreateCleanScopeArray()
    {
        var scopes = new JsonArray();
        foreach (var scope in CleanScopeContracts.All)
        {
            scopes.Add(CreateCleanScope(scope));
        }

        return scopes;
    }

    private static JsonObject CreateCleanScope(CleanScopeContract scope) =>
        new()
        {
            ["scope"] = scope.Scope,
            ["flag"] = scope.Flag,
            ["root"] = scope.Root,
            ["risk"] = scope.Risk,
            ["confirmation"] = scope.Confirmation,
            ["reportExpectation"] = scope.ReportExpectation,
            ["boundary"] = scope.Boundary
        };
}
