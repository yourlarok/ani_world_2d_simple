using System;
using UnityEngine;

namespace AniWorld.Cards.Runtime
{
    public class APManager : MonoBehaviour
    {
        [SerializeField] private int apPerTurn = 8;
        [SerializeField] private int maxAP = 10;
        [SerializeField] private int startingAP = 8;

        public event Action<int, int> APChanged;

        public int CurrentAP { get; private set; }
        public int MaxAP => maxAP;
        public int APPerTurn => apPerTurn;

        private void Awake()
        {
            ResetAP();
        }

        public bool CanSpend(int amount)
        {
            return amount <= 0 || CurrentAP >= amount;
        }

        public bool Spend(int amount)
        {
            if (!CanSpend(amount))
            {
                return false;
            }

            CurrentAP -= Mathf.Max(0, amount);
            APChanged?.Invoke(CurrentAP, maxAP);
            return true;
        }

        public void AddAP(int amount)
        {
            CurrentAP = Mathf.Clamp(CurrentAP + Mathf.Max(0, amount), 0, maxAP);
            APChanged?.Invoke(CurrentAP, maxAP);
        }

        public void ResetAP()
        {
            CurrentAP = Mathf.Clamp(startingAP, 0, maxAP);
            APChanged?.Invoke(CurrentAP, maxAP);
        }

        public void OnTurnStart()
        {
            AddAP(apPerTurn);
        }
    }
}
