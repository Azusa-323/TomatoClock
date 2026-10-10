using System.IO;
using System.Text.Json;
using TomatoClock.Models;

namespace TomatoClock.Services;

public class StatsService
{
    private static readonly string StatsPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "TomatoClock", "stats.json");

    private readonly DailyStats _stats = new();
    private DateTime _currentDate = DateTime.Today;

    public event EventHandler? Changed;

    public int TodayCount => _stats.GetCount(_currentDate);

    public StatsService()
    {
        try
        {
            if (File.Exists(StatsPath))
            {
                _stats = JsonSerializer.Deserialize<DailyStats>(File.ReadAllText(StatsPath)) ?? new DailyStats();
            }
        }
        catch (Exception)
        {
            // 损坏的统计文件:从零开始
        }
    }

    public void IncrementToday()
    {
        _stats.SetCount(DateTime.Today, _stats.GetCount(DateTime.Today) + 1);
        Save();
        Changed?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>跨天检查:日期变化后今日计数归零显示</summary>
    public void CheckDayRollover()
    {
        if (DateTime.Today == _currentDate)
            return;

        _currentDate = DateTime.Today;
        Changed?.Invoke(this, EventArgs.Empty);
    }

    private void Save()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(StatsPath)!);
        File.WriteAllText(StatsPath, JsonSerializer.Serialize(_stats));
    }
}
