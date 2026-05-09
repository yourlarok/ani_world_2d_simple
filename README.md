# ani_world_2d_simple

Unity 2022 2D Isometric Tilemap board framework for a hand-drawn tabletop tactics map.

The first phase focuses on the board foundation:

- visual board: Unity Grid + Isometric Tilemaps
- logical board: `Dictionary<Vector2Int, BoardCell>`
- expandable terrain APIs for manual, chunk, and patch-based map building
- click-to-select and highlight support
- early hooks for unit placement, collision setup, cell states, and future tile effects

## Scene hierarchy

Create the scene with this structure:

```text
Scene
├── BoardRoot
│   ├── Grid
│   │   ├── TerrainTilemap
│   │   ├── DecorationTilemap
│   │   ├── HighlightTilemap
│   │   └── OverlayTilemap
│   └── UnitsRoot
├── Main Camera
└── Canvas
```

Create the Grid via Unity:

```text
GameObject > 2D Object > Tilemap > Isometric
```

Keep `Grid > Cell Layout` set to `Isometric`. The board is not hexagonal.

## Scripts

### `Assets/Scripts/Board`

- `TerrainType.cs`
  Terrain enum: `Grass`, `Forest`, `Snow`, `Desert`, `Water`, `Swamp`, `Stone`, `Mud`, `Unknown`.

- `BoardCell.cs`
  Pure logic data. Stores board position, terrain, walkability, move cost, cell state, destructible flag, and occupied unit reference.

- `BoardTileDatabase.cs`
  `ScriptableObject` mapping each `TerrainType` to one or more `TileBase` assets. This avoids hard-coded asset paths and supports visual variants.

- `BoardManager.cs`
  Core board entry point. Owns `BoardCell` data and updates the terrain Tilemap. Provides:
  - `GenerateRect`
  - `ClearBoard`
  - `AddCell`
  - `RemoveCell`
  - `SetTerrain`
  - `FillRect`
  - `FillRectRandom`
  - `ExpandRight / ExpandLeft / ExpandTop / ExpandBottom`
  - `ExpandRightRandom / ExpandLeftRandom / ExpandTopRandom / ExpandBottomRandom`
  - `GenerateChunk / GenerateChunkRandom`
  - `ApplyTerrainPatch`
  - `CanPlaceUnit`, `SetOccupiedUnit`, `ClearOccupiedUnit`
  - `SetCellState`, `SetCellDestructible`

- `BoardCoordinateUtility.cs`
  Converts between `Vector2Int`, Tilemap `Vector3Int`, and world cell centers.

- `BoardTilemapCollisionSetup.cs`
  Optional helper for `TerrainTilemap`. Adds/configures `TilemapCollider2D`, `CompositeCollider2D`, and a static `Rigidbody2D` for future physical interactions.

- `TerrainPatch.cs`
  `ScriptableObject` terrain template. Used to place prepared terrain modules into the board.

- `BoardInputController.cs`
  Left-clicks the Tilemap, converts mouse world position to board coordinate, logs cell information, and selects valid cells.

- `BoardHighlighter.cs`
  Writes selected/move/attack highlights only to `HighlightTilemap`.

- `BoardGenerator.cs`
  Small helper for regenerating a rectangular board from inspector parameters.

- `BoardExpansionController.cs`
  Runtime test controller:
  - `R`: expand right
  - `L`: expand left
  - `T`: expand top
  - `B`: expand bottom
  - `C`: generate a chunk
  - `P`: apply a test `TerrainPatch`

### `Assets/Scripts/Board/Generation`

- `BoardGenerationConfig.cs`
  `ScriptableObject` rule set for random maps. Controls size, origin, seed, fallback terrain, terrain counts, clustering, region bias, and weighted fill.

- `BoardRandomMapBuilder.cs`
  Pure generation logic. Produces `BoardGenerationResult` without touching Tilemaps directly.

- `BoardGenerationResult.cs`
  Runtime result data for one generated map. Can apply itself to `BoardManager`.

