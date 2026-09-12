namespace Desktop_Creatures.Creatures;

public sealed record CreatureDefinition
{
    public int SchemaVersion { get; init; }

    public required string Id { get; init; }

    public required string Category { get; init; }

    public required CreatureVisualsDefinition Visuals { get; init; }

    public CreatureAppearanceSettingsDefinition? Appearance { get; init; }

    public Dictionary<string, string> Sounds { get; init; } =
        new(StringComparer.OrdinalIgnoreCase);

    public required CreatureMovementDefinition Movement { get; init; }

    public CreatureNeedsDefinition? Needs { get; init; }

    public CreatureInteractionsDefinition? Interactions { get; init; }

    public CreatureBehaviorsDefinition? Behaviors { get; init; }

    public string AssetFolder =>
        $"Assets/Creatures/{ToFolderName(Category)}/{ToFolderName(Id)}";

    private static string ToFolderName(
        string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return value;

        return
            char.ToUpperInvariant(value[0]) +
            value[1..];
    }
}

public sealed record CreatureVisualsDefinition
{
    public bool SpriteFacesRight { get; init; } = true;

    public required int SpriteWidth { get; init; }

    public required int SpriteHeight { get; init; }

    public required CreatureScaleDefinition Scale { get; init; }

    public double FootOffsetY { get; init; }

    public CreaturePointDefinition? PickupAnchor { get; init; }

    public Dictionary<string, CreaturePointDefinition> FootAnchors { get; init; } =
        new(StringComparer.OrdinalIgnoreCase);
}

public sealed record CreatureScaleDefinition
{
    public required double Default { get; init; }

    public required double Min { get; init; }

    public required double Max { get; init; }
}

public sealed record CreaturePointDefinition
{
    public double X { get; init; }

    public double Y { get; init; }
}

public sealed record CreatureAppearanceSettingsDefinition
{
    public bool Generated { get; init; }

    public List<CreatureCosmeticDefinition> Palettes { get; init; } = [];

    public List<CreatureCosmeticDefinition> Patterns { get; init; } = [];

    public List<CreatureCosmeticDefinition> Accessories { get; init; } = [];

    public List<CreatureCosmeticDefinition> Effects { get; init; } = [];
}

public sealed record CreatureCosmeticDefinition
{
    public required string Id { get; init; }

    public string Availability { get; init; } =
        "base";

    public string? SeasonId { get; init; }

    public string? EntitlementId { get; init; }
}

public sealed record CreatureMovementDefinition
{
    public GroundMovementDefinition? Ground { get; init; }

    public FlightDefinition? Flight { get; init; }

    public HopDefinition? Hop { get; init; }

    public ClimbDefinition? Climb { get; init; }

    public SwimDefinition? Swim { get; init; }
}

public sealed record GroundMovementDefinition
{
    public LocomotionDefinition? Walk { get; init; }

    public LocomotionDefinition? Run { get; init; }

    public LocomotionDefinition? Stalk { get; init; }

    public FallDefinition? Fall { get; init; }
}

public sealed record LocomotionDefinition
{
    public bool Enabled { get; init; } = true;

    public string? Animation { get; init; }

    public double Speed { get; init; }

    public double MinDurationSeconds { get; init; }

    public double MaxDurationSeconds { get; init; }

    public double ArrivalDistance { get; init; } = 5;

    public Dictionary<string, double> FrameMovement { get; init; } =
        new(StringComparer.OrdinalIgnoreCase);
}

public sealed record FlightDefinition
{
    public bool Enabled { get; init; } = true;

    public string? Animation { get; init; }

    public double Speed { get; init; }

    public double MinDurationSeconds { get; init; }

    public double MaxDurationSeconds { get; init; }

    public double ArrivalDistance { get; init; } = 5;

    public double? MinTravelDistance { get; init; }

    public double? MaxTravelDistance { get; init; }

    public Dictionary<string, double> FrameMovement { get; init; } =
        new(StringComparer.OrdinalIgnoreCase);

    public HoverDefinition? Hover { get; init; }

    public GlideDefinition? Glide { get; init; }
}

public sealed record HoverDefinition
{
    public bool Enabled { get; init; } = true;

    public string? Animation { get; init; }

    public double Chance { get; init; }

    public double MinDurationSeconds { get; init; }

    public double MaxDurationSeconds { get; init; }

    public double FlipChance { get; init; }

    public double? MinFlipSeconds { get; init; }

    public double? MaxFlipSeconds { get; init; }

