using System.Text.Json;
using System.Text.Json.Nodes;

namespace WastelandForge.Core;

public static class DiagnosticReportSarifSerializer
{
    private const string SchemaUri = "https://json.schemastore.org/sarif-2.1.0.json";
    private const string SarifVersion = "2.1.0";

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    public static string Serialize(DiagnosticReport report, string? toolVersion = null, string command = "validate")
    {
        ArgumentNullException.ThrowIfNull(report);
        ArgumentException.ThrowIfNullOrWhiteSpace(command);

        var rules = new JsonArray();
        foreach (var issue in report.Issues
            .GroupBy(issue => issue.RuleId.ToString(), StringComparer.Ordinal)
            .OrderBy(group => group.Key, StringComparer.Ordinal)
            .Select(group => group.First()))
        {
            rules.Add(ToRule(issue));
        }

        var results = new JsonArray();
        foreach (var issue in report.Issues)
        {
            results.Add(ToResult(issue));
        }

        var driver = new JsonObject
        {
            ["name"] = "WastelandForge",
            ["informationUri"] = "https://github.com/theb0yys/WasteLandForge"
        };
        if (!string.IsNullOrWhiteSpace(toolVersion))
        {
            driver["semanticVersion"] = toolVersion;
        }

        driver["rules"] = rules;

        var run = new JsonObject
        {
            ["tool"] = new JsonObject
            {
                ["driver"] = driver
            },
            ["results"] = results,
            ["properties"] = CreateRunProperties(report, command)
        };

        var root = new JsonObject
        {
            ["$schema"] = SchemaUri,
            ["version"] = SarifVersion,
            ["runs"] = new JsonArray(run)
        };

        return root.ToJsonString(SerializerOptions);
    }

    private static JsonObject ToRule(DiagnosticIssue issue)
    {
        var rule = new JsonObject
        {
            ["id"] = issue.RuleId.ToString(),
            ["name"] = issue.Category,
            ["shortDescription"] = new JsonObject
            {
                ["text"] = issue.Title
            },
            ["fullDescription"] = new JsonObject
            {
                ["text"] = issue.Message
            },
            ["defaultConfiguration"] = new JsonObject
            {
                ["level"] = ToSarifLevel(issue.Severity)
            },
            ["properties"] = new JsonObject
            {
                ["category"] = issue.Category
            }
        };

        var helpText = CreateHelpMarkdown(issue);
        if (!string.IsNullOrWhiteSpace(helpText))
        {
            rule["help"] = new JsonObject
            {
                ["markdown"] = helpText
            };
        }

        return rule;
    }

    private static JsonObject ToResult(DiagnosticIssue issue)
    {
        var result = new JsonObject
        {
            ["ruleId"] = issue.RuleId.ToString(),
            ["level"] = ToSarifLevel(issue.Severity),
            ["message"] = new JsonObject
            {
                ["text"] = issue.Message
            },
            ["locations"] = new JsonArray(
                new JsonObject
                {
                    ["physicalLocation"] = ToPhysicalLocation(issue.PrimaryLocation)
                }),
            ["partialFingerprints"] = new JsonObject
            {
                ["wastelandforgeFingerprint"] = CreateFingerprint(issue)
            },
            ["properties"] = new JsonObject
            {
                ["category"] = issue.Category,
                ["title"] = issue.Title
            }
        };

        if (issue.ProjectId is not null)
        {
            result["properties"]!["projectId"] = issue.ProjectId.ToString();
        }

        if (issue.Evidence.Count > 0)
        {
            result["properties"]!["evidence"] = new JsonArray(issue.Evidence.Select(item => JsonValue.Create(item)).ToArray());
        }

        if (issue.RelatedLocations.Count > 0)
        {
            var relatedLocations = new JsonArray();
            for (var index = 0; index < issue.RelatedLocations.Count; index++)
            {
                relatedLocations.Add(new JsonObject
                {
                    ["id"] = index + 1,
                    ["physicalLocation"] = ToPhysicalLocation(issue.RelatedLocations[index]),
                    ["message"] = new JsonObject
                    {
                        ["text"] = "Related WastelandForge source location."
                    }
                });
            }

            result["relatedLocations"] = relatedLocations;
        }

        return result;
    }

    private static JsonObject ToPhysicalLocation(SourceLocation location)
    {
        var physicalLocation = new JsonObject
        {
            ["artifactLocation"] = new JsonObject
            {
                ["uri"] = ToSarifUri(location.File),
                ["uriBaseId"] = "%SRCROOT%"
            }
        };

        if (location.Line is not null)
        {
            var region = new JsonObject
            {
                ["startLine"] = location.Line
            };
            if (location.Column is not null)
            {
                region["startColumn"] = location.Column;
            }

            physicalLocation["region"] = region;
        }

        if (location.Pointer is not null)
        {
            physicalLocation["properties"] = new JsonObject
            {
                ["jsonPointer"] = location.Pointer.ToString()
            };
        }

        return physicalLocation;
    }

    private static JsonObject CreateRunProperties(DiagnosticReport report, string command)
    {
        var properties = new JsonObject
        {
            ["command"] = command,
            ["formatVersion"] = "1.0"
        };

        if (report.ProjectId is not null)
        {
            properties["projectId"] = report.ProjectId.ToString();
        }

        return properties;
    }

    private static string ToSarifLevel(DiagnosticSeverity severity) => severity switch
    {
        DiagnosticSeverity.Error => "error",
        DiagnosticSeverity.Warning => "warning",
        DiagnosticSeverity.Note => "note",
        _ => throw new ArgumentOutOfRangeException(nameof(severity), severity, null)
    };

    private static string ToSarifUri(string file)
    {
        return file
            .Replace('\\', '/')
            .TrimStart('.', '/');
    }

    private static string CreateFingerprint(DiagnosticIssue issue)
    {
        if (!string.IsNullOrWhiteSpace(issue.Fingerprint))
        {
            return issue.Fingerprint;
        }

        return string.Join(
            ':',
            issue.RuleId.ToString(),
            issue.PrimaryLocation.File.Replace('\\', '/'),
            issue.PrimaryLocation.Pointer?.ToString() ?? "",
            issue.Title);
    }

    private static string? CreateHelpMarkdown(DiagnosticIssue issue)
    {
        if (issue.DocsUri is null && string.IsNullOrWhiteSpace(issue.SuggestedFix))
        {
            return null;
        }

        var parts = new List<string>();
        if (!string.IsNullOrWhiteSpace(issue.SuggestedFix))
        {
            parts.Add(issue.SuggestedFix);
        }

        if (issue.DocsUri is not null)
        {
            parts.Add($"Documentation: {issue.DocsUri}");
        }

        return string.Join(Environment.NewLine + Environment.NewLine, parts);
    }
}
