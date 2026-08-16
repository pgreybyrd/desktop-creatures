using System.IO;

namespace Desktop_Creatures.Tools.Text;

public static class AssetTextLoader
{
    public static string Load(
        string path)
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
                $"Text asset not found: {fullPath}",
                fullPath);
        }

        return File.ReadAllText(
            fullPath);
    }
}