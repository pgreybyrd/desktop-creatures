namespace Desktop_Creatures.UI.RightClick
{
    internal static class CreatureContextMenuBuilder
    {
        public static IReadOnlyList<
            CreatureContextMenuItem> Build(
            Action pet,
            Action favorite,
            Action rename,
            Action goHome,
            Action fieldGuide,
            Action appearance,
            Action breeding,
            Action putAway)
        {
            return
            [
                new(
                    CreatureContextMenuAction.Pet,
                    "pet",
                    pet),

                new(
                    CreatureContextMenuAction.Favorite,
                    "favorite",
                    favorite),

                new(
                    CreatureContextMenuAction.Rename,
                    "rename",
                    rename),

                new(
                    CreatureContextMenuAction.GoHome,
                    "gohome",
                    goHome),

                new(
                    CreatureContextMenuAction.FieldGuide,
                    "fieldguide",
                    fieldGuide),

                new(
                    CreatureContextMenuAction.Appearance,
                    "appearance",
                    appearance),

                new(
                    CreatureContextMenuAction.Breeding,
                    "breeding",
                    breeding),

                new(
                    CreatureContextMenuAction.PutAway,
                    "putaway",
                    putAway,
                    DividerBefore: true)
            ];
        }
    }
}