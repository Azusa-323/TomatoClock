using System.IO;
using System.Windows;
using Microsoft.Win32;

namespace TomatoClock.Services;

/// <summary>
/// 主题跟随 Windows 系统:读取 AppsUseLightTheme 注册表,
/// 并通过 SystemEvents.UserPreferenceChanged 响应实时切换。
/// </summary>
public class ThemeService : IDisposable
{
    private const string PersonalizeKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";

    public event EventHandler? ThemeChanged;

    public ThemeService()
    {
        SystemEvents.UserPreferenceChanged += OnUserPreferenceChanged;
    }

    public bool IsDarkTheme()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(PersonalizeKeyPath);
            if (key?.GetValue("AppsUseLightTheme") is int value)
                return value == 0;
        }
        catch (Exception)
        {
            // 读取失败按浅色处理
        }
        return false;
    }

    public void ApplyCurrentTheme()
    {
        var source = new Uri($"pack://application:,,,/Themes/{(IsDarkTheme() ? "Dark" : "Light")}.xaml");
        var dictionary = new ResourceDictionary { Source = source };

        var merged = Application.Current.Resources.MergedDictionaries;
        var existing = merged.FirstOrDefault(d => d.Source != null && d.Source.OriginalString.Contains("Themes/"));
        if (existing != null)
        {
            merged[merged.IndexOf(existing)] = dictionary;
        }
        else
        {
            merged.Add(dictionary);
        }

        ThemeChanged?.Invoke(this, EventArgs.Empty);
    }

    private void OnUserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
    {
        if (e.Category is UserPreferenceCategory.General or UserPreferenceCategory.VisualStyle)
            ApplyCurrentTheme();
    }

    public void Dispose()
    {
        SystemEvents.UserPreferenceChanged -= OnUserPreferenceChanged;
    }
}
