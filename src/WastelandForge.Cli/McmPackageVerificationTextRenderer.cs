using System.Text;

namespace WastelandForge.Cli;

internal static class McmPackageVerificationTextRenderer
{
    public static string Render(McmPackageVerificationCliResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        var builder = new StringBuilder();
        builder.AppendLine("WastelandForge Package Verification");
        builder.Append("Project: ");
        builder.AppendLine(result.ProjectRoot);
        builder.Append("Target: ");
        builder.AppendLine(result.Target);
        builder.Append("Mode: ");
        builder.AppendLine(result.Mode);
        builder.Append("Evidence: ");
        builder.AppendLine(result.Root);

        builder.AppendLine();
        builder.AppendLine("Evidence Files");
        AppendEvidence(builder, result.PackageManifest);
        AppendEvidence(builder, result.InstallPreview);
        AppendEvidence(builder, result.InstallPreviewSummary);
        AppendEvidence(builder, result.PackageVerification);
        AppendEvidence(builder, result.PackageVerificationSummary);
        AppendEvidence(builder, result.Checksums);
        AppendEvidence(builder, result.BuildManifest);
        if (result.PackageArchive is not null)
        {
            AppendEvidence(builder, result.PackageArchive);
        }

        builder.AppendLine();
        builder.AppendLine("Verify");
        if (result.HasErrors)
        {
            builder.Append("  FAIL ");
            builder.Append(result.Diagnostics.ErrorCount);
            builder.AppendLine(" blocking diagnostic(s)");
        }
        else
        {
            builder.AppendLine("  OK   no blocking diagnostics");
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

    private static void AppendEvidence(StringBuilder builder, string path)
    {
        builder.Append("  READ ");
        builder.AppendLine(path);
    }
}
