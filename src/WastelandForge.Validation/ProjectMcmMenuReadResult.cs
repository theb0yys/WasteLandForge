using System.Text.Json.Nodes;
using WastelandForge.Core;

namespace WastelandForge.Validation;

public sealed record ProjectMcmMenuReadResult(
    string ProjectRoot,
    LogicalId? ProjectId,
    DiagnosticReport Diagnostics,
    IReadOnlyList<McmMenuDefinition> Menus);

public sealed record McmMenuDefinition(
    string Id,
    string Title,
    string OutputFile,
    double MinMcmVersion,
    IReadOnlyList<string> RequiredCapabilities,
    JsonArray? RuntimeRequirements,
    IReadOnlyDictionary<string, string> Translations,
    IReadOnlyList<McmPageDefinition> Pages,
    SourceLocation Source);

public sealed record McmPageDefinition(
    string Id,
    string Title,
    JsonArray? RuntimeRequirements,
    IReadOnlyList<McmSettingDefinition> Settings);

public sealed record McmSettingDefinition(
    string Id,
    string Label,
    string SettingType,
    JsonNode? Default,
    McmIniBinding? Ini,
    IReadOnlyList<string> Choices,
    McmSettingScale? Scale,
    string? TextOn,
    string? TextOff,
    McmImageDefinition? Image);

public sealed record McmIniBinding(
    string File,
    string Section,
    string Key);

public sealed record McmSettingScale(
    double ValueMin,
    double ValueMax,
    double ValueIncrement,
    long ValueDecimal);

public sealed record McmImageDefinition(
    string Filename,
    long Width,
    long Height,
    long SystemColor,
    long? OffsetX,
    long? OffsetY);
