# Desktop Creatures Content Schema

## Creature Definition

Path:

`Assets/Data/Creatures/Definitions/{id}.json`

Defines the creature's core identity, capabilities, appearance system, sounds,
and other data required by the game to create and operate the creature.

### Required

- `id` — Permanent machine-readable creature identifier.
  Use lowercase names.
  Example: `rat`.

- `category` — References a Field Guide category `id`.
  This determines where the creature appears in the Field Guide and is used
  to derive the creature's asset directory.
  Example: `rodents`.

- `movementCapabilities` — Movement types the creature is capable of using.
  A creature may have more than one.
  Example: `["Ground"]`.

### Optional

- `displayName` — Player-facing display name override.
  When omitted, the display name is derived from `id`.
  Example: `great-white-shark` could use `Great White Shark`.
  Keeping display names separate from permanent IDs also allows presentation
  and localization to change without changing creature identity.

- `appearance` — Configuration for the creature's appearance system.
  Omitted when the creature does not require configurable/generated appearance.

- `sounds` — Maps creature events to named sound sets.
  Omitted when the creature has no creature-specific sounds.

### Derived

- Display name:
  Derived from `id` when `displayName` is not provided.

- Asset directory:
  `Assets/Creatures/{Category}/{Creature}/`

  Example:
  `category: rodents` + `id: rat`
  → `Assets/Creatures/Rodents/Rat/`

- Field Guide membership:
  Derived from `category`.

### Rules

- Creature definitions must not contain absolute or repeated asset paths when
  those paths can be derived from `id`, `category`, or established asset
  conventions.

- `id` is permanent identity. Renaming player-facing text must not require
  changing the creature's `id`.

- `category` represents the game's organizational category, not strict
  biological taxonomy.

- Movement does not determine category.
  A flying creature is not necessarily a Bird, and an aquatic creature is not
  necessarily an Aquatic category creature.

- Creature-specific classes should not hardcode asset directories.
  Asset locations are derived from the creature definition and shared
  asset-path conventions.

### Appearance

The optional `appearance` object defines whether the creature uses the generated appearance system.

Example:

```json
"appearance": {
  "generated": true
}
```

**Fields**

- `generated` — Whether the creature uses region-based generated appearances.

**Derived**

Appearance assets and available appearance components are discovered from the creature's derived asset directory.

For a creature with `category: rodents` and `id: rat`:

`Assets/Creatures/Rodents/Rat/Appearance/`

Expected structure:

```text
Appearance/
├── rat.png
├── rat.json
├── rat-regions.png
├── rat-regions.json
├── Palettes/
├── Patterns/
├── Accessories/
├── Effects/
└── Appearances/
```

- `{id}.png` — Base appearance sprite sheet.
- `{id}.json` — Sprite atlas and animation metadata.
- `{id}-regions.png` — Region mask used for recoloring.
- `{id}-regions.json` — Region definitions.
- `Palettes/` — Available palette definitions.
- `Patterns/` — Available pattern definitions.
- `Accessories/` — Available accessory definitions.
- `Effects/` — Available effect definitions.
- `Appearances/` — Named or predefined appearance definitions.

Appearance component IDs are derived from their filenames.

For example:

`Palettes/silver.json` → palette ID `silver`

`Appearances/gerald.json` → appearance ID `gerald`

**Rules**

- Creature Definitions do not enumerate individual palettes, patterns, accessories, effects, or named appearances.
- Files present in the appearance component directories define the components available to that creature.
- Adding a new appearance component should normally require adding its asset file, not modifying the Creature Definition.
- Creature-specific classes must not contain appearance asset paths.
- Appearance asset locations are derived from the creature's `id`, `category`, and established asset conventions.

### Pickup Anchor

The required `pickupAnchor` object defines the point on the creature sprite used when the player picks up and drags the creature.

Example:

```json
"pickupAnchor": {
  "x": 33,
  "y": 11
}
```

**Fields**

- `x` — Horizontal pickup anchor position within the creature sprite.
- `y` — Vertical pickup anchor position within the creature sprite.

**Rules**

- Pickup anchor values belong to the Creature Definition because they describe creature-specific interaction behavior.
- Pickup anchors must not be derived from asset paths or Field Guide data.
- The `x` and `y` values are stored together as one `pickupAnchor` object because they represent a single coordinate.

### Scale

The optional `scale` object defines the allowed display scale range for a creature.

Example:

```json
"scale": {
  "default": 2,
  "min": 2,
  "max": 3
}
```

**Fields**

- `default` — Default display scale used when the creature is spawned without an explicit scale.
- `min` — Smallest display scale allowed for the creature.
- `max` — Largest display scale allowed for the creature.

**Rules**

