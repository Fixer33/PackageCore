using System;

namespace UI
{
    public interface IViewData
    {
        public T Get<T>()
        {
            if (this is T castedValue)
                return castedValue;

            throw new InvalidCastException($"Failed to cast view data of type {this.GetType().Name} to type {typeof(T).Name}");
        }
    }
}