using System;

namespace W65C02.API.Interfaces
{
    public interface IBus
    {
        void Dispose();
        void Publish<T>(T data);
        void Subscribe<T>(Action<T> action);
        void UnSubscribe<T>(Action<T> action);
    }
}