- `BoardGeneratedMap.cs`
  Saved generated map asset. Use this to keep a good random result and load it later as a default map.

- `BoardRandomGenerator.cs`
  MonoBehaviour bridge for generating to the board, loading a saved default map, and saving the latest result as an asset in the Unity Editor.

### `Assets/Editor/Board`

- `BoardGeneratedMapAssetUtility.cs`
  Editor-only helper used by `BoardRandomGenerator` to save the latest random result as a `.asset` file.

### `Assets/Scripts/Cards`

- `Data/*`
  ScriptableObject data for character cards, food cards, food combos, rarity multipliers, and shared card enums.

- `Runtime/*`
  Runtime systems for the 30-card pool, 7-card hand, draw bias, gold, AP, play validation, turn session hooks, and drag-to-deploy card flow.

- `UI/*`
  Lightweight UGUI views for hand cards. `CardView` shows playable/blocked state, red costs for missing resources, food-card click purchase, and character-card drag.
  `CardVisualStyle` centralizes card backgrounds, rarity frames, type frames, cost badges, and state overlays.

### `Assets/Scripts/Tokens`

- `GameToken.cs`
  Board character entity spawned from a character card. Stores current combat stats, team, board position, 3 food slots, and Combo readiness.

- `TokenSpawner.cs`
  Spawns tokens onto valid `BoardCell` positions and writes token occupancy through `BoardManager.SetOccupiedUnit`.

- `DeploymentZone.cs`
  Optional deploy-zone validator. If absent, any placeable board cell is valid for deployment.

- `ClassTokenVisualDatabase.cs`
  Optional class-to-visual database for default token sprites/prefabs/icons. Use this for Warrior circle, Assassin diamond, Mage star, Tank square, Archer triangle, and Support cross defaults.

### `Assets/Scripts/Food`

- `FoodBar.cs`
  Six-slot temporary food storage. Food cards bought from the hand go here before being fed to a token.

- `FoodSlot.cs`
  A token stomach slot with 3-turn duration.

- `FoodBarView.cs`
  Simple UGUI view for food-bar contents.

### `Assets/Scripts/Combos`

- `ComboManager.cs`
  Finds common or race-specific food combos from three food types.

- `ComboResolver.cs`
  Treats combos as skills. It supports single damage, AOE damage, healing, ally buffs, terrain change, summon, and special placeholders.

- `ComboTargetingController.cs`
  Enters target-selection mode for combo skills that need a board cell or token target.

### `Assets/Scripts/Events`

- `GameEventBus.cs`
  Lightweight typed event bus with `Subscribe`, `Unsubscribe`, `Publish`, and `Clear`.

- `GameEvents.cs`
  Standard gameplay events for state changes, run setup, turns, cards, food, tokens, combos, terrain, victory, and defeat.

### `Assets/Scripts/GameFlow`

- `GameStateManager.cs`
  High-level state machine for main menu, run loading, battle preparation, player/enemy turns, victory, defeat, and pause.

- `TurnManager.cs`
  Owns turn number and turn owner. Starts/ends player and enemy turns, grants per-turn resources after the first player turn, refills hand, and ticks token food slots.

- `RunSetupManager.cs`
  Prepares a battle by resetting runtime systems, generating/loading the board, rebuilding the card pool, drawing the initial hand, and resetting turns.

- `VictoryConditionManager.cs`
  Basic placeholder that publishes victory/defeat events when one side has no living tokens.

- `GameFlowBootstrap.cs`
  Optional helper for entering main menu or starting a run automatically.

### `Assets/Scripts/UI/GameFlow`

- `GameStateViewRouter.cs`
  Shows/hides main menu, battle HUD, pause, and result panels based on `GameStateManager.CurrentState`.

- `MainMenuView.cs`
  Simple main-menu shell. Start calls `GameStateManager.StartNewRun()`, options toggles an options panel, quit calls `Application.Quit()`.

- `BattleHUDView.cs`
  Simple battle HUD shell for turn, phase, Gold, AP, end-turn, and pause controls.

- `PauseMenuView.cs`
  Resume and return-to-main-menu controls.

