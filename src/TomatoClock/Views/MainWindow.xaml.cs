using System.ComponentModel;
using System.Windows;
using TomatoClock.Services;
using TomatoClock.ViewModels;

namespace TomatoClock.Views;

public partial class MainWindow : Window
{
    private readonly SettingsService _settingsService;
    private readonly AudioService _audioService;
    private readonly PomodoroEngine _engine;
    private readonly MainViewModel _viewModel;
    private readonly StatsService _statsService;
    private bool _reallyExit;

    public MainWindow()
    {
        InitializeComponent();

        _settingsService = new SettingsService();
        _statsService = new StatsService();
        _audioService = new AudioService();
        _engine = new PomodoroEngine(() => _settingsService.Settings);

        _viewModel = new MainViewModel(_settingsService, _statsService, _engine, _audioService);
        _viewModel.OpenSettingsRequested += (_, _) => OpenSettingsWindow();
        _viewModel.PropertyChanged += ViewModel_PropertyChanged;
        DataContext = _viewModel;

        UpdateTray();
    }

    private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(MainViewModel.RemainingText)
            or nameof(MainViewModel.SessionLabel)
            or nameof(MainViewModel.Progress))
        {
            UpdateTray();
        }
    }

    private void UpdateTray()
    {
        TrayIcon.ToolTipText = $"{_viewModel.RemainingText} · {_viewModel.SessionLabel}";
        TrayIcon.IconSource = TrayIconRenderer.Render(
            _engine.CurrentSession, _viewModel.Progress, _viewModel.RemainingTime);
    }

    private void Window_Closing(object? sender, CancelEventArgs e)
    {
        if (_reallyExit)
            return;

        // 关闭按钮 = 最小化到托盘
        e.Cancel = true;
        Hide();
    }

    private void TrayIcon_OnTrayMouseDoubleClick(object sender, RoutedEventArgs e) => ShowMainWindow();

    private void ShowMenuItem_Click(object sender, RoutedEventArgs e) => ShowMainWindow();

    private void ShowMainWindow()
    {
        Show();
        WindowState = WindowState.Normal;
        Activate();
    }

    private void TogglePauseMenuItem_Click(object sender, RoutedEventArgs e) =>
        _viewModel.StartPauseCommand.Execute(null);

    private void ExitMenuItem_Click(object sender, RoutedEventArgs e)
    {
        _reallyExit = true;
        TrayIcon.Dispose();
        Application.Current.Shutdown();
    }

    private void OpenSettingsWindow()
    {
        var window = new SettingsWindow(_settingsService, _audioService) { Owner = this };
        window.ShowDialog();
    }
}
