using Desktop_Creatures.Rendering.Fonts;
using System.Windows;
using System.Windows.Media;
using WpfPoint = System.Windows.Point;
using WpfSize = System.Windows.Size;

namespace Desktop_Creatures.UI.Controls;

public sealed class BitmapText : FrameworkElement
{
    private BitmapFont? ActiveFont =>
        Font ?? BitmapFontRegistry.DefaultFont;

    public static readonly DependencyProperty TextProperty =
        DependencyProperty.Register(
            nameof(Text),
            typeof(string),
            typeof(BitmapText),
            new FrameworkPropertyMetadata(
                string.Empty,
                FrameworkPropertyMetadataOptions.AffectsRender |
                FrameworkPropertyMetadataOptions.AffectsMeasure));

    public static readonly DependencyProperty FontProperty =
        DependencyProperty.Register(
            nameof(Font),
            typeof(BitmapFont),
            typeof(BitmapText),
            new FrameworkPropertyMetadata(
                null,
                FrameworkPropertyMetadataOptions.AffectsRender |
                FrameworkPropertyMetadataOptions.AffectsMeasure));

    public static readonly DependencyProperty FontScaleProperty =
        DependencyProperty.Register(
            nameof(FontScale),
            typeof(double),
            typeof(BitmapText),
            new FrameworkPropertyMetadata(
                1.0,
                FrameworkPropertyMetadataOptions.AffectsRender |
                FrameworkPropertyMetadataOptions.AffectsMeasure));

    public static readonly DependencyProperty LineSpacingProperty =
        DependencyProperty.Register(
            nameof(LineSpacing),
            typeof(double),
            typeof(BitmapText),
            new FrameworkPropertyMetadata(
                1.0,
                FrameworkPropertyMetadataOptions.AffectsRender |
                FrameworkPropertyMetadataOptions.AffectsMeasure));

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public BitmapFont? Font
    {
        get => (BitmapFont?)GetValue(FontProperty);
        set => SetValue(FontProperty, value);
    }

    public double FontScale
    {
        get => (double)GetValue(FontScaleProperty);
        set => SetValue(FontScaleProperty, value);
    }

    public double LineSpacing
    {
        get => (double)GetValue(LineSpacingProperty);
        set => SetValue(LineSpacingProperty, value);
    }

    public BitmapText()
    {
        RenderOptions.SetBitmapScalingMode(
            this,
            BitmapScalingMode.NearestNeighbor);

        SnapsToDevicePixels = true;
    }

    protected override void OnRender(
        DrawingContext drawingContext)
    {
        base.OnRender(drawingContext);

        BitmapFont? font = ActiveFont;

        if (font is null ||
            string.IsNullOrEmpty(Text))
        {
            return;
        }

        List<string> lines =
            WrapText(ActualWidth, font);

        double y = 0;

        foreach (string line in lines)
        {
            BitmapFontRenderer.DrawText(
                drawingContext,
                font,
                line,
                new WpfPoint(0, y),
                FontScale);

            y += (font.LineHeight + LineSpacing) * FontScale;
        }
    }

    protected override WpfSize MeasureOverride(
        WpfSize availableSize)
    {
        BitmapFont? font = ActiveFont;

        if (font is null ||
            string.IsNullOrEmpty(Text))
        {
            return new WpfSize(0, 0);
        }

        double width = availableSize.Width;

        if (double.IsInfinity(width))
        {
            width =
                font.MeasureText(Text) * FontScale;
        }

        List<string> lines =
            WrapText(width, font);

        double measuredWidth = 0;

        foreach (string line in lines)
        {
            measuredWidth = Math.Max(
                measuredWidth,
                font.MeasureText(line) * FontScale);
        }

        double measuredHeight =
        (
            lines.Count * font.LineHeight +
            Math.Max(0, lines.Count - 1) * LineSpacing
        ) * FontScale;

        return new WpfSize(
            measuredWidth,
            measuredHeight);
    }

    private List<string> WrapText(
        double availableWidth,
        BitmapFont font)
    {
        var lines = new List<string>();

        if (string.IsNullOrEmpty(Text))
            return lines;

        double unscaledWidth = availableWidth / FontScale;

        foreach (string paragraph in Text.Split('\n'))
        {
            string[] words = paragraph.Split(' ');

            string currentLine = "";

            foreach (string word in words)
            {
                string testLine =
                    string.IsNullOrEmpty(currentLine)
                        ? word
                        : $"{currentLine} {word}";

                if (font.MeasureText(testLine) <= unscaledWidth)
                {
                    currentLine = testLine;
                }
                else
                {
                    if (!string.IsNullOrEmpty(currentLine))
                        lines.Add(currentLine);

                    currentLine = word;
                }
            }

            if (!string.IsNullOrEmpty(currentLine))
                lines.Add(currentLine);
        }

        return lines;
    }
}