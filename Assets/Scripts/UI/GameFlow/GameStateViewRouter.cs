using AniWorld.Events;
using AniWorld.GameFlow;
using UnityEngine;

namespace AniWorld.UI.GameFlow
{
    public class GameStateViewRouter : MonoBehaviour
    {
        [SerializeField] private GameStateManager gameStateManager;
        [SerializeField] private GameObject mainMenuPanel;
        [SerializeField] private GameObject battleHudPanel;
        [SerializeField] private GameObject pauseMenuPanel;
        [SerializeField] private GameObject resultPanel;

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
            Refresh();
        }

        private void OnDisable()
        {
            GameEventBus.Unsubscribe<GameStateChangedEvent>(OnGameStateChanged);
        }

        public void Refresh()
        {
            if (gameStateManager == null)
            {
                return;
            }

            ApplyState(gameStateManager.CurrentState);
        }

        private void OnGameStateChanged(GameStateChangedEvent gameEvent)
        {
            ApplyState(gameEvent.NewState);
        }

        private void ApplyState(GameState state)
        {
            SetActive(mainMenuPanel, state == GameState.MainMenu || state == GameState.Boot);
            SetActive(battleHudPanel, IsBattleState(state));
            SetActive(pauseMenuPanel, state == GameState.Paused);
            SetActive(resultPanel, state == GameState.Victory || state == GameState.Defeat);
        }

        private static bool IsBattleState(GameState state)
        {
            return state == GameState.PreparingBattle ||
                state == GameState.PlayerTurnStart ||
                state == GameState.PlayerTurn ||
                state == GameState.PlayerTurnEnd ||
                state == GameState.EnemyTurnStart ||
                state == GameState.EnemyTurn ||
                state == GameState.EnemyTurnEnd;
        }

        private static void SetActive(GameObject target, bool active)
        {
            if (target != null)
            {
                target.SetActive(active);
            }
        }
    }
}
