using System;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Desktop_Creatures;

public partial class POIWindow : Window
{
    public POIWindow(string imagePath, double width, double height, bool alwaysOnTop)
    {
        InitializeComponent();

        Width = width;
        Height = height;
        Topmost = alwaysOnTop;

        PoiImage.Width = width;
        PoiImage.Height = height;

        PoiImage.Source = new BitmapImage(
            new Uri($"pack://application:,,,/{imagePath}")
        );
    }
}