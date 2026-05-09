using AniWorld.Tokens;
using UnityEngine;

namespace AniWorld.Cards.Runtime
{
    public class CardGameSession : MonoBehaviour
    {
        [SerializeField] private GoldManager goldManager;
        [SerializeField] private APManager apManager;
        [SerializeField] private HandManager handManager;
        [SerializeField] private TokenSpawner tokenSpawner;

        public int TurnNumber { get; private set; } = 1;

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

        public void StartNextTurn()
        {
            TurnNumber++;
            goldManager?.OnTurnStart();
            apManager?.OnTurnStart();
            handManager?.OnTurnStart();

            if (tokenSpawner == null)
            {
                return;
            }

            foreach (GameToken token in tokenSpawner.ActiveTokens)
            {
                token?.OnTurnStart();
            }
        }

        public void EndTurn()
        {
            if (tokenSpawner == null)
            {
                return;
            }

            foreach (GameToken token in tokenSpawner.ActiveTokens)
            {
                token?.OnTurnEnd();
            }
        }
    }
}
