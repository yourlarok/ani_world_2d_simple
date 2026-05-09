using AniWorld.Board;
using AniWorld.Cards.Data;
using AniWorld.Cards.Runtime;
using AniWorld.Combos;
using AniWorld.GameFlow;
using AniWorld.Tokens;
using UnityEngine;

namespace AniWorld.Events
{
    public sealed class GameStateChangedEvent : IGameEvent
    {
        public GameState PreviousState { get; }
        public GameState NewState { get; }

        public GameStateChangedEvent(GameState previousState, GameState newState)
        {
            PreviousState = previousState;
            NewState = newState;
        }
    }

    public sealed class RunStartedEvent : IGameEvent
    {
    }

    public sealed class RunPreparedEvent : IGameEvent
    {
    }

    public sealed class RunResetEvent : IGameEvent
    {
    }

    public sealed class TurnStartedEvent : IGameEvent
    {
        public TurnOwner Owner { get; }
        public int TurnNumber { get; }

        public TurnStartedEvent(TurnOwner owner, int turnNumber)
        {
            Owner = owner;
            TurnNumber = turnNumber;
        }
    }

    public sealed class TurnEndedEvent : IGameEvent
    {
        public TurnOwner Owner { get; }
        public int TurnNumber { get; }

        public TurnEndedEvent(TurnOwner owner, int turnNumber)
        {
            Owner = owner;
            TurnNumber = turnNumber;
        }
    }

    public sealed class CardPurchasedEvent : IGameEvent
    {
        public GameCard Card { get; }

        public CardPurchasedEvent(GameCard card)
        {
            Card = card;
        }
    }

    public sealed class CardDiscardedEvent : IGameEvent
    {
        public GameCard Card { get; }

        public CardDiscardedEvent(GameCard card)
        {
            Card = card;
        }
    }

    public sealed class FoodStoredEvent : IGameEvent
    {
        public FoodCardData Food { get; }

        public FoodStoredEvent(FoodCardData food)
        {
            Food = food;
        }
    }

    public sealed class FoodFedEvent : IGameEvent
    {
        public FoodCardData Food { get; }
        public GameToken Target { get; }

        public FoodFedEvent(FoodCardData food, GameToken target)
        {
            Food = food;
            Target = target;
        }
    }

    public sealed class TokenSpawnedEvent : IGameEvent
    {
        public GameToken Token { get; }

        public TokenSpawnedEvent(GameToken token)
        {
            Token = token;
        }
    }

    public sealed class TokenRemovedEvent : IGameEvent
    {
        public GameToken Token { get; }

        public TokenRemovedEvent(GameToken token)
        {
            Token = token;
        }
    }

    public sealed class ComboReadyEvent : IGameEvent
    {
        public GameToken Token { get; }
        public FoodComboData Combo { get; }

        public ComboReadyEvent(GameToken token, FoodComboData combo)
        {
            Token = token;
            Combo = combo;
        }
    }

    public sealed class ComboTriggeredEvent : IGameEvent
    {
        public ComboCastRequest Request { get; }

        public ComboTriggeredEvent(ComboCastRequest request)
        {
            Request = request;
        }
    }

    public sealed class TerrainChangedEvent : IGameEvent
    {
        public Vector2Int Position { get; }
        public TerrainType Terrain { get; }

        public TerrainChangedEvent(Vector2Int position, TerrainType terrain)
        {
            Position = position;
            Terrain = terrain;
        }
    }

    public sealed class VictoryEvent : IGameEvent
    {
    }

    public sealed class DefeatEvent : IGameEvent
    {
    }
}
