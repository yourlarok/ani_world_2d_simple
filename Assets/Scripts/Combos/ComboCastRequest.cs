using AniWorld.Cards.Data;
using AniWorld.Tokens;
using UnityEngine;

namespace AniWorld.Combos
{
    public struct ComboCastRequest
    {
        public GameToken Caster { get; }
        public FoodComboData Combo { get; }
        public Vector2Int TargetPosition { get; }
        public GameToken TargetToken { get; }

        public ComboCastRequest(GameToken caster, FoodComboData combo, Vector2Int targetPosition, GameToken targetToken)
        {
            Caster = caster;
            Combo = combo;
            TargetPosition = targetPosition;
            TargetToken = targetToken;
        }
    }
}
