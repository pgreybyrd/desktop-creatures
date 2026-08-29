using System.Windows;

namespace Desktop_Creatures.UI.RightClick;

public sealed class CreatureContextMenuController
{
    private CreatureContextMenuWindow? _window;

    public bool IsOpen =>
        _window is not null;

    public void SetWindow(
        CreatureContextMenuWindow window)
    {
        _window = window;
    }

    public void ClearWindow(
        CreatureContextMenuWindow window)
    {
        if (ReferenceEquals(
                _window,
                window))
        {
            _window = null;
        }
    }

    public void Close()
    {
        _window?.CloseMenu();
    }
}