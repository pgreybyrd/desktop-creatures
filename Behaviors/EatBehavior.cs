using Desktop_Creatures.Config;
using Desktop_Creatures.Needs;
using Desktop_Creatures.Utilities;
using Desktop_Creatures.World;
using Point = System.Windows.Point;

namespace Desktop_Creatures.Behaviors;

internal class EatBehavior : IBehavior
{
    private readonly NeedManager _needs;
    private readonly EatSettings _settings;
    private readonly PointOfInterestManager _poiManager;
    private readonly Func<Point> _getPosition;
    private readonly Func<bool> _canSearch;
    private readonly Func<WorldInteractionTarget, bool> _trySetFoodTarget;

    private int _foodSearchCooldownTicks;

    public EatBehavior(
        NeedManager needs,
        EatSettings settings,
        PointOfInterestManager poiManager,
        Func<Point> getPosition,
        Func<bool> canSearch,
        Func<WorldInteractionTarget, bool> trySetFoodTarget)
    {
        _needs = needs;
        _settings = settings;
        _poiManager = poiManager;
        _getPosition = getPosition;
        _canSearch = canSearch;
        _trySetFoodTarget = trySetFoodTarget;
    }

    public void Update()
    {
        if (_foodSearchCooldownTicks > 0)
            _foodSearchCooldownTicks--;

        if (!_needs.IsHungry)
            return;

        if (!_canSearch() || _foodSearchCooldownTicks > 0)
            return;

        var foodTarget = _poiManager.FindNearestWorldInteractionPoint(
            _getPosition(),
            WorldInteractionPointType.Eat,
            PointOfInterestType.Food);

        Logger.LogDebug(
            DebugCategory.Behavior,
            foodTarget is null
                ? "No Eat interaction target found."
                : $"Eat target found at " +
                  $"({foodTarget.Position.X:F1}, {foodTarget.Position.Y:F1})");

        if (foodTarget is null)
        {
            _foodSearchCooldownTicks =
                _settings.FoodSearchCooldownTicks;

            return;
        }

        bool accepted = _trySetFoodTarget(foodTarget);

        Logger.LogDebug(
            DebugCategory.Behavior,
            accepted
                ? "Food target accepted by creature."
                : "Food target rejected by creature.");

        if (!accepted)
        {
            _foodSearchCooldownTicks =
                _settings.FoodSearchCooldownTicks;
        }
    }
}