- `ResultView.cs`
  Victory/defeat panel with retry and return-to-main-menu controls.

- `CharacterPanelView.cs`
  Left-side character panel shell with portrait, race/class/rarity visuals, stat icons, three food-slot visuals, Combo indicator, and fast-eat button.

- `ComboReadyView.cs`
  Combo prompt shell with combo icon, AP cost badge, target type icon, ready glow, and fast-eat button.

### `Assets/Scripts/Units`

- `UnitData.cs`
  Minimal unit data asset reserved for future unit work.

- `UnitController.cs`
  Minimal placement helper. Units are independent GameObjects under `UnitsRoot`, not Tilemap tiles.

## Inspector setup

### BoardRoot

Attach:

- `BoardManager`
- `BoardInputController`
- `BoardHighlighter`
- optional `BoardExpansionController`
- optional `BoardGenerator`

Assign references:

- `BoardManager`
  - `TerrainTilemap`
  - `DecorationTilemap`
  - `HighlightTilemap`
  - `OverlayTilemap`
  - `BoardTileDatabase`

- `BoardInputController`
  - `Main Camera`
  - `BoardManager`
  - `BoardHighlighter`
  - `TerrainTilemap`

- `BoardHighlighter`
  - `HighlightTilemap`
  - `Selected Tile`
  - `Move Range Tile`
  - `Attack Range Tile`

- `BoardExpansionController`
  - `BoardManager`
  - optional `TerrainPatch`

- `BoardRandomGenerator`
  - `BoardManager`
  - `BoardGenerationConfig`
  - optional `Default Map`
  - enable `Load Default Map On Start` to use a saved generated map
  - enable `Generate On Start` to create a new random map on play

### Card system objects

Recommended runtime objects:

```text
Scene
├── CardRuntime
│   ├── CardPool
│   ├── HandManager
│   ├── GoldManager
│   ├── APManager
│   ├── CardPlayValidator
│   └── CardDragController
├── FoodRuntime
│   └── FoodBar
├── ComboRuntime
│   ├── ComboManager
│   ├── ComboResolver
│   └── ComboTargetingController
├── GameFlowRuntime
│   ├── GameStateManager
│   ├── TurnManager
│   ├── RunSetupManager
│   ├── VictoryConditionManager
│   └── GameFlowBootstrap
└── BoardRoot
    └── UnitsRoot / TokenSpawner
```

Assign references:

- `CardPool`
  - `All Character Cards`
  - `All Food Cards`
  - `Character Target Count` defaults to 20, leaving 10 food cards in the 30-card pool.

- `HandManager`
  - `CardPool`
  - `GoldManager`
  - `APManager`
  - `FoodBar`
  - `TokenSpawner`
  - `CardPlayValidator`

- `CardPlayValidator`
  - `BoardManager`
  - `GoldManager`
  - `APManager`
  - `FoodBar`
  - `TokenSpawner`

- `TokenSpawner`
  - `BoardManager`
  - `UnitsRoot`
  - optional default `GameToken` prefab
  - optional `DeploymentZone`

- `CardDragController`
  - `Main Camera`
  - `BoardManager`
  - `BoardHighlighter`
  - `TerrainTilemap`
  - `HandManager`
  - `CardPlayValidator`

- `ComboResolver`
  - `BoardManager`
  - `APManager`
  - `TokenSpawner`

- `ComboTargetingController`
  - `BoardManager`
  - `BoardHighlighter`
  - `ComboResolver`
  - `TokenSpawner`

- `GameStateManager`
  - `RunSetupManager`
  - `TurnManager`

- `TurnManager`
  - `GoldManager`
  - `APManager`
  - `HandManager`
  - `TokenSpawner`
  - `Grant Resources On First Player Turn` should usually stay off because the run starts with initial Gold/AP.

- `RunSetupManager`
  - `BoardManager`
  - optional `BoardRandomGenerator`
  - `CardPool`
  - `HandManager`
  - `GoldManager`
  - `APManager`
  - `FoodBar`
  - `TokenSpawner`
  - `TurnManager`

