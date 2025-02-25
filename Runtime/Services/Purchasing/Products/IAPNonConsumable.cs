using Core.Services.DataSaving;
using UnityEngine;

namespace Core.Services.Purchasing.Products
{
    [CreateAssetMenu(fileName = "Non consumable", menuName = "Services/Purchasing/Products/Non consumable data", order = 0)]
    public class IAPNonConsumable : IAPProductBase, IDataSaveKey
    {
        public string GetDataSaveKey() => GetBaseId();

        public string GetDataSaveID() => GetType().Name;
    }
}