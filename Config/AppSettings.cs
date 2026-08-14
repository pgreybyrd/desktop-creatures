namespace Desktop_Creatures.Config;

public class AppSettings
{
    public int WorkingMonitor { get; set; } = 0;
    public bool MenusAlwaysOnTop { get; set; } = true;
    public bool EcosystemAlwaysOnTop { get; set; } = true;
    public bool ClickThrough { get; set; } = true;
    public bool EditMode { get; set; } = false;
    public int SpawnLimit { get; set; } = 20;
    public int Scale { get; set; } = 1;
    public int CreatureDisplayScale { get; set; } = 1;
}

