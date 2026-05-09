namespace AniWorld.GameFlow
{
    public enum GameState
    {
        Boot,
        MainMenu,
        LoadingRun,
        PreparingBattle,
        PlayerTurnStart,
        PlayerTurn,
        PlayerTurnEnd,
        EnemyTurnStart,
        EnemyTurn,
        EnemyTurnEnd,
        Victory,
        Defeat,
        Paused
    }

    public enum TurnOwner
    {
        Player,
        Enemy,
        Neutral
    }
}
