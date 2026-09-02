# Adding a New Creature

Use this checklist whenever adding a new creature to Desktop Creatures.

The goal is to make creature additions predictable, data-driven, and easy to verify without rediscovering every required file or export setting.

## 1. Creature Artwork

Create the creature sprite sheet in its category folder.

Example:

```text
Assets/Creatures/Rodents/Squirrel/Appearance/
```

For a normal precolored creature, include:

```text
squirrel.png
squirrel.json
```

For a generated/recolorable creature, also include:

```text
squirrel-regions.png
squirrel-regions.json
Palettes/
```

Optional source/reference art such as grayscale sheets can stay in the folder if useful, but runtime files must use the expected names.

### Required Aseprite export settings

Use:

```text
JSON Data: Array
Array Data: Tags
```

For sheets that combine layers + states, use:

```text
Split Layers: ON
```

Frame names should be meaningful and stable.

For creature animation sheets, expected tags currently include:

```text
run
idle
dangle
fall
```

Additional tags are fine, such as:

```text
jump
sleep
eat
```

### Animation naming rules

Ground creatures currently expect:

- `run` for normal movement
- either `idle` or one or more `idle_*` animations
- `dangle` for being held
- `fall` for release/falling

The animation dictionary is case-insensitive, so `run` works with code requesting `Run`.

## 2. Creature Definition

Add:

```text
Assets/Data/Creatures/Definitions/<creature-id>.json
```

Example:

```text
Assets/Data/Creatures/Definitions/squirrel.json
```

Include at least:

```json
{
  "id": "squirrel",
  "category": "rodents",
  "movementCapabilities": [
    "Ground"
  ],
  "appearance": {
    "generated": false
  },
  "sounds": {
    "spawn": "chatter",
    "pickup": "chatter"
  },
  "pickupAnchor": {
    "x": 31,
    "y": 16
  },
  "scale": {
    "default": 1,
    "min": 1,
    "max": 4
  }
}
```

Check:

- `id` matches filenames and factory registration
- `category` matches a Field Guide category
- `appearance.generated` matches the actual asset pipeline
- pickup anchor looks correct while dragging
- scale values make sense for the creature

## 3. Creature Settings

Add the creature to:

```text
Assets/Data/Creatures/creature_settings.json
```

Ground creatures generally need:

```text
spriteFacesRight
spriteWidth
spriteHeight
scale
footOffsetY
run
idle
fall
```

Match frame counts to the actual exported animation tags.

## 4. Creature Class

Create or update:

```text
Creatures/<CreatureName>.cs
```

Use the existing creature classes as references.

Typical ground-creature initialization order:

```csharp
InitializeCreatureAssets(...);
SetSoundSet(...);
InitializeGroundCreature(x, y);
PlaySound(CreatureSoundEvent.Spawn);
```

If the creature supports pickup:

```csharp
public override void OnPickedUp()
{
    PlaySound(
        CreatureSoundEvent.Pickup);

    SetAction(
        CreatureAction.Held,
        "dangle");
}
```

Do not add creature-specific hacks to `Creature.cs` unless the behavior should apply generically to future creatures too.

## 5. Factory Registration

Add the creature to:

```text
CreatureFactory.cs
```

Example:

```csharp
"squirrel" => new Squirrel(...),
```

Verify the creature ID exactly matches the definition ID.

## 6. MainWindow Definition Loading

Make sure the creature definition is loaded in `MainWindow.LoadSettings()`.

Example:

```csharp
_creatureDefinitions["squirrel"] =
    CreatureDefinitionLoader.Load("squirrel");
```

If this is later made fully data-driven, remove this checklist step.

## 7. Field Guide Entry

Add:

```text
Assets/Data/FieldGuide/Entries/<creature-id>.json
```

Example:

```text
Assets/Data/FieldGuide/Entries/squirrel.json
```

Current entry structure:

```json
{
  "id": "squirrel",
  "name": "Squirrel",
  "description": "Write creature description here.",
  "habitat": "Urban",
  "activity": "Daytime",
  "diet": "Vegetarian",
  "fieldNotes": [
    "Field note one.",
    "Field note two.",
    "Field note three."
  ]
}
```

