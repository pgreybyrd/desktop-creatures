namespace Desktop_Creatures.Audio;

public sealed class CreatureSoundSet
{
    private readonly Dictionary<string, string[]> _sounds =
        new(StringComparer.OrdinalIgnoreCase);

    public void Add(
        string soundId,
        params string[] paths)
    {
        _sounds[soundId] = paths;
    }

    public bool TryGet(
        string soundId,
        out string[] paths)
    {
        return _sounds.TryGetValue(
            soundId,
            out paths!);
    }
}