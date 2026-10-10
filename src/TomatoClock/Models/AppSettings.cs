namespace TomatoClock.Models;

public class AppSettings
{
    public int FocusMinutes { get; set; } = 25;
    public int ShortBreakMinutes { get; set; } = 5;
    public int LongBreakMinutes { get; set; } = 15;
    public int LongBreakInterval { get; set; } = 4;
    public int SoundIndex { get; set; } = 0;
    public bool AutoCycle { get; set; } = true;
    public bool AlwaysOnTop { get; set; } = false;
    public bool StartWithWindows { get; set; } = false;
}
