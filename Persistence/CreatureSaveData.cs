namespace Desktop_Creatures.Persistence;

public class CreatureSaveData
{
    public Guid Id { get; set; }

    public string CreatureType { get; set; } = "";

    public string Name { get; set; } = "";

    public string Variant { get; set; } = "";

    public double X { get; set; }

    public double Y { get; set; }
}