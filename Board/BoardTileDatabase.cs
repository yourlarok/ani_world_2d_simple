using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace AniWorld.Board
{
    [Serializable]
    public class TerrainTileSet
    {
        public TerrainType terrainType;
        public List<TileBase> tiles = new List<TileBase>();
        public List<TileBase> overlayTiles = new List<TileBase>();
        public List<TileBase> sideTiles = new List<TileBase>();
        public List<TileBase> shadowTiles = new List<TileBase>();
        public List<TileBase> decorationTiles = new List<TileBase>();
        [Range(0f, 1f)] public float decorationChance = 0f;
    }

    [CreateAssetMenu(menuName = "Board/Board Tile Database", fileName = "BoardTileDatabase")]
    public class BoardTileDatabase : ScriptableObject
    {
        [SerializeField] private List<TerrainTileSet> terrainTileSets = new List<TerrainTileSet>();

        private Dictionary<TerrainType, TerrainTileSet> terrainTileLookup;

        public TileBase GetTerrainTile(TerrainType type)
        {
            List<TileBase> tiles = GetTiles(type);
            if (tiles == null || tiles.Count == 0)
            {
                return null;
            }

            return tiles[0];
        }

        public TileBase GetRandomTerrainTile(TerrainType type)
        {
            return GetRandomTile(GetTiles(type));
        }

        public TileBase GetOverlayTile(TerrainType type)
        {
            return GetFirstTile(GetOverlayTiles(type));
        }

        public TileBase GetRandomOverlayTile(TerrainType type)
        {
            return GetRandomTile(GetOverlayTiles(type));
        }

        public TileBase GetSideTile(TerrainType type)
        {
            return GetFirstTile(GetSideTiles(type));
        }

        public TileBase GetRandomSideTile(TerrainType type)
        {
            return GetRandomTile(GetSideTiles(type));
        }

        public TileBase GetShadowTile(TerrainType type)
        {
            return GetFirstTile(GetShadowTiles(type));
        }

        public TileBase GetRandomShadowTile(TerrainType type)
        {
            return GetRandomTile(GetShadowTiles(type));
        }

        public TileBase GetDecorationTile(TerrainType type)
        {
            return GetFirstTile(GetDecorationTiles(type));
        }

        public TileBase GetRandomDecorationTile(TerrainType type)
        {
            TerrainTileSet tileSet = GetTileSet(type);
            if (tileSet == null || tileSet.decorationChance <= 0f || UnityEngine.Random.value > tileSet.decorationChance)
            {
                return null;
            }

            return GetRandomTile(GetDecorationTiles(type));
        }

        private List<TileBase> GetTiles(TerrainType type)
        {
            TerrainTileSet tileSet = GetTileSet(type);
            if (tileSet == null)
            {
                return null;
            }

            tileSet.tiles.RemoveAll(tile => tile == null);
            return tileSet.tiles;
        }

        private List<TileBase> GetOverlayTiles(TerrainType type)
        {
            TerrainTileSet tileSet = GetTileSet(type);
            if (tileSet == null)
            {
                return null;
            }

            tileSet.overlayTiles.RemoveAll(tile => tile == null);
            return tileSet.overlayTiles;
        }

        private List<TileBase> GetSideTiles(TerrainType type)
        {
            TerrainTileSet tileSet = GetTileSet(type);
            if (tileSet == null)
            {
                return null;
            }

            tileSet.sideTiles.RemoveAll(tile => tile == null);
            return tileSet.sideTiles;
        }

        private List<TileBase> GetShadowTiles(TerrainType type)
        {
            TerrainTileSet tileSet = GetTileSet(type);
            if (tileSet == null)
            {
                return null;
            }

            tileSet.shadowTiles.RemoveAll(tile => tile == null);
            return tileSet.shadowTiles;
        }

        private List<TileBase> GetDecorationTiles(TerrainType type)
        {
            TerrainTileSet tileSet = GetTileSet(type);
            if (tileSet == null)
            {
                return null;
            }

            tileSet.decorationTiles.RemoveAll(tile => tile == null);
            return tileSet.decorationTiles;
        }

        private TerrainTileSet GetTileSet(TerrainType type)
        {
            BuildLookupIfNeeded();
            return terrainTileLookup.TryGetValue(type, out TerrainTileSet tileSet) ? tileSet : null;
        }

        private static TileBase GetFirstTile(List<TileBase> tiles)
        {
            if (tiles == null || tiles.Count == 0)
            {
                return null;
            }

            return tiles[0];
        }

        private static TileBase GetRandomTile(List<TileBase> tiles)
        {
            if (tiles == null || tiles.Count == 0)
            {
                return null;
            }

            return tiles[UnityEngine.Random.Range(0, tiles.Count)];
        }

        private void OnValidate()
        {
            terrainTileLookup = null;
        }

        private void BuildLookupIfNeeded()
        {
            if (terrainTileLookup != null)
            {
                return;
            }

            terrainTileLookup = new Dictionary<TerrainType, TerrainTileSet>();

            foreach (TerrainTileSet tileSet in terrainTileSets)
            {
                if (tileSet == null)
                {
                    continue;
                }

                terrainTileLookup[tileSet.terrainType] = tileSet;
            }
        }
    }
}
