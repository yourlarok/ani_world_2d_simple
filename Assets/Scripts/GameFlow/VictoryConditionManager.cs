using AniWorld.Cards.Data;
using AniWorld.Events;
using AniWorld.Tokens;
using UnityEngine;

namespace AniWorld.GameFlow
{
    public class VictoryConditionManager : MonoBehaviour
    {
        [SerializeField] private TokenSpawner tokenSpawner;
        [SerializeField] private bool checkOnTokenRemoved = true;

        private void Awake()
        {
            if (tokenSpawner == null)
            {
                tokenSpawner = FindObjectOfType<TokenSpawner>();
            }
        }

        private void OnEnable()
        {
            if (tokenSpawner != null && checkOnTokenRemoved)
            {
                tokenSpawner.TokenRemoved += OnTokenRemoved;
            }
        }

        private void OnDisable()
        {
            if (tokenSpawner != null)
            {
                tokenSpawner.TokenRemoved -= OnTokenRemoved;
            }
        }

        public void CheckVictoryConditions()
        {
            if (tokenSpawner == null)
            {
                return;
            }

            bool hasPlayer = false;
            bool hasEnemy = false;

            foreach (GameToken token in tokenSpawner.ActiveTokens)
            {
                if (token == null || token.CurrentHP <= 0)
                {
                    continue;
                }

                if (token.Team == TeamType.Player)
                {
                    hasPlayer = true;
                }
                else if (token.Team == TeamType.Enemy)
                {
                    hasEnemy = true;
                }
            }

            if (hasPlayer && !hasEnemy)
            {
                GameEventBus.Publish(new VictoryEvent());
            }
            else if (!hasPlayer && hasEnemy)
            {
                GameEventBus.Publish(new DefeatEvent());
            }
        }

        private void OnTokenRemoved(GameToken token)
        {
            CheckVictoryConditions();
        }
    }
}
