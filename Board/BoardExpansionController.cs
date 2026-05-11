using UnityEngine;

namespace AniWorld.Board
{
    public class BoardExpansionController : MonoBehaviour
    {
        [SerializeField] private BoardManager boardManager;
        [SerializeField] private TerrainPatch testPatch;
        [SerializeField] private int directionExpandAmount = 3;
        [SerializeField] private int chunkSize = 16;
        [SerializeField] private Vector2Int nextChunkCoord = new Vector2Int(1, 0);
        [SerializeField] private Vector2Int patchOrigin = new Vector2Int(20, 0);

        private void Awake()
        {
            if (boardManager == null)
            {
                boardManager = FindObjectOfType<BoardManager>();
            }
        }

        private void Update()
        {
            if (boardManager == null)
            {
                return;
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                boardManager.ExpandRight(directionExpandAmount, TerrainType.Grass);
            }

            if (Input.GetKeyDown(KeyCode.L))
            {
                boardManager.ExpandLeft(directionExpandAmount, TerrainType.Grass);
            }

            if (Input.GetKeyDown(KeyCode.T))
            {
                boardManager.ExpandTop(directionExpandAmount, TerrainType.Forest);
            }

            if (Input.GetKeyDown(KeyCode.B))
            {
                boardManager.ExpandBottom(directionExpandAmount, TerrainType.Stone);
            }

            if (Input.GetKeyDown(KeyCode.C))
            {
                boardManager.GenerateChunk(nextChunkCoord, chunkSize, TerrainType.Stone);
                nextChunkCoord += Vector2Int.right;
            }

            if (Input.GetKeyDown(KeyCode.P))
            {
                if (testPatch == null)
                {
                    Debug.LogWarning("Pressing P requires a TerrainPatch assigned to BoardExpansionController.", this);
                    return;
                }

                boardManager.ApplyTerrainPatch(testPatch, patchOrigin, true);
            }
        }
    }
}
