using System.Collections.Generic;
using AniWorld.Cards.Data;
using UnityEngine;

namespace AniWorld.Combos
{
    public class ComboManager : MonoBehaviour
    {
        public static ComboManager Instance { get; private set; }

        [SerializeField] private List<FoodComboData> commonCombos = new List<FoodComboData>();
        [SerializeField] private List<FoodComboData> raceCombos = new List<FoodComboData>();

        private void Awake()
        {
            Instance = this;
        }

        public FoodComboData FindCombo(FoodType[] recipe, RaceType race)
        {
            FoodComboData raceCombo = FindMatchingCombo(raceCombos, recipe, race, true);
            if (raceCombo != null)
            {
                return raceCombo;
            }

            return FindMatchingCombo(commonCombos, recipe, race, false);
        }

        private static FoodComboData FindMatchingCombo(IEnumerable<FoodComboData> combos, FoodType[] recipe, RaceType race, bool requireRace)
        {
            if (recipe == null || recipe.Length < 3 || combos == null)
            {
                return null;
            }

            foreach (FoodComboData combo in combos)
            {
                if (combo == null || combo.recipe == null || combo.recipe.Length < 3)
                {
                    continue;
                }

                if (requireRace && combo.race != race)
                {
                    continue;
                }

                if (Matches(combo.recipe, recipe))
                {
                    return combo;
                }
            }

            return null;
        }

        private static bool Matches(FoodType[] comboRecipe, FoodType[] actualRecipe)
        {
            for (int i = 0; i < 3; i++)
            {
                if (comboRecipe[i] != actualRecipe[i])
                {
                    return false;
                }
            }

            return true;
        }
    }
}
