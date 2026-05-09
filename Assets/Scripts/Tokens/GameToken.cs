using System;
using System.Collections.Generic;
using AniWorld.Board;
using AniWorld.Cards.Data;
using AniWorld.Combos;
using AniWorld.Food;
using UnityEngine;

namespace AniWorld.Tokens
{
    public class GameToken : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer tokenSpriteRenderer;

        private readonly FoodSlot[] foodSlots = { new FoodSlot(), new FoodSlot(), new FoodSlot() };

        public event Action<GameToken, FoodComboData> ComboReady;
        public event Action<GameToken, FoodComboData> ComboTriggered;
        public event Action<GameToken> StatsChanged;

        public CharacterCardData CardData { get; private set; }
        public TeamType Team { get; private set; }
        public Vector2Int BoardPosition { get; private set; }
        public bool IsPlaced { get; private set; }
        public bool HasActed { get; private set; }

        public int CurrentHP { get; private set; }
        public int MaxHP { get; private set; }
        public int PATK { get; private set; }
        public int MATK { get; private set; }
        public int PDEF { get; private set; }
        public int MDEF { get; private set; }
        public int MOV { get; private set; }
        public int Rage { get; private set; }

        public IReadOnlyList<FoodSlot> FoodSlots => foodSlots;
        public bool IsStomachFull => !foodSlots[0].IsEmpty && !foodSlots[1].IsEmpty && !foodSlots[2].IsEmpty;

        public void Initialize(CharacterCardData card, TeamType team, Vector2Int boardPosition)
        {
            CardData = card;
            Team = team;
            BoardPosition = boardPosition;
            IsPlaced = true;
            HasActed = false;

            MaxHP = RarityMultiplier.GetHP(card);
            CurrentHP = MaxHP;
            PATK = RarityMultiplier.GetPATK(card);
            MATK = RarityMultiplier.GetMATK(card);
            PDEF = RarityMultiplier.GetPDEF(card);
            MDEF = RarityMultiplier.GetMDEF(card);
            MOV = RarityMultiplier.GetMOV(card);
            Rage = RarityMultiplier.GetRAGE(card);

            for (int i = 0; i < foodSlots.Length; i++)
            {
                foodSlots[i].Clear();
            }

            if (tokenSpriteRenderer != null && card.tokenSprite != null)
            {
                tokenSpriteRenderer.sprite = card.tokenSprite;
            }

            StatsChanged?.Invoke(this);
        }

        public bool Feed(FoodCardData food)
        {
            if (food == null)
            {
                return false;
            }

            FoodSlot slot = GetFirstEmptyFoodSlot();
            if (slot == null)
            {
                return false;
            }

            slot.Fill(food);
            ApplyInstantFoodEffect(food);
            FoodComboData combo = FindReadyCombo();
            if (combo != null)
            {
                ComboReady?.Invoke(this, combo);
            }

            StatsChanged?.Invoke(this);
            return true;
        }

        public bool TryFastEat(ComboResolver resolver, Vector2Int targetPosition, GameToken targetToken)
        {
            FoodComboData combo = FindReadyCombo();
            if (combo == null || resolver == null)
            {
                return false;
            }

            bool resolved = resolver.TryResolve(new ComboCastRequest(this, combo, targetPosition, targetToken));
            if (!resolved)
            {
                return false;
            }

            for (int i = 0; i < foodSlots.Length; i++)
            {
                foodSlots[i].Clear();
            }

            ComboTriggered?.Invoke(this, combo);
            StatsChanged?.Invoke(this);
            return true;
        }

        public FoodComboData FindReadyCombo()
        {
            if (!IsStomachFull)
            {
                return null;
            }

            FoodType[] recipe =
            {
                foodSlots[0].Food.foodType,
                foodSlots[1].Food.foodType,
                foodSlots[2].Food.foodType
            };

            return ComboManager.Instance != null ? ComboManager.Instance.FindCombo(recipe, CardData.race) : null;
        }

        public void OnTurnStart()
        {
            HasActed = false;
        }

        public void OnTurnEnd()
        {
            foreach (FoodSlot slot in foodSlots)
            {
                if (slot.IsEmpty)
                {
                    continue;
                }

                ApplyPerTurnFoodEffect(slot.Food);
                slot.OnTurnEnd();
            }

            StatsChanged?.Invoke(this);
        }

        public void MoveTo(Vector2Int position, Vector3 worldPosition)
        {
            BoardPosition = position;
            transform.position = worldPosition;
        }

        public void TakeDamage(int amount)
        {
            CurrentHP = Mathf.Max(0, CurrentHP - Mathf.Max(0, amount));
            StatsChanged?.Invoke(this);
        }

        public void Heal(int amount)
        {
            CurrentHP = Mathf.Min(MaxHP, CurrentHP + Mathf.Max(0, amount));
            StatsChanged?.Invoke(this);
        }

        public void ApplyBuff(int patk, int matk, int pdef, int mdef, int mov, int rage)
        {
            PATK += patk;
            MATK += matk;
            PDEF += pdef;
            MDEF += mdef;
            MOV += mov;
            Rage = Mathf.Clamp(Rage + rage, 0, 100);
            StatsChanged?.Invoke(this);
        }

        private FoodSlot GetFirstEmptyFoodSlot()
        {
            foreach (FoodSlot slot in foodSlots)
            {
                if (slot.IsEmpty)
                {
                    return slot;
                }
            }

            return null;
        }

        private void ApplyInstantFoodEffect(FoodCardData food)
        {
            Heal(food.instantHP);
            PATK += food.instantPATK;
            MATK += food.instantMATK;
            PDEF += food.instantPDEF;
            MDEF += food.instantMDEF;
            Rage = Mathf.Clamp(Rage + food.instantRAGE, 0, 100);
        }

        private void ApplyPerTurnFoodEffect(FoodCardData food)
        {
            Heal(food.perTurnHP);
            PATK += food.perTurnPATK;
            MATK += food.perTurnMATK;
            PDEF += food.perTurnPDEF;
            MDEF += food.perTurnMDEF;
        }
    }
}
