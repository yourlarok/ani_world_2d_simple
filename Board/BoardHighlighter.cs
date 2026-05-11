using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace AniWorld.Board
{
    public class BoardHighlighter : MonoBehaviour
    {
        [SerializeField] private Tilemap highlightTilemap;
        [SerializeField] private TileBase selectedTile;
        [SerializeField] private TileBase moveRangeTile;
        [SerializeField] private TileBase attackRangeTile;

        public void SelectCell(Vector2Int pos)
        {
            ClearHighlights();
            SetHighlight(pos, selectedTile);
        }

        public void ShowMoveRange(IEnumerable<Vector2Int> positions)
        {
            ShowRange(positions, moveRangeTile);
        }

        public void ShowAttackRange(IEnumerable<Vector2Int> positions)
        {
            ShowRange(positions, attackRangeTile);
        }

        public void ClearHighlights()
        {
            if (highlightTilemap != null)
            {
                highlightTilemap.ClearAllTiles();
            }
        }

        private void ShowRange(IEnumerable<Vector2Int> positions, TileBase tile)
        {
            ClearHighlights();

            if (positions == null)
            {
                return;
            }

            foreach (Vector2Int pos in positions)
            {
                SetHighlight(pos, tile);
            }
        }

        private void SetHighlight(Vector2Int pos, TileBase tile)
        {
            if (highlightTilemap == null || tile == null)
            {
                return;
            }

            highlightTilemap.SetTile(BoardCoordinateUtility.ToCellPosition(pos), tile);
        }
    }
}
