namespace Desktop_Creatures.UI.RightClick
{
    public sealed record CreatureContextMenuItem(
        CreatureContextMenuAction Action,
        string AssetName,
        Action Execute,
        bool DividerBefore = false);
}