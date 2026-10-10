using System.Globalization;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TomatoClock.Controls;
using TomatoClock.Models;

namespace TomatoClock.Services;

/// <summary>动态渲染托盘图标:阶段颜色 + 进度弧 + 剩余分钟数</summary>
public static class TrayIconRenderer
{
    private static readonly Dictionary<SessionType, Color> PhaseColors = new()
    {
        [SessionType.Focus] = Color.FromRgb(0xE7, 0x4C, 0x3C),
        [SessionType.ShortBreak] = Color.FromRgb(0x2E, 0xCC, 0x71),
        [SessionType.LongBreak] = Color.FromRgb(0x34, 0x98, 0xDB)
    };

    public static ImageSource Render(SessionType session, double progress, TimeSpan remaining)
    {
        const int size = 32;
        var visual = new DrawingVisual();
        using (var dc = visual.RenderOpen())
        {
            var color = PhaseColors.GetValueOrDefault(session, Colors.Gray);
            var fillBrush = new SolidColorBrush(color);
            fillBrush.Freeze();

            var center = new Point(size / 2.0, size / 2.0);
            var radius = size / 2.0 - 2;

            // 实心阶段色圆底
            dc.DrawEllipse(fillBrush, null, center, radius, radius);

            // 白色进度弧(已进行的比例)
            var arc = ProgressRing.CreateArcGeometry(center, radius, 360 * Math.Clamp(progress, 0.0, 1.0));
            var arcPen = new Pen(Brushes.White, 2.5) { StartLineCap = PenLineCap.Round, EndLineCap = PenLineCap.Round };
            dc.DrawGeometry(null, arcPen, arc);

            // 中央剩余分钟数(不足一分钟显示秒数)
            var label = remaining.TotalMinutes >= 1
                ? ((int)Math.Ceiling(remaining.TotalMinutes)).ToString(CultureInfo.InvariantCulture)
                : remaining.Seconds.ToString(CultureInfo.InvariantCulture);
            var text = new FormattedText(
                label,
                CultureInfo.InvariantCulture,
                FlowDirection.LeftToRight,
                new Typeface(new FontFamily("Segoe UI"), FontStyles.Normal, FontWeights.Bold, FontStretches.Normal),
                12,
                Brushes.White,
                96.0);
            dc.DrawText(text, new Point(center.X - text.Width / 2, center.Y - text.Height / 2));
        }

        var bitmap = new RenderTargetBitmap(size, size, 96, 96, PixelFormats.Pbgra32);
        bitmap.Render(visual);
        bitmap.Freeze();
        return bitmap;
    }
}
