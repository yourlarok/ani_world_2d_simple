using System.Collections.Generic;
using AniWorld.Board;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace AniWorld.Cards.Runtime
{
    public class CardDragController : MonoBehaviour
    {
        [SerializeField] private Camera mainCamera;
        [SerializeField] private BoardManager boardManager;
        [SerializeField] private BoardHighlighter highlighter;
        [SerializeField] private Tilemap terrainTilemap;
        [SerializeField] private HandManager handManager;
        [SerializeField] private CardPlayValidator validator;

        public BoardInteractionMode Mode { get; private set; } = BoardInteractionMode.Inspect;
        public int DraggingHandIndex { get; private set; } = -1;

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

            if (highlighter == null)
            {
                highlighter = FindObjectOfType<BoardHighlighter>();
            }

            if (handManager == null)
            {
                handManager = FindObjectOfType<HandManager>();
            }

            if (validator == null)
            {
                validator = FindObjectOfType<CardPlayValidator>();
            }

            if (terrainTilemap == null && boardManager != null)
            {
                terrainTilemap = boardManager.TerrainTilemap;
            }
        }

        public bool BeginCharacterDrag(int handIndex)
        {
            GameCard card = handManager != null ? handManager.GetCard(handIndex) : null;
            if (card == null || !card.IsCharacter || validator == null || !validator.EvaluateCard(card).CanPlay)
            {
                return false;
            }

            Mode = BoardInteractionMode.DraggingCharacterCard;
            DraggingHandIndex = handIndex;
            ShowDeployHighlights(card);
            return true;
        }

        public bool EndCharacterDrag(Vector2 screenPosition)
        {
            if (Mode != BoardInteractionMode.DraggingCharacterCard || handManager == null)
            {
                ClearDrag();
                return false;
            }

            if (!TryGetBoardPosition(screenPosition, out Vector2Int boardPosition))
            {
                ClearDrag();
                return false;
            }

            bool deployed = handManager.TryDeployCharacterFromHand(DraggingHandIndex, boardPosition);
            ClearDrag();
            return deployed;
        }

        public void CancelDrag()
        {
            ClearDrag();
        }

        private void ShowDeployHighlights(GameCard card)
        {
            if (boardManager == null || highlighter == null || validator == null)
            {
                return;
            }

            List<Vector2Int> positions = new List<Vector2Int>();
            foreach (BoardCell cell in boardManager.Cells)
            {
                if (cell != null && validator.EvaluateCharacterDeployment(card, cell.Position).CanPlay)
                {
                    positions.Add(cell.Position);
                }
            }

            highlighter.ShowMoveRange(positions);
        }

        private bool TryGetBoardPosition(Vector2 screenPosition, out Vector2Int boardPosition)
        {
            if (mainCamera == null || terrainTilemap == null)
            {
                boardPosition = Vector2Int.zero;
                return false;
            }

            Vector3 screen = new Vector3(screenPosition.x, screenPosition.y, Mathf.Abs(mainCamera.transform.position.z));
            Vector3 world = mainCamera.ScreenToWorldPoint(screen);
            world.z = 0f;
            boardPosition = BoardCoordinateUtility.WorldToBoardPosition(terrainTilemap, world);
            return true;
        }

        private void ClearDrag()
        {
            Mode = BoardInteractionMode.Inspect;
            DraggingHandIndex = -1;
            highlighter?.ClearHighlights();
        }
    }
}