### Basic menu and HUD shell

Recommended Canvas structure:

```text
Canvas
├── MainMenuPanel
│   ├── StartButton
│   ├── OptionsButton
│   ├── QuitButton
│   └── OptionsPanel
├── BattleHUDPanel
│   ├── TurnText
│   ├── PhaseText
│   ├── GoldText
│   ├── APText
│   ├── EndTurnButton
│   └── PauseButton
├── CharacterPanel
│   ├── Portrait
│   ├── StatRows
│   ├── FoodSlot x3
│   └── FastEatButton
├── ComboReadyPanel
│   ├── ComboIcon
│   ├── ComboNameText
│   ├── APCostText
│   └── FastEatButton
├── PauseMenuPanel
│   ├── ResumeButton
│   └── MainMenuButton
└── ResultPanel
    ├── TitleText
    ├── MessageText
    ├── RetryButton
    └── MainMenuButton
```

Attach:

- `GameStateViewRouter` to the Canvas or a UI root object.
- `MainMenuView` to `MainMenuPanel`.
- `BattleHUDView` to `BattleHUDPanel`.
- `PauseMenuView` to `PauseMenuPanel`.
- `ResultView` to `ResultPanel`.
- `CharacterPanelView` to `CharacterPanel`.
- `ComboReadyView` to `ComboReadyPanel`.

Wire panel and button references in the Inspector. The UI shell contains only logic and placeholder UGUI references, so its visuals can be replaced with any hand-drawn/tabletop UI sprites later.

## Visual slots to prepare

### Card visuals

Create:

```text
Project window > Create > FatBallKingdom > Card Visual Style
```

Assign:

- character card background
- food card background
- Common/Rare/Epic/Legendary frames
- character/food type frames
- Gold/AP cost badges
- disabled overlay
- playable glow
- selected overlay

`CardView` can also receive direct Image slots for:

- background
- rarity frame
- card type frame
- artwork
- Gold/AP badges
- disabled overlay
- playable glow
- selected overlay

### Token visuals

Create:

```text
Project window > Create > FatBallKingdom > Class Token Visual Database
```

Recommended defaults:

```text
Warrior  -> circle token
Assassin -> diamond token
Mage     -> six-point/star token
Tank     -> square token
Archer   -> triangle token
Support  -> cross token
```

Each class can have:

- token sprite
- token prefab
- class icon

Individual `CharacterCardData` can still override with its own `tokenSprite` or `tokenPrefab`.

### Menu and HUD visuals

The UI shell exposes art slots for:

- main menu background
- title logo
- parchment/wood panel frames
- button icons
- HUD panel frame
- turn/phase banners
- Gold/AP icons
- end-turn and pause button frames
- pause menu frame/icons
- result background/frame/icon

### Food and Combo visuals

`FoodBarView` exposes:

- bar background
- slot frames
- rarity frames
- empty slot icons
- duration overlays
- food icons from `FoodCardData.artwork`

`CharacterPanelView` exposes:

- portrait
- race/class/rarity visuals
- stat icons
- 3 food-slot frames/icons/turn labels
- combo ready indicator

`ComboReadyView` exposes:

- panel frame
- combo icon
- AP cost badge
- target type icon
- ready glow

## Tile database setup

Create the database:

```text
Project window > Create > Board > Board Tile Database
```

Then assign terrain tiles:

```text
Grass   -> Tile_Grass_01, Tile_Grass_02, Tile_Grass_03
Forest  -> Tile_Forest_01, Tile_Forest_02
Snow    -> Tile_Snow_01
Desert  -> Tile_Desert_01
Water   -> Tile_Water_01
Swamp   -> Tile_Swamp_01
Stone   -> Tile_Stone_01
Mud     -> Tile_Mud_01
Unknown -> Tile_Unknown_01
```

Tile images should be imported as Sprite assets, then converted to Unity Tile assets through the Tile Palette or `Create > 2D > Tiles > Tile`.

## Collision setup

