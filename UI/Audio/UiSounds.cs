using System.IO;
using System.Windows.Media;

namespace Desktop_Creatures.UI.Audio;

public static class UiSounds
{
    private static readonly List<MediaPlayer> ActivePlayers = [];

    public static void PlayButtonClick()
    {
        Play("button_click.wav");
    }

    private static void Play(string fileName)
    {
        string path = Path.Combine(
            AppContext.BaseDirectory,
            "Assets",
            "Sounds",
            fileName);

        var player = new MediaPlayer();

        ActivePlayers.Add(player);

        void Cleanup(object? sender, EventArgs e)
        {
            player.Close();
            ActivePlayers.Remove(player);
        }

        player.MediaEnded += Cleanup;
        player.MediaFailed += (_, _) =>
        {
            player.Close();
            ActivePlayers.Remove(player);
        };

        player.Open(
            new Uri(path, UriKind.Absolute));

        player.Play();
    }
}