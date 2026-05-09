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

## If the map does not display

Check:

1. `Grid` Cell Layout is `Isometric`.
2. `BoardManager` has the correct `TerrainTilemap` reference.
3. `BoardManager` has a valid `BoardTileDatabase`.
4. Every terrain type used by the generator has at least one Tile assigned.
5. Tile assets are `TileBase` assets, not raw Sprites dragged directly into the scene.
6. The camera is pointed at the Tilemap and uses a suitable orthographic size.
7. `Generate On Start` is enabled or generation is called manually.