Board logic should still use `BoardCell.Walkable` and `BoardCell.MoveCost`.

For physical collision support, attach `BoardTilemapCollisionSetup` to `TerrainTilemap`, or add these components manually:

- `TilemapCollider2D`
- optional `CompositeCollider2D`
- `Rigidbody2D` set to `Static` when using Composite Collider

Use collider setup for physics interaction, blockers, and future visual effects. Do not use physics raycasts as the primary board selection or movement rule system.

## Game flow and turns

The outer game framework is intentionally separate from board/card systems. Prefer this flow for normal play:

```text
GameFlowBootstrap / MainMenu Start Button
    -> GameStateManager.StartNewRun()
    -> RunSetupManager.PrepareBattle()
        -> reset FoodBar, Tokens, Gold, AP
        -> generate/load Board
        -> rebuild CardPool
        -> draw initial Hand
        -> reset TurnManager
    -> PlayerTurnStart
    -> PlayerTurn
```

Then UI buttons can call:

```csharp
gameStateManager.EndPlayerTurn();
gameStateManager.EndEnemyTurn();
```

`TurnManager` handles:

- turn number
- current owner
- player turn start hand refill
- player token turn start/end
- enemy token turn start/end
- Gold/AP income after the first player turn by default

The first player turn does not grant extra Gold/AP unless `Grant Resources On First Player Turn` is enabled. This keeps the run start at the configured initial 10 Gold and starting AP.

## Events

Use `GameEventBus` to decouple UI, effects, and flow:

```csharp
GameEventBus.Subscribe<TurnStartedEvent>(OnTurnStarted);
GameEventBus.Publish(new CardPurchasedEvent(card));
GameEventBus.Unsubscribe<TurnStartedEvent>(OnTurnStarted);
```

Current standard events include:

- `GameStateChangedEvent`
- `RunStartedEvent`, `RunPreparedEvent`, `RunResetEvent`
- `TurnStartedEvent`, `TurnEndedEvent`
- `CardPurchasedEvent`, `CardDiscardedEvent`
- `FoodStoredEvent`, `FoodFedEvent`
- `TokenSpawnedEvent`, `TokenRemovedEvent`
- `ComboReadyEvent`, `ComboTriggeredEvent`
- `TerrainChangedEvent`
- `VictoryEvent`, `DefeatEvent`

## Stylized UI asset workflow

For the hand-drawn tabletop style from the reference image, prefer UI kits with:

- parchment or paper panel backgrounds
- wooden or brass button frames
- 9-sliced frames and borders
- hand-drawn icon sets
- readable fantasy/tabletop fonts
- separate sprites for normal, hover, pressed, disabled, and highlighted states

Recommended workflow:

1. Import UI sprites as `Sprite (2D and UI)`.
2. Use Sprite Editor borders and set panels/buttons to `Image Type: Sliced`.
3. Build reusable prefabs:
   - `UIPanel_Parchment`
   - `UIButton_Wood`
   - `UICardFrame_Common/Rare/Epic/Legendary`
   - `UIResourceBadge_Gold/AP`
4. Keep logic scripts on parent view objects, not inside art-only child objects.
5. Use one consistent font family for labels and a second accent font only for titles.
6. Put repeated icons and frames in Sprite Atlases once the art direction stabilizes.

Good places to look for assets:

- Unity Asset Store: search `fantasy gui`, `hand drawn ui`, `rpg ui`, `board game ui`.
- itch.io asset packs: often good for indie hand-painted UI frames and icons.
- Kenney UI packs: useful for clean placeholders, though less hand-painted.
- Game-icons.net: useful for prototype icons; check license attribution requirements.

Avoid mixing too many UI packs. Pick one main panel/button style, then recolor or lightly edit it so cards, HUD, menus, and popup panels feel like the same board game.

## Card, gold, AP, and combo flow

The card loop follows the FatBall Kingdom rules:

```text
Build 30-card pool before the game
Draw 7 cards to hand on start
Buy/use 1 hand card
Remove that card from hand
Draw 1 replacement from the pool
```

