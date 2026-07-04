namespace WastelandForge.Cli;

internal sealed record ExplainSubjectContract(
    string Subject,
    string Usage,
    string Purpose,
    string Boundary);

internal static class ExplainSubjectContracts
{
    public static IReadOnlyList<ExplainSubjectContract> All { get; } =
    [
        new(
            "diagnostic",
            "forge explain diagnostic <rule-id>",
            "Explain a diagnostic rule, why it exists, and the next recovery action.",
            "Implemented as deterministic family metadata plus documented rule metadata where available; project diagnostic lookup is future work."),
        new(
            "target",
            "forge explain target <target-id>",
            "Explain a generation, build, docs, graph, package, or release target.",
            "Implemented as deterministic target metadata; no build planning or generator execution is performed."),
        new(
            "output",
            "forge explain output <generated-or-dist-path>",
            "Explain the expected provenance and rebuild path for a generated or packaged output.",
            "Implemented as deterministic output path classification; no manifest read or artifact existence check is performed."),
        new(
            "capability",
            "forge explain capability <capability-id>",
            "Explain how a capability contributes to validation, generation, or recovery.",
            "Implemented as deterministic built-in capability catalogue metadata; no local provider evidence is read."),
        new(
            "provenance",
            "forge explain provenance <manifest-or-output-path>",
            "Explain how provenance evidence should connect sources, schemas, capabilities, generators, and outputs.",
            "Implemented as deterministic provenance boundary planning; no build manifest or provenance sidecar is read.")
    ];
}
