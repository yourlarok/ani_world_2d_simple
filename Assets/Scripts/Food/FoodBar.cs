using System;
using System.Collections.Generic;
using AniWorld.Cards.Data;
using AniWorld.Events;
using AniWorld.Tokens;
using UnityEngine;

namespace AniWorld.Food
{
    public class FoodBar : MonoBehaviour
    {
        public const int Capacity = 6;

        private readonly List<FoodCardData> slots = new List<FoodCardData>(Capacity);

        public event Action FoodBarChanged;

        public IReadOnlyList<FoodCardData> Slots => slots;
        public bool IsFull => slots.Count >= Capacity;
        public int Count => slots.Count;

        public bool Store(FoodCardData food)
        {
            if (food == null || IsFull)
            {
                return false;
            }

            slots.Add(food);
            FoodBarChanged?.Invoke();
            GameEventBus.Publish(new FoodStoredEvent(food));
            return true;
        }

        public FoodCardData Take(int index)
        {
            if (index < 0 || index >= slots.Count)
            {
                return null;
            }

            FoodCardData food = slots[index];
            slots.RemoveAt(index);
            FoodBarChanged?.Invoke();
            return food;
        }

        public bool Feed(int foodIndex, GameToken token)
        {
            if (token == null || foodIndex < 0 || foodIndex >= slots.Count)
            {
                return false;
            }

            FoodCardData food = slots[foodIndex];
            if (!token.Feed(food))
            {
                return false;
            }

            slots.RemoveAt(foodIndex);
            FoodBarChanged?.Invoke();
            GameEventBus.Publish(new FoodFedEvent(food, token));
            return true;
        }

        public void Clear()
        {
            slots.Clear();
            FoodBarChanged?.Invoke();
        }
    }
}
