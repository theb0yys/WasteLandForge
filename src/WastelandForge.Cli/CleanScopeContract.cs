namespace WastelandForge.Cli;

internal sealed record CleanScopeContract(
    string Scope,
    string Flag,
    string Root,
    string Risk,
    string Confirmation,
    string ReportExpectation,
    string Boundary);

internal static class CleanScopeContracts
{
    public static IReadOnlyList<CleanScopeContract> All { get; } =
    [
        new CleanScopeContract(
            "generated",
            "--generated",
            "generated/",
            "safe",
            "non-interactive by default",
            "removed generated artifacts plus a clean report",
            "Generated docs, scripts, reports, metadata, manifests, and checksums only."),
        new CleanScopeContract(
            "dist",
            "--dist",
            "dist/",
            "safe",
            "non-interactive by default",
            "removed staged/package artifacts plus a clean report",
            "Packaged distributables, release dry-run outputs, build manifests, and checksums only."),
        new CleanScopeContract(
            "cache",
            "--cache",
            ".wastelandforge/cache/",
            "safe-with-active-build-warning",
            "non-interactive by default; warn if a build is active in a later execution gate",
            "removed cache state plus a clean report",
            "Incremental planner/cache state only."),
        new CleanScopeContract(
            "all",
            "--all",
            "generated/, dist/, .wastelandforge/cache/",
            "severe",
            "requires --yes and --confirm <project-id> in non-interactive mode; interactive TTY prompts must require the project ID",
            "removed generated, dist, and cache outputs plus a clean report",
            "Generated outputs, distribution outputs, and local cache only; canonical source stays out of scope.")
    ];

    public static bool TryGetByFlag(string flag, out CleanScopeContract contract)
    {
        foreach (var item in All)
        {
            if (StringComparer.Ordinal.Equals(item.Flag, flag))
            {
                contract = item;
                return true;
            }
        }

        contract = All[0];
        return false;
    }

    public static bool TryGetByScope(string scope, out CleanScopeContract contract)
    {
        foreach (var item in All)
        {
            if (StringComparer.Ordinal.Equals(item.Scope, scope))
            {
                contract = item;
                return true;
            }
        }

        contract = All[0];
        return false;
    }
}
