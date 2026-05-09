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

For physical collision support, add these components to `TerrainTilemap`:

- `TilemapCollider2D`
- optional `CompositeCollider2D`
- `Rigidbody2D` set to `Static` when using Composite Collider

Use collider setup for physics interaction, blockers, and future visual effects. Do not use physics raycasts as the primary board selection or movement rule system.

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

## If the map does not display

Check:

1. `Grid` Cell Layout is `Isometric`.
2. `BoardManager` has the correct `TerrainTilemap` reference.
3. `BoardManager` has a valid `BoardTileDatabase`.
4. Every terrain type used by the generator has at least one Tile assigned.
5. Tile assets are `TileBase` assets, not raw Sprites dragged directly into the scene.
6. The camera is pointed at the Tilemap and uses a suitable orthographic size.
7. `Generate On Start` is enabled or generation is called manually.
