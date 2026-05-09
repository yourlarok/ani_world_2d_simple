using System;
using UnityEngine;

namespace AniWorld.Cards.Runtime
{
    public class GoldManager : MonoBehaviour
    {
        [SerializeField] private int initialGold = 10;
        [SerializeField] private int incomePerTurn = 3;

        public event Action<int> GoldChanged;

        public int CurrentGold { get; private set; }
        public int IncomePerTurn => incomePerTurn;

        private void Awake()
        {
            ResetGold();
        }

        public bool CanSpend(int amount)
        {
            return amount <= 0 || CurrentGold >= amount;
        }

        public bool Spend(int amount)
        {
            if (!CanSpend(amount))
            {
                return false;
            }

            CurrentGold -= Mathf.Max(0, amount);
            GoldChanged?.Invoke(CurrentGold);
            return true;
        }

        public void AddGold(int amount)
        {
            CurrentGold += Mathf.Max(0, amount);
            GoldChanged?.Invoke(CurrentGold);
        }

        public void ResetGold()
        {
            CurrentGold = Mathf.Max(0, initialGold);
            GoldChanged?.Invoke(CurrentGold);
        }

        public void OnTurnStart()
        {
            AddGold(incomePerTurn);
        }
    }
}
