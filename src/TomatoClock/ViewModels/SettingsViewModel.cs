using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TomatoClock.Services;

namespace TomatoClock.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly SettingsService _settingsService;
    private readonly AudioService _audio;

    public event Action? RequestClose;

    [ObservableProperty] private int _focusMinutes;
    [ObservableProperty] private int _shortBreakMinutes;
    [ObservableProperty] private int _longBreakMinutes;
    [ObservableProperty] private int _longBreakInterval;
    [ObservableProperty] private int _soundIndex;
    [ObservableProperty] private bool _autoCycle;
    [ObservableProperty] private bool _alwaysOnTop;
    [ObservableProperty] private bool _startWithWindows;

    public IReadOnlyList<string> SoundNames { get; } =
        new[] { "清脆叮声", "双音叮咚", "柔和和弦" };

    public SettingsViewModel(SettingsService settingsService, AudioService audio)
    {
        _settingsService = settingsService;
        _audio = audio;

        var settings = settingsService.Settings;
        FocusMinutes = settings.FocusMinutes;
        ShortBreakMinutes = settings.ShortBreakMinutes;
        LongBreakMinutes = settings.LongBreakMinutes;
        LongBreakInterval = settings.LongBreakInterval;
        SoundIndex = settings.SoundIndex;
        AutoCycle = settings.AutoCycle;
        AlwaysOnTop = settings.AlwaysOnTop;
        StartWithWindows = settings.StartWithWindows;
    }

    [RelayCommand]
    private void Save()
    {
        var settings = _settingsService.Settings;
        settings.FocusMinutes = Math.Clamp(FocusMinutes, 1, 180);
        settings.ShortBreakMinutes = Math.Clamp(ShortBreakMinutes, 1, 60);
        settings.LongBreakMinutes = Math.Clamp(LongBreakMinutes, 1, 90);
        settings.LongBreakInterval = Math.Clamp(LongBreakInterval, 2, 8);
        settings.SoundIndex = Math.Clamp(SoundIndex, 0, AudioService.SoundCount - 1);
        settings.AutoCycle = AutoCycle;
        settings.AlwaysOnTop = AlwaysOnTop;
        settings.StartWithWindows = StartWithWindows;
        _settingsService.Save();

        AutoStartService.SetEnabled(StartWithWindows);

        RequestClose?.Invoke();
    }

    [RelayCommand]
    private void Cancel() => RequestClose?.Invoke();

    [RelayCommand]
    private void TestSound() => _audio.Play(SoundIndex);
}
