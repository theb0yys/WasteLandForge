using System.Text;
using WastelandForge.Generation;

namespace WastelandForge.Cli;

internal static class ProjectSourceGraphTextRenderer
{
    public static string Render(ProjectSourceGraphResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        var builder = new StringBuilder();
        builder.AppendLine("WastelandForge Graph");
        builder.Append("Project: ");
        builder.AppendLine(result.ProjectId?.ToString() ?? "unknown");
        builder.Append("Target: ");
        builder.AppendLine(result.Target);
        builder.Append("Mode: ");
        builder.AppendLine(result.DryRun ? "dry-run" : "write");
        if (result.Outputs is not null)
        {
            builder.Append("Output: ");
            builder.AppendLine(result.Outputs.Root);
        }

        builder.AppendLine();
        builder.AppendLine("Validate");
        if (result.Diagnostics.HasErrors)
        {
            builder.Append("  FAIL ");
            builder.Append(result.Diagnostics.ErrorCount);
            builder.AppendLine(" blocking diagnostic(s)");
        }
        else
        {
            builder.AppendLine("  OK   no blocking diagnostics");
        }

        if (!result.Diagnostics.HasErrors && result.Outputs is not null)
        {
            builder.AppendLine();
            builder.AppendLine("Graph");
            builder.AppendLine(result.DryRun ? "  PLAN project-source-graph.json" : "  OK   project-source-graph.json written");
            builder.AppendLine(result.DryRun ? "  PLAN project-source-graph.md" : "  OK   project-source-graph.md written");
            builder.AppendLine(result.DryRun ? "  PLAN graph-manifest.json" : "  OK   graph-manifest.json written");
            builder.AppendLine(result.DryRun ? "  PLAN checksums.sha256" : "  OK   checksums.sha256 written");

            builder.AppendLine();
            builder.AppendLine("Graph summary");
            builder.Append("  Nodes: ");
            builder.AppendLine(result.Summary.Nodes.ToString());
            builder.Append("  Edges: ");
            builder.AppendLine(result.Summary.Edges.ToString());
            builder.Append("  Source documents: ");
            builder.AppendLine(result.Summary.SourceDocuments.ToString());
            builder.Append("  Output boundaries: ");
            builder.AppendLine(result.Summary.OutputBoundaries.ToString());
            builder.Append("  Capability requirements: ");
            builder.AppendLine(result.Summary.CapabilityRequirements.ToString());
            builder.Append("  Referenced capabilities: ");
            builder.AppendLine(result.Summary.ReferencedCapabilities.ToString());
            builder.Append("  Referenced providers: ");
            builder.AppendLine(result.Summary.ReferencedProviders.ToString());
            builder.Append("  Generator targets: ");
            builder.AppendLine(result.Summary.GeneratorTargets.ToString());
            builder.Append("  Generator target input edges: ");
            builder.AppendLine(result.Summary.GeneratorTargetInputEdges.ToString());
            builder.Append("  Generator target output edges: ");
            builder.AppendLine(result.Summary.GeneratorTargetOutputEdges.ToString());
        }

        foreach (var issue in result.Diagnostics.Issues)
        {
            builder.AppendLine();
            builder.Append(issue.Severity.ToString().ToUpperInvariant());
            builder.Append(' ');
            builder.Append(issue.RuleId);
            builder.Append(' ');
            builder.AppendLine(issue.Title);
            builder.Append("  ");
            builder.AppendLine(issue.Message);
        }

        builder.AppendLine();
        builder.AppendLine("Result");
        builder.Append("  ");
        builder.Append(result.Diagnostics.ErrorCount);
        builder.Append(" error(s), ");
        builder.Append(result.Diagnostics.WarningCount);
        builder.Append(" warning(s), ");
        builder.Append(result.Diagnostics.NoteCount);
        builder.AppendLine(" note(s)");

        return builder.ToString();
    }
}
