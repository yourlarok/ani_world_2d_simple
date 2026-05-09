using UnityEngine;

namespace AniWorld.GameFlow
{
    public class GameFlowBootstrap : MonoBehaviour
    {
        [SerializeField] private GameStateManager gameStateManager;
        [SerializeField] private bool startRunOnStart;
        [SerializeField] private bool goToMainMenuOnStart = true;

        private void Awake()
        {
            if (gameStateManager == null)
            {
                gameStateManager = FindObjectOfType<GameStateManager>();
            }
        }

        private void Start()
        {
            if (gameStateManager == null)
            {
                return;
            }

            if (startRunOnStart)
            {
                gameStateManager.StartNewRun();
            }
            else if (goToMainMenuOnStart)
            {
                gameStateManager.GoToMainMenu();
            }
        }
    }
}