    public Dictionary<string, double> FrameMovement { get; init; } =
        new(StringComparer.OrdinalIgnoreCase);
}

public sealed record GlideDefinition
{
    public bool Enabled { get; init; } = true;

    public string? Animation { get; init; }

    public double Speed { get; init; }

    public double Chance { get; init; }

    public double MinDurationSeconds { get; init; }

    public double MaxDurationSeconds { get; init; }

    public double ArrivalDistance { get; init; } = 5;

    public double MinDownwardGlideDy { get; init; }

    public Dictionary<string, double> FrameMovement { get; init; } =
        new(StringComparer.OrdinalIgnoreCase);
}

public sealed record FallDefinition
{
    public bool Enabled { get; init; } = true;

    public string? Animation { get; init; }

    public double Gravity { get; init; }

    public double MaxFallSpeed { get; init; }

    public double LandingTolerance { get; init; } = 5;
}

public sealed record HopDefinition
{
    public bool Enabled { get; init; } = true;

    public string? Animation { get; init; }

    public double? MinDurationSeconds { get; init; }

    public double? MaxDurationSeconds { get; init; }

    public double HorizontalSpeed { get; init; }

    public double JumpVelocity { get; init; }

    public double Gravity { get; init; }

    public double? MinDistance { get; init; }

    public double? MaxDistance { get; init; }

    public Dictionary<string, double> FrameMovement { get; init; } =
        new(StringComparer.OrdinalIgnoreCase);
}

public sealed record ClimbDefinition
{
    public bool Enabled { get; init; } = true;

    public string? Animation { get; init; }

    public double? MinDurationSeconds { get; init; }

    public double? MaxDurationSeconds { get; init; }

    public double Speed { get; init; }

    public double ArrivalDistance { get; init; } = 5;

    public List<string> SurfaceTypes { get; init; } = [];

    public Dictionary<string, double> FrameMovement { get; init; } =
        new(StringComparer.OrdinalIgnoreCase);
}

public sealed record SwimDefinition
{
    public bool Enabled { get; init; } = true;

    public string? Animation { get; init; }

    public double? MinDurationSeconds { get; init; }

    public double? MaxDurationSeconds { get; init; }

    public double Speed { get; init; }

    public double ArrivalDistance { get; init; } = 5;

    public double Acceleration { get; init; }

    public double Buoyancy { get; init; }

    public Dictionary<string, double> FrameMovement { get; init; } =
        new(StringComparer.OrdinalIgnoreCase);
}

public sealed record CreatureNeedsDefinition
{
    public NeedDefinition? Hunger { get; init; }

    public NeedDefinition? Energy { get; init; }

    public NeedDefinition? Curiosity { get; init; }

    public NeedDefinition? Thirst { get; init; }
}

public sealed record NeedDefinition
{
    public double Initial { get; init; }

    public double Threshold { get; init; }

    public double RatePerSecond { get; init; }

    public string Direction { get; init; } =
        "increase";
}

public sealed record CreatureInteractionsDefinition
{
    public InteractionDefinition? Eat { get; init; }

    public InteractionDefinition? Drink { get; init; }
}

public sealed record InteractionDefinition
{
    public bool Enabled { get; init; } = true;

    public string? Animation { get; init; }

    public required string PointType { get; init; }

    public double DurationSeconds { get; init; }

    public double SearchCooldownSeconds { get; init; }

    public double LeaveDistance { get; init; }

    public double Reach { get; init; }

    public string? SatisfiesNeed { get; init; }
}

public sealed record CreatureBehaviorsDefinition
{
    public IdleDefinition? Idle { get; init; }

    public PerchDefinition? Perch { get; init; }

    public RestDefinition? Sleep { get; init; }

    public RestDefinition? Nesting { get; init; }
}

public sealed record IdleDefinition
{
    public bool Enabled { get; init; } = true;

    public double Chance { get; init; }

    public double MinDurationSeconds { get; init; }

    public double MaxDurationSeconds { get; init; }
}

public sealed record PerchDefinition
{
    public bool Enabled { get; init; } = true;

    public string? Animation { get; init; }

    public double Chance { get; init; }

    public double MinDurationSeconds { get; init; }

    public double MaxDurationSeconds { get; init; }

    public double RuffleChance { get; init; }

    public string? PointType { get; init; }
}

public sealed record RestDefinition
{
    public bool Enabled { get; init; } = true;

    public string? Animation { get; init; }

    public double MinDurationSeconds { get; init; }

    public double MaxDurationSeconds { get; init; }

    public string? PointType { get; init; }
}