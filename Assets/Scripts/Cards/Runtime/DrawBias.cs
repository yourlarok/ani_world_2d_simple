using System;
using AniWorld.Cards.Data;
using UnityEngine;

namespace AniWorld.Cards.Runtime
{
    [Serializable]
    public class DrawBias
    {
        public bool usePreferredRarity;
        public CardRarity preferredRarity;
        public bool usePreferredCategory;
        public CardCategory preferredCategory;
        [Range(0f, 1f)] public float biasStrength = 0.5f;

        private const float BaseWeight = 1f;

        public float GetWeight(GameCard card)
        {
            if (card == null)
            {
                return 0f;
            }

            float weight = BaseWeight;

            if (usePreferredRarity)
            {
                weight += card.Rarity == preferredRarity ? biasStrength * 3f : -biasStrength * 0.3f;
            }

            if (usePreferredCategory)
            {
                weight += card.Category == preferredCategory ? biasStrength * 2f : -biasStrength * 0.2f;
            }

            return Mathf.Max(weight, 0.1f);
        }
    }
}
