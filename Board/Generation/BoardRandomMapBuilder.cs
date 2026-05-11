using System;
using System.Collections.Generic;
using AniWorld.Board;
using UnityEngine;

namespace AniWorld.Board.Generation
{
    public static class BoardRandomMapBuilder
    {
        private static readonly Vector2Int[] NeighborOffsets =
        {
            new Vector2Int(1, 0),
            new Vector2Int(-1, 0),
            new Vector2Int(0, 1),
            new Vector2Int(0, -1)
        };

        public static BoardGenerationResult Generate(BoardGenerationConfig config)
        {
            if (config == null)
            {
                Debug.LogWarning("BoardRandomMapBuilder.Generate requires a BoardGenerationConfig.");
                return null;
            }

            int width = Mathf.Max(1, config.width);
            int height = Mathf.Max(1, config.height);
            int seed = config.ResolveSeed();
            System.Random random = new System.Random(seed);
            BoardGenerationResult result = new BoardGenerationResult(config.origin, width, height, seed);

            List<Vector2Int> positions = BuildPositions(width, height);
            Dictionary<Vector2Int, TerrainType> terrainByPosition = new Dictionary<Vector2Int, TerrainType>();
            HashSet<Vector2Int> assignedByRule = new HashSet<Vector2Int>();

            foreach (Vector2Int position in positions)
            {
                terrainByPosition[position] = config.fallbackTerrain;
            }

            List<TerrainSpawnRule> terrainRules = config.terrainRules ?? new List<TerrainSpawnRule>();

            foreach (TerrainSpawnRule rule in terrainRules)
            {
                if (rule == null)
                {
                    continue;
                }

                int targetCount = rule.ResolveTargetCount(positions.Count, random);
                if (targetCount <= 0)
                {
                    continue;
                }

                List<Vector2Int> selectedPositions = SelectClusteredPositions(
                    positions,
                    assignedByRule,
                    rule,
                    width,
                    height,
                    targetCount,
                    random);

                foreach (Vector2Int position in selectedPositions)
                {
                    terrainByPosition[position] = rule.terrainType;
                    assignedByRule.Add(position);
                }
            }

            if (config.fillRemainingWithWeightedRules)
            {
                FillRemainingByWeight(terrainRules, positions, assignedByRule, terrainByPosition, width, height, random);
            }

            foreach (Vector2Int position in positions)
            {
                result.cells.Add(new GeneratedBoardCell(position, terrainByPosition[position]));
            }

            return result;
        }

        private static List<Vector2Int> BuildPositions(int width, int height)
        {
            List<Vector2Int> positions = new List<Vector2Int>(width * height);

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    positions.Add(new Vector2Int(x, y));
                }
            }

