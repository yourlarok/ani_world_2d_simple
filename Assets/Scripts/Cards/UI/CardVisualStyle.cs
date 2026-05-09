using AniWorld.Cards.Data;
using UnityEngine;

namespace AniWorld.Cards.UI
{
    [CreateAssetMenu(fileName = "CardVisualStyle", menuName = "FatBallKingdom/Card Visual Style")]
    public class CardVisualStyle : ScriptableObject
    {
        [Header("Card Backgrounds")]
        public Sprite characterBackground;
        public Sprite foodBackground;

        [Header("Rarity Frames")]
        public Sprite commonFrame;
        public Sprite rareFrame;
        public Sprite epicFrame;
        public Sprite legendaryFrame;

        [Header("Type Frames")]
        public Sprite characterTypeFrame;
        public Sprite foodTypeFrame;

        [Header("Cost Badges")]
        public Sprite goldCostBadge;
        public Sprite apCostBadge;

        [Header("State Overlays")]
        public Sprite disabledOverlay;
        public Sprite playableGlow;
        public Sprite selectedOverlay;

        public Sprite GetBackground(CardCategory category)
        {
            return category == CardCategory.Character ? characterBackground : foodBackground;
        }

        public Sprite GetRarityFrame(CardRarity rarity)
        {
            switch (rarity)
            {
                case CardRarity.Rare:
                    return rareFrame;
                case CardRarity.Epic:
                    return epicFrame;
                case CardRarity.Legendary:
                    return legendaryFrame;
                default:
                    return commonFrame;
            }
        }

        public Sprite GetTypeFrame(CardCategory category)
        {
            return category == CardCategory.Character ? characterTypeFrame : foodTypeFrame;
        }
    }
}
