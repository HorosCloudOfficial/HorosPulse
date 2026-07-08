namespace HorosPulse.ViewModels;

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HorosPulse.Core.Interfaces;
using HorosPulse.Core.Models;

public sealed partial class DevTempCleanupViewModel : ViewModelBase
{
    private readonly IDevTempCleanupService _cleanupService;
    private readonly IUserConfirmationService _confirmationService;

    public DevTempCleanupViewModel(
        IDevTempCleanupService cleanupService,
        IUserConfirmationService confirmationService)
    {
        _cleanupService = cleanupService;
        _confirmationService = confirmationService;
        _ = RefreshAsync();
    }

    public string Title => "Dev-Cache";

    [ObservableProperty]
    private string _summaryText = "—";

    [ObservableProperty]
    private string _totalReclaimableDisplay = "—";

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string? _statusMessage;

    public ObservableCollection<DevTempCacheEntryViewModel> Entries { get; } = new();

    [RelayCommand]
    private async Task RefreshAsync()
    {
        IsBusy = true;
        StatusMessage = null;
        try
        {
            var result = await _cleanupService.ScanAsync();

            SummaryText = result.SummaryText;
            TotalReclaimableDisplay = DevTempCacheEntry.FormatBytes(result.TotalSafeDeletableBytes);

            Entries.Clear();
            foreach (var entry in result.Entries)
                Entries.Add(new DevTempCacheEntryViewModel(entry));
        }
        catch (Exception ex)
        {
            StatusMessage = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task CleanupSelectedAsync()
    {
        var selected = Entries.Where(e => e.IsSelected && e.Entry.IsDeletable).ToList();
        var extraConfirmation = Entries.Where(e => e.IsSelected && e.Entry.RequiresExtraConfirmation).ToList();

        if (selected.Count == 0 && extraConfirmation.Count == 0)
        {
            StatusMessage = "Bitte mindestens einen löschbaren Cache auswählen.";
            return;
        }

        if (extraConfirmation.Count > 0)
        {
            var names = string.Join("\n• ", extraConfirmation.Select(e => e.DisplayName));
            if (!_confirmationService.Confirm(
                    "Global-packages löschen — Extra-Bestätigung",
                    "ACHTUNG: Die folgenden Einträge löschen installierte NuGet-Pakete systemweit:\n\n" +
                    $"• {names}\n\n" +
                    "Builds können danach Pakete neu herunterladen müssen. " +
                    "Nur fortfahren, wenn Sie sicher sind.\n\nWirklich löschen?",
                    isWarning: true))
                return;
        }

        var safeSelected = selected.Where(e => !e.Entry.RequiresExtraConfirmation).ToList();
        var allIds = safeSelected.Select(e => e.Id)
            .Concat(extraConfirmation.Select(e => e.Id))
            .Distinct(StringComparer.Ordinal)
            .ToList();

        if (allIds.Count == 0)
        {
            StatusMessage = "Keine sicheren Einträge zum Bereinigen ausgewählt.";
            return;
        }

        var sizeSummary = string.Join("\n• ", Entries
            .Where(e => allIds.Contains(e.Id, StringComparer.Ordinal))
            .Select(e => $"{e.DisplayName} ({e.SizeDisplay})"));

        if (!_confirmationService.Confirm(
                "Dev-Cache bereinigen",
                "Folgende Caches werden bereinigt:\n\n" +
                $"• {sizeSummary}\n\n" +
                "Gesperrte Dateien werden übersprungen. HorosPulse löscht niemals node_modules " +
                "oder Ihr Benutzerprofil-Root.\n\nFortfahren?",
                isWarning: true))
            return;

        IsBusy = true;
        try
        {
            var allowGlobal = extraConfirmation.Count > 0;
            var result = await _cleanupService.CleanupAsync(allIds, allowGlobal);

            if (result.Success)
            {
                StatusMessage = $"{result.BytesFreedDisplay} freigegeben. " +
                                string.Join(" ", result.Messages);
            }
            else
            {
                StatusMessage = result.ErrorMessage;
            }

            await RefreshAsync();
        }
        catch (Exception ex)
        {
            StatusMessage = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void SelectAllSafe()
    {
        foreach (var entry in Entries.Where(e => e.Entry.IsDeletable))
            entry.IsSelected = true;
    }

    [RelayCommand]
    private void DeselectAll()
    {
        foreach (var entry in Entries)
            entry.IsSelected = false;
    }
}

public sealed partial class DevTempCacheEntryViewModel : ObservableObject
{
    public DevTempCacheEntryViewModel(DevTempCacheEntry entry)
    {
        Entry = entry;
        IsSelected = entry.IsDeletable;
    }

    public DevTempCacheEntry Entry { get; }

    public string Id => Entry.Id;

    public string DisplayName => Entry.DisplayName;

    public string Path => Entry.Path;

    public string SizeDisplay => Entry.SizeDisplay;

    public string DeletableLabel => Entry.DeletableLabel;

    public string SafetyReason => Entry.SafetyReason;

    public bool PathExists => Entry.PathExists;

    public string ExistsLabel => PathExists ? "Vorhanden" : "Nicht vorhanden";

    public bool CanSelect => Entry.IsDeletable || Entry.RequiresExtraConfirmation;

    [ObservableProperty]
    private bool _isSelected;
}
