namespace HorosPulse.ViewModels;

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HorosPulse.Core.Interfaces;
using HorosPulse.Core.Models;

public sealed partial class DevDriveAdvisorViewModel : ViewModelBase
{
    private readonly IDevDriveAdvisorService _advisorService;

    public DevDriveAdvisorViewModel(IDevDriveAdvisorService advisorService)
    {
        _advisorService = advisorService;
        _ = RefreshAsync();
    }

    public string Title => "Dev Drive";

    [ObservableProperty]
    private string _summaryText = "—";

    [ObservableProperty]
    private bool _hasDevDrive;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string? _statusMessage;

    public ObservableCollection<DevDriveVolumeItemViewModel> DevVolumes { get; } = new();

    public ObservableCollection<DevDriveVolumeItemViewModel> AllVolumes { get; } = new();

    public ObservableCollection<DevPathPlacementItemViewModel> PathPlacements { get; } = new();

    public ObservableCollection<DevDriveRecommendationItemViewModel> Recommendations { get; } = new();

    [RelayCommand]
    private async Task RefreshAsync()
    {
        IsBusy = true;
        StatusMessage = null;
        try
        {
            var state = await _advisorService.GetAssessmentAsync();

            HasDevDrive = state.HasDevDrive;
            SummaryText = state.SummaryText;

            DevVolumes.Clear();
            foreach (var volume in state.DevDriveVolumes)
                DevVolumes.Add(new DevDriveVolumeItemViewModel(volume));

            AllVolumes.Clear();
            foreach (var volume in state.AllVolumes)
                AllVolumes.Add(new DevDriveVolumeItemViewModel(volume));

            PathPlacements.Clear();
            foreach (var placement in state.PathPlacements)
                PathPlacements.Add(new DevPathPlacementItemViewModel(placement));

            Recommendations.Clear();
            foreach (var recommendation in state.Recommendations)
                Recommendations.Add(new DevDriveRecommendationItemViewModel(recommendation));
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
    private void OpenDevDriveSettings()
    {
        try
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = "ms-settings:developers",
                UseShellExecute = true,
            });
        }
        catch (Exception ex)
        {
            StatusMessage = $"Einstellungen konnten nicht geöffnet werden: {ex.Message}";
        }
    }
}

public sealed class DevDriveVolumeItemViewModel
{
    public DevDriveVolumeItemViewModel(DevDriveVolumeInfo volume)
    {
        DriveLetter = volume.DriveLetter;
        VolumeLabel = string.IsNullOrWhiteSpace(volume.VolumeLabel) ? "—" : volume.VolumeLabel;
        FileSystem = volume.FileSystem;
        IsDevDrive = volume.IsDevDrive;
        FreeSpaceDisplay = volume.FreeSpaceDisplay;
        TotalSpaceDisplay = volume.TotalSpaceDisplay;
        StatusText = volume.IsDevDrive ? "Dev Drive" : volume.IsReFs ? "ReFS" : "NTFS/anderes";
    }

    public string DriveLetter { get; }
    public string VolumeLabel { get; }
    public string FileSystem { get; }
    public bool IsDevDrive { get; }
    public string FreeSpaceDisplay { get; }
    public string TotalSpaceDisplay { get; }
    public string StatusText { get; }
}

public sealed class DevPathPlacementItemViewModel
{
    public DevPathPlacementItemViewModel(DevPathPlacement placement)
    {
        DisplayName = placement.DisplayName;
        Path = placement.Path;
        PathExists = placement.PathExists;
        ResolvedDriveLetter = placement.ResolvedDriveLetter ?? "—";
        StatusLabel = placement.StatusLabel;
        IsOptimal = placement.Status == DevPathPlacementStatus.OnDevDrive;
    }

    public string DisplayName { get; }
    public string Path { get; }
    public bool PathExists { get; }
    public string ResolvedDriveLetter { get; }
    public string StatusLabel { get; }
    public bool IsOptimal { get; }
    public string ExistsLabel => PathExists ? "Vorhanden" : "Nicht vorhanden";
}

public sealed class DevDriveRecommendationItemViewModel
{
    public DevDriveRecommendationItemViewModel(DevDriveRecommendation recommendation)
    {
        Title = recommendation.Title;
        Description = recommendation.Description;
        PriorityLabel = recommendation.PriorityLabel;
        RelatedPath = recommendation.RelatedPath;
    }

    public string Title { get; }
    public string Description { get; }
    public string PriorityLabel { get; }
    public string? RelatedPath { get; }
}
