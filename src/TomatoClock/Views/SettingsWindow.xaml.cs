using System.Windows;
using TomatoClock.Services;
using TomatoClock.ViewModels;

namespace TomatoClock.Views;

public partial class SettingsWindow : Window
{
    private readonly SettingsViewModel _viewModel;

    public SettingsWindow(SettingsService settingsService, AudioService audioService)
    {
        InitializeComponent();

        _viewModel = new SettingsViewModel(settingsService, audioService);
        _viewModel.RequestClose += Close;
        DataContext = _viewModel;
    }
}
