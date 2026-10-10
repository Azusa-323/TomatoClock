namespace TomatoClock.Models;

public class DailyStats
{
    public Dictionary<string, int> Counts { get; set; } = new();

    public int GetCount(DateTime date)
    {
        return Counts.TryGetValue(Key(date), out var value) ? value : 0;
    }

    public void SetCount(DateTime date, int value)
    {
        Counts[Key(date)] = value;
    }

    private static string Key(DateTime date) => date.ToString("yyyy-MM-dd");
}
