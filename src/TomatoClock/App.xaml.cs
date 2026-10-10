using System.IO;
using System.Windows;
using TomatoClock.Properties;
using TomatoClock.Services;

namespace TomatoClock;

public partial class App : Application
{
    private Mutex? _singleInstanceMutex;
    private ThemeService? _themeService;

    private static readonly string LogPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "TomatoClock", "error.log");

    protected override void OnStartup(StartupEventArgs e)
    {
        _singleInstanceMutex = new Mutex(true, "TomatoClock_SingleInstance", out var isNewInstance);
        if (!isNewInstance)
        {
            MessageBox.Show(Strings.SingleInstanceMsg, Strings.AppTitle,
                MessageBoxButton.OK, MessageBoxImage.Information);
            Shutdown();
            return;
        }

        DispatcherUnhandledException += (_, args) =>
        {
            LogError(args.Exception);
            MessageBox.Show(
                $"{Strings.AppTitle}遇到了未处理的错误,即将退出。\n\n{args.Exception.Message}\n\n详情见:{LogPath}",
                Strings.AppTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            args.Handled = true;
            Shutdown();
        };

        _themeService = new ThemeService();
        _themeService.ApplyCurrentTheme();

        base.OnStartup(e);
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _themeService?.Dispose();
        try
        {
            _singleInstanceMutex?.ReleaseMutex();
        }
        catch (Exception)
        {
            // 未持有所有权时忽略
        }
        base.OnExit(e);
    }

    internal static void LogError(Exception exception)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(LogPath)!);
            File.AppendAllText(LogPath,
                $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}]{Environment.NewLine}{exception}{Environment.NewLine}");
        }
        catch (Exception)
        {
            // 日志写入失败时无处可报
        }
    }
}