            return positions;
        }

        private static List<Vector2Int> SelectClusteredPositions(
            List<Vector2Int> allPositions,
            HashSet<Vector2Int> assignedByRule,
            TerrainSpawnRule rule,
            int width,
            int height,
            int targetCount,
            System.Random random)
        {
            List<Vector2Int> selectedPositions = new List<Vector2Int>(targetCount);
            HashSet<Vector2Int> selectedLookup = new HashSet<Vector2Int>();
            List<Vector2Int> frontier = new List<Vector2Int>();
            int seedCount = Mathf.Max(1, rule.clusterSeedCount);
            int safety = allPositions.Count * 8;

            if (!rule.overwriteExisting)
            {
                targetCount = Mathf.Min(targetCount, CountAvailablePositions(allPositions, assignedByRule));
            }

            if (targetCount <= 0)
            {
                return selectedPositions;
            }

            while (selectedPositions.Count < targetCount && safety > 0)
            {
                safety--;

                bool shouldStartNewCluster = selectedPositions.Count == 0
                    || frontier.Count == 0
                    || selectedPositions.Count < seedCount
                    || random.NextDouble() > rule.clusterGrowthChance;

                Vector2Int candidate = shouldStartNewCluster
                    ? PickWeightedPosition(allPositions, selectedLookup, assignedByRule, rule, width, height, random)
                    : PickWeightedPosition(frontier, selectedLookup, assignedByRule, rule, width, height, random);

                if (selectedLookup.Contains(candidate))
                {
                    continue;
                }

                if (!rule.overwriteExisting && assignedByRule.Contains(candidate))
                {
                    continue;
                }

                selectedLookup.Add(candidate);
                selectedPositions.Add(candidate);
                AddNeighbors(candidate, frontier, width, height);
            }

            return selectedPositions;
        }

        private static int CountAvailablePositions(List<Vector2Int> positions, HashSet<Vector2Int> assignedByRule)
        {
            int count = 0;

            foreach (Vector2Int position in positions)
            {
                if (!assignedByRule.Contains(position))
                {
                    count++;
                }
            }

            return count;
        }

        private static void FillRemainingByWeight(
            List<TerrainSpawnRule> terrainRules,
            List<Vector2Int> positions,
            HashSet<Vector2Int> assignedByRule,
            Dictionary<Vector2Int, TerrainType> terrainByPosition,
            int width,
            int height,
            System.Random random)
        {
            foreach (Vector2Int position in positions)
            {
                if (assignedByRule.Contains(position))
                {
                    continue;
                }

                TerrainSpawnRule rule = PickWeightedTerrainRule(terrainRules, position, width, height, random);
                if (rule == null)
                {
                    continue;
                }

                terrainByPosition[position] = rule.terrainType;
            }
        }

        private static Vector2Int PickWeightedPosition(
            List<Vector2Int> candidates,
            HashSet<Vector2Int> selectedLookup,
            HashSet<Vector2Int> assignedByRule,
            TerrainSpawnRule rule,
            int width,
            int height,
            System.Random random)
        {
            float totalWeight = 0f;

            foreach (Vector2Int candidate in candidates)
            {
                if (selectedLookup.Contains(candidate))
                {
                    continue;
                }

                if (!rule.overwriteExisting && assignedByRule.Contains(candidate))
                {
                    continue;
                }

                totalWeight += GetRegionWeight(rule, candidate, width, height);
            }

            if (totalWeight <= 0f)
            {
                return candidates[random.Next(0, candidates.Count)];
            }

            double roll = random.NextDouble() * totalWeight;
            float current = 0f;

            foreach (Vector2Int candidate in candidates)
            {
                if (selectedLookup.Contains(candidate))
                {
                    continue;
                }

                if (!rule.overwriteExisting && assignedByRule.Contains(candidate))
                {
                    continue;
                }

                current += GetRegionWeight(rule, candidate, width, height);
                if (roll <= current)
                {
                    return candidate;
                }
            }

            return candidates[random.Next(0, candidates.Count)];
        }

        private static TerrainSpawnRule PickWeightedTerrainRule(
            List<TerrainSpawnRule> terrainRules,
            Vector2Int position,
            int width,
            int height,
            System.Random random)
        {
            float totalWeight = 0f;

            foreach (TerrainSpawnRule rule in terrainRules)
            {
                if (rule == null || !rule.allowWeightedFill || rule.weightedFillWeight <= 0f)
                {
                    continue;
                }

                totalWeight += rule.weightedFillWeight * GetRegionWeight(rule, position, width, height);
            }

            if (totalWeight <= 0f)
            {
                return null;
            }

            double roll = random.NextDouble() * totalWeight;
            float current = 0f;

            foreach (TerrainSpawnRule rule in terrainRules)
            {
                if (rule == null || !rule.allowWeightedFill || rule.weightedFillWeight <= 0f)
                {
                    continue;
                }

                current += rule.weightedFillWeight * GetRegionWeight(rule, position, width, height);
                if (roll <= current)
                {
                    return rule;
                }
            }

            return null;
        }

        private static void AddNeighbors(Vector2Int position, List<Vector2Int> frontier, int width, int height)
        {
            foreach (Vector2Int offset in NeighborOffsets)
            {
                Vector2Int neighbor = position + offset;
                if (neighbor.x < 0 || neighbor.y < 0 || neighbor.x >= width || neighbor.y >= height)
                {
                    continue;
                }

                frontier.Add(neighbor);
            }
        }

        private static float GetRegionWeight(TerrainSpawnRule rule, Vector2Int position, int width, int height)
        {
            if (rule.regionBias == TerrainRegionBiasMode.None || rule.regionBiasStrength <= 0f)
            {
                return 1f;
            }

            float normalizedX = width <= 1 ? 0.5f : position.x / (float)(width - 1);
            float normalizedY = height <= 1 ? 0.5f : position.y / (float)(height - 1);
            float centerDistanceX = Mathf.Abs(normalizedX - 0.5f) * 2f;
            float centerDistanceY = Mathf.Abs(normalizedY - 0.5f) * 2f;
            float score;

            switch (rule.regionBias)
            {
                case TerrainRegionBiasMode.Top:
                    score = normalizedY;
                    break;
                case TerrainRegionBiasMode.Bottom:
                    score = 1f - normalizedY;
                    break;
                case TerrainRegionBiasMode.Left:
                    score = 1f - normalizedX;
                    break;
                case TerrainRegionBiasMode.Right:
                    score = normalizedX;
                    break;
                case TerrainRegionBiasMode.Center:
                    score = 1f - Mathf.Max(centerDistanceX, centerDistanceY);
                    break;
                case TerrainRegionBiasMode.Edge:
                    score = Mathf.Max(centerDistanceX, centerDistanceY);
                    break;
                case TerrainRegionBiasMode.Corner:
                    score = Mathf.Min(1f, centerDistanceX * centerDistanceY * 2f);
                    break;
                default:
                    score = 0f;
                    break;
            }

            return Mathf.Max(0.01f, 1f + score * rule.regionBiasStrength);
        }
    }
}
