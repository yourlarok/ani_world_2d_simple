using AniWorld.Board;
using UnityEngine;

namespace AniWorld.Units
{
    public class UnitController : MonoBehaviour
    {
        [SerializeField] private UnitData unitData;
        [SerializeField] private BoardManager boardManager;

        public UnitData UnitData => unitData;
        public Vector2Int BoardPosition { get; private set; }
        public bool IsPlaced { get; private set; }

        private void Awake()
        {
            if (boardManager == null)
            {
                boardManager = FindObjectOfType<BoardManager>();
            }
        }

        private void OnDestroy()
        {
            if (IsPlaced && boardManager != null)
            {
                boardManager.ClearOccupiedUnit(BoardPosition);
            }
        }

        public bool PlaceAt(Vector2Int boardPosition)
        {
            if (boardManager == null || !boardManager.CanPlaceUnit(boardPosition))
            {
                return false;
            }

            if (IsPlaced)
            {
                boardManager.ClearOccupiedUnit(BoardPosition);
            }

            BoardPosition = boardPosition;
            IsPlaced = boardManager.SetOccupiedUnit(boardPosition, this);
            if (IsPlaced)
            {
                transform.position = boardManager.GetCellWorldCenter(boardPosition);
            }

            return IsPlaced;
        }

        public void RemoveFromBoard()
        {
            if (!IsPlaced || boardManager == null)
            {
                return;
            }

            boardManager.ClearOccupiedUnit(BoardPosition);
            IsPlaced = false;
        }
    }
}
