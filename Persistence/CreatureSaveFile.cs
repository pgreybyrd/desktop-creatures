namespace Desktop_Creatures.Persistence;

public sealed class CreatureSaveFile
{
    public int Version { get; set; } = 2;

    public List<CreatureRecord> Creatures
    {
        get;
        set;
    } = [];
}