using Microsoft.Toolkit.Uwp.Notifications;

namespace TomatoClock.Services;

public static class ToastService
{
    public static void Show(string title, string message)
    {
        try
        {
            new ToastContentBuilder()
                .AddText(title)
                .AddText(message)
                .Show();
        }
        catch (Exception)
        {
            // 通知失败不影响主流程
        }
    }
}
