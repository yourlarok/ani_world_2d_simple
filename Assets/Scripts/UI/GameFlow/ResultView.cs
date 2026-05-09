using AniWorld.Events;
using AniWorld.GameFlow;
using UnityEngine;
using UnityEngine.UI;

namespace AniWorld.UI.GameFlow
{
    public class ResultView : MonoBehaviour
    {
        [SerializeField] private GameStateManager gameStateManager;
        [SerializeField] private Text titleText;
        [SerializeField] private Text messageText;
        [SerializeField] private Button retryButton;
        [SerializeField] private Button mainMenuButton;
        [SerializeField] private string victoryTitle = "Victory";
        [SerializeField] private string defeatTitle = "Defeat";

        private void Awake()
        {
            if (gameStateManager == null)
            {
                gameStateManager = FindObjectOfType<GameStateManager>();
            }
        }

        private void OnEnable()
        {
            GameEventBus.Subscribe<GameStateChangedEvent>(OnGameStateChanged);

            if (retryButton != null)
            {
                retryButton.onClick.AddListener(Retry);
            }

            if (mainMenuButton != null)
            {
                mainMenuButton.onClick.AddListener(GoToMainMenu);
            }

            Refresh(gameStateManager != null ? gameStateManager.CurrentState : GameState.Boot);
        }

        private void OnDisable()
        {
            GameEventBus.Unsubscribe<GameStateChangedEvent>(OnGameStateChanged);

            if (retryButton != null)
            {
                retryButton.onClick.RemoveListener(Retry);
            }

            if (mainMenuButton != null)
            {
                mainMenuButton.onClick.RemoveListener(GoToMainMenu);
            }
        }

        public void Retry()
        {
            gameStateManager?.StartNewRun();
        }

        public void GoToMainMenu()
        {
            gameStateManager?.GoToMainMenu();
        }

        private void OnGameStateChanged(GameStateChangedEvent gameEvent)
        {
            Refresh(gameEvent.NewState);
        }

        private void Refresh(GameState state)
        {
            if (titleText != null)
            {
                titleText.text = state == GameState.Victory ? victoryTitle : defeatTitle;
            }

            if (messageText != null)
            {
                messageText.text = state == GameState.Victory
                    ? "The board is yours."
                    : "Your team has fallen.";
            }
        }
    }
}
