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

        Width = poi.Width;
        Height = poi.Height;
        Topmost = true; // poi.AlwaysOnTop;

        PoiImage.Width = Width;
        PoiImage.Height = Height;

        Left = poi.Position.X;
        Top = poi.Position.Y;

        PoiImage.Source = new BitmapImage(
            new Uri($"pack://application:,,,/{poi.AssetPath}")
        );
    }

    private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        DragMove();

        _poi.Position = new System.Windows.Point(Left, Top);
    }
}