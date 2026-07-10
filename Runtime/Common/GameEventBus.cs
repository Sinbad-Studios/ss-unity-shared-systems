using System;
using System.Collections.Generic;

namespace SinbadStudios.SharedSystems.Runtime
{
    public class GameEventBus
    {
        private static GameEventBus _instance;
        public static GameEventBus Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new GameEventBus();
                }
                return _instance;
            }
        }

        private readonly Dictionary<Type, List<Delegate>> _subscribers = new Dictionary<Type, List<Delegate>>();

        private GameEventBus() { }

        /// <summary>
        /// Subscribe a handler to a specific event type.
        /// </summary>
        /// <typeparam name="T">The type of the event data (should be a class).</typeparam>
        /// <param name="handler">The action to invoke when the event is published.</param>
        public void Subscribe<T>(Action<T> handler) where T : class
        {
            Type eventType = typeof(T);
            if (!_subscribers.ContainsKey(eventType))
            {
                _subscribers[eventType] = new List<Delegate>();
            }
            _subscribers[eventType].Add(handler);
        }

        /// <summary>
        /// Unsubscribe a handler from a specific event type.
        /// </summary>
        /// <typeparam name="T">The type of the event data (should be a class).</typeparam>
        /// <param name="handler">The action to remove.</param>
        public void Unsubscribe<T>(Action<T> handler) where T : class
        {
            Type eventType = typeof(T);
            if (_subscribers.TryGetValue(eventType, out var handlers))
            {
                handlers.Remove(handler);
                if (handlers.Count == 0)
                {
                    _subscribers.Remove(eventType);
                }
            }
        }

        /// <summary>
        /// Publish an event to all subscribed handlers.
        /// </summary>
        /// <typeparam name="T">The type of the event data (should be a class).</typeparam>
        /// <param name="eventData">The data to pass to the handlers.</param>
        public void Publish<T>(T eventData) where T : class
        {
            Type eventType = typeof(T);
            if (_subscribers.TryGetValue(eventType, out var handlers))
            {
                var handlersCopy = new List<Delegate>(handlers);
                foreach (var handler in handlersCopy)
                {
                    ((Action<T>)handler)(eventData);
                }
            }
        }
    }
}
