using System;
using System.Collections.Generic;
using AniWorld.Cards.Data;
using AniWorld.Events;
using AniWorld.Food;
using AniWorld.Tokens;
using UnityEngine;

namespace AniWorld.Cards.Runtime
{
    public class HandManager : MonoBehaviour
    {
        public const int HandSize = 7;

        [Header("References")]
        [SerializeField] private CardPool cardPool;
        [SerializeField] private GoldManager goldManager;
        [SerializeField] private APManager apManager;
        [SerializeField] private FoodBar foodBar;
        [SerializeField] private TokenSpawner tokenSpawner;
        [SerializeField] private CardPlayValidator validator;

        [Header("Rules")]
        [SerializeField] private bool initializeOnStart = true;
        [SerializeField] private int maxDiscardPerTurn = 1;
        [SerializeField] private DrawBias currentBias = new DrawBias();

        private readonly List<GameCard> hand = new List<GameCard>(HandSize);
        private int discardCountThisTurn;

        public event Action HandChanged;
        public event Action<GameCard> CardPurchased;
        public event Action<GameCard> CardDiscarded;

        public IReadOnlyList<GameCard> Hand => hand;
        public int HandCount => hand.Count;
        public int DiscardCountThisTurn => discardCountThisTurn;

        private void Awake()
        {
            ResolveReferences();
        }

        private void Start()
        {
            if (initializeOnStart)
            {
                InitializeHand();
            }
        }

        public void InitializeHand()
        {
            hand.Clear();
            RefillHand();
            HandChanged?.Invoke();
        }

        public void ClearHand()
        {
            hand.Clear();
            discardCountThisTurn = 0;
            HandChanged?.Invoke();
        }

        public GameCard GetCard(int index)
        {
            return index >= 0 && index < hand.Count ? hand[index] : null;
        }

        public CardPlayState EvaluateCard(int handIndex)
        {
            return validator != null
                ? validator.EvaluateCard(GetCard(handIndex))
                : CardPlayState.Blocked(CardPlayBlockReason.MissingSystem, false, false, false);
        }

        public bool TryDeployCharacterFromHand(int handIndex, Vector2Int boardPosition)
        {
            GameCard card = GetCard(handIndex);
            if (validator == null || tokenSpawner == null || goldManager == null || apManager == null)
            {
                return false;
            }

            CardPlayState state = validator.EvaluateCharacterDeployment(card, boardPosition);
            if (!state.CanPlay)
            {
                return false;
            }

            if (!goldManager.Spend(card.GoldCost))
            {
                return false;
            }

            if (!apManager.Spend(card.APCost))
            {
                goldManager.AddGold(card.GoldCost);
                return false;
            }

            GameToken token = tokenSpawner.Spawn(card.CharacterData, boardPosition);
            if (token == null)
            {
                goldManager.AddGold(card.GoldCost);
                apManager.AddAP(card.APCost);
                return false;
            }

            CompletePurchase(handIndex, card);
            return true;
        }

        public bool TryBuyFoodCard(int handIndex)
        {
            GameCard card = GetCard(handIndex);
            if (card == null || !card.IsFood || validator == null || goldManager == null || foodBar == null)
            {
                return false;
            }

            CardPlayState state = validator.EvaluateCard(card);
            if (!state.CanPlay)
            {
                return false;
            }

            if (!goldManager.Spend(card.GoldCost))
            {
                return false;
            }

            if (!foodBar.Store(card.FoodData))
            {
                goldManager.AddGold(card.GoldCost);
                return false;
            }

            CompletePurchase(handIndex, card);
            return true;
        }

        public bool TryDiscardCard(int handIndex)
        {
            if (discardCountThisTurn >= maxDiscardPerTurn || handIndex < 0 || handIndex >= hand.Count)
            {
                return false;
            }

            GameCard card = hand[handIndex];
            hand.RemoveAt(handIndex);
            discardCountThisTurn++;
            CardDiscarded?.Invoke(card);
            GameEventBus.Publish(new CardDiscardedEvent(card));
            HandChanged?.Invoke();
            return true;
        }

        public void SetDrawBias(DrawBias bias)
        {
            currentBias = bias ?? new DrawBias();
        }

        public void OnTurnStart()
        {
            discardCountThisTurn = 0;
            RefillHand();
            HandChanged?.Invoke();
        }

        private void CompletePurchase(int handIndex, GameCard card)
        {
            hand.RemoveAt(handIndex);
            CardPurchased?.Invoke(card);
            GameEventBus.Publish(new CardPurchasedEvent(card));
            RefillHand();
            HandChanged?.Invoke();
        }

        private void RefillHand()
        {
            if (cardPool == null)
            {
                return;
            }

            while (hand.Count < HandSize && cardPool.RemainingCount > 0)
            {
                GameCard newCard = cardPool.DrawFromPoolWithBias(currentBias);
                if (newCard == null)
                {
                    break;
                }

                hand.Add(newCard);
            }
        }

        private void ResolveReferences()
        {
            if (cardPool == null)
            {
                cardPool = FindObjectOfType<CardPool>();
            }

            if (goldManager == null)
            {
                goldManager = FindObjectOfType<GoldManager>();
            }

            if (apManager == null)
            {
                apManager = FindObjectOfType<APManager>();
            }

            if (foodBar == null)
            {
                foodBar = FindObjectOfType<FoodBar>();
            }

            if (tokenSpawner == null)
            {
                tokenSpawner = FindObjectOfType<TokenSpawner>();
            }

            if (validator == null)
            {
                validator = FindObjectOfType<CardPlayValidator>();
            }
        }
    }
}
