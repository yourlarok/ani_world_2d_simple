using UnityEngine;

namespace AniWorld.Cards.Data
{
    public static class RarityMultiplier
    {
        public static float GetStatMultiplier(CardRarity rarity)
        {
            switch (rarity)
            {
                case CardRarity.Rare:
                    return 1.2f;
                case CardRarity.Epic:
                    return 1.5f;
                case CardRarity.Legendary:
                    return 2f;
                default:
                    return 1f;
            }
        }

        public static float GetGoldMultiplier(CardRarity rarity)
        {
            switch (rarity)
            {
                case CardRarity.Rare:
                    return 1.5f;
                case CardRarity.Epic:
                    return 2.5f;
                case CardRarity.Legendary:
                    return 4f;
                default:
                    return 1f;
            }
        }

        public static int GetHP(CharacterCardData card)
        {
            return Mathf.RoundToInt(card.baseHP * GetStatMultiplier(card.rarity));
        }

        public static int GetPATK(CharacterCardData card)
        {
            return Mathf.RoundToInt(card.basePATK * GetStatMultiplier(card.rarity));
        }

        public static int GetMATK(CharacterCardData card)
        {
            return Mathf.RoundToInt(card.baseMATK * GetStatMultiplier(card.rarity));
        }

        public static int GetPDEF(CharacterCardData card)
        {
            return Mathf.RoundToInt(card.basePDEF * GetStatMultiplier(card.rarity));
        }

        public static int GetMDEF(CharacterCardData card)
        {
            return Mathf.RoundToInt(card.baseMDEF * GetStatMultiplier(card.rarity));
        }

        public static int GetMOV(CharacterCardData card)
        {
            return card.baseMOV;
        }

        public static int GetRAGE(CharacterCardData card)
        {
            return Mathf.Clamp(Mathf.RoundToInt(card.baseRAGE * GetStatMultiplier(card.rarity)), 0, 100);
        }

        public static int GetGoldCost(CharacterCardData card)
        {
            return Mathf.RoundToInt(card.goldCost * GetGoldMultiplier(card.rarity));
        }
    }
}
