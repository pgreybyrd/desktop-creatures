namespace Desktop_Creatures.UI.FieldGuide
{
    public sealed class FieldGuideCategoryEntry
    {
        public required string Id { get; init; }

        public FieldGuideTab Tab { get; init; }

        public int Order { get; init; }

        public int RightX { get; init; }

        public int RightY { get; init; }

        public string DisplayName =>
            char.ToUpperInvariant(Id[0]) + Id[1..];

        public string ToolTipAsset =>
            $"Assets/UI/FieldGuide/Common/ToolTip/label-{Id}.png";
    }
}