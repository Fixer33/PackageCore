using Core.Services.Purchasing.Products;
using UnityEngine;

namespace Core.Services.Purchasing
{
    [CreateAssetMenu(fileName = "Debug IAP", menuName = "Services/Purchasing/Debug", order = 0)]
    public class DebugIAP : IAP
    {
        [Header("Debug IAP settings")]
        [SerializeField] private bool _isInitializationSuccessful = true;
        [SerializeField] private bool _areProductsOwned;
        [SerializeField] private bool _arePurchasesSuccessful;
        private EventCallbackCollection _callbacks;
        private bool _hasPurchased;
        
        public override string GetProductCost(IAPProductBase product)
        {
            return "{product_cost}";
        }

        public override bool IsProductOwned(IAPProductBase product)
        {
            return _areProductsOwned || _hasPurchased;
        }

        public override void RestorePurchases()
        {
            Debug.Log("[Debug] Restored purchases");
        }

        protected override void Initialize(EventCallbackCollection callbacksToInvoke)
        {
            _callbacks = callbacksToInvoke;
            _hasPurchased = false;
            
            if (_isInitializationSuccessful)
                _callbacks.Initialized?.Invoke();
            else
                _callbacks.InitializeFailed?.Invoke("Debug IAP is marked for fail in it's properties");
        }

        protected override void PurchaseProduct(IAPProductBase product)
        {
            if (_arePurchasesSuccessful)
            {
                _callbacks.ProductPurchased?.Invoke(product);
                _hasPurchased = true;
            }
            else
            {
                _callbacks.ProductPurchaseFailed?.Invoke(product, "Purchase failed");
            }
        }
    }
}