namespace HorosPulse.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;

public sealed partial class IndexerExcludeItemViewModel : ObservableObject
{
    public IndexerExcludeItemViewModel(string path, bool isSelected, bool isApplied)
    {
        Path = path;
        _isSelected = isSelected;
        IsApplied = isApplied;
    }

    public string Path { get; }

    public bool IsApplied { get; }

    [ObservableProperty]
    private bool _isSelected;
}
