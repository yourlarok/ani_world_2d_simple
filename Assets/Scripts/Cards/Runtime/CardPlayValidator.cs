using AniWorld.Board;
using AniWorld.Cards.Data;
using AniWorld.Food;
using AniWorld.Tokens;
using UnityEngine;

namespace AniWorld.Cards.Runtime
{
    public class CardPlayValidator : MonoBehaviour
    {
        [SerializeField] private BoardManager boardManager;
        [SerializeField] private GoldManager goldManager;
        [SerializeField] private APManager apManager;
        [SerializeField] private FoodBar foodBar;
        [SerializeField] private TokenSpawner tokenSpawner;

        private void Awake()
        {
            if (boardManager == null)
            {
                boardManager = FindObjectOfType<BoardManager>();
            }

            if (goldManager == null)
            {
                goldManager = FindObjectOfType<GoldManager>();
            }

            if (apManager == null)
            {
                apManager = FindObjectOfType<APManager>();
            }

            if (foodBar == null)
            {
                foodBar = FindObjectOfType<FoodBar>();
            }

            if (tokenSpawner == null)
            {
                tokenSpawner = FindObjectOfType<TokenSpawner>();
            }
        }

        public CardPlayState EvaluateCard(GameCard card)
        {
            if (card == null)
            {
                return CardPlayState.Blocked(CardPlayBlockReason.MissingCard, false, false, false);
            }

            if (card.IsFood)
            {
                return EvaluateFoodCard(card);
            }

            bool canAffordGold = goldManager != null && goldManager.CanSpend(card.GoldCost);
            bool canAffordAP = apManager != null && apManager.CanSpend(card.APCost);
            bool hasDeployTarget = HasAnyDeployTarget();

            if (!canAffordGold)
            {
                return CardPlayState.Blocked(CardPlayBlockReason.NotEnoughGold, false, canAffordAP, hasDeployTarget);
            }

            if (!canAffordAP)
            {
                return CardPlayState.Blocked(CardPlayBlockReason.NotEnoughAP, true, false, hasDeployTarget);
            }

            if (!hasDeployTarget)
            {
                return CardPlayState.Blocked(CardPlayBlockReason.InvalidBoardCell, true, true, false);
            }

            return CardPlayState.Playable();
        }

        public CardPlayState EvaluateCharacterDeployment(GameCard card, Vector2Int boardPosition)
        {
            if (card == null)
            {
                return CardPlayState.Blocked(CardPlayBlockReason.MissingCard, false, false, false);
            }

            if (!card.IsCharacter)
            {
                return CardPlayState.Blocked(CardPlayBlockReason.WrongCardType, true, true, false);
            }

            bool canAffordGold = goldManager != null && goldManager.CanSpend(card.GoldCost);
            bool canAffordAP = apManager != null && apManager.CanSpend(card.APCost);

            if (!canAffordGold)
            {
                return CardPlayState.Blocked(CardPlayBlockReason.NotEnoughGold, false, canAffordAP, true);
            }

            if (!canAffordAP)
            {
                return CardPlayState.Blocked(CardPlayBlockReason.NotEnoughAP, true, false, true);
            }

            if (boardManager == null || !boardManager.TryGetCell(boardPosition, out BoardCell cell))
            {
                return CardPlayState.Blocked(CardPlayBlockReason.InvalidBoardCell, true, true, false);
            }

            if (tokenSpawner != null && !tokenSpawner.IsInDeployZone(boardPosition))
            {
                return CardPlayState.Blocked(CardPlayBlockReason.OutsideDeployZone, true, true, false);
            }

            if (cell.IsOccupied)
            {
                return CardPlayState.Blocked(CardPlayBlockReason.OccupiedCell, true, true, false);
            }

            if (!boardManager.CanPlaceUnit(boardPosition))
            {
                return CardPlayState.Blocked(CardPlayBlockReason.InvalidBoardCell, true, true, false);
            }

            return CardPlayState.Playable();
        }

        private CardPlayState EvaluateFoodCard(GameCard card)
        {
            bool canAffordGold = goldManager != null && goldManager.CanSpend(card.GoldCost);
            bool hasFoodSpace = foodBar != null && !foodBar.IsFull;

            if (!canAffordGold)
            {
                return CardPlayState.Blocked(CardPlayBlockReason.NotEnoughGold, false, true, hasFoodSpace);
            }

            if (!hasFoodSpace)
            {
                return CardPlayState.Blocked(CardPlayBlockReason.FoodBarFull, true, true, false);
            }

            return CardPlayState.Playable();
        }

        private bool HasAnyDeployTarget()
        {
            if (boardManager == null)
            {
                return false;
            }

            foreach (BoardCell cell in boardManager.Cells)
            {
                if (cell == null)
                {
                    continue;
                }

                if (tokenSpawner != null && !tokenSpawner.IsInDeployZone(cell.Position))
                {
                    continue;
                }

                if (cell.CanPlaceUnit)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
