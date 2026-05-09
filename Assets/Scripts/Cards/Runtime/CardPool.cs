using System;
using System.Collections.Generic;
using AniWorld.Cards.Data;
using UnityEngine;

namespace AniWorld.Cards.Runtime
{
    public class CardPool : MonoBehaviour
    {
        public const int PoolSize = 30;

        [Header("Source Library")]
        [SerializeField] private List<CharacterCardData> allCharacterCards = new List<CharacterCardData>();
        [SerializeField] private List<FoodCardData> allFoodCards = new List<FoodCardData>();

        [Header("Auto Build")]
        [SerializeField] private bool buildOnAwake = true;
        [SerializeField] private int characterTargetCount = 20;

        private readonly List<GameCard> pool = new List<GameCard>();

        public event Action<int> PoolCountChanged;

        public int RemainingCount => pool.Count;
        public IReadOnlyList<GameCard> Cards => pool;

        private void Awake()
        {
            if (buildOnAwake)
            {
                AutoBuildPool();
            }
        }

        public void AutoBuildPool()
        {
            pool.Clear();

            int charCount = Mathf.Clamp(characterTargetCount, 0, PoolSize);
            int foodCount = PoolSize - charCount;

            List<CharacterCardData> shuffledChars = new List<CharacterCardData>(allCharacterCards);
            List<FoodCardData> shuffledFoods = new List<FoodCardData>(allFoodCards);
            Shuffle(shuffledChars);
            Shuffle(shuffledFoods);

            for (int i = 0; i < Mathf.Min(charCount, shuffledChars.Count); i++)
            {
                pool.Add(new GameCard(shuffledChars[i]));
            }

            for (int i = 0; i < Mathf.Min(foodCount, shuffledFoods.Count); i++)
            {
                pool.Add(new GameCard(shuffledFoods[i]));
            }

            while (pool.Count < PoolSize && (shuffledChars.Count > 0 || shuffledFoods.Count > 0))
            {
                bool useCharacter = shuffledFoods.Count == 0 || (shuffledChars.Count > 0 && UnityEngine.Random.value > 0.33f);
                if (useCharacter)
                {
                    pool.Add(new GameCard(shuffledChars[UnityEngine.Random.Range(0, shuffledChars.Count)]));
                }
                else
                {
                    pool.Add(new GameCard(shuffledFoods[UnityEngine.Random.Range(0, shuffledFoods.Count)]));
                }
            }

            Shuffle(pool);
            PoolCountChanged?.Invoke(pool.Count);
        }

        public void SetPool(IEnumerable<GameCard> customPool)
        {
            pool.Clear();
            if (customPool != null)
            {
                pool.AddRange(customPool);
            }

            PoolCountChanged?.Invoke(pool.Count);
        }

        public GameCard DrawFromPool()
        {
            if (pool.Count == 0)
            {
                return null;
            }

            int index = UnityEngine.Random.Range(0, pool.Count);
            GameCard card = pool[index];
            pool.RemoveAt(index);
            PoolCountChanged?.Invoke(pool.Count);
            return card;
        }

        public GameCard DrawFromPoolWithBias(DrawBias bias)
        {
            if (pool.Count == 0)
            {
                return null;
            }

            int index = bias == null ? UnityEngine.Random.Range(0, pool.Count) : WeightedRandomIndex(pool, bias);
            GameCard card = pool[index];
            pool.RemoveAt(index);
            PoolCountChanged?.Invoke(pool.Count);
            return card;
        }

        private static int WeightedRandomIndex(List<GameCard> cards, DrawBias bias)
        {
            float total = 0f;
            for (int i = 0; i < cards.Count; i++)
            {
                total += bias.GetWeight(cards[i]);
            }

            if (total <= 0f)
            {
                return UnityEngine.Random.Range(0, cards.Count);
            }

            float roll = UnityEngine.Random.Range(0f, total);
            for (int i = 0; i < cards.Count; i++)
            {
                roll -= bias.GetWeight(cards[i]);
                if (roll <= 0f)
                {
                    return i;
                }
            }

            return cards.Count - 1;
        }

        private static void Shuffle<T>(IList<T> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = UnityEngine.Random.Range(0, i + 1);
                T temp = list[i];
                list[i] = list[j];
                list[j] = temp;
            }
        }
    }
}
