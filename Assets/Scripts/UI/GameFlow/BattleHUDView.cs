using AniWorld.Cards.Runtime;
using AniWorld.Events;
using AniWorld.GameFlow;
using UnityEngine;
using UnityEngine.UI;

namespace AniWorld.UI.GameFlow
{
    public class BattleHUDView : MonoBehaviour
    {
        [SerializeField] private GameStateManager gameStateManager;
        [SerializeField] private TurnManager turnManager;
        [SerializeField] private GoldManager goldManager;
        [SerializeField] private APManager apManager;
        [SerializeField] private Text turnText;
        [SerializeField] private Text phaseText;
        [SerializeField] private Text goldText;
        [SerializeField] private Text apText;
        [SerializeField] private Button endTurnButton;
        [SerializeField] private Button pauseButton;

        [Header("Art Slots")]
        [SerializeField] private Image hudPanelFrame;
        [SerializeField] private Image turnBanner;
        [SerializeField] private Image phaseBanner;
        [SerializeField] private Image goldIcon;
        [SerializeField] private Image apIcon;
        [SerializeField] private Image endTurnButtonFrame;
        [SerializeField] private Image pauseButtonFrame;

        private void Awake()
        {
            if (gameStateManager == null)
            {
                gameStateManager = FindObjectOfType<GameStateManager>();
            }

            if (turnManager == null)
            {
                turnManager = FindObjectOfType<TurnManager>();
            }

            if (goldManager == null)
            {
                goldManager = FindObjectOfType<GoldManager>();
            }

            if (apManager == null)
            {
                apManager = FindObjectOfType<APManager>();
            }
        }

        private void OnEnable()
        {
            GameEventBus.Subscribe<GameStateChangedEvent>(OnGameStateChanged);
            GameEventBus.Subscribe<TurnStartedEvent>(OnTurnStarted);
            GameEventBus.Subscribe<TurnEndedEvent>(OnTurnEnded);

            if (goldManager != null)
            {
                goldManager.GoldChanged += OnGoldChanged;
            }

            if (apManager != null)
            {
                apManager.APChanged += OnAPChanged;
            }

            if (endTurnButton != null)
            {
                endTurnButton.onClick.AddListener(EndPlayerTurn);
            }

            if (pauseButton != null)
            {
                pauseButton.onClick.AddListener(Pause);
            }

            RefreshAll();
        }

        private void OnDisable()
        {
            GameEventBus.Unsubscribe<GameStateChangedEvent>(OnGameStateChanged);
            GameEventBus.Unsubscribe<TurnStartedEvent>(OnTurnStarted);
            GameEventBus.Unsubscribe<TurnEndedEvent>(OnTurnEnded);

            if (goldManager != null)
            {
                goldManager.GoldChanged -= OnGoldChanged;
            }

            if (apManager != null)
            {
                apManager.APChanged -= OnAPChanged;
            }

            if (endTurnButton != null)
            {
                endTurnButton.onClick.RemoveListener(EndPlayerTurn);
            }

            if (pauseButton != null)
            {
                pauseButton.onClick.RemoveListener(Pause);
            }
        }

        public void EndPlayerTurn()
        {
            if (gameStateManager != null && gameStateManager.CurrentState == GameState.PlayerTurn)
            {
                gameStateManager.EndPlayerTurn();
            }
        }

        public void Pause()
        {
            gameStateManager?.Pause();
        }

        private void RefreshAll()
        {
            RefreshTurn();
            RefreshPhase(gameStateManager != null ? gameStateManager.CurrentState : GameState.Boot);
            OnGoldChanged(goldManager != null ? goldManager.CurrentGold : 0);
            OnAPChanged(apManager != null ? apManager.CurrentAP : 0, apManager != null ? apManager.MaxAP : 0);
            RefreshArtSlots();
        }

        private void RefreshTurn()
        {
            if (turnText != null)
            {
                turnText.text = turnManager != null ? $"Turn {turnManager.TurnNumber}" : "Turn -";
            }
        }

        private void RefreshPhase(GameState state)
        {
            if (phaseText != null)
            {
                phaseText.text = state.ToString();
            }

            if (endTurnButton != null)
            {
                endTurnButton.interactable = state == GameState.PlayerTurn;
            }
        }

        private void OnGameStateChanged(GameStateChangedEvent gameEvent)
        {
            RefreshPhase(gameEvent.NewState);
        }

        private void OnTurnStarted(TurnStartedEvent gameEvent)
        {
            RefreshTurn();
        }

        private void OnTurnEnded(TurnEndedEvent gameEvent)
        {
            RefreshTurn();
        }

        private void OnGoldChanged(int currentGold)
        {
            if (goldText != null)
            {
                goldText.text = $"Gold {currentGold}";
            }
        }

        private void OnAPChanged(int currentAP, int maxAP)
        {
            if (apText != null)
            {
                apText.text = $"AP {currentAP}/{maxAP}";
            }
        }

        private void RefreshArtSlots()
        {
            SetImageEnabledIfSprite(hudPanelFrame);
            SetImageEnabledIfSprite(turnBanner);
            SetImageEnabledIfSprite(phaseBanner);
            SetImageEnabledIfSprite(goldIcon);
            SetImageEnabledIfSprite(apIcon);
            SetImageEnabledIfSprite(endTurnButtonFrame);
            SetImageEnabledIfSprite(pauseButtonFrame);
        }

        private static void SetImageEnabledIfSprite(Image image)
        {
            if (image != null)
            {
                image.enabled = image.sprite != null;
            }
        }
    }
}
