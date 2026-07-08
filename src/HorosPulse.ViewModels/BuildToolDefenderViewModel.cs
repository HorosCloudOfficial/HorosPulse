namespace HorosPulse.ViewModels;

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HorosPulse.Core.Interfaces;
using HorosPulse.Core.Models;

public sealed partial class BuildToolDefenderViewModel : ViewModelBase
{
    private readonly IBuildToolDefenderService _buildToolDefenderService;
    private readonly IUserConfirmationService _confirmationService;

    public BuildToolDefenderViewModel(
        IBuildToolDefenderService buildToolDefenderService,
        IUserConfirmationService confirmationService)
    {
        _buildToolDefenderService = buildToolDefenderService;
        _confirmationService = confirmationService;
        _ = RefreshAsync();
    }

    public string Title => "Build-Schutz";

    [ObservableProperty]
    private string _summaryText = "—";

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string? _statusMessage;

    public ObservableCollection<BuildToolProcessItemViewModel> ProcessEntries { get; } = new();

    [RelayCommand]
    private async Task RefreshAsync()
    {
        IsBusy = true;
        try
        {
            var state = await _buildToolDefenderService.GetStateAsync();
            ProcessEntries.Clear();
            foreach (var entry in state.Entries)
                ProcessEntries.Add(new BuildToolProcessItemViewModel(entry));

            SummaryText =
                $"{state.AppliedCount}/{state.RecommendedCount} empfohlene Prozesse ausgeschlossen · " +
                $"{state.AddedByApp.Count} von HorosPulse hinzugefügt";
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
    private async Task ApplyAsync()
    {
        if (!_confirmationService.Confirm(
                "Defender Build-Tool-Ausschlüsse",
                "Windows Defender scannt sonst jeden Compiler-/Build-Prozess synchron und verlangsamt dotnet build, npm und IDE-Starts.\n\n" +
                "HorosPulse fügt empfohlene Prozess-Ausschlüsse hinzu (dotnet.exe, MSBuild.exe, node.exe, Cursor.exe, …).\n\n" +
                "Sicherheitshinweis: Prozess-Ausschlüsse reduzieren den Schutz für diese Programme. Nur auf vertrauenswürdigen Dev-Systemen anwenden.\n\nFortfahren?"))
            return;

        IsBusy = true;
        try
        {
            var result = await _buildToolDefenderService.ApplyExclusionsAsync(userConfirmed: true);
            StatusMessage = result.Success
                ? string.Join("; ", result.Changes ?? [])
                : result.ErrorMessage;
            await RefreshAsync();
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task RollbackAsync()
    {
        if (!_confirmationService.Confirm(
                "Build-Tool-Ausschlüsse zurücksetzen",
                "Nur die von HorosPulse hinzugefügten Defender-Prozess-Ausschlüsse werden entfernt.\n\nFortfahren?"))
            return;

        IsBusy = true;
        try
        {
            var result = await _buildToolDefenderService.RollbackExclusionsAsync();
            StatusMessage = result.Success
                ? string.Join("; ", result.Changes ?? [])
                : result.ErrorMessage;
            await RefreshAsync();
        }
        finally
        {
            IsBusy = false;
        }
    }
}

public sealed class BuildToolProcessItemViewModel
{
    public BuildToolProcessItemViewModel(BuildToolProcessEntry entry)
    {
        ProcessName = entry.ProcessName;
        DisplayName = entry.DisplayName;
        Category = entry.Category;
        IsApplied = entry.IsApplied;
    }

    public string ProcessName { get; }
    public string DisplayName { get; }
    public string Category { get; }
    public bool IsApplied { get; }
    public string StatusText => IsApplied ? "Aktiv" : "Fehlt";
}
