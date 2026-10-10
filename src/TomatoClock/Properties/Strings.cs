using System.Resources;

namespace TomatoClock.Properties;

public static class Strings
{
    private static readonly ResourceManager Manager =
        new("TomatoClock.Properties.Strings", typeof(Strings).Assembly);

    public static string AppTitle => Manager.GetString("AppTitle") ?? "";
    public static string SessionFocus => Manager.GetString("SessionFocus") ?? "";
    public static string SessionShortBreak => Manager.GetString("SessionShortBreak") ?? "";
    public static string SessionLongBreak => Manager.GetString("SessionLongBreak") ?? "";
    public static string CycleFormat => Manager.GetString("CycleFormat") ?? "";
    public static string TodayCountFormat => Manager.GetString("TodayCountFormat") ?? "";
    public static string BtnStart => Manager.GetString("BtnStart") ?? "";
    public static string BtnPause => Manager.GetString("BtnPause") ?? "";
    public static string BtnResume => Manager.GetString("BtnResume") ?? "";
    public static string BtnSkip => Manager.GetString("BtnSkip") ?? "";
    public static string BtnReset => Manager.GetString("BtnReset") ?? "";
    public static string BtnSettings => Manager.GetString("BtnSettings") ?? "";
    public static string BtnSave => Manager.GetString("BtnSave") ?? "";
    public static string BtnCancel => Manager.GetString("BtnCancel") ?? "";
    public static string BtnTestSound => Manager.GetString("BtnTestSound") ?? "";
    public static string ToastFocusTitle => Manager.GetString("ToastFocusTitle") ?? "";
    public static string ToastFocusAutoMsg => Manager.GetString("ToastFocusAutoMsg") ?? "";
    public static string ToastFocusMsg => Manager.GetString("ToastFocusMsg") ?? "";
    public static string ToastBreakTitle => Manager.GetString("ToastBreakTitle") ?? "";
    public static string ToastBreakAutoMsg => Manager.GetString("ToastBreakAutoMsg") ?? "";
    public static string ToastBreakMsg => Manager.GetString("ToastBreakMsg") ?? "";
    public static string TrayShow => Manager.GetString("TrayShow") ?? "";
    public static string TrayTogglePause => Manager.GetString("TrayTogglePause") ?? "";
    public static string TrayExit => Manager.GetString("TrayExit") ?? "";
    public static string SettingsTitle => Manager.GetString("SettingsTitle") ?? "";
    public static string LabelFocusMinutes => Manager.GetString("LabelFocusMinutes") ?? "";
    public static string LabelShortBreakMinutes => Manager.GetString("LabelShortBreakMinutes") ?? "";
    public static string LabelLongBreakMinutes => Manager.GetString("LabelLongBreakMinutes") ?? "";
    public static string LabelLongBreakInterval => Manager.GetString("LabelLongBreakInterval") ?? "";
    public static string LabelSound => Manager.GetString("LabelSound") ?? "";
    public static string LabelAutoCycle => Manager.GetString("LabelAutoCycle") ?? "";
    public static string LabelAlwaysOnTop => Manager.GetString("LabelAlwaysOnTop") ?? "";
    public static string LabelStartWithWindows => Manager.GetString("LabelStartWithWindows") ?? "";
    public static string SingleInstanceMsg => Manager.GetString("SingleInstanceMsg") ?? "";
}
