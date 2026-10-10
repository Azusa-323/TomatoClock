using System.IO;
using System.Text.Json;
using TomatoClock.Models;

namespace TomatoClock.Services;

public class SettingsService
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };
    private static readonly string SettingsPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "TomatoClock", "settings.json");

    public AppSettings Settings { get; }

    public event EventHandler? SettingsChanged;

    public SettingsService()
    {
        Settings = Load();
    }

    public void Save()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(SettingsPath)!);
        File.WriteAllText(SettingsPath, JsonSerializer.Serialize(Settings, JsonOptions));
        SettingsChanged?.Invoke(this, EventArgs.Empty);
    }

    private static AppSettings Load()
    {
        try
        {
            if (File.Exists(SettingsPath))
            {
                return JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(SettingsPath)) ?? new AppSettings();
            }
        }
        catch (Exception)
        {
            // 损坏的设置文件:回退到默认值
        }
        return new AppSettings();
    }
}
