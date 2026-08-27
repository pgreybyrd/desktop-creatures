namespace Desktop_Creatures.Creatures;

public sealed class CreatureManager
{
    private readonly Dictionary<Guid, Creature> _activeCreatures = new();

    public IReadOnlyCollection<Creature> ActiveCreatures =>
        _activeCreatures.Values;

    public bool IsActive(Guid creatureId)
    {
        return _activeCreatures.ContainsKey(creatureId);
    }

    public void Add(Creature creature)
    {
        _activeCreatures[creature.Id] = creature;
    }

    public bool Remove(Guid creatureId)
    {
        return _activeCreatures.Remove(creatureId);
    }
}