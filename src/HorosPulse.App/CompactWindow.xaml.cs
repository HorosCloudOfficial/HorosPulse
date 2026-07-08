using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using HorosPulse.Core.Interfaces;
using HorosPulse.ViewModels;

namespace HorosPulse.App;

public partial class CompactWindow : Window
{
    private const double OrganicShellDesignWidth = 318;
    private const double OrganicShellDesignHeight = 630;

    private readonly ICompactWindowCoordinator _coordinator;
    private CompactWindowViewModel? _viewModel;
    private int _lastStatusTick;

    public CompactWindow(CompactWindowViewModel viewModel, ICompactWindowCoordinator coordinator)
    {
        InitializeComponent();
        DataContext = viewModel;
        _viewModel = viewModel;
        _coordinator = coordinator;
        _viewModel.PropertyChanged += OnViewModelPropertyChanged;
        Closing += OnClosing;
        Loaded += (_, _) => UpdateOrganicShellClip();
    }

    private void OnRootShellSizeChanged(object sender, SizeChangedEventArgs e) =>
        UpdateOrganicShellClip();

    private void UpdateOrganicShellClip()
    {
        if (RootShell.ActualWidth <= 0 || RootShell.ActualHeight <= 0)
            return;

        var scaleX = RootShell.ActualWidth / OrganicShellDesignWidth;
        var scaleY = RootShell.ActualHeight / OrganicShellDesignHeight;
        var baseClip = (PathGeometry)Resources["OrganicShellClip"];
        var clip = baseClip.CloneCurrentValue();
        clip.Transform = new ScaleTransform(scaleX, scaleY);
        RootShell.Clip = clip;
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (_viewModel is null)
            return;

        switch (e.PropertyName)
        {
            case nameof(CompactWindowViewModel.StatusAnimationTick):
                if (_viewModel.StatusAnimationTick != _lastStatusTick)
                {
                    _lastStatusTick = _viewModel.StatusAnimationTick;
                    PlayStatusFadeIn();
                }
                break;
            case nameof(CompactWindowViewModel.IsHealing):
                if (_viewModel.IsHealing)
                    PlayHealEffects();
                break;
        }
    }

    private void PlayStatusFadeIn()
    {
        if (string.IsNullOrEmpty(_viewModel?.StatusMessage))
            return;

        var fadeIn = (Storyboard)FindResource("SbStatusFadeIn");
        StatusBlock.BeginStoryboard(fadeIn);
    }

    private void PlayHealEffects()
    {
        var flash = (Storyboard)FindResource("SbHealFlash");
        HealFlashOverlay.BeginStoryboard(flash);

        Particle1.Opacity = 1;
        Particle2.Opacity = 1;
        Particle3.Opacity = 1;

        Particle1.BeginStoryboard((Storyboard)FindResource("SbParticleBurst"));
        Particle2.BeginStoryboard((Storyboard)FindResource("SbParticleBurst2"));
        Particle3.BeginStoryboard((Storyboard)FindResource("SbParticleBurst3"));
    }

    private void OnDragRegionMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed)
            DragMove();
    }

    private void OnKillButtonClick(object sender, RoutedEventArgs e)
    {
        Process.GetCurrentProcess().Kill();
    }

    private void OnClosing(object? sender, CancelEventArgs e)
    {
        e.Cancel = true;
        _coordinator.HideCompactWindow();
    }

    protected override void OnClosed(EventArgs e)
    {
        if (_viewModel is not null)
            _viewModel.PropertyChanged -= OnViewModelPropertyChanged;

        base.OnClosed(e);
    }
}
