using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace HorosPulse.App.Helpers;

/// <summary>
/// Forwards mouse-wheel events from nested controls (e.g. DataGrid) to the nearest parent ScrollViewer.
/// </summary>
public static class ScrollViewerHelper
{
    public static readonly DependencyProperty BubbleMouseWheelProperty =
        DependencyProperty.RegisterAttached(
            "BubbleMouseWheel",
            typeof(bool),
            typeof(ScrollViewerHelper),
            new PropertyMetadata(false, OnBubbleMouseWheelChanged));

    public static bool GetBubbleMouseWheel(DependencyObject obj) =>
        (bool)obj.GetValue(BubbleMouseWheelProperty);

    public static void SetBubbleMouseWheel(DependencyObject obj, bool value) =>
        obj.SetValue(BubbleMouseWheelProperty, value);

    private static void OnBubbleMouseWheelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not UIElement element)
            return;

        if ((bool)e.NewValue)
            element.PreviewMouseWheel += OnPreviewMouseWheel;
        else
            element.PreviewMouseWheel -= OnPreviewMouseWheel;
    }

    private static void OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (sender is not DependencyObject source)
            return;

        if (sender is DataGrid dataGrid && TryScrollDataGridInternally(dataGrid, e))
            return;

        var scrollViewer = FindParent<ScrollViewer>(source);
        if (scrollViewer is null)
            return;

        e.Handled = true;
        scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - e.Delta);
    }

    private static bool TryScrollDataGridInternally(DataGrid dataGrid, MouseWheelEventArgs e)
    {
        var innerScrollViewer = FindChild<ScrollViewer>(dataGrid);
        if (innerScrollViewer is null
            || innerScrollViewer.ComputedVerticalScrollBarVisibility != Visibility.Visible)
            return false;

        if (e.Delta < 0 && innerScrollViewer.VerticalOffset < innerScrollViewer.ScrollableHeight)
            return true;

        if (e.Delta > 0 && innerScrollViewer.VerticalOffset > 0)
            return true;

        return false;
    }

    private static T? FindParent<T>(DependencyObject child) where T : DependencyObject
    {
        var parent = VisualTreeHelper.GetParent(child);
        while (parent is not null)
        {
            if (parent is T match)
                return match;

            parent = VisualTreeHelper.GetParent(parent);
        }

        return null;
    }

    private static T? FindChild<T>(DependencyObject parent) where T : DependencyObject
    {
        for (var i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            if (child is T match)
                return match;

            var descendant = FindChild<T>(child);
            if (descendant is not null)
                return descendant;
        }

        return null;
    }
}
