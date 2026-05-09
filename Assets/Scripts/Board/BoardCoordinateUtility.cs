using UnityEngine;
using UnityEngine.Tilemaps;

namespace AniWorld.Board
{
    public static class BoardCoordinateUtility
    {
        public static Vector3Int ToCellPosition(Vector2Int boardPosition)
        {
            return new Vector3Int(boardPosition.x, boardPosition.y, 0);
        }

        public static Vector2Int ToBoardPosition(Vector3Int cellPosition)
        {
            return new Vector2Int(cellPosition.x, cellPosition.y);
        }

        public static Vector2Int WorldToBoardPosition(Tilemap tilemap, Vector3 worldPosition)
        {
            Vector3Int cellPosition = tilemap.WorldToCell(worldPosition);
            return ToBoardPosition(cellPosition);
        }

        public static Vector3 GetCellWorldCenter(Tilemap tilemap, Vector2Int boardPosition)
        {
            return tilemap.GetCellCenterWorld(ToCellPosition(boardPosition));
        }
    }
}