Keep the text in the tone of the Field Guide rather than generic encyclopedia copy.

## 8. Field Guide Category Registration

Update:

```text
Assets/Data/FieldGuide/fieldGuide.json
```

Add the creature ID to the correct category in the intended display order.

Example:

```json
{
  "id": "rodents",
  "tab": "Crimson",
  "order": 0,
  "rightX": 0,
  "rightY": 40,
  "creatures": [
    "rat",
    "squirrel"
  ]
}
```

The first creature becomes the default page when opening that category.

## 9. Field Guide Portrait

Add:

```text
Assets/UI/FieldGuide/Creatures/<CreatureName>/Portrait/portrait-<creature-id>.png
```

Example:

```text
Assets/UI/FieldGuide/Creatures/Squirrel/Portrait/portrait-squirrel.png
```

Portrait art is creature-specific.

Portrait frames come from:

```text
Assets/UI/FieldGuide/Common/frames.png
Assets/UI/FieldGuide/Common/frames.json
```

Current frame:

```text
basic
```

Future frame options can include things such as:

```text
vines
feathers
aquatic
```

Keep `PortraitFrame` available in `FieldGuideEntry` so creatures can choose different decorative frames later.

## 10. Field Guide Tooltip Label

Add the creature label to:

```text
Assets/UI/FieldGuide/Common/tooltext.png
Assets/UI/FieldGuide/Common/tooltext.json
```

The frame name must exactly match the creature ID.

Example:

```text
squirrel
```

This label is used for the small creature sub-tab tooltip, so long creature names do not need to fit on the actual tab.

## 11. Sounds

If the creature has sounds, add them under its asset folder.

Example:

```text
Assets/Creatures/Rodents/Squirrel/Sounds/
```

Then register them in the creature class using `SoundSet`.

Test:

- spawn sound
- pickup sound

## 12. Full Lifecycle Test

Before calling the creature complete, test all of these:

- Field Guide opens without crashing
- category opens correctly
- creature sub-tab appears
- tooltip appears
- hover state works
- pressed state works
- clicking the tab opens the correct entry
- portrait loads
- Spawn button works
- creature appears
- creature runs/moves
- creature idles
- creature can be picked up
- held/dangle animation works
- creature can be dropped
- fall animation works
- creature lands
- creature resumes normal behavior
- creature respects surfaces correctly
- creature works across intended monitors
- spawn and pickup sounds work
- no missing-frame or missing-settings exceptions

## 13. Commit

Once the creature survives the full lifecycle test, commit before beginning the next creature or cleanup pass.

Suggested commit style:

```text
feat: add squirrel creature and field guide entry
```

## Current Field Guide Common Assets

```text
Assets/UI/FieldGuide/Common/
    tabs.png
    tabs.json
    subTabs.png
    subTabs.json
    tooltext.png
    tooltext.json
    buttons.png
    buttons.json
    frames.png
    frames.json
```

Current frame-name conventions:

```text
tabs:
<color>_normal
<color>_hover
<color>_pressed

subTabs:
current
other_normal
other_hover
other_pressed

buttons:
spawn_normal
spawn_hover
spawn_pressed

frames:
basic
```

Current Field Guide tab colors:

```text
Crimson
Moss
Cobalt
Marigold
Amethyst
Silver
Aqua
Tangerine
Azure
Emerald
Raspberry
Charcoal
```

## Future Cleanup Candidates

These are not required to add the next creature, but would reduce setup work later:

- organize creature classes into category folders such as `Creatures/Rodents/`, `Creatures/Felines/`, etc.
- make creature definition discovery fully data-driven so `MainWindow` does not need one line per creature
- make factory registration more data-driven if practical
- centralize creature asset paths to reduce duplicate filenames in different folders
- add validation that catches missing settings, animations, portraits, and Field Guide registration before runtime
- consider a development-only creature content validator that reports every incomplete creature in one pass instead of discovering issues one crash at a time
