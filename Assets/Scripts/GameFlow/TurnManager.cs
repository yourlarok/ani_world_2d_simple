using AniWorld.Cards.Runtime;
using AniWorld.Events;
using AniWorld.Tokens;
using UnityEngine;

namespace AniWorld.GameFlow
{
    public class TurnManager : MonoBehaviour
    {
        [SerializeField] private GoldManager goldManager;
        [SerializeField] private APManager apManager;
        [SerializeField] private HandManager handManager;
        [SerializeField] private TokenSpawner tokenSpawner;
        [SerializeField] private bool grantResourcesOnFirstPlayerTurn;

        public int TurnNumber { get; private set; } = 1;
        public TurnOwner CurrentOwner { get; private set; } = TurnOwner.Neutral;

        private void Awake()
        {
            if (goldManager == null)
            {
                goldManager = FindObjectOfType<GoldManager>();
            }

            if (apManager == null)
            {
                apManager = FindObjectOfType<APManager>();
            }

            if (handManager == null)
            {
                handManager = FindObjectOfType<HandManager>();
            }

            if (tokenSpawner == null)
            {
                tokenSpawner = FindObjectOfType<TokenSpawner>();
            }
        }

        public void ResetTurns()
        {
            TurnNumber = 1;
            CurrentOwner = TurnOwner.Neutral;
        }

        public void StartPlayerTurn()
        {
            CurrentOwner = TurnOwner.Player;
            if (grantResourcesOnFirstPlayerTurn || TurnNumber > 1)
            {
                goldManager?.OnTurnStart();
                apManager?.OnTurnStart();
            }

            handManager?.OnTurnStart();

            if (tokenSpawner != null)
            {
                foreach (GameToken token in tokenSpawner.ActiveTokens)
                {
                    if (token != null && token.Team == AniWorld.Cards.Data.TeamType.Player)
                    {
                        token.OnTurnStart();
                    }
                }
            }

            GameEventBus.Publish(new TurnStartedEvent(CurrentOwner, TurnNumber));
        }

        public void EndPlayerTurn()
        {
            if (tokenSpawner != null)
            {
                foreach (GameToken token in tokenSpawner.ActiveTokens)
                {
                    if (token != null && token.Team == AniWorld.Cards.Data.TeamType.Player)
                    {
                        token.OnTurnEnd();
                    }
                }
            }

            GameEventBus.Publish(new TurnEndedEvent(TurnOwner.Player, TurnNumber));
        }

        public void StartEnemyTurn()
        {
            CurrentOwner = TurnOwner.Enemy;

            if (tokenSpawner != null)
            {
                foreach (GameToken token in tokenSpawner.ActiveTokens)
                {
                    if (token != null && token.Team == AniWorld.Cards.Data.TeamType.Enemy)
                    {
                        token.OnTurnStart();
                    }
                }
            }

            GameEventBus.Publish(new TurnStartedEvent(CurrentOwner, TurnNumber));
        }

        public void EndEnemyTurn()
        {
            if (tokenSpawner != null)
            {
                foreach (GameToken token in tokenSpawner.ActiveTokens)
                {
                    if (token != null && token.Team == AniWorld.Cards.Data.TeamType.Enemy)
                    {
                        token.OnTurnEnd();
                    }
                }
            }

            GameEventBus.Publish(new TurnEndedEvent(TurnOwner.Enemy, TurnNumber));
        }

        public void AdvanceTurnCounter()
        {
            TurnNumber++;
        }
    }
}
