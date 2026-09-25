using Desktop_Creatures.World;
using Desktop_Creatures.World.Surfaces;
using System.Windows;
using Point = System.Windows.Point;

namespace Desktop_Creatures.Ecosystem.Interaction;

public sealed class PointOfInterestDragController
{
    private readonly SurfaceManager
        _surfaceManager;

    private Point
        _dragOffset;

    public Guid? DraggedPointOfInterestId
    {
        get;
        private set;
    }

    public bool IsDragging =>
        DraggedPointOfInterestId.HasValue;

    public PointOfInterestDragController(
        SurfaceManager surfaceManager)
    {
        _surfaceManager =
            surfaceManager;
    }

    public void Begin(
        PointOfInterest poi,
        Point mouseWorldPosition)
    {
        DraggedPointOfInterestId =
            poi.Id;

        _dragOffset =
            new Point(
                mouseWorldPosition.X -
                    poi.Position.X,
                mouseWorldPosition.Y -
                    poi.Position.Y);
    }

    public void Update(
        PointOfInterest poi,
        Point mouseWorldPosition)
    {
        if (DraggedPointOfInterestId !=
            poi.Id)
        {
            return;
        }

        poi.Position =
            new Point(
                mouseWorldPosition.X -
                    _dragOffset.X,
                mouseWorldPosition.Y -
                    _dragOffset.Y);
    }

    public void End(
        PointOfInterest poi)
    {
        if (DraggedPointOfInterestId !=
            poi.Id)
        {
            return;
        }

        double scale =
            poi.AppSettings.Scale;

        Point? snappedPosition =
            _surfaceManager.SnapPoiToSurface(
                poi.Position,
                poi.Settings.Width * scale,
                poi.Settings.Height * scale,
                maxSnapDistance: 30);

        if (snappedPosition.HasValue)
        {
            poi.Position =
                snappedPosition.Value;
        }

        DraggedPointOfInterestId =
            null;
    }
}