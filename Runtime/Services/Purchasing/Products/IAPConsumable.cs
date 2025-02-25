using System;
using UnityEngine;

namespace Core.Services.Purchasing.Products
{
    [CreateAssetMenu(fileName = "Consumable", menuName = "Services/Purchasing/Products/Consumable data", order = 0)]
    public class IAPConsumable : IAPProductBase
    {
        public event Action Consumed;
        
        public void Consume()
        {
            Consumed?.Invoke();
        }
    }
}