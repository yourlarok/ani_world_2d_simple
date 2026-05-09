using AniWorld.GameFlow;
using UnityEngine;
using UnityEngine.UI;

namespace AniWorld.UI.GameFlow
{
    public class PauseMenuView : MonoBehaviour
    {
        [SerializeField] private GameStateManager gameStateManager;
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button mainMenuButton;

        [Header("Art Slots")]
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Image panelFrame;
        [SerializeField] private Image resumeButtonIcon;
        [SerializeField] private Image mainMenuButtonIcon;

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
            if (resumeButton != null)
            {
                resumeButton.onClick.AddListener(Resume);
            }

            if (mainMenuButton != null)
            {
                mainMenuButton.onClick.AddListener(GoToMainMenu);
            }
        }

        private void OnDisable()
        {
            if (resumeButton != null)
            {
                resumeButton.onClick.RemoveListener(Resume);
            }

            if (mainMenuButton != null)
            {
                mainMenuButton.onClick.RemoveListener(GoToMainMenu);
            }
        }

        public void Resume()
        {
            gameStateManager?.Resume();
        }

        public void GoToMainMenu()
        {
            gameStateManager?.GoToMainMenu();
        }

        private void RefreshArtSlots()
        {
            SetImageEnabledIfSprite(backgroundImage);
            SetImageEnabledIfSprite(panelFrame);
            SetImageEnabledIfSprite(resumeButtonIcon);
            SetImageEnabledIfSprite(mainMenuButtonIcon);
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
