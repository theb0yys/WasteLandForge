using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace WastelandForge.Desktop;

internal sealed record Mo2LaunchRequestPreview(string Token, string RequestId, string RequestPath, string Json, string ToolKind, string ContextKind, string ContextId, string ContextSha256, string SourceApprovalToken, DateTimeOffset CreatedUtc, DateTimeOffset ExpiresUtc, string Details);
internal sealed record Mo2LaunchRequestResult(bool Success, string Message, Mo2LaunchRequestPreview? Preview = null);

internal sealed class Mo2LaunchRequestService
{
    private readonly string requestRoot;
    private readonly Func<DateTimeOffset> utcNow;

    public Mo2LaunchRequestService(string? requestRoot = null, Func<DateTimeOffset>? utcNow = null)
    {
        this.requestRoot = requestRoot ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "WastelandForge", "Mo2LaunchRequests");
        this.utcNow = utcNow ?? (() => DateTimeOffset.UtcNow);
    }

    public Mo2LaunchRequestResult PreviewGeck(string projectRoot, GeckHandoffWorkspaceResult session, GeckLaunchPreview approved, string? stateRoot = null)
    {
        var current = new GeckLaunchService().Preview(projectRoot, session, approved.ExecutablePath, stateRoot);
        if (!current.Success || current.Preview is null || !StringComparer.Ordinal.Equals(current.Preview.Token, approved.Token)) return new(false, "MO2 request refused because the GECK launch approval is stale. " + current.Message);
        return Preview(current.Preview.ProjectRoot, "geck-handoff", "geck-handoff", current.Preview.HandoffSha256, "geck", current.Preview.ExecutablePath, current.Preview.WorkingDirectory, current.Preview.ExecutableLength, current.Preview.ExecutableSha256, current.Preview.Token);
    }

    public Mo2LaunchRequestResult PreviewXEdit(string projectRoot, string selectedArtifactId, XEditLaunchPreview approved)
    {
        var current = new XEditLaunchService().Preview(projectRoot, selectedArtifactId, approved.ExecutablePath);
        if (!current.Success || current.Preview is null || !StringComparer.Ordinal.Equals(current.Preview.Token, approved.Token)) return new(false, "MO2 request refused because the xEdit launch approval is stale. " + current.Message);
        return Preview(current.Preview.ProjectRoot, "pending-plugin-review", current.Preview.ArtifactId, current.Preview.PluginSha256, "xedit", current.Preview.ExecutablePath, current.Preview.WorkingDirectory, current.Preview.ExecutableLength, current.Preview.ExecutableSha256, current.Preview.Token);
    }

    public Mo2LaunchRequestResult CreateGeck(string projectRoot, GeckHandoffWorkspaceResult session, GeckLaunchPreview approved, Mo2LaunchRequestPreview request, string? stateRoot = null)
    {
        var current = new GeckLaunchService().Preview(projectRoot, session, approved.ExecutablePath, stateRoot);
        return Create(current.Preview?.Token, approved.Token, request);
    }

    public Mo2LaunchRequestResult CreateXEdit(string projectRoot, string selectedArtifactId, XEditLaunchPreview approved, Mo2LaunchRequestPreview request)
    {
        var current = new XEditLaunchService().Preview(projectRoot, selectedArtifactId, approved.ExecutablePath);
        return Create(current.Preview?.Token, approved.Token, request);
    }

    private Mo2LaunchRequestResult Preview(string projectRoot, string contextKind, string contextId, string contextSha, string toolKind, string executable, string workingDirectory, long length, string sha, string sourceApprovalToken)
    {
        try
        {
            var created = utcNow().ToUniversalTime();
            var expires = created.AddMinutes(15);
            var id = Guid.NewGuid().ToString("N");
            var path = Path.Combine(Path.GetFullPath(requestRoot), id + ".json");
            var json = new JsonObject
            {
                ["formatVersion"] = "0.1", ["kind"] = "wastelandforge.mo2-launch-request", ["requestId"] = id,
                ["createdUtc"] = created.ToString("O"), ["expiresUtc"] = expires.ToString("O"),
                ["project"] = new JsonObject { ["root"] = Path.GetFullPath(projectRoot), ["contextKind"] = contextKind, ["contextId"] = contextId, ["contextSha256"] = contextSha },
                ["tool"] = new JsonObject { ["kind"] = toolKind, ["executablePath"] = executable, ["workingDirectory"] = workingDirectory, ["length"] = length, ["sha256"] = sha, ["arguments"] = new JsonArray() },
                ["safety"] = new JsonObject { ["shellExecution"] = false, ["elevation"] = false, ["profileMutation"] = false, ["executableRegistration"] = false, ["automaticLaunch"] = false }
            }.ToJsonString(new JsonSerializerOptions { WriteIndented = true }) + "\n";
            var token = DigestText(sourceApprovalToken + "\n" + path + "\n" + json);
            var details = $"MO2 request: {path}{Environment.NewLine}Tool: {toolKind}{Environment.NewLine}Context: {contextKind} / {contextId}{Environment.NewLine}Arguments: none{Environment.NewLine}Expires: {expires:O}{Environment.NewLine}{Environment.NewLine}Request creation does not launch MO2. Import it manually through the optional WastelandForge MO2 companion and explicitly select a profile.";
            return new(true, "MO2 launch-request preview ready. No file was written.", new(token, id, path, json, toolKind, contextKind, contextId, contextSha, sourceApprovalToken, created, expires, details));
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidOperationException or ArgumentException or NotSupportedException) { return new(false, "MO2 launch-request preview failed: " + ex.Message); }
    }

    private Mo2LaunchRequestResult Create(string? currentApprovalToken, string approvedToken, Mo2LaunchRequestPreview request)
    {
        if (!StringComparer.Ordinal.Equals(currentApprovalToken, approvedToken) || !StringComparer.Ordinal.Equals(request.SourceApprovalToken, approvedToken)) return new(false, "MO2 launch-request approval is stale.");
        if (utcNow().ToUniversalTime() > request.ExpiresUtc) return new(false, "MO2 launch-request approval expired. Preview again.");
        if (!StringComparer.Ordinal.Equals(DigestText(request.SourceApprovalToken + "\n" + request.RequestPath + "\n" + request.Json), request.Token)) return new(false, "MO2 launch-request preview content changed.");
        var temp = request.RequestPath + ".tmp-" + Guid.NewGuid().ToString("N");
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(request.RequestPath)!);
            if (File.Exists(request.RequestPath) || Directory.Exists(request.RequestPath)) return new(false, "MO2 launch request already exists; overwrite is refused.");
            File.WriteAllText(temp, request.Json, new UTF8Encoding(false));
            File.Move(temp, request.RequestPath, false);
            return new(true, "MO2 launch request created. Open it from the optional MO2 companion; no process was launched.", request);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidOperationException) { return new(false, "MO2 launch request was not created: " + ex.Message); }
        finally { try { if (File.Exists(temp)) File.Delete(temp); } catch { } }
    }

    private static string DigestText(string value) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value))).ToLowerInvariant();
}
