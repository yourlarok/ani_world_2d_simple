namespace AniWorld.Cards.Runtime
{
    public enum CardPlayBlockReason
    {
        None,
        MissingCard,
        WrongCardType,
        NotEnoughGold,
        NotEnoughAP,
        FoodBarFull,
        InvalidBoardCell,
        OutsideDeployZone,
        OccupiedCell,
        MissingSystem
    }

    public struct CardPlayState
    {
        public bool CanPlay;
        public bool CanAffordGold;
        public bool CanAffordAP;
        public bool HasValidTarget;
        public CardPlayBlockReason BlockReason;

        public static CardPlayState Blocked(CardPlayBlockReason reason, bool canAffordGold, bool canAffordAP, bool hasValidTarget)
        {
            return new CardPlayState
            {
                CanPlay = false,
                CanAffordGold = canAffordGold,
                CanAffordAP = canAffordAP,
                HasValidTarget = hasValidTarget,
                BlockReason = reason
            };
        }

        public static CardPlayState Playable()
        {
            return new CardPlayState
            {
                CanPlay = true,
                CanAffordGold = true,
                CanAffordAP = true,
                HasValidTarget = true,
                BlockReason = CardPlayBlockReason.None
            };
        }
    }
}
