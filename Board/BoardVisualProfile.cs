using UnityEngine;

namespace AniWorld.Board
{
    [CreateAssetMenu(menuName = "Board/Board Visual Profile", fileName = "BoardVisualProfile")]
    public class BoardVisualProfile : ScriptableObject
    {
        [Header("Grid")]
        [SerializeField] private Vector3 gridCellSize = new Vector3(1f, 0.5f, 1f);

        [Header("Sprite Import")]
        [SerializeField] private float pixelsPerUnit = 390f;

        [Header("Tilemap Offsets")]
        [SerializeField] private Vector3 shadowTilemapOffset = new Vector3(0.06f, -0.16f, 0f);
        [SerializeField] private Vector3 sideTilemapOffset = new Vector3(0f, -0.12f, 0f);
        [SerializeField] private Vector3 terrainTilemapOffset = Vector3.zero;
        [SerializeField] private Vector3 terrainOverlayTilemapOffset = Vector3.zero;
        [SerializeField] private Vector3 decorationTilemapOffset = Vector3.zero;
        [SerializeField] private Vector3 highlightTilemapOffset = Vector3.zero;
        [SerializeField] private Vector3 overlayTilemapOffset = Vector3.zero;

        [Header("Placement")]
        [SerializeField] private Vector3 unitPlacementOffset = new Vector3(0f, 0.12f, 0f);

        public Vector3 GridCellSize => gridCellSize;
        public float PixelsPerUnit => pixelsPerUnit;
        public Vector3 ShadowTilemapOffset => shadowTilemapOffset;
        public Vector3 SideTilemapOffset => sideTilemapOffset;
        public Vector3 TerrainTilemapOffset => terrainTilemapOffset;
        public Vector3 TerrainOverlayTilemapOffset => terrainOverlayTilemapOffset;
        public Vector3 DecorationTilemapOffset => decorationTilemapOffset;
        public Vector3 HighlightTilemapOffset => highlightTilemapOffset;
        public Vector3 OverlayTilemapOffset => overlayTilemapOffset;
        public Vector3 UnitPlacementOffset => unitPlacementOffset;
    }
}
