using System;
using AniWorld.Cards.Data;

namespace AniWorld.Food
{
    [Serializable]
    public class FoodSlot
    {
        public FoodCardData Food { get; private set; }
        public int RemainingTurns { get; private set; }
        public bool IsEmpty => Food == null;

        public void Fill(FoodCardData food)
        {
            Food = food;
            RemainingTurns = 3;
        }

        public void Clear()
        {
            Food = null;
            RemainingTurns = 0;
        }

        public void OnTurnEnd()
        {
            if (IsEmpty)
            {
                return;
            }

            RemainingTurns--;
            if (RemainingTurns <= 0)
            {
                Clear();
            }
        }
    }
}
