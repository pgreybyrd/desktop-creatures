using Desktop_Creatures.Utilities;
using Desktop_Creatures.World;
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace Desktop_Creatures;

public partial class POIWindow : Window
{
    private readonly PointOfInterest _poi;

    //public POIWindow(string imagePath, double width, double height, bool alwaysOnTop)
    public POIWindow(PointOfInterest poi)
    {
        InitializeComponent();

        _poi = poi;

        Width = poi.Settings.Width;
        Height = poi.Settings.Height;
        Topmost = true; // poi.AlwaysOnTop;

        PoiImage.Width = Width;
        PoiImage.Height = Height;

        Left = poi.Position.X;
        Top = poi.Position.Y;

        var path = poi.IsAvailable || poi.Settings.EmptyAssetPath is null
            ? poi.Settings.AssetPath
            : poi.Settings.EmptyAssetPath;

        PoiImage.Source = new BitmapImage(
            new Uri($"pack://application:,,,/{path}")
        );
    }

    private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {

        DragMove();

        _poi.Position = new System.Windows.Point(Left, Top);

        Logger.LogDebug(
            $"Bowl window moved to ({Left:F1}, {Top:F1})");

        Logger.LogDebug(
            $"POI says ({_poi.Position.X:F1}, {_poi.Position.Y:F1})");
    }
}