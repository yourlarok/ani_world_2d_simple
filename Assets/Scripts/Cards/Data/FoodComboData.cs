using AniWorld.Board;
using UnityEngine;

namespace AniWorld.Cards.Data
{
    [CreateAssetMenu(fileName = "FoodCombo", menuName = "FatBallKingdom/Food Combo")]
    public class FoodComboData : ScriptableObject
    {
        [Header("Display")]
        public string comboName;
        [TextArea] public string description;

        [Header("Recipe")]
        public FoodType[] recipe = new FoodType[3];
        public bool raceSpecific;
        public RaceType race;

        [Header("Cast")]
        [Min(0)] public int apCost = 2;
        public ComboEffectType effectType;
        public ComboTargetType targetType = ComboTargetType.Self;
        [Min(0)] public int range;
        [Min(0)] public int aoeRadius;

        [Header("Effect Values")]
        public int damage;
        public int healAmount;
        public int buffPATK;
        public int buffMATK;
        public int buffPDEF;
        public int buffMDEF;
        public int buffMOV;
        public int buffRage;
        [Min(0)] public int duration;
        public TerrainType terrainChange = TerrainType.Grass;
        public CharacterCardData summonCharacter;

        public bool RequiresTarget =>
            targetType != ComboTargetType.None &&
            targetType != ComboTargetType.Self;
    }
}
