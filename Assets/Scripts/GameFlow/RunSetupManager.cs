using AniWorld.Board;
using AniWorld.Board.Generation;
using AniWorld.Cards.Runtime;
using AniWorld.Events;
using AniWorld.Food;
using AniWorld.Tokens;
using UnityEngine;

namespace AniWorld.GameFlow
{
    public class RunSetupManager : MonoBehaviour
    {
        [Header("Board")]
        [SerializeField] private BoardManager boardManager;
        [SerializeField] private BoardRandomGenerator boardRandomGenerator;
        [SerializeField] private bool useRandomGenerator = true;
        [SerializeField] private int fallbackWidth = 10;
        [SerializeField] private int fallbackHeight = 8;

        [Header("Cards")]
        [SerializeField] private CardPool cardPool;
        [SerializeField] private HandManager handManager;
        [SerializeField] private GoldManager goldManager;
        [SerializeField] private APManager apManager;

        [Header("Runtime")]
        [SerializeField] private FoodBar foodBar;
        [SerializeField] private TokenSpawner tokenSpawner;
        [SerializeField] private TurnManager turnManager;

        private void Awake()
        {
            ResolveReferences();
        }

        public void PrepareBattle()
        {
            ResetRun();
            PrepareBoard();
            cardPool?.AutoBuildPool();
            handManager?.InitializeHand();
            turnManager?.ResetTurns();
            GameEventBus.Publish(new RunPreparedEvent());
        }

        public void ResetRun()
        {
            foodBar?.Clear();
            tokenSpawner?.ClearAll();
            goldManager?.ResetGold();
            apManager?.ResetAP();
            GameEventBus.Publish(new RunResetEvent());
        }

        private void PrepareBoard()
        {
            if (useRandomGenerator && boardRandomGenerator != null)
            {
                boardRandomGenerator.GenerateToBoard();
                return;
            }

            if (boardManager != null)
            {
                boardManager.GenerateRect(fallbackWidth, fallbackHeight);
            }
        }

        private void ResolveReferences()
        {
            if (boardManager == null)
            {
                boardManager = FindObjectOfType<BoardManager>();
            }

            if (boardRandomGenerator == null)
            {
                boardRandomGenerator = FindObjectOfType<BoardRandomGenerator>();
            }

            if (cardPool == null)
            {
                cardPool = FindObjectOfType<CardPool>();
            }

            if (handManager == null)
            {
                handManager = FindObjectOfType<HandManager>();
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

            if (turnManager == null)
            {
                turnManager = FindObjectOfType<TurnManager>();
            }
        }
    }
}
