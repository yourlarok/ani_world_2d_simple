using System;
using System.Collections.Generic;
using AniWorld.Board;
using UnityEngine;

namespace AniWorld.Board.Generation
{
    [Serializable]
    public class GeneratedBoardCell
    {
        public Vector2Int localPosition;
        public TerrainType terrain;

        public GeneratedBoardCell(Vector2Int localPosition, TerrainType terrain)
        {
            this.localPosition = localPosition;
            this.terrain = terrain;
        }
    }

    [Serializable]
    public class BoardGenerationResult
    {
        public Vector2Int origin;
        public int width;
        public int height;
        public int seed;
        public List<GeneratedBoardCell> cells = new List<GeneratedBoardCell>();

        public BoardGenerationResult(Vector2Int origin, int width, int height, int seed)
        {
            this.origin = origin;
            this.width = width;
            this.height = height;
            this.seed = seed;
        }

        public Vector2Int LocalToBoardPosition(Vector2Int localPosition)
        {
            return origin + localPosition;
        }

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

                boardManager.AddCell(LocalToBoardPosition(cell.localPosition), cell.terrain);
            }
        }
    }
}