Gold and AP are separate resources:

- Gold buys cards.
- AP pays for board actions.
- Character cards require both Gold and AP when deployed.
- Food cards require Gold only and go into the six-slot `FoodBar`.
- Feeding a food to a token costs 0 AP.
- Fast-eat Combo skills spend the combo AP cost, normally 2 AP.

### Character cards

Character cards are not clicked and bought first. Their intended interaction is:

```text
Drag playable character card from hand
Highlight valid deploy cells
Drop on a legal board cell
Spend Gold + AP
Spawn GameToken on the board
Remove card from hand
Draw replacement card
```

`CardView` shows affordability:

- playable card: normal alpha/color
- not enough Gold: Gold cost turns red
- not enough AP: AP cost turns red
- invalid target/no deploy cell: card is dimmed

### Food cards

Food cards are bought into the food bar:

```text
Click playable food card
Spend Gold
Store FoodCardData in FoodBar
Remove card from hand
Draw replacement card
```

Food cards are blocked if Gold is insufficient or the food bar is full.

### Token food slots and Combo skills

Each `GameToken` has 3 stomach slots. Feeding applies instant effects and stores the food for 3 turns. When all 3 slots are filled, `ComboManager` checks the recipe and race.

Combo effects are treated as skills, not just buffs. `ComboResolver` currently supports:

- single-target damage
- AOE damage
- healing
- ally buff
- debuff/damage placeholder
- summon
- terrain change
- special placeholder for custom logic

Combos with a target type use `ComboTargetingController` to highlight valid cells or tokens before resolving.

## Random map generation

Create a random generation config:

```text
Project window > Create > Board > Generation Config
```

Important fields:

- `Origin`: where the generated map starts in board coordinates.
- `Width / Height`: generated map size.
- `Fallback Terrain`: base terrain for cells not assigned by rules.
- `Use Fixed Seed / Fixed Seed`: keep enabled when you want reproducible maps.
- `Fill Remaining With Weighted Rules`: fills cells not claimed by count rules using rule weights.
- `Terrain Rules`: per-terrain generation rules.

Each terrain rule supports:

- `Min Count / Max Count`: random count range. Set both to the same value for an exact count.
- `Target Ratio`: percentage of total cells if count range is not used.
- `Cluster Seed Count`: how many separate clusters this terrain tries to start.
- `Cluster Growth Chance`: higher values keep terrain more connected.
- `Region Bias`: prefer top, bottom, left, right, center, edge, or corner.
- `Region Bias Strength`: how strongly the rule prefers that region.
- `Weighted Fill Weight`: weight used when filling remaining cells.
- `Overwrite Existing`: whether this rule can replace cells already assigned by earlier rules.

Example:

```text
Config: 30 x 20
Fallback Terrain: Grass

Rule 1: Water
- Min Count: 30
- Max Count: 45
- Cluster Seed Count: 2
- Cluster Growth Chance: 0.9
- Region Bias: Center

Rule 2: Forest
- Target Ratio: 0.25
- Cluster Seed Count: 4
- Cluster Growth Chance: 0.8
- Region Bias: Left

Rule 3: Snow
- Target Ratio: 0.15
- Cluster Seed Count: 2
- Cluster Growth Chance: 0.75
- Region Bias: Top
```

Attach `BoardRandomGenerator` to `BoardRoot`, assign `BoardManager` and the config, then use its context menu:

```text
Generate To Board
Save Last Result As Asset
Load Default Map
```

### Saving a generated map as a default map

In the Unity Editor:

1. Select the object with `BoardRandomGenerator`.
2. Run `Generate To Board` until the result looks good.
3. Run `Save Last Result As Asset`.
4. A `BoardGeneratedMap` asset is created under `Assets/GeneratedMaps` by default.
5. Drag that asset into `BoardRandomGenerator > Default Map`.
6. Enable `Load Default Map On Start`.

After this, Play Mode loads the saved generated map instead of rolling a new random result.

## Testing

### Initial board

Enable `BoardManager > Generate On Start`. Running the scene generates a `10 x 8` board by default.

