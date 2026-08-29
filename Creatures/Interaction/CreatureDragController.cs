namespace Desktop_Creatures.Creatures.Interaction;

public sealed class CreatureDragController
{
    public bool IsDragging { get; private set; }

    public void Begin()
    {
        IsDragging = true;
    }

    public void End()
    {
        IsDragging = false;
    }
}