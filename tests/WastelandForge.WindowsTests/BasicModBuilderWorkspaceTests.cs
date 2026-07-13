using WastelandForge.Cli;
using WastelandForge.Desktop;
using System.Security.Cryptography;

namespace WastelandForge.WindowsTests;

[Collection("Forge CLI console")]
public sealed class BasicModBuilderWorkspaceTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task PreviewCreateAndVerifyFomodForBothStarterModes(bool includeJip)
    {
        var parent = Path.Combine(Path.GetTempPath(), "WastelandForge.WindowsTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(parent);
        try
        {
            var workspace = new BasicModBuilderWorkspace(new InProcessRunner());
            var input = Input(parent, includeJip);
            var preview = workspace.Preview(input);
            Assert.True(preview.Success, preview.Message);
            Assert.False(Directory.Exists(preview.Destination));

            var result = await workspace.CreateAsync(input, preview.Token!, CancellationToken.None);
            Assert.True(result.Success, result.Message);
            Assert.True(File.Exists(result.FomodArchive));
            Assert.True(result.ArchiveLength > 0);
            Assert.Equal(64, result.ArchiveSha256!.Length);
            Assert.All(result.Stages, stage => Assert.Equal("completed", stage.State));
            Assert.Equal(includeJip, File.Exists(Path.Combine(result.ProjectRoot!, "src", "registries", "jip-scripts", "main.json")));
            Assert.Equal(0, ForgeCli.Run(["validate", result.ProjectRoot!, "--format", "json", "--no-input"]));
        }
        finally { if (Directory.Exists(parent)) Directory.Delete(parent, recursive: true); }
    }

    [Fact]
    public async Task StalePreviewAndFailedPipelineLeaveNoDestinationOrWorkRoot()
    {
        var parent = Path.Combine(Path.GetTempPath(), "WastelandForge.WindowsTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(parent);
        try
        {
            var input = Input(parent, true);
            var workspace = new BasicModBuilderWorkspace(new InProcessRunner());
            var preview = workspace.Preview(input);
            var stale = await workspace.CreateAsync(input with { MenuTitle = "Changed" }, preview.Token!, CancellationToken.None);
            Assert.False(stale.Success);
            Assert.False(Directory.Exists(preview.Destination));

            workspace = new BasicModBuilderWorkspace(new FailingRunner("validate"));
            preview = workspace.Preview(input);
            var failed = await workspace.CreateAsync(input, preview.Token!, CancellationToken.None);
            Assert.False(failed.Success);
            Assert.False(Directory.Exists(preview.Destination));
            Assert.Empty(Directory.GetDirectories(parent, ".wastelandforge-create-*"));
        }
        finally { if (Directory.Exists(parent)) Directory.Delete(parent, recursive: true); }
    }

    [Fact]
    public async Task CancellationLeavesNoDestinationOrTransactionFolder()
    {
        var parent = Path.Combine(Path.GetTempPath(), "WastelandForge.WindowsTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(parent);
        try
        {
            var workspace = new BasicModBuilderWorkspace(new InProcessRunner());
            var input = Input(parent, true);
            var preview = workspace.Preview(input);
            using var cancellation = new CancellationTokenSource();
            cancellation.Cancel();
            var result = await workspace.CreateAsync(input, preview.Token!, cancellation.Token);
            Assert.False(result.Success);
            Assert.Contains("cancelled", result.Message, StringComparison.OrdinalIgnoreCase);
            Assert.False(Directory.Exists(preview.Destination));
            Assert.Empty(Directory.GetDirectories(parent, ".wastelandforge-create-*"));
        }
        finally { if (Directory.Exists(parent)) Directory.Delete(parent, recursive: true); }
    }

    [Fact]
    public async Task CreatedProjectReopensForSpecialistEditsAndDeterministicRebuild()
    {
        var parent = Path.Combine(Path.GetTempPath(), "WastelandForge.WindowsTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(parent);
        try
        {
            var workspace = new BasicModBuilderWorkspace(new InProcessRunner());
            var input = Input(parent, true) with { ProjectName = "Builder Reopen Test" };
            var preview = workspace.Preview(input);
            var created = await workspace.CreateAsync(input, preview.Token!, CancellationToken.None);
            Assert.True(created.Success, created.Message);
            var originalArchiveHash = SHA256.HashData(File.ReadAllBytes(created.FomodArchive!));

            var mcm = new McmAuthoringInput(
                "ignored", "ignored.json", "ignored", "show_hints", "Show hints",
                "Config/BuilderReopenTest.ini", "General", "bShowHints", "checkbox", true,
                "1", "0", "10", "1", "0", "", "", "33", "", "", "");
            var mcmPreview = McmSourceAuthoring.PreviewAppend(created.ProjectRoot!, mcm);
            Assert.True(mcmPreview.Success, mcmPreview.Message);
            Assert.True(McmSourceAuthoring.Append(created.ProjectRoot!, mcm, mcmPreview.Token!).Success);

            var jip = new JipAuthoringInput("followup", "Inert follow-up script.", "gl_", "builder_followup", "; synthetic follow-up");
            var jipPreview = JipSourceAuthoring.PreviewAppend(created.ProjectRoot!, jip);
            Assert.True(jipPreview.Success, jipPreview.Message);
            Assert.True(JipSourceAuthoring.Append(created.ProjectRoot!, jip, jipPreview.Token!).Success);

            Assert.Equal(0, ForgeCli.Run(["validate", created.ProjectRoot!, "--format", "json", "--no-input"]));
            Assert.Equal(0, ForgeCli.Run(["package", created.ProjectRoot!, "--target", "mod-package", "--format", "json", "--no-input"]));
            Assert.Equal(0, ForgeCli.Run(["package", created.ProjectRoot!, "--target", "fomod", "--format", "json", "--no-input"]));
            var rebuiltArchive = Path.Combine(created.ProjectRoot!, "dist", "fomod", "package.zip");
            Assert.True(File.Exists(rebuiltArchive));
            Assert.NotEqual(originalArchiveHash, SHA256.HashData(File.ReadAllBytes(rebuiltArchive)));
            var outputs = ProjectOutputWorkspace.Inspect(created.ProjectRoot!);
            Assert.True(outputs.Success);
            Assert.Contains(outputs.Lanes, lane => lane.Id == "fomod" && lane.DistributionExists && lane.EntryCount >= 4);
        }
        finally { if (Directory.Exists(parent)) Directory.Delete(parent, recursive: true); }
    }

    private static BasicModBuilderInput Input(string parent, bool includeJip) => new(
        parent, includeJip ? "Builder Jip Test" : "Builder Mcm Test", "Builder Settings", "Enable builder feature",
        "General", "bEnabled", false, includeJip, "Inert startup script.", "; synthetic inert script");

    private sealed class InProcessRunner : IBasicModBuilderCommandRunner
    {
        private static readonly object ConsoleLock = new();
        public Task<ForgeCommandResult> RunAsync(string workingDirectory, CancellationToken cancellationToken, params string[] arguments)
        {
            cancellationToken.ThrowIfCancellationRequested();
            lock (ConsoleLock)
            {
                var originalOut = Console.Out;
                var originalError = Console.Error;
                using var output = new StringWriter();
                using var error = new StringWriter();
                try
                {
                    Console.SetOut(output); Console.SetError(error);
                    var previous = Environment.CurrentDirectory;
                    try { Environment.CurrentDirectory = workingDirectory; return Task.FromResult(new ForgeCommandResult("forge " + string.Join(' ', arguments), ForgeCli.Run(arguments), output.ToString(), error.ToString())); }
                    finally { Environment.CurrentDirectory = previous; }
                }
                finally { Console.SetOut(originalOut); Console.SetError(originalError); }
            }
        }
    }

    private sealed class FailingRunner(string command) : IBasicModBuilderCommandRunner
    {
        private readonly InProcessRunner inner = new();
        public Task<ForgeCommandResult> RunAsync(string workingDirectory, CancellationToken cancellationToken, params string[] arguments) =>
            arguments[0] == command
                ? Task.FromResult(new ForgeCommandResult("forge " + command, 1, "synthetic failure", string.Empty))
                : inner.RunAsync(workingDirectory, cancellationToken, arguments);
    }
}
