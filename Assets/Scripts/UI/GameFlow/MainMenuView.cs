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

        [Header("Art Slots")]
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Image panelFrame;
        [SerializeField] private Image titleLogo;
        [SerializeField] private Image startButtonIcon;
        [SerializeField] private Image optionsButtonIcon;
        [SerializeField] private Image quitButtonIcon;

        private void Awake()
        {
            if (gameStateManager == null)
            {
                gameStateManager = FindObjectOfType<GameStateManager>();
            }

            RefreshArtSlots();
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

        private void RefreshArtSlots()
        {
            SetImageEnabledIfSprite(backgroundImage);
            SetImageEnabledIfSprite(panelFrame);
            SetImageEnabledIfSprite(titleLogo);
            SetImageEnabledIfSprite(startButtonIcon);
            SetImageEnabledIfSprite(optionsButtonIcon);
            SetImageEnabledIfSprite(quitButtonIcon);
        }

        private static void SetImageEnabledIfSprite(Image image)
        {
            if (image != null)
            {
                image.enabled = image.sprite != null;
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
