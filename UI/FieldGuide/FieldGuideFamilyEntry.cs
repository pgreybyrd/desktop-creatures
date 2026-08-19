namespace Desktop_Creatures.UI.FieldGuide
{
    public sealed class FieldGuideFamilyEntry
    {
        public required string Id { get; init; }

        public required string Name { get; init; }

        public required string ToolTipAsset { get; init; }

        public required FieldGuideTab Tab { get; init; }

        public List<string> CreatureIds { get; init; } = [];

        public int RightY { get; init; }

        public int Order { get; init; }
    }
}