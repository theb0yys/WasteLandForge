using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace WastelandForge.Generation;

public sealed record BsArchExecutionOptions(string ProjectRoot, string PackerPath, string ApprovalSha256, string ToolVersion);
public sealed record BsArchArchiveExecution(string ArchiveFile, int EntryCount, string Sha256, long Length, bool RepeatPackMatches, bool ListMatches, bool UnpackedBytesMatch);
public sealed record BsArchInvocationEvidence(IReadOnlyList<string> Arguments, int ExitCode, bool TimedOut, string StandardOutputSha256, int StandardOutputLength, string StandardErrorSha256, int StandardErrorLength);
public sealed record BsArchExecutionResult(string Status, string ProjectRoot, string? OutputRoot, string? ProviderVersion, IReadOnlyList<BsArchArchiveExecution> Archives, IReadOnlyList<BsaPlanVerificationIssue> Issues, bool ExternalToolExecuted, bool PluginMutation)
{
    public bool HasErrors => Issues.Any(issue => issue.Severity == "error");
}

public sealed class BsArchExecutionCoordinator(IBsArchProcessRunner runner)
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public async Task<BsArchExecutionResult> ExecuteAsync(BsArchExecutionOptions options, CancellationToken cancellationToken)
    {
        var root = Path.GetFullPath(options.ProjectRoot);
        var preview = new BsArchPreviewPlanner().Preview(root, options.PackerPath);
        if (preview.HasErrors || preview.Provider is null || preview.PreviewSha256 is null)
            return Failed(root, preview.Issues.Count > 0 ? preview.Issues : [Issue("WF-BUILD-018", "A valid BSArch preview is required.", "dist/bsa-plan")]);
        if (!StringComparer.Ordinal.Equals(preview.PreviewSha256, options.ApprovalSha256))
            return Failed(root, [Issue("WF-BUILD-018", "BSArch approval token does not match the current preview.", "--approve")]);

        var dist = Path.Combine(root, "dist");
        var work = Path.Combine(dist, "bsa-build.work-" + Guid.NewGuid().ToString("N"));
        var final = Path.Combine(dist, "bsa-build");
        var executions = new List<BsArchArchiveExecution>();
        var invocations = new List<BsArchInvocationEvidence>();
        var issues = new List<BsaPlanVerificationIssue>();
        try
        {
            Directory.CreateDirectory(work);
            var probe = await new BsArchProviderProbe(runner).ProbeAsync(preview.Provider, work, cancellationToken).ConfigureAwait(false);
            invocations.Add(Evidence([], probe.Process));
            if (!probe.Success) return Failed(root, [Issue("WF-CAP-020", probe.Message, preview.Provider.Path)], externalToolExecuted: true);
            var plan = JsonNode.Parse(File.ReadAllText(Path.Combine(root, "dist", "bsa-plan", "bsa-pack-plan.json")))!.AsObject();
            foreach (var archive in plan["archives"]!.AsArray().OrderBy(item => item!["file"]!.GetValue<string>(), StringComparer.Ordinal))
            {
                cancellationToken.ThrowIfCancellationRequested();
                var file = archive!["file"]!.GetValue<string>();
                var role = Path.GetFileNameWithoutExtension(file).Split(" - ").Last();
                var entries = archive["entries"]!.AsArray().ToArray();
                var input1 = Path.Combine(work, "inputs-1", role);
                var input2 = Path.Combine(work, "inputs-2", role);
                CopyInputs(root, entries, input1);
                CopyInputs(root, entries, input2);
                var output1 = Path.Combine(work, "run-1", file);
                var output2 = Path.Combine(work, "run-2", file);
                Directory.CreateDirectory(Path.GetDirectoryName(output1)!);
                Directory.CreateDirectory(Path.GetDirectoryName(output2)!);
                var pack1 = await Run(["pack", input1, output1, "-fnv"], preview.Provider.Path, work, invocations, cancellationToken).ConfigureAwait(false);
                var pack2 = await Run(["pack", input2, output2, "-fnv"], preview.Provider.Path, work, invocations, cancellationToken).ConfigureAwait(false);
                if (!Passed(pack1) || !Passed(pack2) || !File.Exists(output1) || !File.Exists(output2)) { issues.Add(Issue("WF-BUILD-019", $"BSArch pack failed for '{file}'.", file)); break; }
                var sha1 = Sha(output1); var sha2 = Sha(output2); var length = new FileInfo(output1).Length;
                var repeat = length > 0 && new FileInfo(output2).Length == length && StringComparer.Ordinal.Equals(sha1, sha2);
                if (!repeat) { issues.Add(Issue("WF-BUILD-020", $"Repeat BSArch outputs differ for '{file}'.", file)); break; }
                var list = await Run([output1, "-list"], preview.Provider.Path, work, invocations, cancellationToken).ConfigureAwait(false);
                var expected = entries.Select(entry => entry!["archivePath"]!.GetValue<string>()).Order(StringComparer.OrdinalIgnoreCase).ToArray();
                var listed = Passed(list) && list.StandardOutput.Contains("Format", StringComparison.OrdinalIgnoreCase) && expected.All(path => ContainsPathLine(list.StandardOutput, path));
                if (!listed) { issues.Add(Issue("WF-BUILD-019", $"BSArch list output did not match '{file}'.", file)); break; }
                var unpack = Path.Combine(work, "unpacked", role); Directory.CreateDirectory(unpack);
                var unpackResult = await Run(["unpack", output1, unpack, "-q"], preview.Provider.Path, work, invocations, cancellationToken).ConfigureAwait(false);
                var unpacked = Passed(unpackResult) && VerifyUnpacked(unpack, entries);
                if (!unpacked) { issues.Add(Issue("WF-ASSET-017", $"Unpacked bytes did not match the plan for '{file}'.", file)); break; }
                executions.Add(new(file, entries.Length, sha1, length, true, true, true));
            }
            if (issues.Count > 0) return Failed(root, issues, externalToolExecuted: true);

            var archivesRoot = Path.Combine(work, "archives"); Directory.CreateDirectory(archivesRoot);
            foreach (var execution in executions) File.Copy(Path.Combine(work, "run-1", execution.ArchiveFile), Path.Combine(archivesRoot, execution.ArchiveFile));
            WriteJson(Path.Combine(work, "bsarch-preview.json"), JsonSerializer.SerializeToNode(preview, JsonOptions)!);
            WriteJson(Path.Combine(work, "bsarch-execution.json"), JsonSerializer.SerializeToNode(new { formatVersion="0.1", kind="wastelandforge.bsarch-execution", status="passed", approvalSha256=options.ApprovalSha256, provider=preview.Provider, providerVersion=probe.Version, archives=executions, invocations, externalToolExecuted=true, pluginMutation=false }, JsonOptions)!);
            WriteJson(Path.Combine(work, "bsa-output-verification.json"), JsonSerializer.SerializeToNode(new { formatVersion="0.1", kind="wastelandforge.bsa-output-verification", status="passed", archives=executions, repeatPackVerified=true, unpackedBytesVerified=true }, JsonOptions)!);
            var manifestInputs = Directory.EnumerateFiles(work, "*", SearchOption.AllDirectories).Order(StringComparer.Ordinal).ToArray();
            WriteJson(Path.Combine(work, "build-manifest.json"), JsonSerializer.SerializeToNode(new { formatVersion="0.1", kind="wastelandforge.build-manifest", target=BsArchPreviewPlanner.Target, toolVersion=options.ToolVersion, outputs=manifestInputs.Select(path => Digest(work,path)), externalToolExecuted=true, pluginMutation=false }, JsonOptions)!);
            WriteChecksums(work);
            Promote(work, final);
            return new("passed", root, Path.GetRelativePath(root, final).Replace('\\','/'), probe.Version, executions, [], true, false);
        }
        catch (OperationCanceledException) { return Failed(root, [Issue("WF-BUILD-019", "BSArch execution was cancelled.", "dist/bsa-build")], externalToolExecuted: true); }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException or InvalidOperationException or ArgumentException)
        { return Failed(root, [Issue("WF-BUILD-019", "BSArch execution failed: " + ex.Message, "dist/bsa-build")], externalToolExecuted: true); }
        finally { if (Directory.Exists(work)) Directory.Delete(work, true); }
    }

    private async Task<BsArchProcessResult> Run(IReadOnlyList<string> arguments, string executable, string working, List<BsArchInvocationEvidence> evidence, CancellationToken token)
    {
        var result = await runner.RunAsync(new(executable, arguments, working, TimeSpan.FromMinutes(2), false, true, true), token).ConfigureAwait(false);
        evidence.Add(Evidence(arguments, result));
        return result;
    }
    private static bool Passed(BsArchProcessResult result) => result.ExitCode == 0 && !result.TimedOut;
    private static void CopyInputs(string root, JsonNode?[] entries, string destination)
    {
        foreach (var entry in entries)
        {
            var dataPath = entry!["dataPath"]!.GetValue<string>();
            var source = Path.GetFullPath(Path.Combine(root, "dist", "mod-package", "staging", "Data", dataPath.Replace('/', Path.DirectorySeparatorChar)));
            var target = Path.GetFullPath(Path.Combine(destination, dataPath.Replace('/', Path.DirectorySeparatorChar)));
            var sourceRoot = Path.Combine(root, "dist", "mod-package", "staging", "Data");
            if (!IsInside(sourceRoot, source) || !IsInside(destination, target) || Path.GetDirectoryName(target) == Path.GetFullPath(destination)) throw new InvalidOperationException("Unsafe BSArch input: " + dataPath);
            if (!File.Exists(source) || (File.GetAttributes(source) & FileAttributes.ReparsePoint) != 0) throw new InvalidOperationException("Missing or redirected BSArch input: " + dataPath);
            Directory.CreateDirectory(Path.GetDirectoryName(target)!); File.Copy(source, target);
            if (new FileInfo(target).Length != entry["length"]!.GetValue<long>() || !StringComparer.Ordinal.Equals(Sha(target), entry["sha256"]!.GetValue<string>())) throw new InvalidOperationException("Copied BSArch input changed: " + dataPath);
        }
    }
    private static bool VerifyUnpacked(string root, JsonNode?[] entries)
    {
        var files = Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories).ToArray();
        if (files.Length != entries.Length || files.Any(path => (File.GetAttributes(path) & FileAttributes.ReparsePoint) != 0)) return false;
        return entries.All(entry => { var path=Path.GetFullPath(Path.Combine(root,entry!["dataPath"]!.GetValue<string>().Replace('/',Path.DirectorySeparatorChar))); return IsInside(root,path)&&File.Exists(path)&&new FileInfo(path).Length==entry["length"]!.GetValue<long>()&&StringComparer.Ordinal.Equals(Sha(path),entry["sha256"]!.GetValue<string>()); });
    }
    private static bool ContainsPathLine(string output, string path) => output.Split(['\r','\n'],StringSplitOptions.RemoveEmptyEntries).Any(line => StringComparer.OrdinalIgnoreCase.Equals(line.Trim().Replace('/','\\'),path.Replace('/','\\')));
    private static object Digest(string root,string path)=>new{path=Path.GetRelativePath(root,path).Replace('\\','/'),length=new FileInfo(path).Length,sha256=Sha(path)};
    private static void WriteJson(string path,JsonNode node)=>File.WriteAllText(path,node.ToJsonString(JsonOptions)+"\n",new UTF8Encoding(false));
    private static void WriteChecksums(string root){var files=Directory.EnumerateFiles(root,"*",SearchOption.AllDirectories).Where(path=>Path.GetFileName(path)!="checksums.sha256").Order(StringComparer.Ordinal);File.WriteAllText(Path.Combine(root,"checksums.sha256"),string.Concat(files.Select(path=>$"{Sha(path)}  {Path.GetRelativePath(root,path).Replace('\\','/')}\n")),new UTF8Encoding(false));}
    private static void Promote(string work,string final){var backup=final+".backup-"+Guid.NewGuid().ToString("N");try{if(Directory.Exists(final))Directory.Move(final,backup);Directory.Move(work,final);if(Directory.Exists(backup))Directory.Delete(backup,true);}catch{if(!Directory.Exists(final)&&Directory.Exists(backup))Directory.Move(backup,final);throw;}}
    private static bool IsInside(string root,string path)=>Path.GetFullPath(path).StartsWith(Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar)+Path.DirectorySeparatorChar,StringComparison.OrdinalIgnoreCase);
    private static BsArchInvocationEvidence Evidence(IReadOnlyList<string> arguments, BsArchProcessResult result) => new(arguments.ToArray(), result.ExitCode, result.TimedOut, ShaText(result.StandardOutput), result.StandardOutput.Length, ShaText(result.StandardError), result.StandardError.Length);
    private static string ShaText(string value)=>Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value))).ToLowerInvariant();
    private static string Sha(string path){using var stream=File.OpenRead(path);return Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant();}
    private static BsaPlanVerificationIssue Issue(string rule,string message,string path)=>new(rule,"error","BSArch execution failed",message,path);
    private static BsArchExecutionResult Failed(string root,IReadOnlyList<BsaPlanVerificationIssue> issues,bool externalToolExecuted=false)=>new("failed",root,null,null,[],issues,externalToolExecuted,false);
}
