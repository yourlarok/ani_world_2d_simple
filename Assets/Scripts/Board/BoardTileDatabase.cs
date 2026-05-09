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
    }

    [CreateAssetMenu(menuName = "Board/Board Tile Database", fileName = "BoardTileDatabase")]
    public class BoardTileDatabase : ScriptableObject
    {
        [SerializeField] private List<TerrainTileSet> terrainTileSets = new List<TerrainTileSet>();

        private Dictionary<TerrainType, List<TileBase>> terrainTileLookup;

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
            List<TileBase> tiles = GetTiles(type);
            if (tiles == null || tiles.Count == 0)
            {
                return null;
            }

            return tiles[UnityEngine.Random.Range(0, tiles.Count)];
        }

        private List<TileBase> GetTiles(TerrainType type)
        {
            BuildLookupIfNeeded();

            if (!terrainTileLookup.TryGetValue(type, out List<TileBase> tiles))
            {
                return null;
            }

            tiles.RemoveAll(tile => tile == null);
            return tiles;
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

            terrainTileLookup = new Dictionary<TerrainType, List<TileBase>>();

            foreach (TerrainTileSet tileSet in terrainTileSets)
            {
                if (tileSet == null)
                {
                    continue;
                }

                if (!terrainTileLookup.TryGetValue(tileSet.terrainType, out List<TileBase> tiles))
                {
                    tiles = new List<TileBase>();
                    terrainTileLookup.Add(tileSet.terrainType, tiles);
                }

                foreach (TileBase tile in tileSet.tiles)
                {
                    if (tile != null)
                    {
                        tiles.Add(tile);
                    }
                }
            }
        }
    }
}
