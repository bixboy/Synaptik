using System;
using System.Collections.Generic;

namespace Synaptik.Game
{
    public static class GameEventBus
    {
        private static readonly Dictionary<string, Action<object>> _listeners = new();

        public static void Publish(string eventName, object payload = null)
        {
            if (string.IsNullOrEmpty(eventName))
            {
                return;
            }

            if (_listeners.TryGetValue(eventName, out var handlers))
            {
                handlers?.Invoke(payload);
            }
        }

        public static void OnEvent(string eventName, Action<object> handler)
        {
            if (string.IsNullOrEmpty(eventName) || handler == null)
            {
                return;
            }

            if (_listeners.TryGetValue(eventName, out var existing))
            {
                existing += handler;
                _listeners[eventName] = existing;
            }
            else
            {
                _listeners.Add(eventName, handler);
            }
        }

        public static void RemoveListener(string eventName, Action<object> handler)
        {
            if (string.IsNullOrEmpty(eventName) || handler == null)
            {
                return;
            }

            if (_listeners.TryGetValue(eventName, out var existing))
            {
                existing -= handler;
                if (existing == null)
                {
                    _listeners.Remove(eventName);
                }
                else
                {
                    _listeners[eventName] = existing;
                }
            }
        }
    }
}