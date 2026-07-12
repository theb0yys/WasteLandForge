using System.Text;
using System.Text.Json.Nodes;
using WastelandForge.Core;
using WastelandForge.Generation;
namespace WastelandForge.Cli;
internal static class FomodPackageResultWriter
{
 public static string Json(FomodPackageResult r)=>new JsonObject{{"formatVersion","0.1"},{"command","package"},{"target","fomod"},{"status",r.Status},{"dryRun",r.DryRun},{"project",new JsonObject{{"id",r.ProjectId},{"root",r.ProjectRoot}}},{"summary",new JsonObject{{"errors",r.Diagnostics.ErrorCount},{"entries",r.Entries.Count}}},{"outputs",r.Outputs is null?null:new JsonObject{{"root",r.Outputs.Root},{"stagingRoot",r.Outputs.StagingRoot},{"packageArchive",r.Outputs.Archive},{"manifest",r.Outputs.Manifest},{"buildManifest",r.Outputs.BuildManifest},{"checksums",r.Outputs.Checksums}}},{"issues",new JsonArray(r.Diagnostics.Issues.Select(DiagnosticIssueJsonSerializer.ToJsonNode).ToArray())}}.ToJsonString(new(){WriteIndented=true});
 public static string Text(FomodPackageResult r){var b=new StringBuilder();b.AppendLine("WastelandForge FOMOD Package");b.AppendLine($"Mode: {(r.DryRun?"dry-run":"write")}");b.AppendLine($"Entries: {r.Entries.Count}");if(r.Outputs is not null)b.AppendLine($"Output: {r.Outputs.Root}");foreach(var i in r.Diagnostics.Issues)b.AppendLine($"{i.Severity.ToString().ToUpperInvariant()} {i.RuleId} {i.Message}");b.AppendLine($"Status: {r.Status}");return b.ToString();}
}
