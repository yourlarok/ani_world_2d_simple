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

            Vector3 mouseScreen = Input.mousePosition;
            mouseScreen.z = Mathf.Abs(mainCamera.transform.position.z);
            Vector3 mouseWorld = mainCamera.ScreenToWorldPoint(mouseScreen);
            mouseWorld.z = 0f;

            Vector2Int boardPos = BoardCoordinateUtility.WorldToBoardPosition(terrainTilemap, mouseWorld);
            if (boardManager.TryGetCell(boardPos, out BoardCell cell))
            {
                Debug.Log($"Clicked Cell: {boardPos}, Terrain: {cell.Terrain}");
                if (highlighter != null)
                {
                    highlighter.SelectCell(boardPos);
                }
            }
            else if (highlighter != null)
            {
                highlighter.ClearHighlights();
            }
        }
    }
}
