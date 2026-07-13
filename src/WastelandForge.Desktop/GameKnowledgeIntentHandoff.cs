using System.IO;
using WastelandForge.Generation;

namespace WastelandForge.Desktop;

internal sealed record GameKnowledgeIntentHandoffResult(bool Success, string Message, GeckIntentResolutionRow? Row);

internal static class GameKnowledgeIntentHandoff
{
    private static readonly HashSet<string> AllowedKinds = new(StringComparer.Ordinal)
    {
        "item", "cell", "worldspace", "container-base"
    };

    public static GameKnowledgeIntentHandoffResult Create(
        FnvGameKnowledgeRecord record,
        string receiptPath,
        string kind,
        IEnumerable<GeckIntentResolutionRow> existing)
    {
        ArgumentNullException.ThrowIfNull(record);
        ArgumentNullException.ThrowIfNull(existing);
        if (string.IsNullOrWhiteSpace(record.EditorId)) return new(false, "A record without an EditorID cannot populate the first-slice intent contract.", null);
        if (!AllowedKinds.Contains(kind)) return new(false, "Choose the intended resolution kind explicitly.", null);
        if (string.IsNullOrWhiteSpace(receiptPath) || !Path.IsPathFullyQualified(receiptPath) || !File.Exists(receiptPath) || !StringComparer.OrdinalIgnoreCase.Equals(Path.GetExtension(receiptPath), ".json"))
            return new(false, "A current local JSON evidence receipt is required.", null);
        if ((File.GetAttributes(receiptPath) & FileAttributes.ReparsePoint) != 0) return new(false, "The local evidence receipt must not be a reparse point.", null);

        var baseId = "gk-" + record.FixedFormId.ToLowerInvariant();
        var occupied = existing.Select(row => row.Id).ToHashSet(StringComparer.Ordinal);
        var id = baseId;
        for (var suffix = 2; occupied.Contains(id); suffix++) id = baseId + "-" + suffix;
        return new(true, "Local catalogue receipt prepared as a provisional resolution.", new GeckIntentResolutionRow
        {
            Id = id,
            Kind = kind,
            EditorId = record.EditorId,
            FormId = record.FixedFormId,
            Signature = record.Signature,
            Status = "provisional",
            EvidencePath = Path.GetFullPath(receiptPath),
            Quantity = kind == "item" ? 1 : 0
        });
    }
}
