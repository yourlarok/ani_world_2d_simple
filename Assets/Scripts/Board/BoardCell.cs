using UnityEngine;

namespace AniWorld.Board
{
    public enum BoardCellState
    {
        Normal,
        Damaged,
        Destroyed,
        Disabled
    }

    public class BoardCell
    {
        public Vector2Int Position { get; private set; }
        public TerrainType Terrain { get; private set; }
        public bool Walkable { get; private set; }
        public int MoveCost { get; private set; }
        public BoardCellState State { get; private set; }
        public bool Destructible { get; private set; }

        // Kept as object until the unit system is implemented.
        public object OccupiedUnit { get; set; }

        public bool IsOccupied => OccupiedUnit != null;
        public bool CanPlaceUnit => Walkable && State != BoardCellState.Destroyed && !IsOccupied;

        public BoardCell(Vector2Int position, TerrainType terrain)
        {
            Position = position;
            State = BoardCellState.Normal;
            Destructible = true;
            SetTerrain(terrain);
        }

        public void SetTerrain(TerrainType terrain)
        {
            Terrain = terrain;
            ApplyTerrainDefaults(terrain);
        }

        public void SetState(BoardCellState state)
        {
            State = state;
        }

        public void SetDestructible(bool destructible)
        {
            Destructible = destructible;
        }

        private void ApplyTerrainDefaults(TerrainType terrain)
        {
            switch (terrain)
            {
                case TerrainType.Water:
                case TerrainType.Unknown:
                    Walkable = false;
                    MoveCost = 999;
                    break;
                case TerrainType.Forest:
                case TerrainType.Swamp:
                case TerrainType.Mud:
                    Walkable = true;
                    MoveCost = 2;
                    break;
                default:
                    Walkable = true;
                    MoveCost = 1;
                    break;
            }
        }
    }
}
