using System;
using AniWorld.Cards.Data;

namespace AniWorld.Cards.Runtime
{
    [Serializable]
    public class GameCard
    {
        public CardCategory Category { get; private set; }
        public CharacterCardData CharacterData { get; private set; }
        public FoodCardData FoodData { get; private set; }

        public string Name => Category == CardCategory.Character ? CharacterData.cardName : FoodData.cardName;
        public CardRarity Rarity => Category == CardCategory.Character ? CharacterData.rarity : FoodData.rarity;
        public int GoldCost => Category == CardCategory.Character ? RarityMultiplier.GetGoldCost(CharacterData) : FoodData.goldCost;
        public int APCost => Category == CardCategory.Character ? CharacterData.apCost : 0;
        public bool IsCharacter => Category == CardCategory.Character;
        public bool IsFood => Category == CardCategory.Food;

        public GameCard(CharacterCardData data)
        {
            Category = CardCategory.Character;
            CharacterData = data;
        }

        public GameCard(FoodCardData data)
        {
            Category = CardCategory.Food;
            FoodData = data;
        }
    }
}
