using System.IO;

namespace Desktop_Creatures.Persistence;

public static class AppDataPaths
{
    public static string RootDirectory { get; } =
        Path.Combine(
            Environment.GetFolderPath(
                Environment.SpecialFolder.LocalApplicationData),
#if DEBUG
            "DesktopCreatures.Debug"
#else
            "DesktopCreatures"
#endif
        );

    public static string CreatureSavePath =>
        Path.Combine(
            RootDirectory,
            "creatures.json");

    public static string SettingsPath =>
        Path.Combine(
            RootDirectory,
            "settings.json");
}