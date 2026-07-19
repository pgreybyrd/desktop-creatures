
using Desktop_Creatures.Utilities;

namespace Desktop_Creatures.Behaviors;

public class BehaviorController
{
    private readonly List<IBehavior> _behaviors = new();

    public IBehavior? CurrentBehavior { get; private set; }

    public void AddBehavior(IBehavior behavior)
    {
        Logger.LogDebug(DebugCategory.Behavior, $"Adding behavior: {behavior.GetType().Name}");

        _behaviors.Add(behavior);
        CurrentBehavior ??= behavior;
    }

    public void SetBehavior(IBehavior behavior)
    {
        Logger.LogDebug(DebugCategory.Behavior, $"Setting current behavior: {behavior.GetType().Name}");
        CurrentBehavior = behavior;

        if (!_behaviors.Contains(behavior))
            _behaviors.Add(behavior);
    }

    public void Update()
    {
        foreach (var behavior in _behaviors)
            behavior.Update();
    }
}
