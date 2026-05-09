using UnityEngine;

namespace AniWorld.Cards.Data
{
    [CreateAssetMenu(fileName = "FoodCard", menuName = "FatBallKingdom/Food Card")]
    public class FoodCardData : ScriptableObject
    {
        [Header("Display")]
        public string cardName;
        public Sprite artwork;
        [TextArea] public string description;

        [Header("Identity")]
        public CardRarity rarity = CardRarity.Common;
        public FoodType foodType;

        [Header("Instant Effect")]
        public int instantHP;
        public int instantPATK;
        public int instantMATK;
        public int instantPDEF;
        public int instantMDEF;
        public int instantRAGE;

        [Header("Per Turn Effect")]
        public int perTurnHP;
        public int perTurnPATK;
        public int perTurnMATK;
        public int perTurnPDEF;
        public int perTurnMDEF;
        public int critDamageBoost;
        public int hitRatePenalty;

        [Header("Costs")]
        [Min(0)] public int goldCost = 1;
    }
}
