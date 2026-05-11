using System;
using System.Collections.Generic;
using AniWorld.Board;
using UnityEngine;

namespace AniWorld.Board.Generation
{
    public enum TerrainRegionBiasMode
    {
        None,
        Top,
        Bottom,
        Left,
        Right,
        Center,
        Edge,
        Corner
    }

    [Serializable]
    public class TerrainSpawnRule
    {
        public TerrainType terrainType = TerrainType.Grass;

        [Header("Quantity")]
        [Min(0)] public int minCount;
        [Min(0)] public int maxCount;
        [Min(0f)] public float targetRatio;

        [Header("Clustering")]
        [Min(1)] public int clusterSeedCount = 1;
        [Min(0f)] public float clusterGrowthChance = 0.7f;

        [Header("Region Bias")]
        public TerrainRegionBiasMode regionBias = TerrainRegionBiasMode.None;
        [Min(0f)] public float regionBiasStrength = 1f;

        [Header("Fill Behavior")]
        [Min(0f)] public float weightedFillWeight = 1f;
        public bool allowWeightedFill = true;
        public bool overwriteExisting;

        public int ResolveTargetCount(int totalCells, System.Random random)
        {
            int lower = Math.Max(0, minCount);
            int upper = Math.Max(0, maxCount);

            if (lower > 0 || upper > 0)
            {
                if (upper <= 0)
                {
                    upper = lower;
                }

                if (lower > upper)
                {
                    int temp = lower;
                    lower = upper;
                    upper = temp;
                }

                return Clamp(random.Next(lower, upper + 1), 0, totalCells);
            }

            if (targetRatio > 0f)
            {
                return Clamp(Mathf.RoundToInt(totalCells * targetRatio), 0, totalCells);
            }

            return 0;
        }

        private static int Clamp(int value, int min, int max)
        {
            if (value < min)
            {
                return min;
            }

            return value > max ? max : value;
        }
    }

    [CreateAssetMenu(menuName = "Board/Generation Config", fileName = "BoardGenerationConfig")]
    public class BoardGenerationConfig : ScriptableObject
    {
        [Header("Board Size")]
        public Vector2Int origin = Vector2Int.zero;
        [Min(1)] public int width = 20;
        [Min(1)] public int height = 15;
        public TerrainType fallbackTerrain = TerrainType.Grass;

        [Header("Seed")]
        public bool useFixedSeed = true;
        public int fixedSeed = 12345;

        [Header("Rules")]
        public bool fillRemainingWithWeightedRules;
        public List<TerrainSpawnRule> terrainRules = new List<TerrainSpawnRule>();

        public int CellCount => Mathf.Max(1, width) * Mathf.Max(1, height);

        public int ResolveSeed()
        {
            return useFixedSeed ? fixedSeed : Environment.TickCount;
        }
    }
}
