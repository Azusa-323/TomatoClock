using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace TomatoClock.Controls;

public class ProgressRing : Control
{
    static ProgressRing()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(ProgressRing),
            new FrameworkPropertyMetadata(typeof(ProgressRing)));
    }

    public static readonly DependencyProperty ProgressProperty = DependencyProperty.Register(
        nameof(Progress), typeof(double), typeof(ProgressRing),
        new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty RingBrushProperty = DependencyProperty.Register(
        nameof(RingBrush), typeof(Brush), typeof(ProgressRing),
        new FrameworkPropertyMetadata(Brushes.Transparent, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty TrackBrushProperty = DependencyProperty.Register(
        nameof(TrackBrush), typeof(Brush), typeof(ProgressRing),
        new FrameworkPropertyMetadata(Brushes.Transparent, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty StrokeThicknessProperty = DependencyProperty.Register(
        nameof(StrokeThickness), typeof(double), typeof(ProgressRing),
        new FrameworkPropertyMetadata(12.0, FrameworkPropertyMetadataOptions.AffectsRender));

    public double Progress
    {
        get => (double)GetValue(ProgressProperty);
        set => SetValue(ProgressProperty, value);
    }

    public Brush RingBrush
    {
        get => (Brush)GetValue(RingBrushProperty);
        set => SetValue(RingBrushProperty, value);
    }

    public Brush TrackBrush
    {
        get => (Brush)GetValue(TrackBrushProperty);
        set => SetValue(TrackBrushProperty, value);
    }

    public double StrokeThickness
    {
        get => (double)GetValue(StrokeThicknessProperty);
        set => SetValue(StrokeThicknessProperty, value);
    }

    protected override void OnRender(DrawingContext drawingContext)
    {
        var size = Math.Min(RenderSize.Width, RenderSize.Height);
        if (size <= 0)
            return;

        var center = new Point(RenderSize.Width / 2, RenderSize.Height / 2);
        var radius = (size - StrokeThickness) / 2;
        if (radius <= 0)
            return;

        drawingContext.DrawEllipse(null, new Pen(TrackBrush, StrokeThickness), center, radius, radius);

        var progress = Math.Clamp(Progress, 0.0, 1.0);
        if (progress > 0)
        {
            var geometry = CreateArcGeometry(center, radius, 360 * progress);
            var pen = new Pen(RingBrush, StrokeThickness)
            {
                StartLineCap = PenLineCap.Round,
                EndLineCap = PenLineCap.Round
            };
            drawingContext.DrawGeometry(null, pen, geometry);
        }
    }

    /// <summary>创建从正上方开始、顺时针扫过 sweepDegrees 的圆弧</summary>
    public static Geometry CreateArcGeometry(Point center, double radius, double sweepDegrees)
    {
        sweepDegrees = Math.Clamp(sweepDegrees, 0.0, 360.0);
        if (sweepDegrees >= 360.0)
            return new EllipseGeometry(center, radius, radius);

        var start = new Point(center.X, center.Y - radius);
        var radians = (sweepDegrees - 90) * Math.PI / 180.0;
        var end = new Point(center.X + radius * Math.Cos(radians), center.Y + radius * Math.Sin(radians));
        var figure = new PathFigure
        {
            StartPoint = start,
            IsClosed = false,
            Segments =
            {
                new ArcSegment(end, new Size(radius, radius), 0, sweepDegrees > 180, SweepDirection.Clockwise, true)
            }
        };
        return new PathGeometry(new[] { figure });
    }
}
