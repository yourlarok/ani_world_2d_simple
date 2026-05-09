using System.Collections.Generic;
using AniWorld.Board;
using UnityEngine;

namespace AniWorld.Board.Generation
{
    [CreateAssetMenu(menuName = "Board/Generated Map", fileName = "GeneratedBoardMap")]
    public class BoardGeneratedMap : ScriptableObject
    {
        [SerializeField] private Vector2Int origin;
        [SerializeField] private int width;
        [SerializeField] private int height;
        [SerializeField] private int seed;
        [SerializeField] private List<GeneratedBoardCell> cells = new List<GeneratedBoardCell>();

        public Vector2Int Origin => origin;
        public int Width => width;
        public int Height => height;
        public int Seed => seed;
        public IReadOnlyList<GeneratedBoardCell> Cells => cells;

        public void ApplyTo(BoardManager boardManager, bool clearExisting)
        {
            if (boardManager == null)
            {
                return;
            }

            if (clearExisting)
            {
                boardManager.ClearBoard();
            }

            foreach (GeneratedBoardCell cell in cells)
            {
                if (cell == null)
                {
                    continue;
                }

                boardManager.AddCell(origin + cell.localPosition, cell.terrain);
            }
        }

        public BoardGenerationResult ToResult()
        {
            BoardGenerationResult result = new BoardGenerationResult(origin, width, height, seed);
            foreach (GeneratedBoardCell cell in cells)
            {
                if (cell != null)
                {
                    result.cells.Add(new GeneratedBoardCell(cell.localPosition, cell.terrain));
                }
            }

            return result;
        }

        public void SetFromResult(BoardGenerationResult result)
        {
            if (result == null)
            {
                return;
            }

            origin = result.origin;
            width = result.width;
            height = result.height;
            seed = result.seed;
            cells.Clear();

            foreach (GeneratedBoardCell cell in result.cells)
            {
                if (cell != null)
                {
                    cells.Add(new GeneratedBoardCell(cell.localPosition, cell.terrain));
                }
            }
        }
    }
}
