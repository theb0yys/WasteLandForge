using System.Diagnostics;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace WastelandForge.Generation;

public sealed record BsArchProviderEvidence(string Path, long Length, string Sha256, string? FileVersion, string SignatureStatus);
public sealed record BsArchPreviewArchive(string ArchiveFile, string InputRoot, string OutputPath, int EntryCount, IReadOnlyList<string> PackArguments, IReadOnlyList<string> ListArguments, IReadOnlyList<string> UnpackArguments);
public sealed record BsArchPreviewResult(string FormatVersion, string Kind, string Status, string ProjectRoot, string? PreviewSha256, BsArchProviderEvidence? Provider, IReadOnlyList<BsArchPreviewArchive> Archives, IReadOnlyList<BsaPlanVerificationIssue> Issues, bool DryRun, bool ExternalToolExecuted, bool FilesWritten)
{
    public bool HasErrors => Issues.Any(issue => issue.Severity == "error");
}

public sealed record BsArchProcessRequest(string ExecutablePath, IReadOnlyList<string> Arguments, string WorkingDirectory, TimeSpan Timeout, bool UseShellExecute, bool CreateNoWindow, bool RedirectOutput);
public sealed record BsArchProcessResult(int ExitCode, string StandardOutput, string StandardError, bool TimedOut);
public interface IBsArchProcessRunner { Task<BsArchProcessResult> RunAsync(BsArchProcessRequest request, CancellationToken cancellationToken); }
public sealed record BsArchProbeResult(bool Success, string? Version, string Message, BsArchProcessResult Process);

public sealed class BsArchProviderProbe(IBsArchProcessRunner runner)
{
    public async Task<BsArchProbeResult> ProbeAsync(BsArchProviderEvidence provider, string workingDirectory, CancellationToken cancellationToken)
    {
        if (!File.Exists(provider.Path) || new FileInfo(provider.Path).Length != provider.Length || !StringComparer.Ordinal.Equals(Sha(provider.Path), provider.Sha256))
            return new(false, null, "WF-CAP-020: BSArch provider changed before probe.", new(-1, "", "", false));
        var request = new BsArchProcessRequest(provider.Path, [], Path.GetFullPath(workingDirectory), TimeSpan.FromSeconds(15), false, true, true);
        var process = await runner.RunAsync(request, cancellationToken).ConfigureAwait(false);
        var marker = "BSArch v";
        var start = process.StandardOutput.IndexOf(marker, StringComparison.Ordinal);
        var version = start < 0 ? null : process.StandardOutput[(start + marker.Length)..].Split(['\r', '\n', ' '], StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
        var success = process.ExitCode == 0 && !process.TimedOut && !string.IsNullOrWhiteSpace(version);
        return new(success, version, success ? "BSArch provider probe passed." : "WF-CAP-020: BSArch provider probe did not return the required banner and exit code.", process);
    }
    private static string Sha(string path) { using var stream = File.OpenRead(path); return Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant(); }
}

public sealed class BsArchPreviewPlanner
{
    public const string Target = "bsa-bsarch";

    public BsArchPreviewResult Preview(string projectRoot, string? packerPath)
    {
        var root = Path.GetFullPath(projectRoot);
        var issues = new List<BsaPlanVerificationIssue>();
        var verification = new BsaPlanVerifier().Verify(root);
        issues.AddRange(verification.Issues);
        BsArchProviderEvidence? provider = null;
        if (string.IsNullOrWhiteSpace(packerPath) || !Path.IsPathRooted(packerPath)) issues.Add(Issue("WF-CAP-020", "An absolute --packer path to bsarch.exe is required.", packerPath ?? ""));
        else
        {
            var path = Path.GetFullPath(packerPath);
            var dist = Path.GetFullPath(Path.Combine(root, "dist"));
            if (!StringComparer.OrdinalIgnoreCase.Equals(Path.GetFileName(path), "bsarch.exe")) issues.Add(Issue("WF-CAP-020", "The provider basename must be bsarch.exe.", path));
            else if (!File.Exists(path)) issues.Add(Issue("WF-CAP-020", "The BSArch provider file does not exist.", path));
            else if (IsInside(dist, path)) issues.Add(Issue("WF-CAP-020", "The BSArch provider must stay outside the project dist tree.", path));
            else if ((File.GetAttributes(path) & FileAttributes.ReparsePoint) != 0) issues.Add(Issue("WF-CAP-020", "A reparse-point BSArch provider is refused.", path));
            else provider = new(path, new FileInfo(path).Length, Sha(path), FileVersionInfo.GetVersionInfo(path).FileVersion, Signature(path));
        }
        if (issues.Any(issue => issue.Severity == "error") || provider is null) return new("0.1", "wastelandforge.bsa-bsarch-preview", "refused", root, null, provider, [], issues, true, false, false);

        var plan = JsonNode.Parse(File.ReadAllText(Path.Combine(root, "dist", "bsa-plan", "bsa-pack-plan.json")))!.AsObject();
        var archives = new List<BsArchPreviewArchive>();
        foreach (var archive in plan["archives"]!.AsArray().OrderBy(item => item!["file"]!.GetValue<string>(), StringComparer.Ordinal))
        {
            var file = archive!["file"]!.GetValue<string>();
            var role = Path.GetFileNameWithoutExtension(file).Split(" - ").Last();
            var input = Path.Combine(root, "dist", "bsa-build", ".work-<approval>", "inputs", role);
            var output = Path.Combine(root, "dist", "bsa-build", ".work-<approval>", "run-1", file);
            var unpack = Path.Combine(root, "dist", "bsa-build", ".work-<approval>", "unpacked", role);
            archives.Add(new(file, input, Path.Combine(root, "dist", "bsa-build", "archives", file), archive["entryCount"]!.GetValue<int>(),
                ["pack", input, output, "-fnv"], [output, "-list"], ["unpack", output, unpack, "-q"]));
        }
        var approvalPayload = JsonSerializer.Serialize(new { projectRoot = root, planSha256 = Sha(Path.Combine(root, "dist", "bsa-plan", "bsa-pack-plan.json")), provider, archives });
        return new("0.1", "wastelandforge.bsa-bsarch-preview", "planned", root, Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(approvalPayload))).ToLowerInvariant(), provider, archives, [], true, false, false);
    }

    private static string Signature(string path) { try { using var certificate = X509CertificateLoader.LoadCertificateFromFile(path); return certificate.Subject.Length > 0 ? "signed" : "unsigned"; } catch (CryptographicException) { return "unsigned"; } }
    private static string Sha(string path) { using var stream = File.OpenRead(path); return Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant(); }
    private static bool IsInside(string root, string path) => Path.GetFullPath(path).StartsWith(Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);
    private static BsaPlanVerificationIssue Issue(string rule, string message, string path) => new(rule, "error", "BSArch preview refused", message, path);
}
