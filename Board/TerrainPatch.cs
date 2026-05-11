using System;
using System.Collections.Generic;
using UnityEngine;

namespace AniWorld.Board
{
    [CreateAssetMenu(menuName = "Board/Terrain Patch", fileName = "TerrainPatch")]
    public class TerrainPatch : ScriptableObject
    {
        [Min(1)] public int width = 1;
        [Min(1)] public int height = 1;
        public List<TerrainPatchCell> cells = new List<TerrainPatchCell>();
    }

    [Serializable]
    public class TerrainPatchCell
    {
        public Vector2Int localPosition;
        public TerrainType terrain;
    }
}
