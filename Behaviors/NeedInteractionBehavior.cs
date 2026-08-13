using Desktop_Creatures.Needs;
using Desktop_Creatures.Utilities;
using Desktop_Creatures.World;
using Point = System.Windows.Point;

namespace Desktop_Creatures.Behaviors;

internal class NeedInteractionBehavior : IBehavior
{
    private readonly NeedManager _needs;
    private readonly NeedType _needType;

    private readonly PointOfInterestManager _poiManager;
    private readonly PointOfInterestType _poiType;
    private readonly WorldInteractionPointType _interactionType;

    private readonly Func<Point> _getPosition;
    private readonly Func<bool> _canSearch;
    private readonly Func<WorldInteractionTarget, bool> _trySetTarget;

    private readonly int _searchCooldownTicks;

    private int _cooldownTicks;

    public NeedInteractionBehavior(
        NeedManager needs,
        NeedType needType,
        PointOfInterestManager poiManager,
        PointOfInterestType poiType,
        WorldInteractionPointType interactionType,
        Func<Point> getPosition,
        Func<bool> canSearch,
        Func<WorldInteractionTarget, bool> trySetTarget,
        int searchCooldownTicks)
    {
        _needs = needs;
        _needType = needType;

        _poiManager = poiManager;
        _poiType = poiType;
        _interactionType = interactionType;

        _getPosition = getPosition;
        _canSearch = canSearch;
        _trySetTarget = trySetTarget;

        _searchCooldownTicks =
            searchCooldownTicks;
    }

    public void Update()
    {
        if (_cooldownTicks > 0)
            _cooldownTicks--;

        if (!_needs.IsActive(_needType))
            return;

        if (!_canSearch() ||
            _cooldownTicks > 0)
        {
            return;
        }

        var target =
            _poiManager.FindNearestWorldInteractionPoint(
                _getPosition(),
                _interactionType,
                _poiType);

        Logger.LogDebug(
            DebugCategory.Behavior,
            target is null
                ? $"No {_interactionType} interaction target found."
                : $"{_interactionType} target found at " +
                  $"({target.Position.X:F1}, {target.Position.Y:F1})");

        if (target is null)
        {
            StartCooldown();
            return;
        }

        bool accepted =
            _trySetTarget(target);

        Logger.LogDebug(
            DebugCategory.Behavior,
            accepted
                ? $"{_interactionType} target accepted by creature."
                : $"{_interactionType} target rejected by creature.");

        if (!accepted)
            StartCooldown();
    }

    private void StartCooldown()
    {
        _cooldownTicks =
            _searchCooldownTicks;
    }
}