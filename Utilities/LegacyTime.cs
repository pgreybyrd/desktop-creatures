namespace Desktop_Creatures.Utilities;

public static class LegacyTime
{
    public const double TickSeconds = 0.016;

    public static double ToSeconds(
        int ticks)
    {
        return ticks * TickSeconds;
    }
}