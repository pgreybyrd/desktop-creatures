namespace Desktop_Creatures.Tools.Fonts;

public sealed class FontDefinition
{
    public required string Name { get; init; }

    public required string[] CharacterRows { get; init; }

    public int Baseline { get; init; }

    public int SpaceAdvance { get; init; } = 3;

    public int GlyphSpacing { get; init; } = 1;

    public Dictionary<char, int> BaselineAdjustments
    { get; init; } = new();
}