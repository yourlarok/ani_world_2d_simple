using UnityEngine;

namespace AniWorld.Units
{
    [CreateAssetMenu(menuName = "Units/Unit Data", fileName = "UnitData")]
    public class UnitData : ScriptableObject
    {
        public string unitId;
        public string displayName;
        [Min(1)] public int maxHealth = 10;
        [Min(0)] public int moveRange = 4;
    }
}
