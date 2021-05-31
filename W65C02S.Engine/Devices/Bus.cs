using System;
using System.Collections.Generic;
using W65C02.API.Interfaces;

namespace W65C02S.Engine.Devices
{
    // EventAggregator pattern
    public class Bus : IDisposable, IBus
    {
        object locker = new object();
        List<(Type eventType, Delegate methodToCall)> components;

        public Bus()
        {
            components = new List<(Type eventType, Delegate methodToCall)>();
        }

        public void Subscribe<T>(Action<T> action)
        {
            if (action != null)
            {
                components.Add((typeof(T), action));
            }
        }

        public void UnSubscribe<T>(Action<T> action)
        {
            if (components.Contains((typeof(T), action)))
            {
                components.Remove((typeof(T), action));
            }
        }

        public void Publish<T>(T data)
        {
            lock (locker)
            {
                foreach (var component in components)
                {
                    if (component.eventType == typeof(T))
                    {
                        ((Action<T>)component.methodToCall)(data);
                    }
                }
            }
        }

        public void Dispose()
        {
            while (components.Count > 0)
            {
                components.RemoveAt(0);
            }
        }
    }
}
