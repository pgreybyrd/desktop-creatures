using System.Windows.Media;

namespace Desktop_Creatures.Audio;

public sealed class CreatureSoundPlayer
{
    private readonly SoundSet _soundSet;
    private readonly Random _random = new();

    public CreatureSoundPlayer(
        SoundSet soundSet)
    {
        _soundSet = soundSet;
    }

    public void PlayRandom(
        string soundId,
        double volume = 1.0)
    {
        if (!_soundSet.TryGet(
                soundId,
                out var paths))
        {
            return;
        }

        if (paths.Length == 0)
            return;

        string path =
            paths[_random.Next(paths.Length)];

        var player =
            new MediaPlayer();

        player.Volume =
            Math.Clamp(volume, 0, 1);

        player.Open(
            new Uri(
                path,
                UriKind.RelativeOrAbsolute));

        player.MediaEnded += (_, _) =>
        {
            player.Close();
        };

        player.MediaFailed += (_, _) =>
        {
            player.Close();
        };

        player.Play();
    }
}