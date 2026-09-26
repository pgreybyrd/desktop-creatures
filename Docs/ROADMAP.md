# Desktop Creatures Roadmap

Current Release:  
**Desktop Rat Alpha v0.1.0**

---

# NOW — Core Game Foundation

These are the immediate priorities. Finish these in order unless a genuinely blocking bug appears.

## 1. Window Layering / Z-Order Architecture

- [x] Separate `EcosystemAlwaysOnTop` and `MenusAlwaysOnTop`
- [x] Add separate Settings toggles for ecosystem and menus
- [x] Add centralized `ZOrderManager`
- [x] Enforce intended layer policy:
  - ecosystem / creatures above menus
  - menus above ordinary desktop windows when enabled
- [ ] Remove remaining scattered / legacy topmost workarounds
- [x] Make POIs follow ecosystem topmost only
- [x] Make Main Menu / Settings / Field Guide follow menu topmost only
- [ ] Stress-test activation, focus changes, minimizing, multiple monitors, and other applications

## 2. Persistence v2 — Creature Records

**Status:** core record/runtime separation is working end-to-end. Gerald can be put away, respawned as the same individual, favorited, saved, and restored.

**Goal:** a creature existing in the player's collection must be separate from whether it is currently spawned on the desktop.

- [x] Create persistent `CreatureRecord` / equivalent model
- [x] Permanent creature ID
- [x] Creature type
- [x] Name
- [x] Appearance traits / appearance ID
- [x] Favorite state
- [x] Saved / owned state
- [x] Last known position
- [x] Separate persistent creature records from runtime `CreatureWindow` instances
- [x] Save all supported creature types, not only rats
- [x] Load persistent creatures safely on startup
- [x] Define clear action semantics: Spawn, Put Away, Save, Favorite, Delete
- [ ] Implement permanent Delete with double confirmation
- [ ] Handle missing / invalid save data gracefully
- [ ] Prepare save format for future friendship, home, breeding, and progression data

## 3. Creature Roster

A central window for managing individual creatures.

- [x] Roster window shell / custom pixel UI
- [x] Live roster updates while creatures are added
- [x] Custom bitmap-font name / species labels
- [x] Dynamic scrollbar thumb that shrinks as roster grows
- [x] Mouse-wheel / arrow navigation
- [x] Draggable roster window
- [x] Roster follows Main Menu when Main Menu is dragged
- [x] Default placement to the right of Main Menu

- [ ] Creature portrait
- [x] Creature name
- [ ] Rename
- [x] Favorite
- [x] Spawn / Put Away
- [x] Saved / persistent status
- [x] Creature type / family
- [ ] Appearance preview
- [x] Support many creatures cleanly
- [ ] Later: sorting / filtering / family groups

## 4. Creature Context Menu

Initial menu exists with individual "put away" support.

- [x] Right-click creature
- [x] Put Away individual creature
- [x] Context-menu artwork designed

### Near-term additions

- [ ] Delete with double-confirmation flow
- [x] Favorite
- [ ] Rename
- [x] Put Away
- [x] Field Guide
- [ ] Appearance submenu

### Later additions

- [x] Dynamic menu assembly from top / middle / bottom / divider assets
- [ ] Pet
- [ ] Go Home
- [ ] Breeding submenu
- [ ] Creature details
- [ ] Other creature-specific actions

---

# NEXT — UI / Asset Architecture

## Window Layout / Utility UI Polish

- [x] Settings defaults to left of Main Menu
- [x] Roster defaults to right of Main Menu
- [x] Field Guide opens centered over Main Menu area
- [ ] Persist utility-window positions after manual dragging
- [ ] Final coordinated Main Menu / Settings / Roster art-layout pass
- [ ] Replace temporary auto-open Roster test with real Main Menu button


## UI Spritesheet Migration

**Goal:** stop maintaining huge piles of individual normal / hover / pressed PNGs.

