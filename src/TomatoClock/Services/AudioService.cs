using System.IO;
using System.Media;
using System.Windows;

namespace TomatoClock.Services;

/// <summary>
/// 提示音播放。使用 winmm 的 SoundPlayer 而非 WPF MediaPlayer:
/// MediaPlayer 依赖 Windows Media Player 运行时且异步打开、失败静默,
/// 在无 WMP 的系统(如 Windows Server)上完全不发声。
/// </summary>
public class AudioService
{
    public const int SoundCount = 3;

    private readonly SoundPlayer[] _players = new SoundPlayer[SoundCount];

    public AudioService()
    {
        for (var i = 0; i < SoundCount; i++)
        {
            var uri = new Uri($"pack://application:,,,/Resources/Sounds/sound{i + 1}.wav");
            var resource = Application.GetResourceStream(uri);
            var memory = new MemoryStream();
            resource.Stream.CopyTo(memory);
            memory.Position = 0;
            _players[i] = new SoundPlayer(memory);
        }
    }

    public void Play(int soundIndex)
    {
        var player = _players[Math.Clamp(soundIndex, 0, SoundCount - 1)];
        try
        {
            player.Play(); // 异步播放,不阻塞 UI
        }
        catch (Exception ex)
        {
            App.LogError(ex);
        }
    }
}
