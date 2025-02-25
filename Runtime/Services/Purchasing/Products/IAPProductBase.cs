using System;
using Services;
using UnityEngine;

namespace Core.Services.Purchasing.Products
{
    public abstract class IAPProductBase : ScriptableObject
    {
        [SerializeField] private string _id;
        [Space, Header("Leave empty to use base id")]
        [SerializeField] private PlatformDependentReference<string> _storeOverrides;

        public string GetId()
        {
            string result = _storeOverrides.Get();
            if (result is not { Length: > 1 })
                result = _id;

            return result;
        }
        
        public string GetBaseId()
        {
            return _id;
        }
        
        public string GetPrice()
        {
            if (IAP.Instance == false)
                throw new Exception("IAP object does not exist yet!");

            return IAP.Instance.GetProductCost(this);
        }

        public bool IsOwned()
        {
            if (IAP.Instance == false)
                throw new Exception("IAP object does not exist yet!");

            return IAP.Instance.IsProductOwned(this);
        }
    }
}