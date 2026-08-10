

namespace Desktop_Creatures.UI.FieldGuide
{
    public sealed class FieldGuideEntry
    {
        public required string Id { get; init; }
        public required string Name { get; init; }
        public required string Description { get; init; }

        public required string Habitat { get; init; }
        public required string Activity { get; init; }
        public required string Diet { get; init; }

        public List<string> FieldNotes { get; init; } = [];

        public string? PortraitFrame { get; init; }
        public string? SpawnIcon { get; init; }

        public string FactsText =>
            $"Habitat: {Habitat}\n" +
            $"Active: {Activity}\n" +
            $"Diet: {Diet}";

        public string FieldNotesText =>
            "FIELD NOTES\n" +
            string.Join("\n", FieldNotes);
    }
}
