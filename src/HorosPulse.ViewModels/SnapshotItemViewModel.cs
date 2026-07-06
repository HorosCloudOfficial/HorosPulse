namespace HorosPulse.ViewModels;

using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using HorosPulse.Core.Models;
using HorosPulse.Data;

public sealed partial class SnapshotItemViewModel : ObservableObject
{
    private readonly SnapshotEntry _entry;

    public SnapshotItemViewModel(SnapshotEntry entry)
    {
        _entry = entry;
        PrettyStateJson = BuildPrettyStateJson(entry);
    }

    public Guid Id => _entry.Id;

    public string Label => _entry.Label;

    public string Module => _entry.Module;

    public DateTimeOffset CreatedAt => _entry.CreatedAt;

    public string CreatedAtDisplay => CreatedAt.LocalDateTime.ToString("g");

    public bool IsValid => _entry.IsValid;

    public bool CanRollback => _entry.CanRollback;

    public string ValidityLabel => IsValid ? "Gültig" : "Ungültig";

    public StatusBadgeKind ValidityStatus => IsValid ? StatusBadgeKind.Active : StatusBadgeKind.Error;

    [ObservableProperty]
    private bool _isExpanded;

    public string PrettyStateJson { get; }

    private static string BuildPrettyStateJson(SnapshotEntry entry)
    {
        if (!entry.IsValid)
            return "{ \"error\": \"Snapshot-Checksum ungültig\" }";

        try
        {
            var json = SnapshotCompression.Decompress(entry.StateJson);
            using var doc = JsonDocument.Parse(json);
            return JsonSerializer.Serialize(doc, JsonDefaults.Options);
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message }, JsonDefaults.Options);
        }
    }
}
