using System.Windows;

namespace Desktop_Creatures.UI.RightClick;

public sealed class CreatureContextMenuController
{
    private CreatureContextMenuWindow? _window;

    public bool IsOpen =>
        _window is not null;

    public void Close()
    {
        _window?.Close();
    }
}