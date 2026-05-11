using AniWorld.Board;
using UnityEngine;

namespace AniWorld.Board.Generation
{
    public class BoardRandomGenerator : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private BoardManager boardManager;
        [SerializeField] private BoardGenerationConfig generationConfig;
        [SerializeField] private BoardGeneratedMap defaultMap;

        [Header("Startup")]
        [SerializeField] private bool loadDefaultMapOnStart;
        [SerializeField] private bool generateOnStart;
        [SerializeField] private bool clearBoardBeforeApply = true;

        [Header("Editor Save")]
        [SerializeField] private string saveFolder = "Assets/GeneratedMaps";
        [SerializeField] private string saveAssetName = "GeneratedBoardMap";

        public BoardGenerationResult LastResult { get; private set; }

        private void Awake()
        {
            if (boardManager == null)
            {
                boardManager = FindObjectOfType<BoardManager>();
            }
        }

        private void Start()
        {
            if (loadDefaultMapOnStart && defaultMap != null)
            {
                LoadDefaultMap();
                return;
            }

            if (generateOnStart)
            {
                GenerateToBoard();
            }
        }

        [ContextMenu("Generate To Board")]
        public void GenerateToBoard()
        {
            LastResult = BoardRandomMapBuilder.Generate(generationConfig);
            if (LastResult == null)
            {
                return;
            }

            LastResult.ApplyTo(boardManager, clearBoardBeforeApply);
            Debug.Log($"Generated board map: {LastResult.width}x{LastResult.height}, seed {LastResult.seed}");
        }

        [ContextMenu("Load Default Map")]
        public void LoadDefaultMap()
        {
            if (defaultMap == null)
            {
                Debug.LogWarning("BoardRandomGenerator.LoadDefaultMap requires a default map asset.", this);
                return;
            }

            defaultMap.ApplyTo(boardManager, clearBoardBeforeApply);
            LastResult = defaultMap.ToResult();
            Debug.Log($"Loaded default board map: {defaultMap.Width}x{defaultMap.Height}, seed {defaultMap.Seed}");
        }

        public void SetDefaultMap(BoardGeneratedMap generatedMap)
        {
            defaultMap = generatedMap;
        }
    }
}
