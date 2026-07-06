namespace HorosPulse.ViewModels;

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HorosPulse.Core.Interfaces;
using HorosPulse.Core.Models;

public sealed partial class CursorViewModel : ViewModelBase
{
    private readonly ICursorOptimizer _cursorOptimizer;
    private readonly IProcessPriorityService _processPriorityService;
    private readonly IDefenderExclusionService _defenderExclusionService;
    private readonly IIndexerExclusionService _indexerExclusionService;
    private readonly IUserConfirmationService _confirmationService;

    public CursorViewModel(
        ICursorOptimizer cursorOptimizer,
        IProcessPriorityService processPriorityService,
        IDefenderExclusionService defenderExclusionService,
        IIndexerExclusionService indexerExclusionService,
        IUserConfirmationService confirmationService)
    {
        _cursorOptimizer = cursorOptimizer;
        _processPriorityService = processPriorityService;
        _defenderExclusionService = defenderExclusionService;
        _indexerExclusionService = indexerExclusionService;
        _confirmationService = confirmationService;
        _ = RefreshAsync();
    }

    public string Title => "Cursor";

    [ObservableProperty]
    private string _description = "Cursor IDE Optimierungen";

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string? _statusMessage;

    [ObservableProperty]
    private string? _cursorProcessStatus;

    [ObservableProperty]
    private bool _defenderOptIn;

    [ObservableProperty]
    private bool _hasSettingsBackup;

    public ObservableCollection<string> PreviewChanges { get; } = new();
    public ObservableCollection<IndexerExcludeItemViewModel> IndexerEntries { get; } = new();

    [RelayCommand]
    private async Task RefreshAsync()
    {
        IsBusy = true;
        try
        {
            PreviewChanges.Clear();
            var preview = await _cursorOptimizer.PreviewOptimizationsAsync();
            foreach (var key in preview.ChangedKeys)
                PreviewChanges.Add(key);

            HasSettingsBackup = _cursorOptimizer.HasBackup;
            CursorProcessStatus = _processPriorityService.GetCursorProcessStatus()
                ?? "Cursor läuft nicht — Prozessprioritäten nach Start anwendbar.";

            IndexerEntries.Clear();
            var entries = await _indexerExclusionService.GetAvailableEntriesAsync();
            foreach (var entry in entries)
                IndexerEntries.Add(new IndexerExcludeItemViewModel(entry.Path, entry.IsSelected, entry.IsApplied));

            Description = $"Einstellungen: {_cursorOptimizer.SettingsPath}";
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
    private async Task ApplySettingsAsync()
    {
        if (!_confirmationService.Confirm(
                "Cursor-Einstellungen anwenden",
                "Die Cursor settings.json wird mit Performance- und Editor-Vorlagen zusammengeführt. Ein Backup wird vorher erstellt.\n\nFortfahren?"))
            return;

        IsBusy = true;
        StatusMessage = null;
        try
        {
            var result = await _cursorOptimizer.ApplyOptimizationsAsync();
            StatusMessage = result.Success ? "Einstellungen angewendet." : result.ErrorMessage;
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
    private async Task RestoreSettingsAsync()
    {
        if (!_confirmationService.Confirm(
                "Cursor-Einstellungen wiederherstellen",
                "settings.json wird aus dem Backup wiederhergestellt.\n\nFortfahren?"))
            return;

        IsBusy = true;
        try
        {
            var result = await _cursorOptimizer.RevertOptimizationsAsync();
            StatusMessage = result.Success ? "Einstellungen wiederhergestellt." : result.ErrorMessage;
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
    private async Task ApplyProcessPrioritiesAsync()
    {
        IsBusy = true;
        try
        {
            var result = await _processPriorityService.ApplyCursorPrioritiesAsync();
            StatusMessage = result.Success
                ? "Prozessprioritäten angewendet."
                : result.ErrorMessage;
            CursorProcessStatus = _processPriorityService.GetCursorProcessStatus()
                ?? "Cursor läuft nicht.";
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
    private async Task ApplyDefenderExclusionsAsync()
    {
        if (!DefenderOptIn)
        {
            StatusMessage = "Bitte aktivieren Sie die Opt-in-Checkbox für Defender-Ausschlüsse.";
            return;
        }

        if (!_confirmationService.Confirm(
                "Sicherheitswarnung — Defender-Ausschlüsse",
                "Das Hinzufügen von Defender-Ausschlüssen reduziert den Schutz für die ausgewählten Ordner. " +
                "Nur Cursor-bezogene Pfade werden ausgeschlossen (kein node_modules).\n\n" +
                "Möchten Sie fortfahren?",
                isWarning: true))
            return;

        IsBusy = true;
        try
        {
            var result = await _defenderExclusionService.ApplyExclusionsAsync(userConfirmed: true);
            StatusMessage = result.Success
                ? "Defender-Ausschlüsse hinzugefügt (Admin erforderlich)."
                : result.ErrorMessage;
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
    private async Task ApplyIndexerExclusionsAsync()
    {
        var selected = IndexerEntries.Where(e => e.IsSelected).Select(e => e.Path).ToList();
        if (selected.Count == 0)
        {
            StatusMessage = "Bitte mindestens einen Ordner für Indexer-Ausschlüsse auswählen.";
            return;
        }

        if (!_confirmationService.Confirm(
                "Windows-Suche Ausschlüsse",
                $"Es werden {selected.Count} Ordner von der Windows-Suche ausgeschlossen (Admin erforderlich).\n\nFortfahren?"))
            return;

        IsBusy = true;
        try
        {
            var result = await _indexerExclusionService.ApplyExclusionsAsync(selected);
            StatusMessage = result.Success
                ? "Indexer-Ausschlüsse angewendet."
                : result.ErrorMessage;
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
}
