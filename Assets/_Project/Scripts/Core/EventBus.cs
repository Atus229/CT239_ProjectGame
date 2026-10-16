using System;
using System.Collections.Generic;

namespace _Project.Scripts.Core
{
    public static class EventBus
    {
        //Danh bạ để lưu tru key(Kiểu sự kiện) -> Action(Delegate)
        private static readonly Dictionary<Type, Delegate> _events = new Dictionary<Type, Delegate>();
        
        //Dang ky nhan su kien
        public static void Subscribe<T>(Action<T> listener) where T : struct
        {
            Type eventType = typeof(T);

            if (_events.ContainsKey(eventType))
            {
                //neu do co dang ky roi
                //noi them listener moi vao su kien -> 1 su kien nhieu listener
                _events[eventType] = Delegate.Combine(_events[eventType], listener);
            }
            else
            {
                _events[eventType] = listener;
            }
        }
        
        //Ham huy dang ky
        public static void Unsubscribe<T>(Action<T> listener) where T : struct
        {
            Type eventType = typeof(T);
            if (_events.ContainsKey(eventType))
            {
                var currentDelegate = Delegate.Remove(_events[eventType], listener);

                if (currentDelegate == null)
                {
                    _events.Remove(eventType);
                }
                else
                {
                    _events[eventType] = currentDelegate;
                }
            }
        }
        
        //Ham phat tin 
        public static void Publish<T>(T eventData)
        {
            Type eventType = typeof(T);
            if (_events.TryGetValue(eventType, out Delegate d))
            {
                (d as Action<T>)?.Invoke(eventData);
            }
        }
    }
}