using System;
using System.Collections.Generic;

namespace AniWorld.Events
{
    public static class GameEventBus
    {
        private static readonly Dictionary<Type, Delegate> Subscribers = new Dictionary<Type, Delegate>();

        public static void Subscribe<TEvent>(Action<TEvent> handler) where TEvent : IGameEvent
        {
            if (handler == null)
            {
                return;
            }

            Type eventType = typeof(TEvent);
            if (Subscribers.TryGetValue(eventType, out Delegate existing))
            {
                Subscribers[eventType] = Delegate.Combine(existing, handler);
            }
            else
            {
                Subscribers[eventType] = handler;
            }
        }

        public static void Unsubscribe<TEvent>(Action<TEvent> handler) where TEvent : IGameEvent
        {
            if (handler == null)
            {
                return;
            }

            Type eventType = typeof(TEvent);
            if (!Subscribers.TryGetValue(eventType, out Delegate existing))
            {
                return;
            }

            Delegate current = Delegate.Remove(existing, handler);
            if (current == null)
            {
                Subscribers.Remove(eventType);
            }
            else
            {
                Subscribers[eventType] = current;
            }
        }

        public static void Publish<TEvent>(TEvent gameEvent) where TEvent : IGameEvent
        {
            if (gameEvent == null)
            {
                return;
            }

            Type eventType = typeof(TEvent);
            if (!Subscribers.TryGetValue(eventType, out Delegate existing))
            {
                return;
            }

            if (existing is Action<TEvent> handler)
            {
                handler.Invoke(gameEvent);
            }
        }

        public static void Clear()
        {
            Subscribers.Clear();
        }
    }
}
