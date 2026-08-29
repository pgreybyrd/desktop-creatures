using System.Windows;
using System.Windows.Media;

using DrawingPoint = System.Drawing.Point;
using DrawingRectangle = System.Drawing.Rectangle;
using FormsControl = System.Windows.Forms.Control;
using FormsScreen = System.Windows.Forms.Screen;
using Point = System.Windows.Point;

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

    public void Position(
        CreatureContextMenuWindow menu,
        Visual relativeTo)
    {
        PresentationSource? source =
            PresentationSource.FromVisual(
                relativeTo);

        if (source?.CompositionTarget is null)
            return;

        DrawingPoint mousePixels =
            FormsControl.MousePosition;

        FormsScreen screen =
            FormsScreen.FromPoint(
                mousePixels);

        DrawingRectangle workPixels =
            screen.WorkingArea;

        Matrix fromDevice =
            source.CompositionTarget
                .TransformFromDevice;

        Point mouseDip =
            fromDevice.Transform(
                new Point(
                    mousePixels.X,
                    mousePixels.Y));

        Point workTopLeft =
            fromDevice.Transform(
                new Point(
                    workPixels.Left,
                    workPixels.Top));

        Point workBottomRight =
            fromDevice.Transform(
                new Point(
                    workPixels.Right,
                    workPixels.Bottom));

        double left =
            mouseDip.X;

        double top =
            mouseDip.Y;

        if (left + menu.Width >
            workBottomRight.X)
        {
            left =
                mouseDip.X -
                menu.Width;
        }

        if (top + menu.Height >
            workBottomRight.Y)
        {
            top =
                mouseDip.Y -
                menu.Height;
        }

        left =
            Math.Clamp(
                left,
                workTopLeft.X,
                workBottomRight.X -
                    menu.Width);

        top =
            Math.Clamp(
                top,
                workTopLeft.Y,
                workBottomRight.Y -
                    menu.Height);

        menu.Left = left;
        menu.Top = top;
    }

    public CreatureContextMenuWindow Open(
        IReadOnlyList<CreatureContextMenuItem> items,
        int uiScale,
        Visual relativeTo)
    {
        Close();

        CreatureContextMenuWindow menu =
            new CreatureContextMenuWindow(
                items,
                uiScale);

        menu.Opacity = 0;

        SetWindow(
            menu);

        menu.ReadyToPosition +=
            menu =>
            {
                Position(
                    menu,
                    relativeTo);

                menu.Opacity = 1;
            };

        menu.Closed +=
            (_, _) =>
            {
                ClearWindow(
                    menu);
            };

        menu.Show();

        return menu;
    }

    public void Close()
    {
        _window?.CloseMenu();
    }
}