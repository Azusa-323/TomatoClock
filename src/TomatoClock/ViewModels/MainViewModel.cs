using System.Windows.Media;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TomatoClock.Models;
using TomatoClock.Properties;
using TomatoClock.Services;

namespace TomatoClock.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly SettingsService _settingsService;
    private readonly StatsService _statsService;
    private readonly PomodoroEngine _engine;
    private readonly AudioService _audio;
    private readonly DispatcherTimer _timer;

    public event EventHandler? OpenSettingsRequested;

    [ObservableProperty] private string _title = Strings.AppTitle;
    [ObservableProperty] private string _remainingText = "";
    [ObservableProperty] private double _progress;
    [ObservableProperty] private string _sessionLabel = "";
    [ObservableProperty] private string _cycleLabel = "";
    [ObservableProperty] private string _todayCountText = "";
    [ObservableProperty] private string _startPauseText = Strings.BtnStart;
    [ObservableProperty] private bool _isTopmost;
    [ObservableProperty] private Brush _phaseBrush = Brushes.Transparent;

    /// <summary>托盘图标用:当前显示的剩余时间(非绑定属性)</summary>
    public TimeSpan RemainingTime { get; private set; }

    public MainViewModel(SettingsService settingsService, StatsService statsService,
        PomodoroEngine engine, AudioService audio)
    {
        _settingsService = settingsService;
        _statsService = statsService;
        _engine = engine;
        _audio = audio;

        _engine.PhaseCompleted += OnPhaseCompleted;

        _settingsService.SettingsChanged += (_, _) =>
        {
            IsTopmost = _settingsService.Settings.AlwaysOnTop;
            RefreshDisplay();
        };
        _statsService.Changed += (_, _) => RefreshDisplay();

        _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(250) };
        _timer.Tick += (_, _) =>
        {
            _engine.Tick();
            _statsService.CheckDayRollover();
            RefreshDisplay();
        };
        _timer.Start();

        IsTopmost = settingsService.Settings.AlwaysOnTop;
        RefreshDisplay();
    }

    [RelayCommand]
    private void StartPause()
    {
        if (_engine.State == EngineState.Running)
            _engine.Pause();
        else
            _engine.Start();
        RefreshDisplay();
    }

    [RelayCommand]
    private void Skip()
    {
        _engine.Skip();
        RefreshDisplay();
    }

    [RelayCommand]
    private void Reset()
    {
        _engine.Reset();
        RefreshDisplay();
    }

    [RelayCommand]
    private void OpenSettings() => OpenSettingsRequested?.Invoke(this, EventArgs.Empty);

    private void OnPhaseCompleted(object? sender, SessionType completed)
    {
        if (completed == SessionType.Focus)
        {
            _statsService.IncrementToday();
            ToastService.Show(Strings.ToastFocusTitle,
                _settingsService.Settings.AutoCycle ? Strings.ToastFocusAutoMsg : Strings.ToastFocusMsg);
        }
        else
        {
            ToastService.Show(Strings.ToastBreakTitle,
                _settingsService.Settings.AutoCycle ? Strings.ToastBreakAutoMsg : Strings.ToastBreakMsg);
        }

        _audio.Play(_settingsService.Settings.SoundIndex);
    }

    private void RefreshDisplay()
    {
        var state = _engine.State;
        var remaining = state == EngineState.Idle ? _engine.CurrentDuration : _engine.Remaining;
        RemainingTime = remaining;

        var totalSeconds = (int)Math.Ceiling(remaining.TotalSeconds);
        if (totalSeconds < 0)
            totalSeconds = 0;
        RemainingText = $"{totalSeconds / 60:D2}:{totalSeconds % 60:D2}";

        var duration = _engine.CurrentDuration;
        Progress = duration > TimeSpan.Zero
            ? Math.Clamp(1.0 - remaining.Ticks / (double)duration.Ticks, 0.0, 1.0)
            : 0.0;

        SessionLabel = _engine.CurrentSession switch
        {
            SessionType.Focus => Strings.SessionFocus,
            SessionType.ShortBreak => Strings.SessionShortBreak,
            _ => Strings.SessionLongBreak
        };

        var interval = Math.Max(1, _settingsService.Settings.LongBreakInterval);
        CycleLabel = string.Format(Strings.CycleFormat, _engine.FocusInCycle, interval);
        TodayCountText = string.Format(Strings.TodayCountFormat, _statsService.TodayCount);

        StartPauseText = state switch
        {
            EngineState.Running => Strings.BtnPause,
            EngineState.Paused => Strings.BtnResume,
            _ => Strings.BtnStart
        };

        PhaseBrush = FindPhaseBrush(_engine.CurrentSession);

        Title = state == EngineState.Idle
            ? Strings.AppTitle
            : $"{RemainingText} · {SessionLabel} - {Strings.AppTitle}";
    }

    private static Brush FindPhaseBrush(SessionType session)
    {
        var key = session switch
        {
            SessionType.Focus => "FocusBrush",
            SessionType.ShortBreak => "ShortBreakBrush",
            _ => "LongBreakBrush"
        };
        return System.Windows.Application.Current.TryFindResource(key) as Brush ?? Brushes.Transparent;
    }
}
