using Desktop_Creatures.Rendering.Fonts;
using System.Windows;
using System.Windows.Media;
using WpfPoint = System.Windows.Point;
using WpfSize = System.Windows.Size;

namespace Desktop_Creatures.UI.Controls;

public sealed class BitmapText : FrameworkElement
{
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

    public BitmapText()
    {
        RenderOptions.SetBitmapScalingMode(
            this,
            BitmapScalingMode.NearestNeighbor);

        SnapsToDevicePixels = true;
    }

    protected override void OnRender(DrawingContext drawingContext)
    {
        base.OnRender(drawingContext);

        if (Font is null ||
            string.IsNullOrEmpty(Text))
        {
            return;
        }

        BitmapFontRenderer.DrawText(
            drawingContext,
            Font,
            Text,
            new WpfPoint(0, 0),
            FontScale);
    }

    protected override WpfSize MeasureOverride(
        WpfSize availableSize)
    {
        if (Font is null ||
            string.IsNullOrEmpty(Text))
        {
            return new WpfSize(0, 0);
        }

        double width =
            Font.MeasureText(Text) * FontScale;

        double height =
            Font.LineHeight * FontScale;

        return new WpfSize(
            width,
            height);
    }
}