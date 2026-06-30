using System.Diagnostics;
using WastelandForge.Core;
using WastelandForge.Validation;

namespace WastelandForge.Desktop;

public sealed class ForgeMainForm : Form
{
    private readonly TextBox projectRootTextBox = new();
    private readonly TextBox geckExportTextBox = new();
    private readonly Button browseProjectButton = new();
    private readonly Button browseExportButton = new();
    private readonly Button validateButton = new();
    private readonly Button copyJsonButton = new();
    private readonly TextBox outputTextBox = new();
    private readonly Label statusLabel = new();
    private string lastJson = string.Empty;

    public ForgeMainForm()
    {
        Text = "WastelandForge";
        MinimumSize = new Size(860, 560);
        StartPosition = FormStartPosition.CenterScreen;
        Font = new Font("Segoe UI", 9F);

        var root = FindRepositoryRoot();
        if (root is not null)
        {
            projectRootTextBox.Text = Path.Combine(root, "fixtures", "projects", "ExampleMod");
        }

        BuildLayout();
    }

    private void BuildLayout()
    {
        var main = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 5,
            Padding = new Padding(16)
        };
        main.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        main.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        main.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        main.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        main.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        var title = new Label
        {
            AutoSize = true,
            Font = new Font(Font, FontStyle.Bold),
            Text = "WastelandForge validation"
        };
        main.Controls.Add(title, 0, 0);

        main.Controls.Add(BuildPathRow("Project root", projectRootTextBox, browseProjectButton, BrowseProjectRoot), 0, 1);
        main.Controls.Add(BuildPathRow("GECK dialogue export", geckExportTextBox, browseExportButton, BrowseGeckExport), 0, 2);

        outputTextBox.Dock = DockStyle.Fill;
        outputTextBox.Multiline = true;
        outputTextBox.ScrollBars = ScrollBars.Both;
        outputTextBox.ReadOnly = true;
        outputTextBox.WordWrap = false;
        outputTextBox.Font = new Font("Consolas", 9F);
        main.Controls.Add(outputTextBox, 0, 3);

        var bottom = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            AutoSize = true,
            WrapContents = false
        };

        validateButton.Text = "Validate";
        validateButton.AutoSize = true;
        validateButton.Click += (_, _) => ValidateProject();
        bottom.Controls.Add(validateButton);

        copyJsonButton.Text = "Copy JSON";
        copyJsonButton.AutoSize = true;
        copyJsonButton.Enabled = false;
        copyJsonButton.Click += (_, _) => Clipboard.SetText(lastJson);
        bottom.Controls.Add(copyJsonButton);

        statusLabel.AutoSize = true;
        statusLabel.Text = "Ready";
        statusLabel.TextAlign = ContentAlignment.MiddleLeft;
        bottom.Controls.Add(statusLabel);

        main.Controls.Add(bottom, 0, 4);
        Controls.Add(main);
    }

    private static Control BuildPathRow(string labelText, TextBox textBox, Button button, EventHandler browseHandler)
    {
        var row = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            ColumnCount = 3,
            AutoSize = true,
            Padding = new Padding(0, 10, 0, 0)
        };
        row.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150));
        row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        row.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

        var label = new Label
        {
            Text = labelText,
            TextAlign = ContentAlignment.MiddleLeft,
            Dock = DockStyle.Fill,
            AutoSize = true
        };

        textBox.Dock = DockStyle.Fill;

        button.Text = "Browse";
        button.AutoSize = true;
        button.Click += browseHandler;

        row.Controls.Add(label, 0, 0);
        row.Controls.Add(textBox, 1, 0);
        row.Controls.Add(button, 2, 0);
        return row;
    }

    private void BrowseProjectRoot(object? sender, EventArgs e)
    {
        using var dialog = new FolderBrowserDialog
        {
            Description = "Select the WastelandForge project root.",
            SelectedPath = Directory.Exists(projectRootTextBox.Text) ? projectRootTextBox.Text : Environment.CurrentDirectory,
            UseDescriptionForTitle = true
        };

        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            projectRootTextBox.Text = dialog.SelectedPath;
        }
    }

    private void BrowseGeckExport(object? sender, EventArgs e)
    {
        using var dialog = new OpenFileDialog
        {
            Title = "Select GECK dialogue export",
            Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*",
            CheckFileExists = true
        };

        if (File.Exists(geckExportTextBox.Text))
        {
            dialog.FileName = geckExportTextBox.Text;
        }

        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            geckExportTextBox.Text = dialog.FileName;
        }
    }

    private void ValidateProject()
    {
        validateButton.Enabled = false;
        copyJsonButton.Enabled = false;
        outputTextBox.Clear();
        statusLabel.Text = "Running validation...";

        try
        {
            var projectRoot = string.IsNullOrWhiteSpace(projectRootTextBox.Text)
                ? "."
                : projectRootTextBox.Text.Trim();

            var report = new ProjectValidationPipeline().Validate(projectRoot);
            if (!string.IsNullOrWhiteSpace(geckExportTextBox.Text))
            {
                var geckIssues = new GeckDialogueExportValidator().Validate(geckExportTextBox.Text.Trim(), report.ProjectId);
                report = new DiagnosticReport(report.ProjectId, report.Issues.Concat(geckIssues));
            }

            lastJson = DiagnosticReportJsonSerializer.Serialize(report, "0.1.0");
            outputTextBox.Text = RenderReport(report) + Environment.NewLine + Environment.NewLine + lastJson;
            copyJsonButton.Enabled = true;
            statusLabel.Text = report.HasErrors
                ? $"Failed: {report.ErrorCount} error(s)"
                : "Passed";
        }
        catch (Exception ex)
        {
            lastJson = string.Empty;
            outputTextBox.Text = ex.ToString();
            statusLabel.Text = "Internal error";
        }
        finally
        {
            validateButton.Enabled = true;
        }
    }

    private static string RenderReport(DiagnosticReport report)
    {
        var lines = new List<string>
        {
            $"validate: {report.ErrorCount} error(s), {report.WarningCount} warning(s), {report.NoteCount} note(s)"
        };

        foreach (var issue in report.Issues)
        {
            lines.Add(string.Empty);
            lines.Add($"{issue.Severity.ToString().ToUpperInvariant()} {issue.RuleId} {FormatLocation(issue.PrimaryLocation)}");
            lines.Add(issue.Title);
            lines.Add(issue.Message);
            if (!string.IsNullOrWhiteSpace(issue.SuggestedFix))
            {
                lines.Add($"Fix: {issue.SuggestedFix}");
            }
        }

        return string.Join(Environment.NewLine, lines);
    }

    private static string FormatLocation(SourceLocation location) =>
        location.Pointer is null
            ? location.File
            : $"{location.File}#{location.Pointer}";

    private static string? FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "WastelandForge.sln")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        return null;
    }
}
