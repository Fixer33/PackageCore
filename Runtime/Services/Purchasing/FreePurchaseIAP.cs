using Core.Services.Purchasing.Products;
using UnityEngine;

namespace Core.Services.Purchasing
{
    [CreateAssetMenu(fileName = "Free IAP", menuName = "Services/Purchasing/Free IAP")]
    public class FreePurchaseIAP : IAP
    {
        [SerializeField] private string _priceString = "0";
        private EventCallbackCollection _callbackCollection;

        public override string GetProductCost(IAPProductBase product) => _priceString;

        public override bool IsProductOwned(IAPProductBase product) => true;

        public override void RestorePurchases() { }

        protected override void Initialize(EventCallbackCollection callbacksToInvoke)
        {
            _callbackCollection = callbacksToInvoke;
            callbacksToInvoke.Initialized?.Invoke();
        }

        protected override void PurchaseProduct(IAPProductBase product)
        {
            _callbackCollection.ProductPurchased?.Invoke(product);
        }
    }
}