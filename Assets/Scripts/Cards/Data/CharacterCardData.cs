using UnityEngine;

namespace AniWorld.Cards.Data
{
    [CreateAssetMenu(fileName = "CharacterCard", menuName = "FatBallKingdom/Character Card")]
    public class CharacterCardData : ScriptableObject
    {
        [Header("Display")]
        public string cardName;
        public Sprite artwork;
        public Sprite tokenSprite;
        public GameObject tokenPrefab;
        [TextArea] public string description;
        [TextArea] public string skillDescription;

        [Header("Identity")]
        public CardRarity rarity = CardRarity.Common;
        public RaceType race;
        public ClassType classType;

        [Header("Base Stats")]
        [Min(1)] public int baseHP = 10;
        [Min(0)] public int basePATK;
        [Min(0)] public int baseMATK;
        [Min(0)] public int basePDEF;
        [Min(0)] public int baseMDEF;
        [Min(0)] public int baseMOV = 3;
        [Range(0, 100)] public int baseRAGE;

        [Header("Costs")]
        [Min(0)] public int goldCost = 1;
        [Min(0)] public int apCost = 1;
    }
}
