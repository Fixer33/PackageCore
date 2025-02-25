#if SIS_IAP
using SIS;
using Core;
using Core.Services.Purchasing;
using Core.Services.Purchasing.Products;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
#endif

namespace Core.Services.Purchasing
{
#if SIS_IAP == FALSE
    public class SimpleIAP : ServiceScriptableObject
    {
        public override void InitializeService()
        {
            throw new System.NotImplementedException();
        }
    }
#else
    [CreateAssetMenu(fileName = "Simple IAP", menuName = "Services/Purchasing/Simple IAP")]
    public class SimpleIAP : IAP
    {
        [SerializeField] private IAPManager _iapManager;
        private EventCallbackCollection _callbacks;
        private IAPProductBase _productBeingPurchased;

        private void Awake()
        {
            _iapManager.autoInitialize = false;
            IAPManager.initializeSucceededEvent += IAPManagerOninitializeSucceededEvent;
            IAPManager.initializeFailedEvent += IAPManagerOninitializeFailedEvent;
            IAPManager.purchaseFailedEvent += IAPManagerOnpurchaseFailedEvent;
            IAPManager.purchaseSucceededEvent += IAPManagerOnpurchaseSucceededEvent;
        }

        protected override void DisposeIAP()
        {
            IAPManager.initializeSucceededEvent -= IAPManagerOninitializeSucceededEvent;
            IAPManager.initializeFailedEvent -= IAPManagerOninitializeFailedEvent;
            IAPManager.purchaseFailedEvent -= IAPManagerOnpurchaseFailedEvent;
            IAPManager.purchaseSucceededEvent -= IAPManagerOnpurchaseSucceededEvent;
        }

        private void IAPManagerOnpurchaseSucceededEvent(string args)
        {
            _callbacks.ProductPurchased?.Invoke(_productBeingPurchased);
        }

        private void IAPManagerOnpurchaseFailedEvent(string args)
        {
            _callbacks.ProductPurchaseFailed?.Invoke(_productBeingPurchased, args);
        }

        private void IAPManagerOninitializeFailedEvent(string args)
        {
            _callbacks.InitializeFailed?.Invoke(args);
        }

        private void IAPManagerOninitializeSucceededEvent()
        {
            _callbacks.Initialized?.Invoke();
            _productBeingPurchased = null;
        }

        public override string GetProductCost(IAPProductBase product)
        {
            return IAPManager.GetIAPProduct(product.GetBaseId()).GetPriceString();
        }

        public override bool IsProductOwned(IAPProductBase product)
        {
#warning If sub is recalled it must be checked here because DBManager is not going to :)
            return DBManager.IsPurchased(product.GetBaseId());
        }

        public override void RestorePurchases()
        {
            IAPManager.RestoreTransactions();
        }

        public override bool HasFreeTrial(IAPSubscription subscription)
        {
#warning Free trial detecting does not seem possible with this plugin. Therefore unsupported at the moment
            Debug.LogWarning("Trying to get free trial from SimpleIAP. It is unsupported at the moment and will always return false!");
            return false;
        }

        protected override void Initialize(EventCallbackCollection callbacksToInvoke)
        {
            _callbacks = callbacksToInvoke;
        }

        protected override void PurchaseProduct(IAPProductBase product)
        {
            _productBeingPurchased = product;
            IAPManager.Purchase(product.GetBaseId());
        }
        
#if UNITY_EDITOR && SIS_IAP
        private void OnValidate()
        {
            if (_iapManager != null && _iapManager.autoInitialize)
            {
                Debug.LogWarning("SimpleIAP Manager must not be auto initialized!");
                _iapManager.autoInitialize = false;
                EditorUtility.SetDirty(_iapManager);
            }
        }
#endif
    }
#endif
}