- Scale limits belong to the Creature Definition because they describe creature-specific presentation constraints.
- Scale values are independent from the physical pixel dimensions of the source artwork.
- `default` must be greater than or equal to `min` and less than or equal to `max`.
- Creatures may use different scale ranges to preserve appropriate relative visual size.
- Scale constraints must not be inferred from creature category.

### Sounds

The optional `sounds` object maps creature events to named sound sets.

Example:

```json
"sounds": {
  "spawn": "squeaks",
  "pickup": "squeaks"
}
```

**Fields**

Each field represents a creature sound event.

The field value is the ID of a sound set defined in the creature's `Sounds/sounds.json`.

Common sound events may include:

- `spawn` — Played when the creature is spawned.
- `pickup` — Played when the player picks up the creature.

Additional sound events may be added as creature behavior requires them.

**Derived**

The creature's sound directory is derived from its category and ID.

For a creature with `category: rodents` and `id: rat`:

`Assets/Creatures/Rodents/Rat/Sounds/`

Expected structure:

```text
Sounds/
├── sounds.json
├── squeak_01.wav
├── squeak_02.wav
└── ...
```

The sound manifest is always:

`Sounds/sounds.json`

**Rules**

- Creature Definitions map game events to sound set IDs, not individual audio files.
- Creature-specific classes must not contain sound asset paths.
- Multiple events may reference the same sound set.
- Creatures without creature-specific sounds may omit the `sounds` object.
- Adding or removing files from a sound set should require changing only the creature's sound manifest, not its Creature Definition.

## Canonical Creature Definition — Rat

Path:

`Assets/Data/Creatures/Definitions/rat.json`

```json
{
  "id": "rat",
  "category": "rodents",

  "movementCapabilities": [
    "Ground"
  ],

  "appearance": {
    "generated": true
  },

  "sounds": {
    "spawn": "squeaks",
    "pickup": "squeaks"
  },

  "pickupAnchor": {
    "x": 33,
    "y": 11
  },

  "scale": {
    "default": 2,
    "min": 2,
    "max": 3
  }
}
```

This example represents the intended structure for a creature using generated appearances, creature-specific sounds, pickup interaction, and constrained display scaling.

Fields should only be added when they describe data genuinely owned by the creature definition rather than information that can be derived from established conventions or owned by another system.

## Creature Sound Manifest

Path:

`Assets/Creatures/{Category}/{Creature}/Sounds/sounds.json`

Defines the named sound sets available to a creature.

Example:

```json
{
  "sets": {
    "squeaks": [
      "squeak_01.wav",
      "squeak_02.wav",
      "squeak_03.wav"
    ]
  }
}
```

### Required

- `sets` — Collection of named sound sets available to the creature.

Each sound set contains one or more audio filenames located in the same `Sounds/` directory as the manifest.

### Derived

Sound set IDs are defined by their keys.

Example:

`squeaks` → sound set ID `squeaks`

Audio paths are derived from the creature's sound directory and the filenames listed in the set.

Example:

`squeak_01.wav`

for `category: rodents` and `id: rat` resolves to:

`Assets/Creatures/Rodents/Rat/Sounds/squeak_01.wav`

### Rules

- Sound manifests contain filenames, not full asset paths.
- Sound set IDs must be unique within a creature.
- A sound set may be referenced by multiple creature events.
- Creature Definitions reference sound set IDs and must not duplicate the filenames contained in those sets.
- Creature-specific classes must not enumerate individual sound files.

### Field Guide Category

**Required**
- `id` — Stable machine-readable category ID. Use lowercase plural names.
  Example: `rodents`.
- `tab` — Tab visual/color identifier.
- `order` — Logical display order.
- `rightX` — Horizontal position of the category tab on right-hand pages.
- `rightY` — Vertical position of the category tab on right-hand pages.

**Optional**
- `name` — Player-facing display name override.
  When omitted, the display name is derived from `id`.
  Example: `rodents` → `Rodents`.
  Keep this separate from `id` so presentation/localization can change
  without changing the permanent category identifier.

**Derived**
- Tooltip asset:
  `Assets/UI/FieldGuide/Common/ToolTip/label-{id}.png`

**Membership**
- Categories do NOT list creature IDs.
- Each creature definition declares its own `category`.
- The Field Guide discovers category membership from creature definitions.

## Field Guide Entry

Path:
Assets/Data/FieldGuide/Entries/{id}.json

**Required**
- `id` — References a creature definition.
- `description`
- `habitat`
- `activity`
- `diet`
- `fieldNotes`

**Optional**
- None yet.

**Derived**
- Creature display name comes from the creature definition.
- Portrait assets are derived from the creature ID by convention.

**Rule**
Field Guide entries must not duplicate creature identity, category membership,
or canonical display-name data already owned by the creature definition.