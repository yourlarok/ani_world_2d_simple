using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace AniWorld.Board
{
    public class BoardManager : MonoBehaviour
    {
        [Header("Tilemaps")]
        [SerializeField] private Tilemap terrainTilemap;
        [SerializeField] private Tilemap decorationTilemap;
        [SerializeField] private Tilemap highlightTilemap;
        [SerializeField] private Tilemap overlayTilemap;

        [Header("Tiles")]
        [SerializeField] private BoardTileDatabase tileDatabase;
        [SerializeField] private bool useRandomTileVariants = true;

        [Header("Initial Board")]
        [SerializeField] private bool generateOnStart = true;
        [SerializeField] private int initialWidth = 10;
        [SerializeField] private int initialHeight = 8;
        [SerializeField] private TerrainType initialTerrain = TerrainType.Grass;

        private readonly Dictionary<Vector2Int, BoardCell> cells = new Dictionary<Vector2Int, BoardCell>();
        private bool hasBounds;

        public event Action<BoardCell> CellAdded;
        public event Action<BoardCell> CellUpdated;
        public event Action<Vector2Int> CellRemoved;

        public int MinX { get; private set; }
        public int MaxX { get; private set; }
        public int MinY { get; private set; }
        public int MaxY { get; private set; }
        public int CellCount => cells.Count;
        public IEnumerable<BoardCell> Cells => cells.Values;
        public Tilemap TerrainTilemap => terrainTilemap;
        public Tilemap DecorationTilemap => decorationTilemap;
        public Tilemap HighlightTilemap => highlightTilemap;
        public Tilemap OverlayTilemap => overlayTilemap;

        private void Start()
        {
            if (generateOnStart && cells.Count == 0)
            {
                GenerateRect(initialWidth, initialHeight);
            }
        }

        public void GenerateRect(int width, int height)
        {
            ClearBoard();
            FillRect(Vector2Int.zero, width, height, initialTerrain);
        }

        public void ClearBoard()
        {
            cells.Clear();
            hasBounds = false;
            MinX = MaxX = MinY = MaxY = 0;

            ClearTilemap(terrainTilemap);
            ClearTilemap(decorationTilemap);
            ClearTilemap(highlightTilemap);
            ClearTilemap(overlayTilemap);
        }

        public void AddCell(Vector2Int pos, TerrainType terrain)
        {
            if (cells.TryGetValue(pos, out BoardCell existingCell))
            {
                existingCell.SetTerrain(terrain);
                RefreshTerrainTile(pos, terrain);
                CellUpdated?.Invoke(existingCell);
                return;
            }

            BoardCell cell = new BoardCell(pos, terrain);
            cells.Add(pos, cell);
            UpdateBoundsForAddedCell(pos);
            RefreshTerrainTile(pos, terrain);
            CellAdded?.Invoke(cell);
        }

        public void RemoveCell(Vector2Int pos)
        {
            if (!cells.Remove(pos))
            {
                return;
            }

            Vector3Int tilePos = BoardCoordinateUtility.ToCellPosition(pos);
            SetTileSafe(terrainTilemap, tilePos, null);
            SetTileSafe(decorationTilemap, tilePos, null);
            SetTileSafe(highlightTilemap, tilePos, null);
            SetTileSafe(overlayTilemap, tilePos, null);

            RecalculateBounds();
            CellRemoved?.Invoke(pos);
        }

        public bool HasCell(Vector2Int pos)
        {
            return cells.ContainsKey(pos);
        }

        public bool TryGetCell(Vector2Int pos, out BoardCell cell)
        {
            return cells.TryGetValue(pos, out cell);
        }

        public BoardCell GetCell(Vector2Int pos)
        {
            cells.TryGetValue(pos, out BoardCell cell);
            return cell;
        }

        public void SetTerrain(Vector2Int pos, TerrainType terrain)
        {
            AddCell(pos, terrain);
        }

        public TerrainType GetTerrain(Vector2Int pos)
        {
            return cells.TryGetValue(pos, out BoardCell cell) ? cell.Terrain : TerrainType.Unknown;
        }

        public Vector3 GetCellWorldCenter(Vector2Int pos)
        {
            if (terrainTilemap == null)
            {
                Debug.LogWarning("BoardManager.GetCellWorldCenter requires a TerrainTilemap reference.", this);
                return Vector3.zero;
            }

            return BoardCoordinateUtility.GetCellWorldCenter(terrainTilemap, pos);
        }

        public bool TryGetCellWorldCenter(Vector2Int pos, out Vector3 worldCenter)
        {
            if (!HasCell(pos) || terrainTilemap == null)
            {
                worldCenter = Vector3.zero;
                return false;
            }

            worldCenter = BoardCoordinateUtility.GetCellWorldCenter(terrainTilemap, pos);
            return true;
        }

        public bool CanPlaceUnit(Vector2Int pos)
        {
            return cells.TryGetValue(pos, out BoardCell cell) && cell.CanPlaceUnit;
        }

        public bool SetOccupiedUnit(Vector2Int pos, object occupiedUnit)
        {
            if (!cells.TryGetValue(pos, out BoardCell cell))
            {
                return false;
            }

            cell.OccupiedUnit = occupiedUnit;
            CellUpdated?.Invoke(cell);
            return true;
        }

        public void ClearOccupiedUnit(Vector2Int pos)
        {
            if (!cells.TryGetValue(pos, out BoardCell cell))
            {
                return;
            }

            cell.OccupiedUnit = null;
            CellUpdated?.Invoke(cell);
        }

        public bool SetCellState(Vector2Int pos, BoardCellState state)
        {
            if (!cells.TryGetValue(pos, out BoardCell cell))
            {
                return false;
            }

            cell.SetState(state);
            CellUpdated?.Invoke(cell);
            return true;
        }

        public bool SetCellDestructible(Vector2Int pos, bool destructible)
        {
            if (!cells.TryGetValue(pos, out BoardCell cell))
            {
                return false;
            }

            cell.SetDestructible(destructible);
            CellUpdated?.Invoke(cell);
            return true;
        }

        public void FillRect(Vector2Int origin, int width, int height, TerrainType terrain)
        {
            if (width <= 0 || height <= 0)
            {
                return;
            }

            for (int x = origin.x; x < origin.x + width; x++)
            {
                for (int y = origin.y; y < origin.y + height; y++)
                {
                    AddCell(new Vector2Int(x, y), terrain);
                }
            }
        }

        public void FillRectRandom(Vector2Int origin, int width, int height, TerrainType[] terrainPool)
        {
            if (width <= 0 || height <= 0 || terrainPool == null || terrainPool.Length == 0)
            {
                return;
            }

            for (int x = origin.x; x < origin.x + width; x++)
            {
                for (int y = origin.y; y < origin.y + height; y++)
                {
                    AddCell(new Vector2Int(x, y), terrainPool[UnityEngine.Random.Range(0, terrainPool.Length)]);
                }
            }
        }

        public void ExpandRight(int columns, TerrainType defaultTerrain)
        {
            if (columns <= 0)
            {
                return;
            }

            int startX = hasBounds ? MaxX + 1 : 0;
            int startY = hasBounds ? MinY : 0;
            int height = hasBounds ? MaxY - MinY + 1 : 1;
            FillRect(new Vector2Int(startX, startY), columns, height, defaultTerrain);
        }

        public void ExpandLeft(int columns, TerrainType defaultTerrain)
        {
            if (columns <= 0)
            {
                return;
            }

            int startX = hasBounds ? MinX - columns : -columns;
            int startY = hasBounds ? MinY : 0;
            int height = hasBounds ? MaxY - MinY + 1 : 1;
            FillRect(new Vector2Int(startX, startY), columns, height, defaultTerrain);
        }

        public void ExpandTop(int rows, TerrainType defaultTerrain)
        {
            if (rows <= 0)
            {
                return;
            }

            int startX = hasBounds ? MinX : 0;
            int startY = hasBounds ? MaxY + 1 : 0;
            int width = hasBounds ? MaxX - MinX + 1 : 1;
            FillRect(new Vector2Int(startX, startY), width, rows, defaultTerrain);
        }

        public void ExpandBottom(int rows, TerrainType defaultTerrain)
        {
            if (rows <= 0)
            {
                return;
            }

            int startX = hasBounds ? MinX : 0;
            int startY = hasBounds ? MinY - rows : -rows;
            int width = hasBounds ? MaxX - MinX + 1 : 1;
            FillRect(new Vector2Int(startX, startY), width, rows, defaultTerrain);
        }

        public void ExpandRightRandom(int columns, TerrainType[] terrainPool)
        {
            if (columns <= 0)
            {
                return;
            }

            int startX = hasBounds ? MaxX + 1 : 0;
            int startY = hasBounds ? MinY : 0;
            int height = hasBounds ? MaxY - MinY + 1 : 1;
            FillRectRandom(new Vector2Int(startX, startY), columns, height, terrainPool);
        }

        public void ExpandLeftRandom(int columns, TerrainType[] terrainPool)
        {
            if (columns <= 0)
            {
                return;
            }

            int startX = hasBounds ? MinX - columns : -columns;
            int startY = hasBounds ? MinY : 0;
            int height = hasBounds ? MaxY - MinY + 1 : 1;
            FillRectRandom(new Vector2Int(startX, startY), columns, height, terrainPool);
        }

        public void ExpandTopRandom(int rows, TerrainType[] terrainPool)
        {
            if (rows <= 0)
            {
                return;
            }

            int startX = hasBounds ? MinX : 0;
            int startY = hasBounds ? MaxY + 1 : 0;
            int width = hasBounds ? MaxX - MinX + 1 : 1;
            FillRectRandom(new Vector2Int(startX, startY), width, rows, terrainPool);
        }

        public void ExpandBottomRandom(int rows, TerrainType[] terrainPool)
        {
            if (rows <= 0)
            {
                return;
            }

            int startX = hasBounds ? MinX : 0;
            int startY = hasBounds ? MinY - rows : -rows;
            int width = hasBounds ? MaxX - MinX + 1 : 1;
            FillRectRandom(new Vector2Int(startX, startY), width, rows, terrainPool);
        }

        public void GenerateChunk(Vector2Int chunkCoord, int chunkSize, TerrainType defaultTerrain)
        {
            if (chunkSize <= 0)
            {
                return;
            }

            Vector2Int origin = new Vector2Int(chunkCoord.x * chunkSize, chunkCoord.y * chunkSize);
            FillRect(origin, chunkSize, chunkSize, defaultTerrain);
        }

        public void GenerateChunkRandom(Vector2Int chunkCoord, int chunkSize, TerrainType[] terrainPool)
        {
            if (chunkSize <= 0)
            {
                return;
            }

            Vector2Int origin = new Vector2Int(chunkCoord.x * chunkSize, chunkCoord.y * chunkSize);
            FillRectRandom(origin, chunkSize, chunkSize, terrainPool);
        }

        public void ApplyTerrainPatch(TerrainPatch patch, Vector2Int origin)
        {
            ApplyTerrainPatch(patch, origin, true);
        }

        public void ApplyTerrainPatch(TerrainPatch patch, Vector2Int origin, bool overwriteExisting)
        {
            if (patch == null || patch.cells == null)
            {
                return;
            }

            foreach (TerrainPatchCell patchCell in patch.cells)
            {
                if (patchCell == null)
                {
                    continue;
                }

                Vector2Int worldPos = origin + patchCell.localPosition;
                if (!overwriteExisting && HasCell(worldPos))
                {
                    continue;
                }

                AddCell(worldPos, patchCell.terrain);
            }
        }

        private void RefreshTerrainTile(Vector2Int pos, TerrainType terrain)
        {
            if (terrainTilemap == null)
            {
                return;
            }

            TileBase tile = null;
            if (tileDatabase != null)
            {
                tile = useRandomTileVariants
                    ? tileDatabase.GetRandomTerrainTile(terrain)
                    : tileDatabase.GetTerrainTile(terrain);
            }

            terrainTilemap.SetTile(BoardCoordinateUtility.ToCellPosition(pos), tile);
        }

        private static void SetTileSafe(Tilemap tilemap, Vector3Int pos, TileBase tile)
        {
            if (tilemap != null)
            {
                tilemap.SetTile(pos, tile);
            }
        }

        private static void ClearTilemap(Tilemap tilemap)
        {
            if (tilemap != null)
            {
                tilemap.ClearAllTiles();
            }
        }

        private void UpdateBoundsForAddedCell(Vector2Int pos)
        {
            if (!hasBounds)
            {
                MinX = MaxX = pos.x;
                MinY = MaxY = pos.y;
                hasBounds = true;
                return;
            }

            MinX = Mathf.Min(MinX, pos.x);
            MaxX = Mathf.Max(MaxX, pos.x);
            MinY = Mathf.Min(MinY, pos.y);
            MaxY = Mathf.Max(MaxY, pos.y);
        }

        private void RecalculateBounds()
        {
            hasBounds = false;
            MinX = MaxX = MinY = MaxY = 0;

            foreach (Vector2Int pos in cells.Keys)
            {
                UpdateBoundsForAddedCell(pos);
            }
        }
    }
}