### Click selection

Left-click a valid cell. Console should print:

```text
Clicked Cell: (3, 5), Terrain: Grass
```

The selected marker should appear on `HighlightTilemap`. Clicking empty space clears highlights.

### AddCell

Call:

```csharp
boardManager.AddCell(new Vector2Int(20, 5), TerrainType.Grass);
```

The new cell should display, have `BoardCell` data, and be clickable.

### FillRect

Call:

```csharp
boardManager.FillRect(new Vector2Int(10, 0), 5, 8, TerrainType.Forest);
```

This fills a rectangular region while preserving existing board behavior.

### Direction expansion

Use code:

```csharp
boardManager.ExpandRight(5, TerrainType.Grass);
boardManager.ExpandTop(3, TerrainType.Snow);
```

Or press `R`, `L`, `T`, `B` with `BoardExpansionController` attached.

### Chunk generation

Call:

```csharp
boardManager.GenerateChunk(new Vector2Int(1, 0), 16, TerrainType.Stone);
```

Or press `C` with `BoardExpansionController`.

### TerrainPatch

Create a patch:

```text
Project window > Create > Board > Terrain Patch
```

Fill its local cells, then call:

```csharp
boardManager.ApplyTerrainPatch(patch, new Vector2Int(20, 0), true);
```

Or assign it to `BoardExpansionController` and press `P`.

### Random generated map

Create and assign a `BoardGenerationConfig`, then call:

```csharp
boardRandomGenerator.GenerateToBoard();
```

The generated terrain is applied through `BoardManager.AddCell`, so every cell still has logical `BoardCell` data, Tilemap display, click selection, highlighting, unit placement checks, and future collision/state behavior.

To keep a generated result:

```csharp
boardRandomGenerator.SaveLastResultAsAsset();
```

This Editor-only method creates a `BoardGeneratedMap` asset. Assign that asset as `Default Map` and call:

```csharp
boardRandomGenerator.LoadDefaultMap();
```

### Card hand and resources

Create character and food assets:

```text
Project window > Create > FatBallKingdom > Character Card
Project window > Create > FatBallKingdom > Food Card
Project window > Create > FatBallKingdom > Food Combo
```

Assign them to `CardPool`, then run the scene. `HandManager` draws up to 7 cards. `GoldManager` starts at 10 gold, and `APManager` starts with AP according to its inspector values.

### Character drag deployment

Requirements:

1. The hand has a character card.
2. Gold is at least the card Gold cost.
3. AP is at least the card AP cost.
4. The target cell exists, is walkable, is empty, and is inside `DeploymentZone` if one is assigned.

Drag a playable `CardView` from the hand onto the board. On a legal drop:

```text
Gold/AP are spent
GameToken is spawned under UnitsRoot
BoardCell.OccupiedUnit is set
The card leaves the hand
One replacement card is drawn from CardPool
```

### Food card purchase and feeding

Click a playable food card. It spends Gold and enters the six-slot `FoodBar`. Feeding can then be done in code:

```csharp
foodBar.Feed(foodIndex, targetToken);
```

Feeding costs 0 AP. The target token receives instant food effects and stores the food in one of its 3 stomach slots.

### Combo skill resolution

When a token has 3 filled food slots, call:

```csharp
comboTargetingController.BeginCombo(token);
```

If the combo needs no target, it resolves immediately. If it needs a target, valid cells are highlighted and the selected target can be confirmed with:

```csharp
comboTargetingController.ConfirmTarget(boardPosition);
```

## If the map does not display

Check:

1. `Grid` Cell Layout is `Isometric`.
2. `BoardManager` has the correct `TerrainTilemap` reference.
3. `BoardManager` has a valid `BoardTileDatabase`.
4. Every terrain type used by the generator has at least one Tile assigned.
5. Tile assets are `TileBase` assets, not raw Sprites dragged directly into the scene.
6. The camera is pointed at the Tilemap and uses a suitable orthographic size.
7. `Generate On Start` is enabled or generation is called manually.
