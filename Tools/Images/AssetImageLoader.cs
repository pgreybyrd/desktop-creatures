using System.IO;
using System.Windows.Media.Imaging;

namespace Desktop_Creatures.Assets;

public static class AssetImageLoader
{
    public static BitmapImage Load(string path)
    {
        string fullPath =
            Path.Combine(
                AppContext.BaseDirectory,
                path.Replace(
                    '/',
                    Path.DirectorySeparatorChar));

        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException(
                $"Image asset not found: {fullPath}",
                fullPath);
        }

        var image = new BitmapImage();

        image.BeginInit();

        image.UriSource =
            new Uri(
                fullPath,
                UriKind.Absolute);

        image.CacheOption =
            BitmapCacheOption.OnLoad;

        image.EndInit();

        image.Freeze();

        return image;
    }
}