- [x] Field Guide tabs migrated to spritesheet
- [x] Create reusable sprite-button loader
- [x] Standardize button state order: Normal / Hover / Pressed
- [ ] Migrate Settings buttons
- [ ] Migrate Main Menu buttons
- [ ] Migrate Field Guide common buttons
- [x] Migrate creature context-menu buttons
- [ ] Clean up inconsistent asset naming during migration
- [ ] Keep individual PNGs only where a spritesheet provides no real benefit

Spritesheet work can happen during artwork / low-energy time while core systems continue in code.

## Audio System

- [x] Add shared NAudio engine
- [x] Cache UI sounds in memory
- [x] Support overlapping rapid sound playback
- [x] Universal responsive UI button click
- [x] Field Guide page-flip variants
- [x] Field Guide book-open variants
- [x] Field Guide book-close variants
- [ ] Migrate Field Guide sounds fully onto shared audio engine
- [ ] Migrate creature sounds fully onto shared audio engine
- [ ] Add future master / UI / creature volume controls

---

# CURRENT CREATURE FOUNDATION

## Rat

- [x] Multiple rats
- [x] Drag and drop
- [x] Walking / idle / sniff / sit animations
- [x] Falling / gravity
- [x] Window-surface traversal
- [x] Multi-monitor ground traversal
- [x] Pickup / dangling animation
- [x] Spawn and pickup squeaks
- [x] Appearance palettes
- [x] Patterns
- [x] Accessories
- [x] Effects
- [x] Gerald canonical appearance
- [x] Field Guide page
- [x] Persistence v2 migration
- [ ] Home / burrow behavior

## Eagle

- [x] Flying
- [x] Gliding
- [x] Perching
- [x] Looking left / right
- [x] Feather ruffling
- [x] Multiple POI support
- [x] Field Guide page
- [ ] Generalize full multi-monitor flight area
- [x] Persistence v2 migration
- [ ] Nest / home behavior

## Ocelot

- [x] Walk / run behavior using walk animation
- [x] Idle animation
- [x] Falling
- [x] Pickup / dangling animation
- [x] Appearance / region assets
- [x] Field Guide page
- [ ] Polish animation frames
- [ ] Add sounds
- [x] Persistence v2 migration
- [ ] Home / resting behavior

## Tiny Creatures / Bugs

- [x] Initial tiny beetle flight concept / artwork
- [ ] Define insect scaling rules
- [ ] Build first insect creature
- [ ] Tiny-creature movement behavior

---

# FIELD GUIDE

- [x] Book opening animation
- [x] Book closing animation
- [x] Page-turn animation
- [x] Dynamic creature-family tabs
- [x] Left / right tab behavior
- [x] Tab artwork spritesheet
- [x] Custom family tooltips
- [x] Rodent family
- [x] Bird family
- [x] Feline family
- [x] Creature pages loaded from JSON
- [x] Spawn from creature page
- [x] Page-flip sound variants
- [x] Book-open sound variants
- [x] Book-close sound variants
- [ ] More creature families
- [ ] More creature entries
- [ ] Integrate Roster / collection state

---

# HABITATS & WORLD INTERACTIONS

The desktop becomes a living ecosystem.

## Habitats / POIs

- [ ] Bird nests
- [ ] Burrows
- [ ] Hollow trees
- [ ] Ponds
- [ ] Bee hotels
- [ ] Feeders
- [ ] Research stations
- [ ] Camera traps

## Decorations

- [ ] Trees
- [ ] Rocks
- [ ] Flowers
- [ ] Logs
- [ ] Seasonal decorations

## Placement & Editing

- [ ] Tiny placement menu
- [ ] Spawn POIs from menu
- [ ] Drag trees, nests, rocks, ponds, and burrows
- [ ] Save POI positions
- [ ] Load POI positions on startup
- [ ] Delete POIs
- [ ] Lock / unlock edit mode

---

# CREATURE BEHAVIOR / WILDLIFE SIMULATION

Creatures should feel alive.

- [x] Wander
- [x] Rest / idle foundations
- [x] Window-aware movement
- [ ] Drink behavior
- [ ] Eat behavior
- [ ] Sleep
- [ ] Groom
- [ ] Explore
- [ ] Visit habitats
- [ ] Socialize
- [ ] Home behavior
- [ ] Friendship progression
- [ ] Personality traits
- [ ] React to weather

