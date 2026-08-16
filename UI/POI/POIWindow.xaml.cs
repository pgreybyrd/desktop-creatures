using Desktop_Creatures.Tools.Images;
using Desktop_Creatures.World;
using Desktop_Creatures.World.Surfaces;
using PixelRecolor.Wpf;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Point = System.Windows.Point;

namespace Desktop_Creatures;

public partial class POIWindow : Window
{
    private readonly PointOfInterest _poi;
    private readonly SurfaceManager _surfaceManager;

    //public POIWindow(string imagePath, double width, double height, bool alwaysOnTop)
    public POIWindow(
        PointOfInterest poi,
        SurfaceManager surfaceManager)
    {
        InitializeComponent();

        _poi = poi;
        _surfaceManager = surfaceManager;

        Width = 
            poi.Settings.Width * _poi.AppSettings.Scale;
        Height = 
            poi.Settings.Height * _poi.AppSettings.Scale;

        PoiImage.Width = Width;
        PoiImage.Height = Height;

        Left = 
            poi.Position.X;
        Top = 
            poi.Position.Y;

        var path = 
            poi.IsEnabled || poi.Settings.EmptyAssetPath is null
            ? poi.Settings.AssetPath
            : poi.Settings.EmptyAssetPath;

        PoiImage.Source =
            LoadPoiImage();
    }

    //public void SnapToSurface()
    //{
    //    System.Windows.Point requestedBottomCenter = new(
    //        Left + ActualWidth / 2,
    //        Top + ActualHeight);

    //    WalkableSurface? surface =
    //        _surfaceManager.FindNearestSurface(requestedBottomCenter);

    //    if (surface is null)
    //        return;

    //    double visualOverlap = 4;

    //    Top = surface.Y - ActualHeight + visualOverlap;

    //    CurrentSurface = surface;

    //    UpdateInteractionPoints();
    //}

    private BitmapSource LoadPoiImage()
    {
        bool useEmpty =
            !_poi.IsEnabled &&
            _poi.Settings.EmptyAssetPath is not null;

        string assetPath =
            useEmpty
                ? _poi.Settings.EmptyAssetPath!
                : _poi.Settings.AssetPath;

        string? maskPath =
            useEmpty
                ? _poi.Settings.EmptyMaskPath
                : _poi.Settings.MaskPath;

        BitmapSource source =
            AssetImageLoader.Load(assetPath);

        if (maskPath is null)
            return source;

        BitmapSource mask =
            AssetImageLoader.Load(maskPath);

        return BitmapRecolorer.RecolorGrayscale(
            source,
            mask,
            hue: 285,
            saturation: 0.8);
    }

    private void Window_MouseLeftButtonDown(
        object sender,
        MouseButtonEventArgs e)
    {
        DragMove();

        var droppedPosition =
            new Point(
                Left,
                Top);

        var snappedPosition =
            _surfaceManager.SnapPoiToSurface(
                droppedPosition,
                ActualWidth,
                ActualHeight,
                30);

        if (snappedPosition is not null)
        {
            Left =
                snappedPosition.Value.X;

            Top =
                snappedPosition.Value.Y;
        }

        _poi.Position =
            new Point(
                Left,
                Top);
    }
}