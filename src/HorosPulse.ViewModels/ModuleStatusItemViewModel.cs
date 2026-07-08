namespace HorosPulse.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;

public sealed partial class ModuleStatusItemViewModel : ObservableObject
{
    public ModuleStatusItemViewModel(string moduleName, StatusBadgeKind status, string detail)
    {
        ModuleName = moduleName;
        Status = status;
        Detail = detail;
    }

    public string ModuleName { get; }

    public StatusBadgeKind Status { get; }

    public string StatusTag => Status.ToString();

    public string Detail { get; }

    public string StatusLabel => Status switch
    {
        StatusBadgeKind.Active => "Aktiv",
        StatusBadgeKind.Warning => "Warnung",
        StatusBadgeKind.Error => "Fehler",
        _ => "Bereit",
    };
}
