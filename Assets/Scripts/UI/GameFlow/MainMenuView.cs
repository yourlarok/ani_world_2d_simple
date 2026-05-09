using AniWorld.GameFlow;
using UnityEngine;
using UnityEngine.UI;

namespace AniWorld.UI.GameFlow
{
    public class MainMenuView : MonoBehaviour
    {
        [SerializeField] private GameStateManager gameStateManager;
        [SerializeField] private Button startButton;
        [SerializeField] private Button optionsButton;
        [SerializeField] private Button quitButton;
        [SerializeField] private GameObject optionsPanel;

        private void Awake()
        {
            if (gameStateManager == null)
            {
                gameStateManager = FindObjectOfType<GameStateManager>();
            }
        }

        private void OnEnable()
        {
            AddListeners();
        }

        private void OnDisable()
        {
            RemoveListeners();
        }

        public void StartGame()
        {
            gameStateManager?.StartNewRun();
        }

        public void ToggleOptions()
        {
            if (optionsPanel != null)
            {
                optionsPanel.SetActive(!optionsPanel.activeSelf);
            }
        }

        public void QuitGame()
        {
            Application.Quit();
        }

        private void AddListeners()
        {
            if (startButton != null)
            {
                startButton.onClick.AddListener(StartGame);
            }

            if (optionsButton != null)
            {
                optionsButton.onClick.AddListener(ToggleOptions);
            }

            if (quitButton != null)
            {
                quitButton.onClick.AddListener(QuitGame);
            }
        }

        private void RemoveListeners()
        {
            if (startButton != null)
            {
                startButton.onClick.RemoveListener(StartGame);
            }

            if (optionsButton != null)
            {
                optionsButton.onClick.RemoveListener(ToggleOptions);
            }

            if (quitButton != null)
            {
                quitButton.onClick.RemoveListener(QuitGame);
            }
        }
    }
}
