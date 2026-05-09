using AniWorld.Board;
using AniWorld.Tokens;
using UnityEngine;

namespace AniWorld.UI.GameFlow
{
    public class BoardSelectionPanelController : MonoBehaviour
    {
        [SerializeField] private BoardInputController boardInputController;
        [SerializeField] private CharacterPanelView characterPanelView;

        private void Awake()
        {
            if (boardInputController == null)
            {
                boardInputController = FindObjectOfType<BoardInputController>();
            }

            if (characterPanelView == null)
            {
                characterPanelView = FindObjectOfType<CharacterPanelView>();
            }
        }

        private void OnEnable()
        {
            if (boardInputController != null)
            {
                boardInputController.CellClicked += OnCellClicked;
                boardInputController.EmptyCellClicked += OnEmptyCellClicked;
            }
        }

        private void OnDisable()
        {
            if (boardInputController != null)
            {
                boardInputController.CellClicked -= OnCellClicked;
                boardInputController.EmptyCellClicked -= OnEmptyCellClicked;
            }
        }

        private void OnCellClicked(Vector2Int position, BoardCell cell)
        {
            GameToken token = cell != null ? cell.OccupiedUnit as GameToken : null;
            if (token != null)
            {
                characterPanelView?.Bind(token);
            }
            else
            {
                characterPanelView?.Clear();
            }
        }

        private void OnEmptyCellClicked(Vector2Int position)
        {
            characterPanelView?.Clear();
        }
    }
}
