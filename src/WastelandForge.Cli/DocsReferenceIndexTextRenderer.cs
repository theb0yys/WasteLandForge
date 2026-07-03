using System.Text;
using WastelandForge.Generation;

namespace WastelandForge.Cli;

internal static class DocsReferenceIndexTextRenderer
{
    public static string Render(DocsReferenceIndexResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        var builder = new StringBuilder();
        builder.AppendLine("WastelandForge Docs");
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
            builder.AppendLine("Docs");
            builder.AppendLine(result.DryRun ? "  PLAN reference-index.json" : "  OK   reference-index.json written");
            builder.AppendLine(result.DryRun ? "  PLAN reference-index.md" : "  OK   reference-index.md written");
            builder.Append("  ");
            builder.Append(result.DryRun ? "PLAN " : "OK   ");
            builder.Append(result.Summary.SchemaReferences);
            builder.AppendLine(result.Summary.SchemaReferences == 1
                ? " schema reference page"
                : " schema reference pages");
            builder.Append("  ");
            builder.Append(result.DryRun ? "PLAN " : "OK   ");
            builder.Append(result.Summary.RegistryReferences);
            builder.AppendLine(result.Summary.RegistryReferences == 1
                ? " registry reference page"
                : " registry reference pages");
            builder.Append("  ");
            builder.Append(result.DryRun ? "PLAN " : "OK   ");
            builder.Append(result.Summary.RuleReferences);
            builder.AppendLine(result.Summary.RuleReferences == 1
                ? " rule reference page"
                : " rule reference pages");
            builder.Append("  ");
            builder.Append(result.DryRun ? "PLAN " : "OK   ");
            builder.Append(result.Summary.CapabilityReferences);
            builder.AppendLine(result.Summary.CapabilityReferences == 1
                ? " capability reference page"
                : " capability reference pages");
            builder.AppendLine(result.DryRun ? "  PLAN docs-manifest.json" : "  OK   docs-manifest.json written");
            builder.AppendLine(result.DryRun ? "  PLAN checksums.sha256" : "  OK   checksums.sha256 written");

            builder.AppendLine();
            builder.AppendLine("Reference summary");
            builder.Append("  Schemas: ");
            builder.AppendLine(result.Summary.Schemas.ToString());
            builder.Append("  Schema references: ");
            builder.AppendLine(result.Summary.SchemaReferences.ToString());
            builder.Append("  Registries: ");
            builder.AppendLine(result.Summary.Registries.ToString());
            builder.Append("  Registry references: ");
            builder.AppendLine(result.Summary.RegistryReferences.ToString());
            builder.Append("  Rule families: ");
            builder.AppendLine(result.Summary.RuleFamilies.ToString());
            builder.Append("  Rule references: ");
            builder.AppendLine(result.Summary.RuleReferences.ToString());
            builder.Append("  Capabilities: ");
            builder.AppendLine(result.Summary.Capabilities.ToString());
            builder.Append("  Capability references: ");
            builder.AppendLine(result.Summary.CapabilityReferences.ToString());
            builder.Append("  Providers: ");
            builder.AppendLine(result.Summary.Providers.ToString());
            builder.Append("  Commands: ");
            builder.AppendLine(result.Summary.Commands.ToString());
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
