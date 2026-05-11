using System;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace AniWorld.Board
{
    public class BoardInputController : MonoBehaviour
    {
        [SerializeField] private Camera mainCamera;
        [SerializeField] private BoardManager boardManager;
        [SerializeField] private BoardHighlighter highlighter;
        [SerializeField] private Tilemap terrainTilemap;

        public event Action<Vector2Int, BoardCell> CellClicked;
        public event Action<Vector2Int> EmptyCellClicked;

        private void Awake()
        {
            if (mainCamera == null)
            {
                mainCamera = Camera.main;
            }

            if (boardManager == null)
            {
                boardManager = FindObjectOfType<BoardManager>();
            }

            if (terrainTilemap == null && boardManager != null)
            {
                terrainTilemap = boardManager.TerrainTilemap;
            }
        }

        private void Update()
        {
            if (!Input.GetMouseButtonDown(0))
            {
                return;
            }

            HandleLeftClick();
        }

        private void HandleLeftClick()
        {
            if (mainCamera == null || terrainTilemap == null || boardManager == null)
            {
                return;
            }

            if (!ScreenCoordinateUtility.TryScreenToWorldOnBoardPlane(mainCamera, Input.mousePosition, out Vector3 mouseWorld))
            {
                return;
            }

            Vector2Int boardPos = BoardCoordinateUtility.WorldToBoardPosition(terrainTilemap, mouseWorld);
            if (boardManager.TryGetCell(boardPos, out BoardCell cell))
            {
                Debug.Log($"Clicked Cell: {boardPos}, Terrain: {cell.Terrain}");
                CellClicked?.Invoke(boardPos, cell);
                if (highlighter != null)
                {
                    highlighter.SelectCell(boardPos);
                }
            }
            else
            {
                EmptyCellClicked?.Invoke(boardPos);
                if (highlighter != null)
                {
                    highlighter.ClearHighlights();
                }
            }
        }
    }
}
