using UnityEngine;

namespace AniWorld.Board
{
    public class BoardGenerator : MonoBehaviour
    {
        [SerializeField] private BoardManager boardManager;
        [SerializeField] private int width = 10;
        [SerializeField] private int height = 8;
        [SerializeField] private TerrainType terrain = TerrainType.Grass;

        public void Generate()
        {
            if (boardManager == null)
            {
                boardManager = FindObjectOfType<BoardManager>();
            }

            if (boardManager == null)
            {
                Debug.LogWarning("BoardGenerator requires a BoardManager reference.", this);
                return;
            }

            boardManager.ClearBoard();
            boardManager.FillRect(Vector2Int.zero, width, height, terrain);
        }
    }
}
