using AniWorld.Events;
using UnityEngine;

namespace AniWorld.GameFlow
{
    public class GameStateManager : MonoBehaviour
    {
        [SerializeField] private GameState initialState = GameState.Boot;
        [SerializeField] private RunSetupManager runSetupManager;
        [SerializeField] private TurnManager turnManager;

        public GameState CurrentState { get; private set; }
        public GameState PreviousState { get; private set; }

        private void Awake()
        {
            CurrentState = initialState;

            if (runSetupManager == null)
            {
                runSetupManager = FindObjectOfType<RunSetupManager>();
            }

            if (turnManager == null)
            {
                turnManager = FindObjectOfType<TurnManager>();
            }
        }

        private void OnEnable()
        {
            GameEventBus.Subscribe<VictoryEvent>(OnVictory);
            GameEventBus.Subscribe<DefeatEvent>(OnDefeat);
        }

        private void OnDisable()
        {
            GameEventBus.Unsubscribe<VictoryEvent>(OnVictory);
            GameEventBus.Unsubscribe<DefeatEvent>(OnDefeat);
        }

        public void ChangeState(GameState newState)
        {
            if (CurrentState == newState)
            {
                return;
            }

            PreviousState = CurrentState;
            CurrentState = newState;
            GameEventBus.Publish(new GameStateChangedEvent(PreviousState, CurrentState));
        }

        public void GoToMainMenu()
        {
            ChangeState(GameState.MainMenu);
        }

        public void StartNewRun()
        {
            ChangeState(GameState.LoadingRun);
            GameEventBus.Publish(new RunStartedEvent());

            if (runSetupManager != null)
            {
                ChangeState(GameState.PreparingBattle);
                runSetupManager.PrepareBattle();
            }

            StartPlayerTurn();
        }

        public void StartPlayerTurn()
        {
            ChangeState(GameState.PlayerTurnStart);
            turnManager?.StartPlayerTurn();
            ChangeState(GameState.PlayerTurn);
        }

        public void EndPlayerTurn()
        {
            ChangeState(GameState.PlayerTurnEnd);
            turnManager?.EndPlayerTurn();
            StartEnemyTurn();
        }

        public void StartEnemyTurn()
        {
            ChangeState(GameState.EnemyTurnStart);
            turnManager?.StartEnemyTurn();
            ChangeState(GameState.EnemyTurn);
        }

        public void EndEnemyTurn()
        {
            ChangeState(GameState.EnemyTurnEnd);
            turnManager?.EndEnemyTurn();
            turnManager?.AdvanceTurnCounter();
            StartPlayerTurn();
        }

        public void Pause()
        {
            if (CurrentState == GameState.Paused)
            {
                return;
            }

            ChangeState(GameState.Paused);
        }

        public void Resume()
        {
            if (CurrentState != GameState.Paused)
            {
                return;
            }

            ChangeState(PreviousState);
        }

        public void EndRunVictory()
        {
            ChangeState(GameState.Victory);
            GameEventBus.Publish(new VictoryEvent());
        }

        public void EndRunDefeat()
        {
            ChangeState(GameState.Defeat);
            GameEventBus.Publish(new DefeatEvent());
        }

        private void OnVictory(VictoryEvent gameEvent)
        {
            ChangeState(GameState.Victory);
        }

        private void OnDefeat(DefeatEvent gameEvent)
        {
            ChangeState(GameState.Defeat);
        }
    }
}