---

# SAVE / COLLECTION GAMEPLAY

- [ ] Friendship mode
- [ ] Free Play mode
- [ ] Creature rarity / variants
- [ ] Collect appearances
- [x] Favorite creatures
- [x] Creature names
- [x] Persistent roster
- [ ] Creature homes
- [ ] Optional breeding
- [ ] Predation toggle OFF by default
- [ ] Collection completion tracking

---

# WEATHER / TIME REACTIONS

- [ ] Rain
- [ ] Wind
- [ ] Snow
- [ ] Heat
- [ ] Sunrise
- [ ] Sunset
- [ ] Moon phase

---

# CONSERVATION GAMEPLAY

**Player fantasy:**  
Build habitats, attract wildlife, help endangered species thrive, and release them back into the wild.

## Conservation Points

- [ ] Habitat bonuses
- [ ] Wildlife discoveries
- [ ] Research rewards
- [ ] Species milestones

## Release System

- [ ] Release creature
- [ ] Conservation reward
- [ ] Species released tracker
- [ ] Lifetime statistics

---

# FUTURE CREATURE PACKS

## Backyard Pack

- [ ] Chickens
- [ ] Ducks
- [ ] Turkeys
- [ ] Songbirds

## Forest Pack

- [ ] Owls
- [ ] Foxes
- [ ] Deer

## Conservation Pack

- [ ] Pangolin
- [ ] Red Panda
- [ ] Black-footed Ferret
- [ ] California Condor
- [ ] Axolotl

## Fantasy Pack

- [ ] Fairy
- [ ] Sprite
- [ ] Dragon
- [ ] Arcane Acres familiars

---

# WALLPAPER INTEGRATION

Desktop Creatures + Dynamic Wallpaper

- [ ] Shared world state
- [ ] Weather integration
- [ ] Season integration
- [ ] Moon phase integration
- [ ] Holiday integration

### Examples

- Eagle flies farther during windy weather
- Fairy glows brighter during full moons
- Creatures wake at sunrise
- Creatures sleep at night
- Snow leaves footprints

---

# FUTURE SYSTEMS

## Monitor Preferences

- [ ] Eagles prefer sky monitors
- [ ] Chickens prefer bottom monitors
- [ ] Fairies prefer moonlit areas
- [ ] Creatures develop favorite locations

## Research & Discovery

- [ ] Discover new species
- [ ] Wildlife journal
- [x] Field Guide / creature encyclopedia foundation
- [ ] Species statistics

## Habitat Progression

- [ ] Unlock larger habitats
- [ ] Habitat upgrades
- [ ] Rare creature attraction
- [ ] Seasonal migrations

## Idle Progression

- [ ] Creatures gather materials
- [ ] Nest building
- [ ] Habitat maintenance
- [ ] Long-term ecosystem growth

## Usability

- [x] Separate ecosystem and menu always-on-top settings
- [ ] Click-through mode
- [ ] Edit mode toggle
- [ ] Placement menu
- [ ] Draggable POIs in edit mode

---

# RELEASE QUALITY / COMMERCIAL POLISH

These are requirements for a paid-quality release, not optional cleanup.

- [ ] Stable save migration / versioning strategy
- [ ] Clear error handling for missing or invalid assets
- [ ] Consistent asset naming conventions
- [x] Centralized audio infrastructure
- [x] Centralized Z-order policy
- [ ] Multi-monitor stress testing
- [ ] Small-display stress testing
- [ ] Performance testing with many creatures
- [ ] Installer / packaging
- [ ] Update strategy
- [ ] Settings persistence
- [ ] Volume controls
- [ ] First-launch experience
- [ ] README refresh
- [ ] Screenshots / GIFs / trailer material
- [ ] Release checklist

---

# VERSION 1.0

## A Living Desktop Ecosystem

Build habitats, attract wildlife, observe their behavior, develop friendships with individual creatures, collect variants, assist endangered species, and create a living world that reacts to weather, seasons, time of day, and your desktop environment.

The guiding standard:

**Cute outside. Boring, dependable engineering inside.**