namespace WindowsPerformance.Core.Models;

public sealed class VisualEffectsState
{
    public bool AnimationsEnabled { get; init; }
    public bool DragFullWindows { get; init; }
    public bool MenuAnimation { get; init; }
    public bool ComboBoxAnimation { get; init; }
    public bool ListBoxSmoothScrolling { get; init; }
    public bool CursorShadow { get; init; }
    public string? UserPreferencesMaskHex { get; init; }
}
